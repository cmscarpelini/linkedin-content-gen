using System.Text.Json;
using ContentGen.Application.Exceptions;
using ContentGen.Application.Interfaces;
using ContentGen.Domain.Entities;

namespace ContentGen.Application.UseCases;

/// <summary>
/// Marks or unmarks a single generated post (article + language + index) as published on LinkedIn.
/// Idempotent: publishing an already-published post or unpublishing a non-published one is a no-op.
/// </summary>
public class SetPostPublishedUseCase(IContentRepository repository)
{
    private static readonly HashSet<string> SupportedLanguages = new(StringComparer.OrdinalIgnoreCase) { "pt-BR", "en-US" };

    public async Task ExecuteAsync(Guid articleId, string language, int postIndex, bool published, CancellationToken ct = default)
    {
        if (!SupportedLanguages.Contains(language))
            throw new ValidationException($"Language '{language}' is not supported. Use 'pt-BR' or 'en-US'.");

        // Canonical casing for storage, regardless of how the caller cased the route value.
        var canonicalLanguage = SupportedLanguages.First(l => l.Equals(language, StringComparison.OrdinalIgnoreCase));

        var content = await repository.GetProcessedContentByArticleIdAsync(articleId, ct)
            ?? throw new NotFoundException($"No generated content found for article {articleId}.");

        var postsJson = canonicalLanguage == "pt-BR" ? content.PostSuggestionsPtBR : content.PostSuggestionsEnUS;
        var postCount = (JsonSerializer.Deserialize<List<string>>(postsJson) ?? []).Count;
        if (postIndex < 0 || postIndex >= postCount)
            throw new ValidationException($"Post index {postIndex} is out of range (valid: 0-{postCount - 1}).");

        var existing = await repository.GetPublicationAsync(articleId, canonicalLanguage, postIndex, ct);

        if (published && existing is null)
        {
            await repository.AddPublicationAsync(new PostPublication
            {
                Id = Guid.NewGuid(),
                ArticleId = articleId,
                Language = canonicalLanguage,
                PostIndex = postIndex,
                PublishedAt = DateTime.UtcNow
            }, ct);
        }
        else if (!published && existing is not null)
        {
            await repository.RemovePublicationAsync(existing, ct);
        }
    }
}
