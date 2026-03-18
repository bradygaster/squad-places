using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SquadPlaces.AppHost.Tests.Tests;

/// <summary>
/// Wave 2 security tests covering:
///   - CORS lockdown (#12) — restrictive origins, preflight, discovery exemption
///   - HMAC API key lifecycle (#13) — generation, validation, management, revocation
///   - Kill switches (#25) — squad suspension, read-only mode, admin endpoints
///
/// Written from requirements before implementations land.
/// Tests use the shared ApiTestFixture (boots Aspire host once).
/// </summary>
public class Wave2SecurityTests : IClassFixture<ApiTestFixture>
{
    private readonly HttpClient _client;

    // Expected allowed origins once CORS lockdown lands (#12)
    private const string AllowedOrigin = "https://squadplaces.dev";
    private const string DisallowedOrigin = "https://evil-site.example.com";

    // Expected API key header name (#13)
    private const string ApiKeyHeader = "X-Api-Key";

    // Dev bypass key for development mode (#13)
    private const string DevBypassKey = "dev-bypass-key";

    public Wave2SecurityTests(ApiTestFixture fixture)
    {
        _client = fixture.Client;
    }

    private static StringContent JsonBody(object obj) =>
        new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

    /// <summary>Creates a valid squad and returns its ID.</summary>
    private async Task<Guid> CreateTestSquadAsync()
    {
        var response = await _client.PostAsync("/api/squads/enlist",
            JsonBody(new { Name = $"w2-squad-{Guid.NewGuid():N}", Description = "Wave 2 security test squad" }));
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("id").GetGuid();
    }

    /// <summary>
    /// Enlists a squad and extracts the API key from the response.
    /// Once #13 lands, enlistment should return an apiKey field.
    /// </summary>
    private async Task<(Guid SquadId, string ApiKey)> CreateSquadWithApiKeyAsync()
    {
        var response = await _client.PostAsync("/api/squads/enlist",
            JsonBody(new { Name = $"w2-keyed-{Guid.NewGuid():N}", Description = "API key test squad" }));
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var squadId = doc.RootElement.GetProperty("id").GetGuid();
        var apiKey = doc.RootElement.GetProperty("apiKey").GetString()!;
        return (squadId, apiKey);
    }

    /// <summary>Builds an HttpRequestMessage with custom Origin header for CORS testing.</summary>
    private static HttpRequestMessage RequestWithOrigin(HttpMethod method, string url, string origin, HttpContent? content = null)
    {
        var request = new HttpRequestMessage(method, url) { Content = content };
        request.Headers.Add("Origin", origin);
        return request;
    }

    /// <summary>Builds an HttpRequestMessage with an API key header.</summary>
    private static HttpRequestMessage AuthenticatedRequest(HttpMethod method, string url, string apiKey, HttpContent? content = null)
    {
        var request = new HttpRequestMessage(method, url) { Content = content };
        request.Headers.Add(ApiKeyHeader, apiKey);
        return request;
    }

    // ================================================================
    //  #12 — CORS Lockdown
    //  Restrictive origins replace AllowAnyOrigin.
    // ================================================================

    [Fact]
    public async Task Cors_AllowedOrigin_IncludesAccessControlHeaders()
    {
        // Requests from an allowed origin should get CORS response headers
        var request = RequestWithOrigin(HttpMethod.Get, "/api/feed", AllowedOrigin);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.True(
            response.Headers.Contains("Access-Control-Allow-Origin"),
            "Response must include Access-Control-Allow-Origin for allowed origins");

        var allowOrigin = response.Headers.GetValues("Access-Control-Allow-Origin").FirstOrDefault();
        Assert.Equal(AllowedOrigin, allowOrigin);
    }

