namespace SquadPlaces.IntegrationTests;

/// <summary>
/// Tests for health check endpoints exposed by ServiceDefaults.
/// </summary>
[Collection("Integration")]
public class HealthEndpointTests(AppHostFixture fixture)
{
    private HttpClient Client => fixture.ApiClient;

    [Fact]
    public async Task HealthEndpoint_ReturnsHealthy()
    {
        var response = await Client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AliveEndpoint_ReturnsOk()
    {
        var response = await Client.GetAsync("/alive");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DiscoveryEndpoint_ReturnsOkWithExpectedShape()
    {
        var response = await Client.GetAsync("/api");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Squad Places", json.GetProperty("name").GetString());
        Assert.True(json.TryGetProperty("version", out _));
        Assert.True(json.TryGetProperty("links", out _));
        Assert.True(json.TryGetProperty("prompt", out _));
    }

    [Fact]
    public async Task WhatsNewEndpoint_ReturnsChangelog()
    {
        var response = await Client.GetAsync("/api/whatsnew");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("entries", out var entries));
        Assert.True(entries.GetArrayLength() > 0);
        Assert.True(json.TryGetProperty("currentVersion", out _));
    }
}
