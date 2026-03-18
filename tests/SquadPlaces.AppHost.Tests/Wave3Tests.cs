using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SquadPlaces.AppHost.Tests.Tests;

/// <summary>
/// Wave 3 integration tests covering:
///   - Audit log (#27) — append-only, hash chain, actor/resource filtering
///   - SSRF protection (#16) — private IP / loopback / metadata blocking
///   - Authority framework (#20) — authority levels, domain scopes, violations
///   - Admin dashboard (#23) — overview, squad list, detail views
///   - Content moderation queue (#24) — queue, count, approve/reject
///
/// Written from requirements before implementations land.
/// Tests use the shared ApiTestFixture (boots Aspire host once).
/// </summary>
public class Wave3Tests : IClassFixture<ApiTestFixture>
{
    private readonly HttpClient _client;

    public Wave3Tests(ApiTestFixture fixture)
    {
        _client = fixture.Client;
    }

    private static StringContent JsonBody(object obj) =>
        new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

    private async Task<Guid> CreateTestSquadAsync()
    {
        var response = await _client.PostAsync("/api/squads/enlist",
            JsonBody(new { Name = $"w3-squad-{Guid.NewGuid():N}", Description = "Wave 3 test squad" }));
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("id").GetGuid();
    }

    private async Task<Guid> PublishTestArtifactAsync(Guid squadId, string? title = null, string? gifUrl = null)
    {
        var payload = new Dictionary<string, object?>
        {
            ["SquadId"] = squadId,
            ["Title"] = title ?? $"W3 Artifact {Guid.NewGuid():N}",
            ["Summary"] = "Wave 3 test artifact",
            ["ArtifactType"] = "decision",
            ["Tags"] = "wave3-test"
        };
        if (gifUrl != null)
            payload["GifUrl"] = gifUrl;

        var response = await _client.PostAsync("/api/artifacts", JsonBody(payload));
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("id").GetGuid();
    }

    // ================================================================
    //  #27 — Audit Log
    //  Append-only, tamper-evident audit trail with hash chain.
    // ================================================================

    [Fact]
    public async Task AuditLog_CreateArtifact_GeneratesAuditEntry()
    {
        // Creating an artifact should produce an audit log entry
        var squadId = await CreateTestSquadAsync();
        var artifactId = await PublishTestArtifactAsync(squadId);

        var response = await _client.GetAsync("/api/admin/audit");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        // At least one entry must exist
        Assert.True(doc.RootElement.GetArrayLength() >= 1,
            "Audit log should contain at least one entry after artifact creation");
    }

    [Fact]
    public async Task AuditLog_GetById_ReturnsSingleEntry()
    {
        // Create something to generate an audit entry, then fetch audit log
        var squadId = await CreateTestSquadAsync();
        await PublishTestArtifactAsync(squadId);

        var listResponse = await _client.GetAsync("/api/admin/audit");
        listResponse.EnsureSuccessStatusCode();

        var listJson = await listResponse.Content.ReadAsStringAsync();
        using var listDoc = JsonDocument.Parse(listJson);
        var firstEntry = listDoc.RootElement[0];
        var entryId = firstEntry.GetProperty("id").GetString()!;

        // Fetch single entry by ID
        var response = await _client.GetAsync($"/api/admin/audit/{entryId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var entryDoc = JsonDocument.Parse(json);
        Assert.Equal(entryId, entryDoc.RootElement.GetProperty("id").GetString());
    }

    [Fact]
    public async Task AuditLog_VerifyIntegrity_ReturnsValid()
    {
        // Hash chain verification endpoint should report valid
        var response = await _client.GetAsync("/api/admin/audit/verify");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.TryGetProperty("valid", out var validProp),
            "Verify response must include 'valid' field");
        Assert.True(validProp.GetBoolean(),
            "Hash chain should be valid (no tampering)");
    }

    [Fact]
    public async Task AuditLog_FilterByActor_ReturnsMatchingEntries()
    {
        // Create a squad (the system or squad creator is the actor)
        var squadId = await CreateTestSquadAsync();
        await PublishTestArtifactAsync(squadId);

        // Use the squad ID as the actor filter (implementation may use squadId as actor)
        var response = await _client.GetAsync($"/api/admin/audit/actor/{squadId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.ValueKind == JsonValueKind.Array,
            "Actor filter should return an array of audit entries");
    }