    [Fact]
    public async Task Cors_DisallowedOrigin_OmitsAccessControlHeaders()
    {
        // Requests from a disallowed origin should NOT get CORS headers
        var request = RequestWithOrigin(HttpMethod.Get, "/api/feed", DisallowedOrigin);
        var response = await _client.SendAsync(request);

        // The server still responds (CORS is enforced by the browser),
        // but the response must NOT include Access-Control-Allow-Origin
        Assert.False(
            response.Headers.Contains("Access-Control-Allow-Origin"),
            "Response must NOT include Access-Control-Allow-Origin for disallowed origins");
    }

    [Fact]
    public async Task Cors_WriteEndpoint_DisallowedOrigin_OmitsCorsHeaders()
    {
        // Write endpoints from disallowed origins should lack CORS headers
        var squadId = await CreateTestSquadAsync();
        var content = JsonBody(new
        {
            SquadId = squadId,
            Title = $"CORS Write Test {Guid.NewGuid():N}",
            Summary = "Testing CORS on write",
            ArtifactType = "decision"
        });
        var request = RequestWithOrigin(HttpMethod.Post, "/api/artifacts", DisallowedOrigin, content);
        var response = await _client.SendAsync(request);

        Assert.False(
            response.Headers.Contains("Access-Control-Allow-Origin"),
            "Write endpoint must NOT include CORS headers for disallowed origin");
    }

    [Fact]
    public async Task Cors_DiscoveryEndpoint_AccessibleRegardlessOfOrigin()
    {
        // GET /api is the discovery endpoint — should be accessible from any origin
        var request = RequestWithOrigin(HttpMethod.Get, "/api", DisallowedOrigin);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Cors_PreflightOptions_AllowedOrigin_Returns200()
    {
        // OPTIONS preflight from an allowed origin should succeed with CORS headers
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/artifacts");
        request.Headers.Add("Origin", AllowedOrigin);
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "Content-Type");

        var response = await _client.SendAsync(request);

        // Preflight should return 200 or 204
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent,
            $"Preflight should return 200 or 204, got {(int)response.StatusCode}");

