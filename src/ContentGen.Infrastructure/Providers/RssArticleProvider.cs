using ContentGen.Application.Interfaces;
using ContentGen.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.ServiceModel.Syndication;
using System.Xml;

namespace ContentGen.Infrastructure.Providers;

public class RssArticleProvider : IArticleProvider
{
    private readonly string[] _sources;
    private readonly int _maxArticles;
    private readonly ILogger<RssArticleProvider> _logger;

    public RssArticleProvider(IConfiguration configuration, ILogger<RssArticleProvider> logger)
    {
        _sources = configuration.GetSection("RssSources").Get<string[]>() ?? [];
        _maxArticles = configuration.GetValue<int>("RssMaxArticles", 5);
        _logger = logger;
    }

    public async Task<IEnumerable<ArticleRawContent>> FetchArticlesAsync(CancellationToken cancellationToken = default)
    {
        var articles = new List<ArticleRawContent>();

        foreach (var source in _sources)
        {
            try
            {
                var items = await FetchFromRssAsync(source, cancellationToken);
                articles.AddRange(items);
            }
            catch (Exception ex)
            {
                // Fonte indisponível — registra e continua com as demais
                _logger.LogWarning(ex, "Failed to fetch RSS feed from {FeedUrl}; skipping this source", source);
            }
        }

        return articles
            .OrderByDescending(a => a.PublishedAt)
            .Take(_maxArticles);
    }

    private static Task<IEnumerable<ArticleRawContent>> FetchFromRssAsync(string feedUrl, CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            using var reader = XmlReader.Create(feedUrl);
            var feed = SyndicationFeed.Load(reader);

            return feed.Items.Select(item => new ArticleRawContent
            {
                Id = Guid.NewGuid(),
                Title = item.Title.Text,
                Url = item.Links.FirstOrDefault()?.Uri.ToString() ?? string.Empty,
                Source = feedUrl,
                PublishedAt = item.PublishDate.UtcDateTime
            });
        }, cancellationToken);
    }
}
