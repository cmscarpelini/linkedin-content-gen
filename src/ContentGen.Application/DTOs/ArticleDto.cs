namespace ContentGen.Application.DTOs;

public record ArticleDto(
    Guid Id,
    string Title,
    string Url,
    string Source,
    DateTime PublishedAt
);
