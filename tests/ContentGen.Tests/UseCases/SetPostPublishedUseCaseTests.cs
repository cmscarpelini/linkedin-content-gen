using ContentGen.Application.Exceptions;
using ContentGen.Application.UseCases;
using ContentGen.Tests.TestDoubles;

namespace ContentGen.Tests.UseCases;

public class SetPostPublishedUseCaseTests
{
    private static (SetPostPublishedUseCase useCase, InMemoryContentRepository repo, Guid articleId) Setup()
    {
        var repo = new InMemoryContentRepository();
        var article = Fakes.Article();
        repo.Articles.Add(article);
        repo.ProcessedContents.Add(Fakes.Processed(article.Id, article));
        return (new SetPostPublishedUseCase(repo), repo, article.Id);
    }

    [Fact]
    public async Task ExecuteAsync_UnsupportedLanguage_ThrowsValidation()
    {
        var (useCase, _, articleId) = Setup();

        await Assert.ThrowsAsync<ValidationException>(
            () => useCase.ExecuteAsync(articleId, "fr-FR", 0, true));
    }

    [Fact]
    public async Task ExecuteAsync_IndexOutOfRange_ThrowsValidation()
    {
        var (useCase, _, articleId) = Setup();

        await Assert.ThrowsAsync<ValidationException>(
            () => useCase.ExecuteAsync(articleId, "pt-BR", 3, true)); // only 0-2 exist
    }

    [Fact]
    public async Task ExecuteAsync_NoContentForArticle_ThrowsNotFound()
    {
        var repo = new InMemoryContentRepository();
        var useCase = new SetPostPublishedUseCase(repo);

        await Assert.ThrowsAsync<NotFoundException>(
            () => useCase.ExecuteAsync(Guid.NewGuid(), "pt-BR", 0, true));
    }

    [Fact]
    public async Task ExecuteAsync_Publish_AddsExactlyOnePublication_AndIsIdempotent()
    {
        var (useCase, repo, articleId) = Setup();

        await useCase.ExecuteAsync(articleId, "pt-BR", 0, true);
        await useCase.ExecuteAsync(articleId, "pt-BR", 0, true); // repeat

        var publication = Assert.Single(repo.Publications);
        Assert.Equal("pt-BR", publication.Language);
        Assert.Equal(0, publication.PostIndex);
        Assert.Equal(articleId, publication.ArticleId);
    }

    [Fact]
    public async Task ExecuteAsync_Unpublish_RemovesPublication_AndIsIdempotent()
    {
        var (useCase, repo, articleId) = Setup();
        await useCase.ExecuteAsync(articleId, "en-US", 2, true);

        await useCase.ExecuteAsync(articleId, "en-US", 2, false);
        await useCase.ExecuteAsync(articleId, "en-US", 2, false); // repeat

        Assert.Empty(repo.Publications);
    }

    [Fact]
    public async Task ExecuteAsync_NormalizesLanguageCasingForStorage()
    {
        var (useCase, repo, articleId) = Setup();

        await useCase.ExecuteAsync(articleId, "PT-br", 1, true);

        Assert.Equal("pt-BR", Assert.Single(repo.Publications).Language);
    }
}
