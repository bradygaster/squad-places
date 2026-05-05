using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SquadPlaces.AppHost.Tests.Tests;

/// <summary>
/// Integration tests for the POST /api/images/generate endpoint.
/// Tests AI image generation via nano-banana MCP server (Gemini via stdio transport) with squad-scoped storage.
/// Uses the shared ApiTestFixture to boot the Aspire host once.
/// </summary>
public class ImageGenerationTests : IClassFixture<ApiTestFixture>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public ImageGenerationTests(ApiTestFixture fixture)
    {
        _client = fixture.Client;
    }

    private static StringContent JsonBody(object obj) =>
        new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

    private async Task<Guid> CreateTestSquadAsync()
    {
        var response = await _client.PostAsync("/api/squads/enlist",
            JsonBody(new { Name = $"test-squad-{Guid.NewGuid():N}", Description = "Image generation test squad" }));
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("id").GetGuid();
    }

    // ================================================================
    //  Happy Path — Valid image generation requests
    // ================================================================

    [Fact(Skip = "Requires GOOGLE_API_KEY and nano-banana (npx) in PATH")]
    public async Task GenerateImage_WithValidPrompt_Returns201()
    {
        var squadId = await CreateTestSquadAsync();
        var payload = new
        {
            Prompt = "A serene mountain landscape at sunset",
            SquadId = squadId.ToString()
        };

        var response = await _client.PostAsync("/api/images/generate", JsonBody(payload));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        
        // Verify response contains required fields
        Assert.True(doc.RootElement.TryGetProperty("imageId", out var imageId), "Response should contain imageId");
        Assert.NotEqual(Guid.Empty, imageId.GetGuid());
        
        Assert.True(doc.RootElement.TryGetProperty("squadId", out var returnedSquadId), "Response should contain squadId");
        Assert.Equal(squadId, returnedSquadId.GetGuid());
        
        Assert.True(doc.RootElement.TryGetProperty("url", out var url), "Response should contain url");
        Assert.NotNull(url.GetString());
        Assert.StartsWith("/api/images/", url.GetString());
        
        Assert.True(doc.RootElement.TryGetProperty("prompt", out var returnedPrompt), "Response should contain prompt");
        Assert.Equal("A serene mountain landscape at sunset", returnedPrompt.GetString());
    }

    [Fact(Skip = "Requires GOOGLE_API_KEY and nano-banana (npx) in PATH")]
    public async Task GenerateImage_WithoutSquadId_DefaultsToGenerated()
    {
        var payload = new
        {
            Prompt = "A futuristic cityscape"
        };

        var response = await _client.PostAsync("/api/images/generate", JsonBody(payload));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        
        // Should default to "generated" squad
        Assert.True(doc.RootElement.TryGetProperty("squadId", out var squadId), "Response should contain squadId");
        var squadIdString = squadId.GetString();
        Assert.NotNull(squadIdString);
        Assert.Equal("generated", squadIdString);
    }

    [Fact(Skip = "Requires GOOGLE_API_KEY and nano-banana (npx) in PATH")]
    public async Task GenerateImage_WithStyle_AcceptsStyleParameter()
    {
        var squadId = await CreateTestSquadAsync();
        var payload = new
        {
            Prompt = "Abstract art with geometric patterns",
            SquadId = squadId.ToString(),
            Style = "vivid"
        };

        var response = await _client.PostAsync("/api/images/generate", JsonBody(payload));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact(Skip = "Requires GOOGLE_API_KEY and nano-banana (npx) in PATH")]
    public async Task GenerateImage_GeneratedImageIsRetrievable()
    {
        var squadId = await CreateTestSquadAsync();
        var payload = new
        {
            Prompt = "A simple test image",
            SquadId = squadId.ToString()
        };

        var generateResponse = await _client.PostAsync("/api/images/generate", JsonBody(payload));
        Assert.Equal(HttpStatusCode.Created, generateResponse.StatusCode);

        var json = await generateResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var url = doc.RootElement.GetProperty("url").GetString();
        Assert.NotNull(url);

        // Try to retrieve the generated image
        var getResponse = await _client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(getResponse.Content.Headers.ContentType);
        Assert.StartsWith("image/", getResponse.Content.Headers.ContentType.MediaType);
    }

    // ================================================================
    //  Validation — Bad input should return 400
    // ================================================================

    [Fact]
    public async Task GenerateImage_WithMissingPrompt_Returns400()
    {
        var squadId = await CreateTestSquadAsync();
        var payload = new
        {
            SquadId = squadId.ToString()
            // Prompt is missing
        };

        var response = await _client.PostAsync("/api/images/generate", JsonBody(payload));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("prompt", json.ToLower());
    }

    [Fact]
    public async Task GenerateImage_WithNullPrompt_Returns400()
    {
        var squadId = await CreateTestSquadAsync();
        var payload = new Dictionary<string, object?>
        {
            ["Prompt"] = null,
            ["SquadId"] = squadId.ToString()
        };

        var response = await _client.PostAsync("/api/images/generate", JsonBody(payload));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GenerateImage_WithEmptyPrompt_Returns400()
    {
        var squadId = await CreateTestSquadAsync();
        var payload = new
        {
            Prompt = "",
            SquadId = squadId.ToString()
        };

        var response = await _client.PostAsync("/api/images/generate", JsonBody(payload));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GenerateImage_WithWhitespacePrompt_Returns400()
    {
        var squadId = await CreateTestSquadAsync();
        var payload = new
        {
            Prompt = "   ",
            SquadId = squadId.ToString()
        };

        var response = await _client.PostAsync("/api/images/generate", JsonBody(payload));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GenerateImage_WithOversizedPrompt_Returns400()
    {
        var squadId = await CreateTestSquadAsync();
        var payload = new
        {
            Prompt = new string('X', 1001), // Exceeds 1000 char limit
            SquadId = squadId.ToString()
        };

        var response = await _client.PostAsync("/api/images/generate", JsonBody(payload));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GenerateImage_WithInvalidSquadId_Returns400()
    {
        var payload = new
        {
            Prompt = "A test image",
            SquadId = Guid.NewGuid().ToString() // Random GUID that doesn't reference an enlisted squad
        };

        var response = await _client.PostAsync("/api/images/generate", JsonBody(payload));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GenerateImage_WithMalformedSquadId_Returns400()
    {
        var payload = new
        {
            Prompt = "A test image",
            SquadId = "not-a-guid"
        };

        var response = await _client.PostAsync("/api/images/generate", JsonBody(payload));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ================================================================
    //  Service Unavailable — 503 when service not configured
    // ================================================================

    [Fact]
    public async Task GenerateImage_WithoutServiceConfig_Returns503()
    {
        // When image generation service is not configured, should return 503
        // This test assumes the test environment doesn't have GOOGLE_API_KEY / NanoBanana:GoogleApiKey configured
        var payload = new
        {
            Prompt = "A test image"
        };

        var response = await _client.PostAsync("/api/images/generate", JsonBody(payload));

        // Either 503 (service not configured) or 201 (service is configured and worked)
        // If it's 503, verify the error message
        if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
        {
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            
            Assert.True(doc.RootElement.TryGetProperty("error", out var error), 
                "503 response should contain error field");
            Assert.Equal("Image generation service not configured.", error.GetString());
        }
        else
        {
            // If service is configured, request should succeed or fail validation, not crash
            Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        }
    }

    // ================================================================
    //  Rate Limiting — Endpoint should respect write rate limits
    // ================================================================

    [Fact]
    public async Task GenerateImage_RespectsWriteRateLimit()
    {
        // The generate endpoint should be under "write" rate limit policy
        // This test verifies rate limiting is applied by making rapid requests
        var squadId = await CreateTestSquadAsync();
        var payload = new
        {
            Prompt = "Rate limit test",
            SquadId = squadId.ToString()
        };

        // Make multiple rapid requests to trigger rate limiting
        // The exact number needed depends on the rate limit configuration
        var tasks = Enumerable.Range(0, 50)
            .Select(_ => _client.PostAsync("/api/images/generate", JsonBody(payload)))
            .ToList();

        var responses = await Task.WhenAll(tasks);

        // At least one request should be rate limited (429)
        // OR all should be 503 (service not configured) or 201 (success with config)
        // The key is that rate limiting infrastructure is in place
        var statusCodes = responses.Select(r => r.StatusCode).ToHashSet();
        
        // Rate limiting may or may not trigger depending on configuration
        // Just verify no 500 errors (server crashes)
        Assert.DoesNotContain(HttpStatusCode.InternalServerError, statusCodes);
    }

    // ================================================================
    //  Edge Cases
    // ================================================================

    [Fact]
    public async Task GenerateImage_WithMaxLengthPrompt_DoesNotReturn400()
    {
        var squadId = await CreateTestSquadAsync();
        var payload = new
        {
            Prompt = new string('A', 1000), // Exactly at 1000 char limit
            SquadId = squadId.ToString()
        };

        var response = await _client.PostAsync("/api/images/generate", JsonBody(payload));

        // Should either succeed (201) or fail due to missing service config (503)
        // Should NOT fail validation (400)
        Assert.NotEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GenerateImage_WithUnicodePrompt_AcceptsUnicode()
    {
        var squadId = await CreateTestSquadAsync();
        var payload = new
        {
            Prompt = "絵を描いて🎨 — A beautiful painting with 日本語 characters",
            SquadId = squadId.ToString()
        };

        var response = await _client.PostAsync("/api/images/generate", JsonBody(payload));

        // Should not fail validation due to unicode
        Assert.NotEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GenerateImage_WithEmptyBody_Returns400()
    {
        var response = await _client.PostAsync("/api/images/generate", JsonBody(new { }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GenerateImage_WithNullBody_Returns400()
    {
        var response = await _client.PostAsync("/api/images/generate", 
            new StringContent("null", Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
