using ContentGen.Api;
using ContentGen.Application.DTOs;
using ContentGen.Application.Interfaces;
using ContentGen.Application.UseCases;
using ContentGen.Infrastructure.Persistence;
using ContentGen.Infrastructure.Providers;
using ContentGen.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Global error handling — maps exceptions to RFC 7807 ProblemDetails
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// CORS — permite o app React (porta 5173) chamar a API
builder.Services.AddCors(opt => opt.AddPolicy("WebApp", policy =>
    policy.WithOrigins("http://localhost:5173")
          .AllowAnyHeader()
          .AllowAnyMethod()));

// Persistence
builder.Services.AddDbContext<ContentDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Application
builder.Services.AddScoped<IContentRepository, ContentRepository>();
builder.Services.AddScoped<SearchArticlesUseCase>();
builder.Services.AddScoped<GenerateContentUseCase>();
builder.Services.AddScoped<ListContentUseCase>();
builder.Services.AddScoped<GetContentUseCase>();
builder.Services.AddScoped<ListArticlesUseCase>();
builder.Services.AddScoped<SetPostPublishedUseCase>();

// Infrastructure
builder.Services.AddScoped<IArticleProvider, RssArticleProvider>();
builder.Services.AddScoped<IPromptBuilder, PromptBuilder>();
builder.Services.AddScoped<IAiContentService, OpenAiContentService>();
builder.Services.AddHttpClient<IContentExtractor, HtmlContentExtractor>();

// OpenAPI
builder.Services.AddOpenApi();

// Health checks (liveness probe for containers / uptime monitors)
builder.Services.AddHealthChecks();

var app = builder.Build();

// Global exception handler — must run first so it wraps the whole pipeline
app.UseExceptionHandler();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ContentDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(opt => opt.Title = "ContentGen API");
}

app.UseCors("WebApp");

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

// GET /health — liveness check
app.MapHealthChecks("/health")
    .WithName("HealthCheck")
    .WithSummary("Liveness probe — retorna 200 quando a API está no ar.");

// GET /articles/search
app.MapGet("/articles/search", async (SearchArticlesUseCase useCase, CancellationToken ct) =>
{
    var articles = await useCase.ExecuteAsync(ct);
    return Results.Ok(articles);
})
.WithName("SearchArticles")
.WithSummary("Busca artigos técnicos nas fontes RSS configuradas e retorna até 5 resultados.");

// GET /articles
app.MapGet("/articles", async (ListArticlesUseCase useCase, CancellationToken ct) =>
    Results.Ok(await useCase.ExecuteAsync(ct)))
.WithName("ListArticles")
.WithSummary("Lista todos os artigos já salvos no banco de dados.");

// POST /content/generate
app.MapPost("/content/generate", async (GenerateContentRequest request, GenerateContentUseCase useCase, CancellationToken ct) =>
    Results.Ok(await useCase.ExecuteAsync(request.ArticleId, ct)))
.WithName("GenerateContent")
.WithSummary("Gera conteúdo técnico bilíngue (PT-BR e EN-US) para LinkedIn a partir de um artigo.");

// GET /content
app.MapGet("/content", async (ListContentUseCase useCase, CancellationToken ct) =>
    Results.Ok(await useCase.ExecuteAsync(ct)))
.WithName("ListContent")
.WithSummary("Lista todo o conteúdo já gerado.");

// GET /content/count
app.MapGet("/content/count", (ListContentUseCase useCase, CancellationToken ct) =>
{
    var count = useCase.ExecuteAsync(ct).Result.Count;
    return Results.Ok(new { count });
})
.WithName("CountContent")
.WithSummary("Retorna a quantidade total de conteúdos gerados.");

// GET /content/{articleId}
app.MapGet("/content/{articleId:guid}", async (Guid articleId, GetContentUseCase useCase, CancellationToken ct) =>
{
    var result = await useCase.ExecuteAsync(articleId, ct);
    return result is null ? Results.NotFound() : Results.Ok(result);
})
.WithName("GetContent")
.WithSummary("Retorna o conteúdo gerado para um artigo específico.");

// PUT /content/{articleId}/posts/{language}/{index}/published
app.MapPut("/content/{articleId:guid}/posts/{language}/{index:int}/published",
    async (Guid articleId, string language, int index, SetPostPublishedRequest request, SetPostPublishedUseCase useCase, CancellationToken ct) =>
{
    await useCase.ExecuteAsync(articleId, language, index, request.Published, ct);
    return Results.NoContent();
})
.WithName("SetPostPublished")
.WithSummary("Marca ou desmarca um post específico (idioma + índice) como publicado no LinkedIn.");

app.Run();

// Required for test project access
public partial class Program { }
