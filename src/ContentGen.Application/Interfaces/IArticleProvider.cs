using ContentGen.Domain.Entities;

namespace ContentGen.Application.Interfaces;

public interface IArticleProvider
{
    Task<IEnumerable<ArticleRawContent>> FetchArticlesAsync(CancellationToken cancellationToken = default);
}
