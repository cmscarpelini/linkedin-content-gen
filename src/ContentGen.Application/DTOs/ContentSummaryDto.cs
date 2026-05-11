namespace ContentGen.Application.DTOs;

public record ContentSummaryDto(
    Guid ArticleId,
    string ArticleTitle,
    string ArticleUrl,
    DateTime CreatedAt
);
