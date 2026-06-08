namespace ContentGen.Domain.Entities;

public class ArticleRawContent
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
}
