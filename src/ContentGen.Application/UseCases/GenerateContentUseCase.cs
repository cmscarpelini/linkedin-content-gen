using System.Text;
using ContentGen.Application.DTOs;
using ContentGen.Application.Interfaces;
using ContentGen.Domain.Entities;
using System.Text.Json;

namespace ContentGen.Application.UseCases;

public class GenerateContentUseCase
{
    private readonly IContentRepository _repository;
    private readonly IContentExtractor _extractor;
    private readonly IAiContentService _aiService;

    public GenerateContentUseCase(
        IContentRepository repository,
        IContentExtractor extractor,
        IAiContentService aiService)
    {
        _repository = repository;
        _extractor = extractor;
        _aiService = aiService;
    }

    public async Task<ReviewPackageDto> ExecuteAsync(Guid articleId, CancellationToken cancellationToken = default)
    {
        var article = await _repository.GetArticleByIdAsync(articleId, cancellationToken)
            ?? throw new InvalidOperationException($"Article {articleId} not found.");

        // Return already-generated content without calling the AI again
        var existing = await _repository.GetProcessedContentByArticleIdAsync(articleId, cancellationToken);
        if (existing != null)
            return BuildReviewPackage(article, existing);

        var rawText = await _extractor.ExtractTextAsync(article.Url, cancellationToken);

        var aiResponse = await _aiService.GenerateContentAsync(article.Title, article.Url, rawText, cancellationToken);

        var processed = new ProcessedContent
        {
            Id = Guid.NewGuid(),
            ArticleId = article.Id,
            TechnicalSummaryPtBR = aiResponse.PtBR.TechnicalSummary,
            InsightsPtBR = JsonSerializer.Serialize(aiResponse.PtBR.Insights),
            CasualExplanationPtBR = aiResponse.PtBR.CasualExplanation,
            PostSuggestionsPtBR = JsonSerializer.Serialize(aiResponse.PtBR.PostSuggestions),
            TechnicalSummaryEnUS = aiResponse.EnUS.TechnicalSummary,
            InsightsEnUS = JsonSerializer.Serialize(aiResponse.EnUS.Insights),
            CasualExplanationEnUS = aiResponse.EnUS.CasualExplanation,
            PostSuggestionsEnUS = JsonSerializer.Serialize(aiResponse.EnUS.PostSuggestions),
            CreatedAt = DateTime.UtcNow
        };

        await _repository.SaveProcessedContentAsync(processed, cancellationToken);

        return new ReviewPackageDto(
            article.Id,
            article.Title,
            article.Url,
            BuildContentBlock(aiResponse.PtBR, "pt-BR"),
            BuildContentBlock(aiResponse.EnUS, "en-US")
        );
    }

    internal static ReviewPackageDto BuildReviewPackage(ArticleRawContent article, ProcessedContent p)
    {
        var ptBRBlock = new AiContentBlock(
            p.TechnicalSummaryPtBR,
            JsonSerializer.Deserialize<List<string>>(p.InsightsPtBR) ?? [],
            p.CasualExplanationPtBR,
            JsonSerializer.Deserialize<List<string>>(p.PostSuggestionsPtBR) ?? []);
        var enUSBlock = new AiContentBlock(
            p.TechnicalSummaryEnUS,
            JsonSerializer.Deserialize<List<string>>(p.InsightsEnUS) ?? [],
            p.CasualExplanationEnUS,
            JsonSerializer.Deserialize<List<string>>(p.PostSuggestionsEnUS) ?? []);
        return new ReviewPackageDto(
            article.Id, article.Title, article.Url,
            BuildContentBlock(ptBRBlock, "pt-BR"),
            BuildContentBlock(enUSBlock, "en-US"));
    }

    internal static ContentBlockDto BuildContentBlock(AiContentBlock block, string language)
    {
        var sb = new StringBuilder();
        bool isPtBR = language == "pt-BR";

        sb.AppendLine(isPtBR ? "[Resumo Técnico]" : "[Technical Summary]");
        sb.AppendLine(block.TechnicalSummary);
        sb.AppendLine();
        sb.AppendLine(isPtBR ? "Principais insights:" : "Key insights:");
        foreach (var insight in block.Insights)
            sb.AppendLine($"• {insight}");
        sb.AppendLine();
        sb.AppendLine(isPtBR ? "Explicação rápida:" : "Quick explanation:");
        sb.AppendLine(block.CasualExplanation);
        sb.AppendLine();
        sb.AppendLine(isPtBR ? "Sugestões de post:" : "Post suggestions:");
        for (int i = 0; i < block.PostSuggestions.Count; i++)
        {
            sb.AppendLine();
            sb.AppendLine($"{i + 1})");
            sb.AppendLine(block.PostSuggestions[i]);
        }

        return new ContentBlockDto(
            block.TechnicalSummary,
            block.Insights,
            block.CasualExplanation,
            block.PostSuggestions,
            sb.ToString()
        );
    }
}
