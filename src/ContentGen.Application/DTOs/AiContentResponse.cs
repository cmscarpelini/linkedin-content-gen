namespace ContentGen.Application.DTOs;

public record AiContentBlock(
    string TechnicalSummary,
    List<string> Insights,
    string CasualExplanation,
    List<string> PostSuggestions
);

public record AiContentResponse(
    AiContentBlock PtBR,
    AiContentBlock EnUS
);
