using System.Text.Json;
using ContentGen.Application.Exceptions;
using ContentGen.Application.UseCases;
using ContentGen.Tests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace ContentGen.Tests.UseCases;

public class GenerateContentUseCaseTests
{
    private static GenerateContentUseCase Build(
        InMemoryContentRepository repo,
        out StubContentExtractor extractor,
        out StubAiContentService ai)
    {
        extractor = new StubContentExtractor();
        ai = new StubAiContentService(Fakes.AiResponse());
        return new GenerateContentUseCase(repo, extractor, ai, NullLogger<GenerateContentUseCase>.Instance);
    }

    [Fact]
    public async Task ExecuteAsync_WhenArticleDoesNotExist_ThrowsNotFound()
    {
        var repo = new InMemoryContentRepository();
        var useCase = Build(repo, out _, out _);

        await Assert.ThrowsAsync<NotFoundException>(
            () => useCase.ExecuteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoContentYet_ExtractsCallsAiAndPersists()
    {
        var repo = new InMemoryContentRepository();
        var article = Fakes.Article();
        repo.Articles.Add(article);
        var useCase = Build(repo, out var extractor, out var ai);

        var result = await useCase.ExecuteAsync(article.Id);

        Assert.Equal(1, extractor.CallCount);
        Assert.Equal(1, ai.CallCount);
        Assert.Equal(1, repo.SaveProcessedContentCalls);
        Assert.Single(repo.ProcessedContents);
        Assert.Equal(article.Id, result.ArticleId);
        Assert.Equal(article.Title, result.ArticleTitle);
        Assert.Equal(article.Url, result.ArticleUrl);
    }

    [Fact]
    public async Task ExecuteAsync_WhenContentAlreadyExists_IsIdempotentAndSkipsAi()
    {
        var repo = new InMemoryContentRepository();
        var article = Fakes.Article();
        repo.Articles.Add(article);
        repo.ProcessedContents.Add(Fakes.Processed(article.Id, article));
        var useCase = Build(repo, out var extractor, out var ai);

        var result = await useCase.ExecuteAsync(article.Id);

        Assert.Equal(0, ai.CallCount);
        Assert.Equal(0, extractor.CallCount);
        Assert.Equal(0, repo.SaveProcessedContentCalls);
        Assert.Equal(article.Id, result.ArticleId);
        // Content comes from the persisted record, not the AI stub.
        Assert.Equal("Summary PT", result.PtBR.TechnicalSummary);
    }

    [Fact]
    public async Task ExecuteAsync_PersistsInsightsAndPostsAsRoundTrippableJson()
    {
        var repo = new InMemoryContentRepository();
        var article = Fakes.Article();
        repo.Articles.Add(article);
        var useCase = Build(repo, out _, out _);

        await useCase.ExecuteAsync(article.Id);

        var saved = Assert.Single(repo.ProcessedContents);
        var insights = JsonSerializer.Deserialize<List<string>>(saved.InsightsPtBR);
        var posts = JsonSerializer.Deserialize<List<string>>(saved.PostSuggestionsEnUS);
        Assert.Equal(["Insight A PT", "Insight B PT", "Insight C PT"], insights);
        Assert.Equal(["Post 1 EN", "Post 2 EN", "Post 3 EN"], posts);
    }

    [Fact]
    public async Task ExecuteAsync_ConsolidatedText_UsesPortugueseHeadingsAndStructure()
    {
        var repo = new InMemoryContentRepository();
        var article = Fakes.Article();
        repo.Articles.Add(article);
        var useCase = Build(repo, out _, out _);

        var result = await useCase.ExecuteAsync(article.Id);
        var text = result.PtBR.ConsolidatedText;

        Assert.Contains("[Resumo Técnico]", text);
        Assert.Contains("Principais insights:", text);
        Assert.Contains("• Insight A PT", text);
        Assert.Contains("Explicação rápida:", text);
        Assert.Contains("Sugestões de post:", text);
        Assert.Contains("1)", text);
        Assert.Contains("Post 1 PT", text);
        // Heading order is preserved.
        Assert.True(text.IndexOf("[Resumo Técnico]") < text.IndexOf("Principais insights:"));
        Assert.True(text.IndexOf("Principais insights:") < text.IndexOf("Sugestões de post:"));
    }

    [Fact]
    public async Task ExecuteAsync_ConsolidatedText_UsesEnglishHeadings()
    {
        var repo = new InMemoryContentRepository();
        var article = Fakes.Article();
        repo.Articles.Add(article);
        var useCase = Build(repo, out _, out _);

        var result = await useCase.ExecuteAsync(article.Id);
        var text = result.EnUS.ConsolidatedText;

        Assert.Contains("[Technical Summary]", text);
        Assert.Contains("Key insights:", text);
        Assert.Contains("Quick explanation:", text);
        Assert.Contains("Post suggestions:", text);
        Assert.Contains("• Insight A EN", text);
    }
}
