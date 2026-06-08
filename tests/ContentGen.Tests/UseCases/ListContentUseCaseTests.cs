using ContentGen.Application.UseCases;
using ContentGen.Tests.TestDoubles;

namespace ContentGen.Tests.UseCases;

public class ListContentUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsHistoryNewestFirstWithArticleMetadata()
    {
        var repo = new InMemoryContentRepository();

        var older = Fakes.Article(title: "Older", url: "https://example.com/older");
        var newer = Fakes.Article(title: "Newer", url: "https://example.com/newer");
        repo.Articles.Add(older);
        repo.Articles.Add(newer);

        var olderContent = Fakes.Processed(older.Id, older);
        olderContent.CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var newerContent = Fakes.Processed(newer.Id, newer);
        newerContent.CreatedAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        repo.ProcessedContents.Add(olderContent);
        repo.ProcessedContents.Add(newerContent);

        var result = await new ListContentUseCase(repo).ExecuteAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal(newer.Id, result[0].ArticleId);   // newest first
        Assert.Equal("Newer", result[0].ArticleTitle);
        Assert.Equal(newer.Url, result[0].ArticleUrl);
        Assert.Equal(older.Id, result[1].ArticleId);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNothingGenerated_ReturnsEmpty()
    {
        var result = await new ListContentUseCase(new InMemoryContentRepository()).ExecuteAsync();

        Assert.Empty(result);
    }
}
