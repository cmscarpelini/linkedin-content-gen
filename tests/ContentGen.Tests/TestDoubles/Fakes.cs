using System.Text.Json;
using ContentGen.Application.DTOs;
using ContentGen.Domain.Entities;

namespace ContentGen.Tests.TestDoubles;

/// <summary>Factory helpers that build domain entities and AI responses with sensible defaults.</summary>
internal static class Fakes
{
    public static ArticleRawContent Article(
        Guid? id = null,
        string title = "Test Article",
        string url = "https://example.com/article",
        DateTime? publishedAt = null) => new()
        {
            Id = id ?? Guid.NewGuid(),
            Title = title,
            Url = url,
            Source = "https://example.com/feed",
            PublishedAt = publishedAt ?? new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

    public static AiContentBlock Block(string lang) => new(
        TechnicalSummary: $"Summary {lang}",
        Insights: [$"Insight A {lang}", $"Insight B {lang}", $"Insight C {lang}"],
        CasualExplanation: $"Casual {lang}",
        PostSuggestions: [$"Post 1 {lang}", $"Post 2 {lang}", $"Post 3 {lang}"]);

    public static AiContentResponse AiResponse() => new(Block("PT"), Block("EN"));

    public static ProcessedContent Processed(Guid articleId, ArticleRawContent? article = null) => new()
    {
        Id = Guid.NewGuid(),
        ArticleId = articleId,
        TechnicalSummaryPtBR = "Summary PT",
        InsightsPtBR = JsonSerializer.Serialize(new[] { "iA-PT", "iB-PT", "iC-PT" }),
        CasualExplanationPtBR = "Casual PT",
        PostSuggestionsPtBR = JsonSerializer.Serialize(new[] { "pA-PT", "pB-PT", "pC-PT" }),
        TechnicalSummaryEnUS = "Summary EN",
        InsightsEnUS = JsonSerializer.Serialize(new[] { "iA-EN", "iB-EN", "iC-EN" }),
        CasualExplanationEnUS = "Casual EN",
        PostSuggestionsEnUS = JsonSerializer.Serialize(new[] { "pA-EN", "pB-EN", "pC-EN" }),
        CreatedAt = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc),
        Article = article!
    };
}
