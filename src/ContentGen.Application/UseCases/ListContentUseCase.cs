using ContentGen.Application.DTOs;
using ContentGen.Application.Interfaces;

namespace ContentGen.Application.UseCases;

public class ListContentUseCase(IContentRepository repository)
{
    public async Task<List<ContentSummaryDto>> ExecuteAsync(CancellationToken ct = default)
    {
        var items = await repository.GetAllProcessedContentAsync(ct);
        return items.Select(p => new ContentSummaryDto(
            p.ArticleId,
            p.Article.Title,
            p.Article.Url,
            p.CreatedAt
        )).ToList();
    }
}
