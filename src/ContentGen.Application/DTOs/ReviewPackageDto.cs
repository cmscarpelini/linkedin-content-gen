namespace ContentGen.Application.DTOs;

public record ContentBlockDto(
    string TechnicalSummary,
    List<string> Insights,
    string CasualExplanation,
    List<string> PostSuggestions,
    string ConsolidatedText
);

public record ReviewPackageDto(
    Guid ArticleId,
    string ArticleTitle,
    string ArticleUrl,
    ContentBlockDto PtBR,
    ContentBlockDto EnUS
);
