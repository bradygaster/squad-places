using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SquadPlaces.AppHost.Tests.Tests;

/// <summary>
/// Integration tests for the Comments/Replies and GIF features.
/// Written against the API contract before implementation lands.
/// Uses the shared ApiTestFixture to boot the Aspire host once.
/// </summary>
public class CommentAndGifTests : IClassFixture<ApiTestFixture>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public CommentAndGifTests(ApiTestFixture fixture)
    {
        _client = fixture.Client;
    }

    private static StringContent JsonBody(object obj) =>
        new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

    private async Task<Guid> CreateTestSquadAsync()
    {
        var response = await _client.PostAsync("/api/squads/enlist",
            JsonBody(new { Name = $"test-squad-{Guid.NewGuid():N}", Description = "Comment test squad" }));
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("id").GetGuid();
    }

    private async Task<Guid> PublishTestArtifactAsync(Guid squadId, string? gifUrl = null)
    {
        var payload = new Dictionary<string, object?>
        {
            ["SquadId"] = squadId,
            ["Title"] = $"Test Artifact {Guid.NewGuid():N}",
            ["Summary"] = "An artifact for comment testing",
            ["ArtifactType"] = "decision",
            ["Tags"] = "testing"
        };
        if (gifUrl != null)
            payload["GifUrl"] = gifUrl;

        var response = await _client.PostAsync("/api/artifacts", JsonBody(payload));
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("id").GetGuid();
    }

    private async Task<(HttpResponseMessage Response, Guid? CommentId)> PostCommentAsync(
        Guid artifactId, Guid squadId, string body,
        string? gifUrl = null, Guid? parentCommentId = null)
    {
        var payload = new Dictionary<string, object?>
        {
            ["SquadId"] = squadId,
            ["Body"] = body
        };
        if (gifUrl != null)
            payload["GifUrl"] = gifUrl;
        if (parentCommentId != null)
            payload["ParentCommentId"] = parentCommentId;

        var response = await _client.PostAsync($"/api/artifacts/{artifactId}/comments", JsonBody(payload));

        Guid? commentId = null;
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("id", out var idProp))
                commentId = idProp.GetGuid();
        }
        return (response, commentId);
    }

    // ================================================================
    //  Comments — Happy Path
    // ================================================================

    [Fact]
    public async Task PostComment_OnValidArtifact_Returns201()
    {
        var squadId = await CreateTestSquadAsync();
        var artifactId = await PublishTestArtifactAsync(squadId);

        var (response, commentId) = await PostCommentAsync(artifactId, squadId, "Great artifact!");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(commentId);
        Assert.NotEqual(Guid.Empty, commentId!.Value);
    }

    [Fact]
    public async Task GetComments_ReturnsPostedComments()
    {
        var squadId = await CreateTestSquadAsync();
        var artifactId = await PublishTestArtifactAsync(squadId);

        await PostCommentAsync(artifactId, squadId, "First comment");
        await PostCommentAsync(artifactId, squadId, "Second comment");

        var response = await _client.GetAsync($"/api/artifacts/{artifactId}/comments");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var comments = doc.RootElement;
        Assert.True(comments.GetArrayLength() >= 2,
            $"Expected at least 2 comments, got {comments.GetArrayLength()}");
    }

    [Fact]
    public async Task GetComment_ById_Returns200()
    {
        var squadId = await CreateTestSquadAsync();
        var artifactId = await PublishTestArtifactAsync(squadId);

        var (_, commentId) = await PostCommentAsync(artifactId, squadId, "Comment to retrieve");
        Assert.NotNull(commentId);

        var response = await _client.GetAsync($"/api/comments/{commentId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("Comment to retrieve", doc.RootElement.GetProperty("body").GetString());
        Assert.Equal(squadId, doc.RootElement.GetProperty("squadId").GetGuid());
        Assert.Equal(artifactId, doc.RootElement.GetProperty("artifactId").GetGuid());
    }

    [Fact]
    public async Task PostReply_WithParentCommentId_Returns201()
    {
        var squadId = await CreateTestSquadAsync();
        var artifactId = await PublishTestArtifactAsync(squadId);

        var (_, parentId) = await PostCommentAsync(artifactId, squadId, "Parent comment");
        Assert.NotNull(parentId);

        var (response, replyId) = await PostCommentAsync(
            artifactId, squadId, "This is a reply", parentCommentId: parentId);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(replyId);

        // Verify the reply references the parent
        var getResponse = await _client.GetAsync($"/api/comments/{replyId}");
        var json = await getResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal(parentId, doc.RootElement.GetProperty("parentCommentId").GetGuid());
    }

    [Fact]
    public async Task PostComment_WithGifUrl_Returns201()
    {
        var squadId = await CreateTestSquadAsync();
        var artifactId = await PublishTestArtifactAsync(squadId);

        var gifUrl = "https://media.giphy.com/media/xT9IgzoKnwFNmISR8I/giphy.gif";
        var (response, commentId) = await PostCommentAsync(
            artifactId, squadId, "Check this out!", gifUrl: gifUrl);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        // Verify GifUrl is preserved
        var getResponse = await _client.GetAsync($"/api/comments/{commentId}");
        var json = await getResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal(gifUrl, doc.RootElement.GetProperty("gifUrl").GetString());
    }

    [Fact]
    public async Task PublishArtifact_WithGifUrl_Returns201()
    {
        var squadId = await CreateTestSquadAsync();
        var gifUrl = "https://media.giphy.com/media/3o7TKSjRrfIPjeiVyQ/giphy.gif";

        var artifactId = await PublishTestArtifactAsync(squadId, gifUrl: gifUrl);

        // Verify the artifact retained the GifUrl
        var response = await _client.GetAsync($"/api/artifacts/{artifactId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal(gifUrl, doc.RootElement.GetProperty("gifUrl").GetString());
    }

    // ================================================================
    //  Comments — Validation
    // ================================================================

    [Fact]
    public async Task PostComment_EmptyBody_Returns400()
    {
        var squadId = await CreateTestSquadAsync();
        var artifactId = await PublishTestArtifactAsync(squadId);

        var (response, _) = await PostCommentAsync(artifactId, squadId, "");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostComment_MissingBody_Returns400()
    {
        var squadId = await CreateTestSquadAsync();
        var artifactId = await PublishTestArtifactAsync(squadId);

        // Send payload without Body field at all
        var payload = new { SquadId = squadId };
        var response = await _client.PostAsync(
            $"/api/artifacts/{artifactId}/comments", JsonBody(payload));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostComment_BodyTooLong_Returns400()
    {
        var squadId = await CreateTestSquadAsync();
        var artifactId = await PublishTestArtifactAsync(squadId);

        var oversizedBody = new string('X', 10_000); // exceeds 5000 char limit
        var (response, _) = await PostCommentAsync(artifactId, squadId, oversizedBody);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostComment_InvalidGifUrl_Returns400()
    {
        var squadId = await CreateTestSquadAsync();
        var artifactId = await PublishTestArtifactAsync(squadId);

        var (response, _) = await PostCommentAsync(
            artifactId, squadId, "Has a bad gif", gifUrl: "not-a-url");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostComment_InvalidSquadId_Returns400()
    {
        var squadId = await CreateTestSquadAsync();
        var artifactId = await PublishTestArtifactAsync(squadId);

        // Use a random GUID that isn't an enlisted squad
        var (response, _) = await PostCommentAsync(
            artifactId, Guid.NewGuid(), "Comment from ghost squad");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostComment_InvalidArtifactId_Returns404()
    {
        var squadId = await CreateTestSquadAsync();

        var (response, _) = await PostCommentAsync(
            Guid.NewGuid(), squadId, "Comment on nothing");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostComment_InvalidParentCommentId_Returns400()
    {
        var squadId = await CreateTestSquadAsync();
        var artifactId = await PublishTestArtifactAsync(squadId);

        // Reply to a comment that doesn't exist
        var (response, _) = await PostCommentAsync(
            artifactId, squadId, "Replying to a ghost",
            parentCommentId: Guid.NewGuid());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostComment_ParentOnDifferentArtifact_Returns400()
    {
        var squadId = await CreateTestSquadAsync();
        var artifactId1 = await PublishTestArtifactAsync(squadId);
        var artifactId2 = await PublishTestArtifactAsync(squadId);

        // Post a comment on artifact 1
        var (_, parentId) = await PostCommentAsync(artifactId1, squadId, "Comment on artifact 1");
        Assert.NotNull(parentId);

        // Try to reply to that comment but on artifact 2
        var (response, _) = await PostCommentAsync(
            artifactId2, squadId, "Cross-artifact reply",
            parentCommentId: parentId);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ================================================================
    //  Comments — Edge Cases
    // ================================================================

    [Fact]
    public async Task GetComments_NoComments_ReturnsEmptyList()
    {
        var squadId = await CreateTestSquadAsync();
        var artifactId = await PublishTestArtifactAsync(squadId);

        var response = await _client.GetAsync($"/api/artifacts/{artifactId}/comments");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal(0, doc.RootElement.GetArrayLength());
    }

    [Fact]
    public async Task GetComment_InvalidId_Returns404()
    {
        var response = await _client.GetAsync($"/api/comments/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ================================================================
    //  GIF on Artifacts — Validation
    // ================================================================

    [Fact]
    public async Task PublishArtifact_WithInvalidGifUrl_Returns400()
    {
        var squadId = await CreateTestSquadAsync();

        var payload = new
        {
            SquadId = squadId,
            Title = "Artifact with bad gif",
            Summary = "Should fail validation",
            ArtifactType = "decision",
            GifUrl = "not-a-valid-url"
        };
        var response = await _client.PostAsync("/api/artifacts", JsonBody(payload));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
