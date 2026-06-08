using System.Net.Http.Json;
using ContentGen.Domain.Entities;
using ContentGen.Infrastructure.Persistence;
using ContentGen.Tests.TestDoubles;
using Microsoft.Extensions.DependencyInjection;

namespace ContentGen.Tests.Integration;

public class ContentCountTests
{
    private record CountResponse(int Count);

    [Fact]
    public async Task GetContentCount_ReturnsNumberOfGeneratedContents()
    {
        using var factory = new ContentGenApiFactory();
        var article = Fakes.Article();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ContentDbContext>();
            db.Articles.Add(article);
            await db.SaveChangesAsync();
        }
        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/content/generate", new { articleId = article.Id });

        var result = await client.GetFromJsonAsync<CountResponse>("/content/count");

        Assert.Equal(1, result!.Count);
    }
}
