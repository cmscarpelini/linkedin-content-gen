using ContentGen.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContentGen.Infrastructure.Persistence;

public class ContentDbContext : DbContext
{
    public ContentDbContext(DbContextOptions<ContentDbContext> options) : base(options) { }

    public DbSet<ArticleRawContent> Articles => Set<ArticleRawContent>();
    public DbSet<ProcessedContent> ProcessedContents => Set<ProcessedContent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ArticleRawContent>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.RawHtml).HasColumnType("TEXT");
        });

        modelBuilder.Entity<ProcessedContent>(e =>
        {
            e.HasKey(x => x.Id);

            e.HasOne(x => x.Article)
             .WithOne()
             .HasForeignKey<ProcessedContent>(x => x.ArticleId);

            e.Property(x => x.InsightsPtBR).HasColumnType("TEXT");
            e.Property(x => x.PostSuggestionsPtBR).HasColumnType("TEXT");
            e.Property(x => x.InsightsEnUS).HasColumnType("TEXT");
            e.Property(x => x.PostSuggestionsEnUS).HasColumnType("TEXT");
        });
    }
}
