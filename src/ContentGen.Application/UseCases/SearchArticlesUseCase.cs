using ContentGen.Application.DTOs;
using ContentGen.Application.Interfaces;
using ContentGen.Domain.Entities;

namespace ContentGen.Application.UseCases;

public class SearchArticlesUseCase
{
    private readonly IArticleProvider _provider;
    private readonly IContentRepository _repository;

    public SearchArticlesUseCase(IArticleProvider provider, IContentRepository repository)
    {
        _provider = provider;
        _repository = repository;
    }

    public async Task<IEnumerable<ArticleDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var articles = await _provider.FetchArticlesAsync(cancellationToken);

        var result = new List<ArticleDto>();
        foreach (var article in articles)
        {
            var existing = await _repository.GetArticleByUrlAsync(article.Url, cancellationToken);
            if (existing is null)
            {
                await _repository.SaveArticleAsync(article, cancellationToken);
                existing = article;
            }
            result.Add(new ArticleDto(existing.Id, existing.Title, existing.Url, existing.Source, existing.PublishedAt));
        }

        return result;
    }
}
