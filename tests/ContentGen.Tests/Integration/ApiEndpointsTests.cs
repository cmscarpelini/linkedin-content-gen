using System.Net;
using System.Net.Http.Json;
using ContentGen.Application.DTOs;
using ContentGen.Domain.Entities;
using ContentGen.Infrastructure.Persistence;
using ContentGen.Tests.TestDoubles;
using Microsoft.Extensions.DependencyInjection;

namespace ContentGen.Tests.Integration;

public class ApiEndpointsTests
{
    private static async Task SeedArticleAsync(ContentGenApiFactory factory, ArticleRawContent article)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ContentDbContext>();
        db.Articles.Add(article);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task SearchArticles_ReturnsRssResults_AndPersistsThem()
    {
        using var factory = new ContentGenApiFactory();
        factory.RssArticles.Add(Fakes.Article(url: "https://example.com/a"));
        factory.RssArticles.Add(Fakes.Article(url: "https://example.com/b"));
        var client = factory.CreateClient();

        var searchResponse = await client.GetAsync("/articles/search");
        Assert.Equal(HttpStatusCode.OK, searchResponse.StatusCode);
        var found = await searchResponse.Content.ReadFromJsonAsync<List<ArticleDto>>();
        Assert.Equal(2, found!.Count);

        // They are now persisted and surface through GET /articles.
        var saved = await client.GetFromJsonAsync<List<SavedArticleDto>>("/articles");
        Assert.Equal(2, saved!.Count);
        Assert.All(saved, a => Assert.False(a.HasContent));
    }

    [Fact]
    public async Task ListArticles_WhenEmpty_ReturnsEmptyList()
    {
        using var factory = new ContentGenApiFactory();
        var client = factory.CreateClient();

        var saved = await client.GetFromJsonAsync<List<SavedArticleDto>>("/articles");

        Assert.Empty(saved!);
    }

    [Fact]
    public async Task GenerateContent_UnknownArticle_Returns404()
    {
        using var factory = new ContentGenApiFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/content/generate", new { articleId = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(0, factory.Ai.CallCount);
    }

    [Fact]
    public async Task GenerateContent_UnknownArticle_ReturnsProblemDetailsBody()
    {
        using var factory = new ContentGenApiFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/content/generate", new { articleId = Guid.NewGuid() });

        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>();
        Assert.Equal(404, problem!.Status);
        Assert.Equal("Resource not found", problem.Title);
        Assert.Contains("not found", problem.Detail);
    }

    private record ProblemResponse(string? Title, int? Status, string? Detail);

    [Fact]
    public async Task GenerateContent_ValidArticle_ReturnsBilingualPackage_AndCallsAiOnce()
    {
        using var factory = new ContentGenApiFactory();
        var article = Fakes.Article();
        await SeedArticleAsync(factory, article);
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/content/generate", new { articleId = article.Id });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var package = await response.Content.ReadFromJsonAsync<ReviewPackageDto>();
        Assert.Equal(article.Id, package!.ArticleId);
        Assert.Equal("Summary PT", package.PtBR.TechnicalSummary);
        Assert.Equal("Summary EN", package.EnUS.TechnicalSummary);
        Assert.Equal(1, factory.Ai.CallCount);
        Assert.Equal(1, factory.Extractor.CallCount);
    }

    [Fact]
    public async Task GenerateContent_CalledTwice_IsIdempotent_AndSkipsAiSecondTime()
    {
        using var factory = new ContentGenApiFactory();
        var article = Fakes.Article();
        await SeedArticleAsync(factory, article);
        var client = factory.CreateClient();

        await client.PostAsJsonAsync("/content/generate", new { articleId = article.Id });
        var second = await client.PostAsJsonAsync("/content/generate", new { articleId = article.Id });

        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal(1, factory.Ai.CallCount); // not called again
    }

    [Fact]
    public async Task GetContent_BeforeGeneration_Returns404_AfterGeneration_Returns200()
    {
        using var factory = new ContentGenApiFactory();
        var article = Fakes.Article();
        await SeedArticleAsync(factory, article);
        var client = factory.CreateClient();

        var before = await client.GetAsync($"/content/{article.Id}");
        Assert.Equal(HttpStatusCode.NotFound, before.StatusCode);

        await client.PostAsJsonAsync("/content/generate", new { articleId = article.Id });

        var after = await client.GetAsync($"/content/{article.Id}");
        Assert.Equal(HttpStatusCode.OK, after.StatusCode);
        var package = await after.Content.ReadFromJsonAsync<ReviewPackageDto>();
        Assert.Equal(article.Id, package!.ArticleId);
    }

    [Fact]
    public async Task ListContent_AfterGeneration_ReturnsHistory()
    {
        using var factory = new ContentGenApiFactory();
        var article = Fakes.Article(title: "Generated piece");
        await SeedArticleAsync(factory, article);
        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/content/generate", new { articleId = article.Id });

        var history = await client.GetFromJsonAsync<List<ContentSummaryDto>>("/content");

        var item = Assert.Single(history!);
        Assert.Equal(article.Id, item.ArticleId);
        Assert.Equal("Generated piece", item.ArticleTitle);
    }

    [Fact]
    public async Task GetContent_WithMalformedGuid_Returns404()
    {
        using var factory = new ContentGenApiFactory();
        var client = factory.CreateClient();

        // Route constraint {articleId:guid} rejects non-GUID segments.
        var response = await client.GetAsync("/content/not-a-guid");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SetPostPublished_MarksPost_AndGetContentReflectsIt()
    {
        using var factory = new ContentGenApiFactory();
        var article = Fakes.Article();
        await SeedArticleAsync(factory, article);
        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/content/generate", new { articleId = article.Id });

        var publish = await client.PutAsJsonAsync(
            $"/content/{article.Id}/posts/pt-BR/0/published", new { published = true });
        Assert.Equal(HttpStatusCode.NoContent, publish.StatusCode);

        var package = await client.GetFromJsonAsync<ReviewPackageDto>($"/content/{article.Id}");
        Assert.True(package!.PtBR.PostSuggestions[0].Published);
        Assert.False(package.PtBR.PostSuggestions[1].Published);
        Assert.All(package.EnUS.PostSuggestions, p => Assert.False(p.Published));
    }

    [Fact]
    public async Task SetPostPublished_ThenUnpublish_ClearsTheFlag()
    {
        using var factory = new ContentGenApiFactory();
        var article = Fakes.Article();
        await SeedArticleAsync(factory, article);
        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/content/generate", new { articleId = article.Id });

        await client.PutAsJsonAsync($"/content/{article.Id}/posts/en-US/2/published", new { published = true });
        await client.PutAsJsonAsync($"/content/{article.Id}/posts/en-US/2/published", new { published = false });

        var package = await client.GetFromJsonAsync<ReviewPackageDto>($"/content/{article.Id}");
        Assert.All(package!.EnUS.PostSuggestions, p => Assert.False(p.Published));
    }

    [Fact]
    public async Task SetPostPublished_InvalidLanguage_Returns400()
    {
        using var factory = new ContentGenApiFactory();
        var article = Fakes.Article();
        await SeedArticleAsync(factory, article);
        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/content/generate", new { articleId = article.Id });

        var response = await client.PutAsJsonAsync(
            $"/content/{article.Id}/posts/fr-FR/0/published", new { published = true });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
