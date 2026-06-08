using ContentGen.Application.UseCases;
using ContentGen.Tests.TestDoubles;

namespace ContentGen.Tests.UseCases;

public class SearchArticlesUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenArticlesAreNew_PersistsAllAndReturnsThem()
    {
        var repo = new InMemoryContentRepository();
        var fetched = new[]
        {
            Fakes.Article(url: "https://example.com/a"),
            Fakes.Article(url: "https://example.com/b")
        };
        var useCase = new SearchArticlesUseCase(new StubArticleProvider(fetched), repo);

        var result = (await useCase.ExecuteAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(2, repo.SaveArticleCalls);
        Assert.Equal(2, repo.Articles.Count);
    }

    [Fact]
    public async Task ExecuteAsync_WhenArticleUrlAlreadyKnown_ReusesItWithoutSaving()
    {
        var repo = new InMemoryContentRepository();
        var existing = Fakes.Article(url: "https://example.com/known");
        repo.Articles.Add(existing);

        // Same URL but a fresh GUID, as the RSS provider produces on every fetch.
        var fetched = new[] { Fakes.Article(url: "https://example.com/known") };
        var useCase = new SearchArticlesUseCase(new StubArticleProvider(fetched), repo);

        var result = (await useCase.ExecuteAsync()).ToList();

        Assert.Equal(0, repo.SaveArticleCalls);
        Assert.Single(repo.Articles);
        // The returned id is the persisted one — reuse keys on URL, not on the incoming GUID.
        Assert.Equal(existing.Id, Assert.Single(result).Id);
    }

    [Fact]
    public async Task ExecuteAsync_MapsArticleFieldsToDto()
    {
        var repo = new InMemoryContentRepository();
        var article = Fakes.Article(title: "Async streams in .NET", url: "https://example.com/x");
        var useCase = new SearchArticlesUseCase(new StubArticleProvider([article]), repo);

        var dto = Assert.Single(await useCase.ExecuteAsync());

        Assert.Equal(article.Title, dto.Title);
        Assert.Equal(article.Url, dto.Url);
        Assert.Equal(article.Source, dto.Source);
        Assert.Equal(article.PublishedAt, dto.PublishedAt);
    }
}
