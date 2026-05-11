using ContentGen.Application.Interfaces;
using ContentGen.Domain.Entities;
using ContentGen.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContentGen.Infrastructure.Persistence;

public class ContentRepository : IContentRepository
{
    private readonly ContentDbContext _db;

    public ContentRepository(ContentDbContext db) => _db = db;

    public async Task SaveArticleAsync(ArticleRawContent article, CancellationToken cancellationToken = default)
    {
        _db.Articles.Add(article);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<ArticleRawContent?> GetArticleByIdAsync(Guid articleId, CancellationToken cancellationToken = default)
        => await _db.Articles.FindAsync([articleId], cancellationToken);

    public async Task<ArticleRawContent?> GetArticleByUrlAsync(string url, CancellationToken cancellationToken = default)
        => await _db.Articles.FirstOrDefaultAsync(a => a.Url == url, cancellationToken);

    public async Task<bool> ArticleExistsAsync(string url, CancellationToken cancellationToken = default)
        => await _db.Articles.AnyAsync(a => a.Url == url, cancellationToken);

    public async Task SaveProcessedContentAsync(ProcessedContent content, CancellationToken cancellationToken = default)
    {
        _db.ProcessedContents.Add(content);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProcessedContent?> GetProcessedContentByArticleIdAsync(Guid articleId, CancellationToken cancellationToken = default)
        => await _db.ProcessedContents.FirstOrDefaultAsync(p => p.ArticleId == articleId, cancellationToken);

    public async Task<List<ProcessedContent>> GetAllProcessedContentAsync(CancellationToken cancellationToken = default)
        => await _db.ProcessedContents
            .Include(p => p.Article)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<List<(ArticleRawContent Article, bool HasContent)>> GetAllArticlesAsync(CancellationToken cancellationToken = default)
    {
        var articles = await _db.Articles
            .OrderByDescending(a => a.PublishedAt)
            .ToListAsync(cancellationToken);

        var articleIdsWithContent = await _db.ProcessedContents
            .Select(p => p.ArticleId)
            .ToHashSetAsync(cancellationToken);

        return articles
            .Select(a => (a, articleIdsWithContent.Contains(a.Id)))
            .ToList();
    }
}
