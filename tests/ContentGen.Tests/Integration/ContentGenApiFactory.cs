using ContentGen.Application.Interfaces;
using ContentGen.Domain.Entities;
using ContentGen.Tests.TestDoubles;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ContentGen.Tests.Integration;

/// <summary>
/// Boots the real API (real EF Core + SQLite + repository + use cases) but replaces the three
/// external boundaries — RSS, HTML extraction and the AI service — with deterministic stubs.
/// Each factory instance owns an isolated temp SQLite file, deleted on dispose.
/// </summary>
public class ContentGenApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"contentgen-test-{Guid.NewGuid():N}.db");

    public StubAiContentService Ai { get; } = new(Fakes.AiResponse());
    public StubContentExtractor Extractor { get; } = new();
    public List<ArticleRawContent> RssArticles { get; } = [];

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={_dbPath}",
                ["OpenAI:ApiKey"] = "test-key"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IAiContentService>();
            services.AddScoped<IAiContentService>(_ => Ai);

            services.RemoveAll<IContentExtractor>();
            services.AddScoped<IContentExtractor>(_ => Extractor);

            services.RemoveAll<IArticleProvider>();
            services.AddScoped<IArticleProvider>(_ => new StubArticleProvider(RssArticles));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && File.Exists(_dbPath))
        {
            try { File.Delete(_dbPath); } catch { /* best-effort cleanup */ }
        }
    }
}
