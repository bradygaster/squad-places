namespace SquadPlaces.IntegrationTests;

/// <summary>
/// Tests that verify Redis cache is operational (connected and healthy)
/// and that blob storage (Azure Storage emulator) works end-to-end.
/// </summary>
[Collection("Integration")]
public class InfrastructureTests(AppHostFixture fixture)
{
    [Fact]
    public async Task RedisCache_IsHealthy()
    {
        // The health endpoint aggregates all health checks including Redis
        var response = await fixture.ApiClient.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        // If Redis were unhealthy, the /health endpoint would return Unhealthy/Degraded
        Assert.DoesNotContain("Unhealthy", content);
    }

    [Fact]
    public async Task BlobStorage_SquadPersistsAndRetrieves()
    {
        // Enlist a squad (stored in blob storage)
        var uniqueName = $"Blob Test Squad {Guid.NewGuid():N}";
        var enlistResponse = await fixture.ApiClient.PostAsJsonAsync("/api/squads/enlist", new
        {
            name = uniqueName,
            description = "Testing blob storage persistence"
        });
        Assert.Equal(HttpStatusCode.OK, enlistResponse.StatusCode);

        // Verify it can be retrieved from the squads list
        var squadsResponse = await fixture.ApiClient.GetAsync("/api/squads");
        Assert.Equal(HttpStatusCode.OK, squadsResponse.StatusCode);

        var json = await squadsResponse.Content.ReadFromJsonAsync<JsonElement>();
        var squads = json.EnumerateArray().ToList();
        Assert.Contains(squads, s =>
            s.TryGetProperty("name", out var name) &&
            name.GetString() == uniqueName);
    }

    [Fact]
    public async Task BlobStorage_ArtifactPersistsAndRetrieves()
    {
        // Enlist a squad
        var enlistResponse = await fixture.ApiClient.PostAsJsonAsync("/api/squads/enlist", new
        {
            name = $"Artifact Blob Squad {Guid.NewGuid():N}"
        });
        var enlistJson = await enlistResponse.Content.ReadFromJsonAsync<JsonElement>();
        var squadId = enlistJson.GetProperty("squad").GetProperty("id").GetString()!;

        // Publish an artifact
        var title = $"Blob Artifact {Guid.NewGuid():N}";
        var publishResponse = await fixture.ApiClient.PostAsJsonAsync("/api/artifacts", new
        {
            squadId,
            title,
            summary = "Testing blob storage for artifacts",
            artifactType = "lesson"
        });
        Assert.Equal(HttpStatusCode.OK, publishResponse.StatusCode);

        // Verify artifact appears in feed
        var feedResponse = await fixture.ApiClient.GetAsync("/api/feed");
        var feedJson = await feedResponse.Content.ReadFromJsonAsync<JsonElement>();
        var items = feedJson.GetProperty("items");

        var found = false;
        foreach (var item in items.EnumerateArray())
        {
            if (item.TryGetProperty("title", out var t) && t.GetString() == title)
            {
                found = true;
                break;
            }
        }
        Assert.True(found, $"Expected artifact '{title}' in feed");
    }

    [Fact]
    public async Task CacheResource_IsRunning()
    {
        // Verify the Redis resource started successfully by checking
        // that the app host itself is healthy (it waits for Redis)
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await fixture.App.ResourceNotifications
            .WaitForResourceHealthyAsync("cache", cts.Token);
    }

    [Fact]
    public async Task BlobStorageResource_IsRunning()
    {
        // Verify the storage emulator resource started successfully
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await fixture.App.ResourceNotifications
            .WaitForResourceHealthyAsync("storage", cts.Token);
    }
}
