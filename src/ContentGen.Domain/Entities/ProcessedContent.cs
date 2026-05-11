namespace ContentGen.Domain.Entities;

public class ProcessedContent
{
    public Guid Id { get; set; }
    public Guid ArticleId { get; set; }

    // PT-BR
    public string TechnicalSummaryPtBR { get; set; } = string.Empty;
    public string InsightsPtBR { get; set; } = string.Empty;         // JSON: string[]
    public string CasualExplanationPtBR { get; set; } = string.Empty;
    public string PostSuggestionsPtBR { get; set; } = string.Empty;  // JSON: string[]

    // EN-US
    public string TechnicalSummaryEnUS { get; set; } = string.Empty;
    public string InsightsEnUS { get; set; } = string.Empty;         // JSON: string[]
    public string CasualExplanationEnUS { get; set; } = string.Empty;
    public string PostSuggestionsEnUS { get; set; } = string.Empty;  // JSON: string[]

    public DateTime CreatedAt { get; set; }

    // Navigation
    public ArticleRawContent Article { get; set; } = null!;
}
