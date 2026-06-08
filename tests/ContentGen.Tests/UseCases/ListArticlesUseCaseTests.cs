using ContentGen.Application.UseCases;
using ContentGen.Tests.TestDoubles;

namespace ContentGen.Tests.UseCases;

public class ListArticlesUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_FlagsArticlesThatAlreadyHaveContent()
    {
        var repo = new InMemoryContentRepository();
        var withContent = Fakes.Article(url: "https://example.com/has");
        var withoutContent = Fakes.Article(url: "https://example.com/none");
        repo.Articles.Add(withContent);
        repo.Articles.Add(withoutContent);
        repo.ProcessedContents.Add(Fakes.Processed(withContent.Id, withContent));

        var useCase = new ListArticlesUseCase(repo);

        var result = (await useCase.ExecuteAsync()).ToList();

        Assert.True(result.Single(a => a.Id == withContent.Id).HasContent);
        Assert.False(result.Single(a => a.Id == withoutContent.Id).HasContent);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoArticles_ReturnsEmpty()
    {
        var useCase = new ListArticlesUseCase(new InMemoryContentRepository());

        Assert.Empty(await useCase.ExecuteAsync());
    }
}
