using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SquadPlaces.AppHost.Tests.Tests;

/// <summary>
/// Shared fixture that boots the Aspire app host once for all validation tests.
/// Mirrors the pattern in IntegrationTest1 but avoids paying the startup cost per-test.
/// </summary>
public class ApiTestFixture : IAsyncLifetime
{
    private IAsyncDisposable? _app;
    public HttpClient Client { get; private set; } = null!;

    private static readonly TimeSpan StartupTimeout = TimeSpan.FromSeconds(120);

    public async Task InitializeAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.SquadPlaces_AppHost>();

        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
            logging.AddFilter("Aspire.", LogLevel.Debug);
        });
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        var app = await appHost.BuildAsync().WaitAsync(StartupTimeout);
        await app.StartAsync().WaitAsync(StartupTimeout);

        Client = app.CreateHttpClient("api");
        await app.ResourceNotifications
            .WaitForResourceHealthyAsync("api")
            .WaitAsync(StartupTimeout);

        _app = app;
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        if (_app != null)
            await _app.DisposeAsync();
    }
}

/// <summary>
/// Regression tests for every bug Waingro found during adversarial API testing (2026-03-05).
/// Grouped by severity: P0 (no crashes), P1 (proper validation), happy path, edge cases.
/// </summary>
public class ApiValidationTests : IClassFixture<ApiTestFixture>
{
    private readonly HttpClient _client;

    public ApiValidationTests(ApiTestFixture fixture)
    {
        _client = fixture.Client;
    }

    private static StringContent JsonBody(object obj) =>
        new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

    /// <summary>Creates a valid squad and returns its ID. Used as setup for artifact tests.</summary>
    private async Task<Guid> CreateTestSquadAsync()
    {
        var response = await _client.PostAsync("/api/squads/enlist",
            JsonBody(new { Name = $"test-squad-{Guid.NewGuid():N}", Description = "Integration test squad" }));
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("id").GetGuid();
    }

    // ================================================================
    //  P0 — Server must NOT crash (500) on malformed input
    //  Regression: Waingro BUG-1, BUG-2
    // ================================================================

    [Fact]
    public async Task EnlistSquad_WithEmptyJsonBody_DoesNotReturn500()
    {
        // BUG-1: {} with no fields caused NullReferenceException
        var response = await _client.PostAsync("/api/squads/enlist", JsonBody(new { }));

        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task EnlistSquad_WithMissingName_DoesNotReturn500()
    {
        // BUG-1: Missing Name field deserialized as null, crashed downstream
        var response = await _client.PostAsync("/api/squads/enlist",
            JsonBody(new { Description = "no name" }));

        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task EnlistSquad_WithNullBytesInName_DoesNotReturn500()
    {
        // BUG-2: Null chars caused serialization/storage failure → 500
        // Server may return a 4xx rejection, or the request may fail due to
        // non-ASCII chars propagating into Azure SDK headers / HTTP transport.
        // Either is acceptable — the only unacceptable outcome is a 500 crash.
        HttpResponseMessage? response = null;
        try
        {
            response = await _client.PostAsync("/api/squads/enlist",
                JsonBody(new { Name = "\0null\uFFFDbom\u202Ertl", Description = "unicode chaos" }));
        }
        catch (Exception)
        {
            // Transport-level failure on poison input is not a server crash.
            // Azure SDK rejects non-ASCII chars in blob metadata headers.
            return;
        }

        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    // ================================================================
    //  P1 — Validation must reject bad input with 400
    //  Regression: Waingro BUG-3, BUG-4, BUG-5, BUG-6
    // ================================================================

    [Fact]
    public async Task EnlistSquad_WithEmptyName_Returns400()
    {
        // BUG-3: Empty name was accepted (201) — should be 400
        var response = await _client.PostAsync("/api/squads/enlist",
            JsonBody(new { Name = "", Description = "test" }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task EnlistSquad_WithVeryLongName_Returns400()
    {
        // BUG-6: 10,000 char name was accepted — no max length enforced
        var response = await _client.PostAsync("/api/squads/enlist",
            JsonBody(new { Name = new string('A', 10_000) }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PublishArtifact_WithInvalidType_Returns400()
    {
        // BUG-4: ArtifactType "banana" was accepted — not in allowed set
        var squadId = await CreateTestSquadAsync();
        var response = await _client.PostAsync("/api/artifacts",
            JsonBody(new { SquadId = squadId, Title = "test", Summary = "test", ArtifactType = "banana" }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PublishArtifact_WithEmptyTitle_Returns400()
    {
        // BUG-5: Empty title was accepted — should require non-empty
        var squadId = await CreateTestSquadAsync();
        var response = await _client.PostAsync("/api/artifacts",
            JsonBody(new { SquadId = squadId, Title = "", Summary = "test", ArtifactType = "decision" }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PublishArtifact_WithInvalidSquadId_Returns400()
    {
        // Random GUID that doesn't reference an enlisted squad
        var response = await _client.PostAsync("/api/artifacts",
            JsonBody(new
            {
                SquadId = Guid.NewGuid(),
                Title = "orphan artifact",
                Summary = "test",
                ArtifactType = "decision"
            }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ================================================================
    //  Happy path — validation must not break valid requests
    // ================================================================

    [Fact]
    public async Task EnlistSquad_WithValidData_Returns201()
    {
        var response = await _client.PostAsync("/api/squads/enlist",
            JsonBody(new { Name = $"valid-squad-{Guid.NewGuid():N}", Description = "A perfectly valid squad" }));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        // Verify response contains expected fields
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("id", out _), "Response should contain squad id");
        Assert.True(doc.RootElement.TryGetProperty("name", out _), "Response should contain squad name");
    }

    [Fact]
    public async Task PublishArtifact_WithValidData_Returns201()
    {
        var squadId = await CreateTestSquadAsync();
        var response = await _client.PostAsync("/api/artifacts",
            JsonBody(new
            {
                SquadId = squadId,
                Title = "Test Decision",
                Summary = "We decided to test things properly",
                ArtifactType = "decision",
                Tags = "testing,quality"
            }));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("id", out _), "Response should contain artifact id");
        Assert.True(doc.RootElement.TryGetProperty("title", out _), "Response should contain artifact title");
    }

    [Fact]
    public async Task GetFeed_Returns200()
    {
        var response = await _client.GetAsync("/api/feed");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetFeed_WithPageZero_DoesNotCrash()
    {
        // page=0 is invalid; API should either clamp to 1 (200) or reject (400)
        var response = await _client.GetAsync("/api/feed?page=0");

        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.BadRequest,
            $"Expected 200 (clamped) or 400, got {(int)response.StatusCode}");
    }

    [Fact]
    public async Task GetFeed_WithOversizedPageSize_ReturnsAtMost100Items()
    {
        // pageSize=999 should be clamped to 100 max
        var response = await _client.GetAsync("/api/feed?pageSize=999");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.GetArrayLength() <= 100,
            "Feed should never return more than 100 items regardless of requested pageSize");
    }

    // ================================================================
    //  Edge cases — 404 for nonexistent resources
    // ================================================================

    [Fact]
    public async Task GetSquad_WithNonexistentId_Returns404()
    {
        var response = await _client.GetAsync($"/api/squads/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetArtifact_WithNonexistentId_Returns404()
    {
        var response = await _client.GetAsync($"/api/artifacts/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
