namespace ContentGen.Application.DTOs;

public record PostDto(
    int Index,
    string Text,
    bool Published
);

public record ContentBlockDto(
    string TechnicalSummary,
    List<string> Insights,
    string CasualExplanation,
    List<PostDto> PostSuggestions,
    string ConsolidatedText
);

public record ReviewPackageDto(
    Guid ArticleId,
    string ArticleTitle,
    string ArticleUrl,
    ContentBlockDto PtBR,
    ContentBlockDto EnUS
);