    [Fact]
    public async Task AuditLog_FilterByResource_ReturnsMatchingEntries()
    {
        var squadId = await CreateTestSquadAsync();
        var artifactId = await PublishTestArtifactAsync(squadId);

        var response = await _client.GetAsync($"/api/admin/audit/resource/{artifactId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.ValueKind == JsonValueKind.Array,
            "Resource filter should return an array of audit entries");
        Assert.True(doc.RootElement.GetArrayLength() >= 1,
            "Should have at least one audit entry for the created artifact");
    }

    [Fact]
    public async Task AuditLog_EntriesHaveRequiredFields()
    {
        // Each audit entry must have: timestamp, eventType, hash, previousHash
        var squadId = await CreateTestSquadAsync();
        await PublishTestArtifactAsync(squadId);

        var response = await _client.GetAsync("/api/admin/audit");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var entry = doc.RootElement[doc.RootElement.GetArrayLength() - 1];

        Assert.True(entry.TryGetProperty("timestamp", out _),
            "Audit entry must have 'timestamp' field");
        Assert.True(entry.TryGetProperty("eventType", out _),
            "Audit entry must have 'eventType' field");
        Assert.True(entry.TryGetProperty("hash", out _),
            "Audit entry must have 'hash' field");
        Assert.True(entry.TryGetProperty("previousHash", out _),
            "Audit entry must have 'previousHash' field");
    }

    [Fact]
    public async Task AuditLog_HashChain_EntryNPreviousHashEqualsEntryNMinus1Hash()
    {
        // Create multiple artifacts to generate multiple audit entries
        var squadId = await CreateTestSquadAsync();
        await PublishTestArtifactAsync(squadId, "Chain Test 1");
        await PublishTestArtifactAsync(squadId, "Chain Test 2");

        var response = await _client.GetAsync("/api/admin/audit");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.GetArrayLength() >= 2,
            "Need at least 2 audit entries to verify hash chain");

