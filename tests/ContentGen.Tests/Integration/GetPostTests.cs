using System.Net;
using System.Net.Http.Json;
using ContentGen.Application.DTOs;
using ContentGen.Infrastructure.Persistence;
using ContentGen.Tests.TestDoubles;
using Microsoft.Extensions.DependencyInjection;

namespace ContentGen.Tests.Integration;

public class GetPostTests
{
    [Fact]
    public async Task GetPost_ReturnsTheRequestedPost()
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

        var response = await client.GetAsync($"/content/{article.Id}/posts/pt-BR/0");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var post = await response.Content.ReadFromJsonAsync<PostDto>();
        Assert.NotNull(post);
        Assert.Equal(0, post.Index);
        Assert.Equal("Post 1 PT", post.Text);
    }
}
