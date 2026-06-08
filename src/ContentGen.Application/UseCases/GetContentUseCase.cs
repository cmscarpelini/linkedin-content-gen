using ContentGen.Application.DTOs;
using ContentGen.Application.Interfaces;

namespace ContentGen.Application.UseCases;

public class GetContentUseCase(IContentRepository repository)
{
    public async Task<ReviewPackageDto?> ExecuteAsync(Guid articleId, CancellationToken ct = default)
    {
        var content = await repository.GetProcessedContentByArticleIdAsync(articleId, ct);
        if (content is null) return null;
        var article = await repository.GetArticleByIdAsync(articleId, ct);
        if (article is null) return null;
        var publications = await repository.GetPublicationsByArticleIdAsync(articleId, ct);
        return GenerateContentUseCase.BuildReviewPackage(article, content, publications);
    }
}