        Assert.True(
            response.Headers.Contains("Access-Control-Allow-Origin"),
            "Preflight must include Access-Control-Allow-Origin for allowed origins");
        Assert.True(
            response.Headers.Contains("Access-Control-Allow-Methods"),
            "Preflight must include Access-Control-Allow-Methods");
    }

    [Fact]
    public async Task Cors_PreflightOptions_DisallowedOrigin_OmitsCorsHeaders()
    {
        // OPTIONS preflight from a disallowed origin should not include CORS headers
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/artifacts");
        request.Headers.Add("Origin", DisallowedOrigin);
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "Content-Type");

        var response = await _client.SendAsync(request);

        Assert.False(
            response.Headers.Contains("Access-Control-Allow-Origin"),
            "Preflight must NOT include Access-Control-Allow-Origin for disallowed origins");
    }

    [Theory]
    [InlineData("https://squadplaces.dev")]
    [InlineData("https://www.squadplaces.dev")]
    public async Task Cors_KnownAllowedOrigins_IncludeCorsHeaders(string origin)
    {
        var request = RequestWithOrigin(HttpMethod.Get, "/api/squads", origin);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(
            response.Headers.Contains("Access-Control-Allow-Origin"),
            $"Origin '{origin}' should be in the CORS allowlist");
    }

    // ================================================================
    //  #13 — HMAC API Key Lifecycle
    //  Write endpoints require a valid API key.
    //  Enlistment returns a key. Keys can be managed per-squad.
    // ================================================================

    [Fact]
    public async Task ApiKey_WriteEndpointWithoutKey_Returns401()
    {
        // POST to a write endpoint with no API key header should be unauthorized
        var squadId = await CreateTestSquadAsync();
        var response = await _client.PostAsync("/api/artifacts",
            JsonBody(new
            {
                SquadId = squadId,
                Title = $"No Key {Guid.NewGuid():N}",
                Summary = "Should be rejected",
                ArtifactType = "decision"
            }));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ApiKey_WriteEndpointWithInvalidKey_Returns403()
    {
        // POST with a bogus API key should be forbidden
        var squadId = await CreateTestSquadAsync();
        var content = JsonBody(new
        {
            SquadId = squadId,
            Title = $"Bad Key {Guid.NewGuid():N}",
            Summary = "Invalid key test",
            ArtifactType = "decision"
        });
        var request = AuthenticatedRequest(HttpMethod.Post, "/api/artifacts", "totally-invalid-key-12345", content);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ApiKey_EnlistmentReturnsApiKey()
    {
        // Enlistment response should contain an apiKey field
        var response = await _client.PostAsync("/api/squads/enlist",
            JsonBody(new { Name = $"w2-enlist-key-{Guid.NewGuid():N}", Description = "Key generation test" }));
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.TryGetProperty("apiKey", out var keyProp),
            "Enlistment response must contain apiKey field");
        Assert.False(string.IsNullOrWhiteSpace(keyProp.GetString()),
            "apiKey must not be empty");
    }

    [Fact]
    public async Task ApiKey_ValidKeyOnSubsequentWrite_Returns201()
    {
        // Enlist a squad (get an API key), then use it to publish an artifact
        var (squadId, apiKey) = await CreateSquadWithApiKeyAsync();

        var content = JsonBody(new
        {
            SquadId = squadId,
            Title = $"Keyed Write {Guid.NewGuid():N}",
            Summary = "Using the enlistment API key",
            ArtifactType = "decision"
        });
        var request = AuthenticatedRequest(HttpMethod.Post, "/api/artifacts", apiKey, content);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task ApiKey_CreateNewKey_Returns201()
    {
        // POST /api/squads/{id}/keys creates a new API key
        var (squadId, apiKey) = await CreateSquadWithApiKeyAsync();

        var request = AuthenticatedRequest(HttpMethod.Post, $"/api/squads/{squadId}/keys", apiKey);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.TryGetProperty("key", out var newKeyProp),
            "Key creation response must contain the raw key");
        Assert.False(string.IsNullOrWhiteSpace(newKeyProp.GetString()),
            "Newly created key must not be empty");
        Assert.True(doc.RootElement.TryGetProperty("prefix", out _),
            "Key creation response must contain a prefix for future reference");
    }

    [Fact]
    public async Task ApiKey_ListKeys_ReturnsMetadataWithoutRawKeys()
    {
        // GET /api/squads/{id}/keys should return key metadata but NOT raw key material
        var (squadId, apiKey) = await CreateSquadWithApiKeyAsync();

        // Create an extra key so we have at least 2
        var createRequest = AuthenticatedRequest(HttpMethod.Post, $"/api/squads/{squadId}/keys", apiKey);
        await _client.SendAsync(createRequest);

        var listRequest = AuthenticatedRequest(HttpMethod.Get, $"/api/squads/{squadId}/keys", apiKey);
        var response = await _client.SendAsync(listRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.GetArrayLength() >= 1,
            "Should return at least one key entry");

        // Verify metadata is present but raw key is NOT
        foreach (var keyEntry in doc.RootElement.EnumerateArray())
        {
            Assert.True(keyEntry.TryGetProperty("prefix", out _),
                "Each key entry must include a prefix");
            Assert.True(keyEntry.TryGetProperty("createdAt", out _),
                "Each key entry must include createdAt timestamp");
            Assert.False(keyEntry.TryGetProperty("key", out _),
                "Key listing must NOT expose raw key material");
        }
    }

    [Fact]
    public async Task ApiKey_RevokeKey_Returns204()
    {
        // DELETE /api/squads/{id}/keys/{prefix} revokes a key
        var (squadId, apiKey) = await CreateSquadWithApiKeyAsync();

        // Create a second key so we can revoke it
        var createRequest = AuthenticatedRequest(HttpMethod.Post, $"/api/squads/{squadId}/keys", apiKey);
        var createResponse = await _client.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();

        var createJson = await createResponse.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createJson);
        var prefix = createDoc.RootElement.GetProperty("prefix").GetString()!;

        // Revoke the second key
        var revokeRequest = AuthenticatedRequest(HttpMethod.Delete, $"/api/squads/{squadId}/keys/{prefix}", apiKey);
        var revokeResponse = await _client.SendAsync(revokeRequest);

        Assert.True(
            revokeResponse.StatusCode == HttpStatusCode.NoContent || revokeResponse.StatusCode == HttpStatusCode.OK,
            $"Revocation should return 204 or 200, got {(int)revokeResponse.StatusCode}");
    }

    [Fact]
    public async Task ApiKey_RevokedKey_Returns403()
    {
        // A revoked key should be rejected on subsequent requests
        var (squadId, apiKey) = await CreateSquadWithApiKeyAsync();

        // Create a second key
        var createRequest = AuthenticatedRequest(HttpMethod.Post, $"/api/squads/{squadId}/keys", apiKey);
        var createResponse = await _client.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();

        var createJson = await createResponse.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createJson);
        var secondKey = createDoc.RootElement.GetProperty("key").GetString()!;
        var prefix = createDoc.RootElement.GetProperty("prefix").GetString()!;

        // Revoke the second key using the original key
        var revokeRequest = AuthenticatedRequest(HttpMethod.Delete, $"/api/squads/{squadId}/keys/{prefix}", apiKey);
        await _client.SendAsync(revokeRequest);

        // Now try to use the revoked key
        var content = JsonBody(new
        {
            SquadId = squadId,
            Title = $"Revoked Key {Guid.NewGuid():N}",
            Summary = "Should be forbidden",
            ArtifactType = "decision"
        });
        var writeRequest = AuthenticatedRequest(HttpMethod.Post, "/api/artifacts", secondKey, content);
        var writeResponse = await _client.SendAsync(writeRequest);

        Assert.Equal(HttpStatusCode.Forbidden, writeResponse.StatusCode);
    }

    [Fact]
    public async Task ApiKey_DevBypassKey_WorksInDevelopmentMode()
    {
        // In development/test environments, the dev bypass key should be accepted
        var squadId = await CreateTestSquadAsync();
        var content = JsonBody(new
        {
            SquadId = squadId,
            Title = $"Dev Bypass {Guid.NewGuid():N}",
            Summary = "Using dev bypass key",
            ArtifactType = "decision"
        });
        var request = AuthenticatedRequest(HttpMethod.Post, "/api/artifacts", DevBypassKey, content);
        var response = await _client.SendAsync(request);

        // In development mode, the dev bypass key should allow writes
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task ApiKey_ReadEndpointsDoNotRequireKey()
    {
        // GET endpoints should remain accessible without an API key
        var response = await _client.GetAsync("/api/feed");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ================================================================
    //  #25 — Kill Switches
    //  Admin endpoints to suspend squads and enable read-only mode.
    // ================================================================

    [Fact]
    public async Task KillSwitch_SuspendSquad_Returns200()
    {
        // POST /api/admin/kill-switch/squad/{id}/suspend should succeed
        var squadId = await CreateTestSquadAsync();

        var response = await _client.PostAsync($"/api/admin/kill-switch/squad/{squadId}/suspend", null);

        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent,
            $"Suspend should return 200 or 204, got {(int)response.StatusCode}");
    }

    [Fact]
    public async Task KillSwitch_SuspendedSquad_PostReturns403()
    {
        // A suspended squad's write requests should be forbidden
        var (squadId, apiKey) = await CreateSquadWithApiKeyAsync();

        // Suspend the squad
        await _client.PostAsync($"/api/admin/kill-switch/squad/{squadId}/suspend", null);

        // Try to publish an artifact to the suspended squad
        var content = JsonBody(new
        {
            SquadId = squadId,
            Title = $"Suspended Write {Guid.NewGuid():N}",
            Summary = "Should be rejected",
            ArtifactType = "decision"
        });
        var request = AuthenticatedRequest(HttpMethod.Post, "/api/artifacts", apiKey, content);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task KillSwitch_SuspendedSquad_CanStillRead()
    {
        // A suspended squad should still allow GET operations
        var squadId = await CreateTestSquadAsync();

        // Suspend the squad
        await _client.PostAsync($"/api/admin/kill-switch/squad/{squadId}/suspend", null);

        // GET should still work
        var response = await _client.GetAsync($"/api/squads/{squadId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Feed for the squad should still work
        var feedResponse = await _client.GetAsync($"/api/feed/{squadId}");
        Assert.Equal(HttpStatusCode.OK, feedResponse.StatusCode);
    }

    [Fact]
    public async Task KillSwitch_EnableReadOnly_Returns200()
    {
        // POST /api/admin/kill-switch/readonly enables global read-only mode
        var response = await _client.PostAsync("/api/admin/kill-switch/readonly", null);

        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent,
            $"Enable read-only should return 200 or 204, got {(int)response.StatusCode}");

        // Clean up: disable read-only so other tests aren't affected
        await _client.DeleteAsync("/api/admin/kill-switch/readonly");
    }

    [Fact]
    public async Task KillSwitch_ReadOnlyMode_BlocksAllWrites()
    {
        // When read-only mode is active, ALL writes should return 503
        try
        {
            await _client.PostAsync("/api/admin/kill-switch/readonly", null);

            // Try to enlist a new squad
            var enlistResponse = await _client.PostAsync("/api/squads/enlist",
                JsonBody(new { Name = $"readonly-block-{Guid.NewGuid():N}", Description = "Should be blocked" }));
            Assert.Equal(HttpStatusCode.ServiceUnavailable, enlistResponse.StatusCode);
        }
        finally
        {
            // Always clean up read-only mode
            await _client.DeleteAsync("/api/admin/kill-switch/readonly");
        }
    }

    [Fact]
    public async Task KillSwitch_ReadOnlyMode_AllowsAllReads()
    {
        // When read-only mode is active, all GET requests should still work
        try
        {
            await _client.PostAsync("/api/admin/kill-switch/readonly", null);

            var feedResponse = await _client.GetAsync("/api/feed");
            Assert.Equal(HttpStatusCode.OK, feedResponse.StatusCode);

            var squadsResponse = await _client.GetAsync("/api/squads");
            Assert.Equal(HttpStatusCode.OK, squadsResponse.StatusCode);
        }
        finally
        {
            await _client.DeleteAsync("/api/admin/kill-switch/readonly");
        }
    }

    [Fact]
    public async Task KillSwitch_DisableReadOnly_RestoresWrites()
    {
        // DELETE /api/admin/kill-switch/readonly should restore normal operation
        await _client.PostAsync("/api/admin/kill-switch/readonly", null);

        var disableResponse = await _client.DeleteAsync("/api/admin/kill-switch/readonly");
        Assert.True(
            disableResponse.StatusCode == HttpStatusCode.OK || disableResponse.StatusCode == HttpStatusCode.NoContent,
            $"Disable read-only should return 200 or 204, got {(int)disableResponse.StatusCode}");

        // Writes should work again
        var enlistResponse = await _client.PostAsync("/api/squads/enlist",
            JsonBody(new { Name = $"post-readonly-{Guid.NewGuid():N}", Description = "Writes restored" }));
        Assert.Equal(HttpStatusCode.Created, enlistResponse.StatusCode);
    }

    [Fact]
    public async Task KillSwitch_GetStatus_ReturnsCurrentState()
    {
        // GET /api/admin/kill-switch/status should return current kill switch state
        var response = await _client.GetAsync("/api/admin/kill-switch/status");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        // Status should include readOnly flag and suspended squads list
        Assert.True(doc.RootElement.TryGetProperty("readOnly", out _),
            "Status must include readOnly field");
        Assert.True(doc.RootElement.TryGetProperty("suspendedSquads", out _),
            "Status must include suspendedSquads field");
    }

    [Fact]
    public async Task KillSwitch_UnsuspendSquad_RestoresWriteAccess()
    {
        // Suspending and then unsuspending a squad should restore write ability
        var (squadId, apiKey) = await CreateSquadWithApiKeyAsync();

        // Suspend
        await _client.PostAsync($"/api/admin/kill-switch/squad/{squadId}/suspend", null);

        // Verify suspended (write blocked)
        var blockedContent = JsonBody(new
        {
            SquadId = squadId,
            Title = $"Blocked {Guid.NewGuid():N}",
            Summary = "Suspended",
            ArtifactType = "decision"
        });
        var blockedRequest = AuthenticatedRequest(HttpMethod.Post, "/api/artifacts", apiKey, blockedContent);
        var blockedResponse = await _client.SendAsync(blockedRequest);
        Assert.Equal(HttpStatusCode.Forbidden, blockedResponse.StatusCode);

        // Unsuspend
        var unsuspendResponse = await _client.DeleteAsync($"/api/admin/kill-switch/squad/{squadId}/suspend");
        Assert.True(
            unsuspendResponse.StatusCode == HttpStatusCode.OK || unsuspendResponse.StatusCode == HttpStatusCode.NoContent,
            $"Unsuspend should return 200 or 204, got {(int)unsuspendResponse.StatusCode}");

        // Write should now work again
        var restoredContent = JsonBody(new
        {
            SquadId = squadId,
            Title = $"Restored {Guid.NewGuid():N}",
            Summary = "After unsuspend",
            ArtifactType = "decision"
        });
        var restoredRequest = AuthenticatedRequest(HttpMethod.Post, "/api/artifacts", apiKey, restoredContent);
        var restoredResponse = await _client.SendAsync(restoredRequest);
        Assert.Equal(HttpStatusCode.Created, restoredResponse.StatusCode);
    }

    [Fact]
    public async Task KillSwitch_StatusReflectsSuspendedSquads()
    {
        // After suspending a squad, the status endpoint should list it
        var squadId = await CreateTestSquadAsync();

        await _client.PostAsync($"/api/admin/kill-switch/squad/{squadId}/suspend", null);

        try
        {
            var response = await _client.GetAsync("/api/admin/kill-switch/status");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var suspendedSquads = doc.RootElement.GetProperty("suspendedSquads");
            var suspendedIds = new List<string>();
            foreach (var item in suspendedSquads.EnumerateArray())
            {
                suspendedIds.Add(item.GetString() ?? item.ToString());
            }

            Assert.Contains(suspendedIds, id => id.Contains(squadId.ToString(), StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            // Clean up suspension
            await _client.DeleteAsync($"/api/admin/kill-switch/squad/{squadId}/suspend");
        }
    }

    [Fact]
    public async Task KillSwitch_ReadOnlyMode_ArtifactWriteBlocked()
    {
        // Verify artifact creation specifically returns 503 in read-only mode
        var (squadId, apiKey) = await CreateSquadWithApiKeyAsync();
        try
        {
            await _client.PostAsync("/api/admin/kill-switch/readonly", null);

            var content = JsonBody(new
            {
                SquadId = squadId,
                Title = $"Readonly Block {Guid.NewGuid():N}",
                Summary = "Should 503",
                ArtifactType = "decision"
            });
            var request = AuthenticatedRequest(HttpMethod.Post, "/api/artifacts", apiKey, content);
            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        }
        finally
        {
            await _client.DeleteAsync("/api/admin/kill-switch/readonly");
        }
    }

    [Fact]
    public async Task KillSwitch_SuspendNonexistentSquad_Returns404()
    {
        // Suspending a squad that doesn't exist should return 404
        var response = await _client.PostAsync($"/api/admin/kill-switch/squad/{Guid.NewGuid()}/suspend", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
