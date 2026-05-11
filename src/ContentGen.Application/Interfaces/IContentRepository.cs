using ContentGen.Domain.Entities;

namespace ContentGen.Application.Interfaces;

public interface IContentRepository
{
    Task SaveArticleAsync(ArticleRawContent article, CancellationToken cancellationToken = default);
    Task<ArticleRawContent?> GetArticleByIdAsync(Guid articleId, CancellationToken cancellationToken = default);
    Task<ArticleRawContent?> GetArticleByUrlAsync(string url, CancellationToken cancellationToken = default);
    Task<bool> ArticleExistsAsync(string url, CancellationToken cancellationToken = default);
    Task SaveProcessedContentAsync(ProcessedContent content, CancellationToken cancellationToken = default);
    Task<ProcessedContent?> GetProcessedContentByArticleIdAsync(Guid articleId, CancellationToken cancellationToken = default);
    Task<List<ProcessedContent>> GetAllProcessedContentAsync(CancellationToken cancellationToken = default);
    Task<List<(ArticleRawContent Article, bool HasContent)>> GetAllArticlesAsync(CancellationToken cancellationToken = default);
}
