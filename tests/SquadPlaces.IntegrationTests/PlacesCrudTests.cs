namespace SquadPlaces.IntegrationTests;

/// <summary>
/// Tests for Squad and Artifact CRUD operations via the API.
/// Exercises blob storage (squads/artifacts stored in Azure Blob emulator).
/// </summary>
[Collection("Integration")]
public class PlacesCrudTests(AppHostFixture fixture)
{
    private HttpClient Client => fixture.ApiClient;

    [Fact]
    public async Task EnlistSquad_ReturnsCreatedWithId()
    {
        var request = new
        {
            name = $"Test Squad {Guid.NewGuid():N}",
            description = "Integration test squad"
        };

        var response = await Client.PostAsJsonAsync("/api/squads/enlist", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("squad", out var squad));
        Assert.True(squad.TryGetProperty("id", out var id));
        Assert.NotEqual(Guid.Empty, Guid.Parse(id.GetString()!));
    }

    [Fact]
    public async Task EnlistSquad_InvalidRequest_ReturnsValidationError()
    {
        // Name is required
        var request = new { name = "", description = "missing name" };
        var response = await Client.PostAsJsonAsync("/api/squads/enlist", request);

        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.UnprocessableEntity,
            $"Expected 400 or 422, got {response.StatusCode}");
    }

    [Fact]
    public async Task ListSquads_ReturnsOk()
    {
        var response = await Client.GetAsync("/api/squads");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PublishArtifact_ThenAppearInFeed()
    {
        // First, enlist a squad
        var enlistResponse = await Client.PostAsJsonAsync("/api/squads/enlist", new
        {
            name = $"Artifact Test Squad {Guid.NewGuid():N}",
            description = "For artifact tests"
        });
        Assert.Equal(HttpStatusCode.OK, enlistResponse.StatusCode);
        var enlistJson = await enlistResponse.Content.ReadFromJsonAsync<JsonElement>();
        var squadId = enlistJson.GetProperty("squad").GetProperty("id").GetString()!;

        // Publish an artifact
        var artifact = new
        {
            squadId,
            title = "Test Pattern",
            summary = "A test pattern for integration testing",
            artifactType = "pattern",
            tags = "testing,integration"
        };

        var publishResponse = await Client.PostAsJsonAsync("/api/artifacts", artifact);
        Assert.Equal(HttpStatusCode.OK, publishResponse.StatusCode);
        var publishJson = await publishResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(publishJson.TryGetProperty("id", out _));

        // Verify it appears in the feed
        var feedResponse = await Client.GetAsync("/api/feed");
        Assert.Equal(HttpStatusCode.OK, feedResponse.StatusCode);
        var feedJson = await feedResponse.Content.ReadFromJsonAsync<JsonElement>();
        var items = feedJson.GetProperty("items");
        Assert.True(items.GetArrayLength() > 0);
    }

    [Fact]
    public async Task PublishArtifact_InvalidType_ReturnsError()
    {
        // Enlist first
        var enlistResponse = await Client.PostAsJsonAsync("/api/squads/enlist", new
        {
            name = $"Invalid Type Squad {Guid.NewGuid():N}"
        });
        var enlistJson = await enlistResponse.Content.ReadFromJsonAsync<JsonElement>();
        var squadId = enlistJson.GetProperty("squad").GetProperty("id").GetString()!;

        var artifact = new
        {
            squadId,
            title = "Bad Artifact",
            summary = "Invalid type test",
            artifactType = "not_a_valid_type"
        };

        var response = await Client.PostAsJsonAsync("/api/artifacts", artifact);
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.UnprocessableEntity,
            $"Expected error status, got {response.StatusCode}");
    }

    [Fact]
    public async Task GetFeed_ReturnsOkWithItems()
    {
        var response = await Client.GetAsync("/api/feed");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("items", out _));
    }
}
