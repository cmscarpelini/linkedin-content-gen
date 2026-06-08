namespace ContentGen.Domain.Entities;

/// <summary>
/// Records that a single generated post (identified by article + language + position) was marked
/// as published on LinkedIn. The absence of a row means "not published"; toggling off deletes it.
/// </summary>
public class PostPublication
{
    public Guid Id { get; set; }
    public Guid ArticleId { get; set; }
    public string Language { get; set; } = string.Empty;  // "pt-BR" | "en-US"
    public int PostIndex { get; set; }                    // 0-based position within the language's posts
    public DateTime PublishedAt { get; set; }
}
