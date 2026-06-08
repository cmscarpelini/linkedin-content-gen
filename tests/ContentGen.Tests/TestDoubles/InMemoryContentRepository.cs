using ContentGen.Application.Interfaces;
using ContentGen.Domain.Entities;

namespace ContentGen.Tests.TestDoubles;

/// <summary>
/// Hand-rolled in-memory <see cref="IContentRepository"/> that mirrors the behaviour of the
/// EF Core implementation (ordering, navigation include, content flag) so the use cases can be
/// exercised without a database. Exposes call counters to assert side effects.
/// </summary>
public class InMemoryContentRepository : IContentRepository
{
    public List<ArticleRawContent> Articles { get; } = [];
    public List<ProcessedContent> ProcessedContents { get; } = [];
    public List<PostPublication> Publications { get; } = [];

    public int SaveArticleCalls { get; private set; }
    public int SaveProcessedContentCalls { get; private set; }

    public Task SaveArticleAsync(ArticleRawContent article, CancellationToken cancellationToken = default)
    {
        SaveArticleCalls++;
        Articles.Add(article);
        return Task.CompletedTask;
    }

    public Task<ArticleRawContent?> GetArticleByIdAsync(Guid articleId, CancellationToken cancellationToken = default)
        => Task.FromResult(Articles.FirstOrDefault(a => a.Id == articleId));

    public Task<ArticleRawContent?> GetArticleByUrlAsync(string url, CancellationToken cancellationToken = default)
        => Task.FromResult(Articles.FirstOrDefault(a => a.Url == url));

    public Task<bool> ArticleExistsAsync(string url, CancellationToken cancellationToken = default)
        => Task.FromResult(Articles.Any(a => a.Url == url));

    public Task SaveProcessedContentAsync(ProcessedContent content, CancellationToken cancellationToken = default)
    {
        SaveProcessedContentCalls++;
        ProcessedContents.Add(content);
        return Task.CompletedTask;
    }

    public Task<ProcessedContent?> GetProcessedContentByArticleIdAsync(Guid articleId, CancellationToken cancellationToken = default)
        => Task.FromResult(ProcessedContents.FirstOrDefault(p => p.ArticleId == articleId));

    public Task<List<ProcessedContent>> GetAllProcessedContentAsync(CancellationToken cancellationToken = default)
    {
        // Mirror EF Core's Include(p => p.Article) by resolving the navigation property.
        foreach (var p in ProcessedContents)
            p.Article ??= Articles.FirstOrDefault(a => a.Id == p.ArticleId)!;

        return Task.FromResult(ProcessedContents.OrderByDescending(p => p.CreatedAt).ToList());
    }

    public Task<List<(ArticleRawContent Article, bool HasContent)>> GetAllArticlesAsync(CancellationToken cancellationToken = default)
    {
        var idsWithContent = ProcessedContents.Select(p => p.ArticleId).ToHashSet();
        var result = Articles
            .OrderByDescending(a => a.PublishedAt)
            .Select(a => (a, idsWithContent.Contains(a.Id)))
            .ToList();
        return Task.FromResult(result);
    }

    public Task<List<PostPublication>> GetPublicationsByArticleIdAsync(Guid articleId, CancellationToken cancellationToken = default)
        => Task.FromResult(Publications.Where(p => p.ArticleId == articleId).ToList());

    public Task<PostPublication?> GetPublicationAsync(Guid articleId, string language, int postIndex, CancellationToken cancellationToken = default)
        => Task.FromResult(Publications.FirstOrDefault(p => p.ArticleId == articleId && p.Language == language && p.PostIndex == postIndex));

    public Task AddPublicationAsync(PostPublication publication, CancellationToken cancellationToken = default)
    {
        Publications.Add(publication);
        return Task.CompletedTask;
    }

    public Task RemovePublicationAsync(PostPublication publication, CancellationToken cancellationToken = default)
    {
        Publications.Remove(publication);
        return Task.CompletedTask;
    }
}
