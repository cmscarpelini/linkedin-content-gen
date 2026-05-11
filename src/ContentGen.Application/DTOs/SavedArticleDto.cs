namespace ContentGen.Application.DTOs;

public record SavedArticleDto(
    Guid Id,
    string Title,
    string Url,
    string Source,
    DateTime PublishedAt,
    bool HasContent
);
