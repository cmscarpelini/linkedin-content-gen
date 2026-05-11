using ContentGen.Application.DTOs;
using ContentGen.Application.Interfaces;

namespace ContentGen.Application.UseCases;

public class ListArticlesUseCase
{
    private readonly IContentRepository _repository;

    public ListArticlesUseCase(IContentRepository repository) => _repository = repository;

    public async Task<IEnumerable<SavedArticleDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var articles = await _repository.GetAllArticlesAsync(cancellationToken);
        return articles.Select(t => new SavedArticleDto(
            t.Article.Id,
            t.Article.Title,
            t.Article.Url,
            t.Article.Source,
            t.Article.PublishedAt,
            t.HasContent));
    }
}
