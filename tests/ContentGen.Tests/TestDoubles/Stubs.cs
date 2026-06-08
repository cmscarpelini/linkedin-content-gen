using ContentGen.Application.DTOs;
using ContentGen.Application.Interfaces;
using ContentGen.Domain.Entities;

namespace ContentGen.Tests.TestDoubles;

/// <summary>Article provider stub that returns a fixed list and records how often it was invoked.</summary>
public class StubArticleProvider(IEnumerable<ArticleRawContent> articles) : IArticleProvider
{
    public int CallCount { get; private set; }

    public Task<IEnumerable<ArticleRawContent>> FetchArticlesAsync(CancellationToken cancellationToken = default)
    {
        CallCount++;
        return Task.FromResult(articles);
    }
}

/// <summary>Content extractor stub that returns fixed text and records how often it was invoked.</summary>
public class StubContentExtractor(string text = "extracted article text") : IContentExtractor
{
    public int CallCount { get; private set; }

    public Task<string> ExtractTextAsync(string url, CancellationToken cancellationToken = default)
    {
        CallCount++;
        return Task.FromResult(text);
    }
}

/// <summary>AI service stub that returns a fixed response and records how often it was invoked.</summary>
public class StubAiContentService(AiContentResponse response) : IAiContentService
{
    public int CallCount { get; private set; }

    public Task<AiContentResponse> GenerateContentAsync(string articleTitle, string articleUrl, string articleRawText, CancellationToken cancellationToken = default)
    {
        CallCount++;
        return Task.FromResult(response);
    }
}
