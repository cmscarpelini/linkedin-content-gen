using ContentGen.Application.DTOs;

namespace ContentGen.Application.Interfaces;

public interface IAiContentService
{
    Task<AiContentResponse> GenerateContentAsync(string articleTitle, string articleUrl, string articleRawText, CancellationToken cancellationToken = default);
}