        // Walk the chain: entry[i].previousHash == entry[i-1].hash
        for (int i = 1; i < doc.RootElement.GetArrayLength(); i++)
        {
            var prev = doc.RootElement[i - 1];
            var curr = doc.RootElement[i];

            var prevHash = prev.GetProperty("hash").GetString();
            var currPrevHash = curr.GetProperty("previousHash").GetString();

            Assert.Equal(prevHash, currPrevHash);
        }
    }

    // ================================================================
    //  #16 — SSRF Protection
    //  Block requests to private/loopback/link-local/metadata IPs.
    // ================================================================

    [Fact]
    public async Task SsrfProtection_LocalhostGifUrl_Returns400()
    {
        var squadId = await CreateTestSquadAsync();
        var response = await _client.PostAsync("/api/artifacts",
            JsonBody(new
            {
                SquadId = squadId,
                Title = $"SSRF localhost {Guid.NewGuid():N}",
                Summary = "SSRF test",
                ArtifactType = "decision",
                GifUrl = "http://localhost/evil"
            }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SsrfProtection_LoopbackIpGifUrl_Returns400()
    {
        var squadId = await CreateTestSquadAsync();
        var response = await _client.PostAsync("/api/artifacts",
            JsonBody(new
            {
                SquadId = squadId,
                Title = $"SSRF loopback {Guid.NewGuid():N}",
                Summary = "SSRF test",
                ArtifactType = "decision",
                GifUrl = "http://127.0.0.1/evil"
            }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SsrfProtection_AwsMetadataGifUrl_Returns400()
    {
        var squadId = await CreateTestSquadAsync();
        var response = await _client.PostAsync("/api/artifacts",
            JsonBody(new
            {
                SquadId = squadId,
                Title = $"SSRF metadata {Guid.NewGuid():N}",
                Summary = "SSRF test",
                ArtifactType = "decision",
                GifUrl = "http://169.254.169.254/metadata"
            }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SsrfProtection_PrivateClassAGifUrl_Returns400()
    {
        var squadId = await CreateTestSquadAsync();
        var response = await _client.PostAsync("/api/artifacts",
            JsonBody(new
            {
                SquadId = squadId,
                Title = $"SSRF class A {Guid.NewGuid():N}",
                Summary = "SSRF test",
                ArtifactType = "decision",
                GifUrl = "http://10.0.0.1/internal"
            }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SsrfProtection_PrivateClassCGifUrl_Returns400()
    {
        var squadId = await CreateTestSquadAsync();
        var response = await _client.PostAsync("/api/artifacts",
            JsonBody(new
            {
                SquadId = squadId,
                Title = $"SSRF class C {Guid.NewGuid():N}",
                Summary = "SSRF test",
                ArtifactType = "decision",
                GifUrl = "http://192.168.1.1/internal"
            }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SsrfProtection_Ipv6LoopbackGifUrl_Returns400()
    {
        var squadId = await CreateTestSquadAsync();
        var response = await _client.PostAsync("/api/artifacts",
            JsonBody(new
            {
                SquadId = squadId,
                Title = $"SSRF IPv6 {Guid.NewGuid():N}",
                Summary = "SSRF test",
                ArtifactType = "decision",
                GifUrl = "http://[::1]/evil"
            }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SsrfProtection_ValidExternalGifUrl_Accepted()
    {
        // A legitimate external URL should NOT be blocked
        var squadId = await CreateTestSquadAsync();
        var response = await _client.PostAsync("/api/artifacts",
            JsonBody(new
            {
                SquadId = squadId,
                Title = $"SSRF valid {Guid.NewGuid():N}",
                Summary = "SSRF test — valid URL",
                ArtifactType = "decision",
                GifUrl = "https://example.com/valid.gif"
            }));

        // Should succeed (201 Created) — not blocked by SSRF filter
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    // ================================================================
    //  #20 — Authority Framework
    //  Authority levels, domain scopes, and violation detection.
    // ================================================================

    [Fact]
    public async Task Authority_UpdateAuthorityLevel_Returns200()
    {
        var squadId = await CreateTestSquadAsync();

        var response = await _client.PutAsync($"/api/admin/squads/{squadId}/authority",
            JsonBody(new { AuthorityLevel = "elevated" }));

        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent,
            $"Authority update should return 200 or 204, got {(int)response.StatusCode}");
    }

    [Fact]
    public async Task Authority_SetDomainScopes_Returns200()
    {
        var squadId = await CreateTestSquadAsync();

        var response = await _client.PutAsync($"/api/admin/squads/{squadId}/domains",
            JsonBody(new { DomainScopes = new[] { "security", "infrastructure" } }));

        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent,
            $"Domain scope update should return 200 or 204, got {(int)response.StatusCode}");
    }

    [Fact]
    public async Task Authority_GetViolations_ReturnsArray()
    {
        var response = await _client.GetAsync("/api/admin/authority-violations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.ValueKind == JsonValueKind.Array,
            "Authority violations endpoint should return an array (may be empty)");
    }

    [Fact]
    public async Task Authority_SquadModelHasAuthorityFields()
    {
        // After enlistment, squad detail should include authority-related fields
        var squadId = await CreateTestSquadAsync();

        var response = await _client.GetAsync($"/api/squads/{squadId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.TryGetProperty("authorityLevel", out _),
            "Squad model must include 'authorityLevel' field");
        Assert.True(doc.RootElement.TryGetProperty("domainScopes", out _),
            "Squad model must include 'domainScopes' field");
    }

    [Fact]
    public async Task Authority_UpdateThenGet_ReflectsNewLevel()
    {
        var squadId = await CreateTestSquadAsync();

        // Set authority level
        await _client.PutAsync($"/api/admin/squads/{squadId}/authority",
            JsonBody(new { AuthorityLevel = "restricted" }));

        // Get squad and verify it reflects the new level
        var response = await _client.GetAsync($"/api/squads/{squadId}");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var level = doc.RootElement.GetProperty("authorityLevel").GetString();
        Assert.Equal("restricted", level);
    }

    // ================================================================
    //  #23 — Admin Dashboard
    //  Overview metrics, squad listing, and detailed squad view.
    // ================================================================

    [Fact]
    public async Task AdminDashboard_GetOverview_ReturnsMetrics()
    {
        // Ensure at least one squad exists
        await CreateTestSquadAsync();

        var response = await _client.GetAsync("/api/admin/dashboard");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        // Dashboard should contain aggregate counts
        Assert.True(doc.RootElement.TryGetProperty("squadCount", out var squadCount),
            "Dashboard must include 'squadCount'");
        Assert.True(squadCount.GetInt32() >= 1,
            "Squad count should be at least 1");

        Assert.True(doc.RootElement.TryGetProperty("artifactCount", out _),
            "Dashboard must include 'artifactCount'");
    }

    [Fact]
    public async Task AdminDashboard_ListSquads_ReturnsArray()
    {
        await CreateTestSquadAsync();

        var response = await _client.GetAsync("/api/admin/squads");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.ValueKind == JsonValueKind.Array,
            "Admin squads endpoint should return an array");
        Assert.True(doc.RootElement.GetArrayLength() >= 1,
            "Should have at least one squad in admin listing");
    }

    [Fact]
    public async Task AdminDashboard_GetSquadDetail_ReturnsSquadInfo()
    {
        var squadId = await CreateTestSquadAsync();

        var response = await _client.GetAsync($"/api/admin/squads/{squadId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.TryGetProperty("id", out _),
            "Squad detail must include 'id'");
        Assert.True(doc.RootElement.TryGetProperty("name", out _),
            "Squad detail must include 'name'");
    }

    [Fact]
    public async Task AdminDashboard_GetSquadDetail_NonexistentId_Returns404()
    {
        var bogusId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/admin/squads/{bogusId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ================================================================
    //  #24 — Content Moderation Queue
    //  Queue listing, count, approve/reject workflows.
    // ================================================================

    [Fact]
    public async Task ModerationQueue_GetPendingItems_ReturnsArray()
    {
        var response = await _client.GetAsync("/api/admin/moderation-queue");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.ValueKind == JsonValueKind.Array,
            "Moderation queue should return an array (may be empty)");
    }

    [Fact]
    public async Task ModerationQueue_GetCount_ReturnsNumber()
    {
        var response = await _client.GetAsync("/api/admin/moderation-queue/count");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        Assert.True(
            doc.RootElement.TryGetProperty("count", out var countProp),
            "Count endpoint must include 'count' field");
        Assert.True(countProp.GetInt32() >= 0,
            "Count must be non-negative");
    }

    [Fact]
    public async Task ModerationQueue_ApproveArtifact_Returns200()
    {
        // Create an artifact that may land in moderation queue
        var squadId = await CreateTestSquadAsync();
        var artifactId = await PublishTestArtifactAsync(squadId);

        var response = await _client.PostAsync(
            $"/api/admin/moderation/artifact/{artifactId}/approve", null);

        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent,
            $"Approve should return 200 or 204, got {(int)response.StatusCode}");
    }

    [Fact]
    public async Task ModerationQueue_RejectArtifactWithReason_Returns200()
    {
        var squadId = await CreateTestSquadAsync();
        var artifactId = await PublishTestArtifactAsync(squadId);

        var response = await _client.PostAsync(
            $"/api/admin/moderation/artifact/{artifactId}/reject",
            JsonBody(new { Reason = "Violates community guidelines" }));

        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent,
            $"Reject should return 200 or 204, got {(int)response.StatusCode}");
    }

    [Fact]
    public async Task ModerationQueue_ApproveNonexistentArtifact_Returns404()
    {
        var bogusId = Guid.NewGuid();

        var response = await _client.PostAsync(
            $"/api/admin/moderation/artifact/{bogusId}/approve", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ModerationQueue_RejectNonexistentArtifact_Returns404()
    {
        var bogusId = Guid.NewGuid();

        var response = await _client.PostAsync(
            $"/api/admin/moderation/artifact/{bogusId}/reject",
            JsonBody(new { Reason = "Does not exist" }));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
