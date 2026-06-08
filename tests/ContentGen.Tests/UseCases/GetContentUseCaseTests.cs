using ContentGen.Application.UseCases;
using ContentGen.Domain.Entities;
using ContentGen.Tests.TestDoubles;

namespace ContentGen.Tests.UseCases;

public class GetContentUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenNoContentForArticle_ReturnsNull()
    {
        var repo = new InMemoryContentRepository();
        var result = await new GetContentUseCase(repo).ExecuteAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task ExecuteAsync_WhenContentExistsButArticleMissing_ReturnsNull()
    {
        var repo = new InMemoryContentRepository();
        var articleId = Guid.NewGuid();
        repo.ProcessedContents.Add(Fakes.Processed(articleId)); // no matching article persisted

        var result = await new GetContentUseCase(repo).ExecuteAsync(articleId);

        Assert.Null(result);
    }

    [Fact]
    public async Task ExecuteAsync_WhenContentExists_ReturnsReviewPackageWithDeserializedLists()
    {
        var repo = new InMemoryContentRepository();
        var article = Fakes.Article();
        repo.Articles.Add(article);
        repo.ProcessedContents.Add(Fakes.Processed(article.Id, article));

        var result = await new GetContentUseCase(repo).ExecuteAsync(article.Id);

        Assert.NotNull(result);
        Assert.Equal(article.Id, result!.ArticleId);
        Assert.Equal(["iA-PT", "iB-PT", "iC-PT"], result.PtBR.Insights);
        Assert.Equal(["pA-EN", "pB-EN", "pC-EN"], result.EnUS.PostSuggestions.Select(p => p.Text));
        Assert.Equal("Summary PT", result.PtBR.TechnicalSummary);
        // Posts carry their position and default to not published.
        Assert.Equal([0, 1, 2], result.EnUS.PostSuggestions.Select(p => p.Index));
        Assert.All(result.PtBR.PostSuggestions, p => Assert.False(p.Published));
        Assert.All(result.EnUS.PostSuggestions, p => Assert.False(p.Published));
    }

    [Fact]
    public async Task ExecuteAsync_ReflectsPublishedStateForTheRightLanguageAndIndex()
    {
        var repo = new InMemoryContentRepository();
        var article = Fakes.Article();
        repo.Articles.Add(article);
        repo.ProcessedContents.Add(Fakes.Processed(article.Id, article));
        repo.Publications.Add(new PostPublication
        {
            Id = Guid.NewGuid(),
            ArticleId = article.Id,
            Language = "pt-BR",
            PostIndex = 1,
            PublishedAt = DateTime.UtcNow
        });

        var result = await new GetContentUseCase(repo).ExecuteAsync(article.Id);

        Assert.True(result!.PtBR.PostSuggestions[1].Published);
        Assert.False(result.PtBR.PostSuggestions[0].Published);
        Assert.False(result.PtBR.PostSuggestions[2].Published);
        // Same index in the other language is unaffected.
        Assert.All(result.EnUS.PostSuggestions, p => Assert.False(p.Published));
    }
}
