namespace ContentGen.Application.Interfaces;

public interface IContentExtractor
{
    Task<string> ExtractTextAsync(string url, CancellationToken cancellationToken = default);
}
