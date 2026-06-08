using System.Net;

namespace ContentGen.Tests.Integration;

public class HealthEndpointTests
{
    [Fact]
    public async Task GetHealth_ReturnsOkAndHealthyStatus()
    {
        using var factory = new ContentGenApiFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal("Healthy", body);
    }
}
