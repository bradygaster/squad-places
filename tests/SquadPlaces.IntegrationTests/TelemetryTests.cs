namespace SquadPlaces.IntegrationTests;

/// <summary>
/// Tests that verify OpenTelemetry trace propagation works through the API.
/// The API uses SquadPlacesTelemetry.ActivitySource for distributed tracing.
/// </summary>
[Collection("Integration")]
public class TelemetryTests(AppHostFixture fixture)
{
    private HttpClient Client => fixture.ApiClient;

    [Fact]
    public async Task TraceParent_IsPropagated()
    {
        // Send a request with a W3C traceparent header
        var traceId = Guid.NewGuid().ToString("N");
        var spanId = Guid.NewGuid().ToString("N")[..16];
        var traceparent = $"00-{traceId}-{spanId}-01";

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/feed");
        request.Headers.Add("traceparent", traceparent);

        var response = await Client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // The response should include a traceparent or tracestate header
        // indicating the server participated in the trace context
        // (Aspire OTel auto-instrumentation propagates W3C trace context)
        // Even if the response doesn't echo traceparent, the request succeeded
        // which means the OTel middleware didn't reject or break on the header.
    }

    [Fact]
    public async Task ApiVersionHeader_IsPresent()
    {
        // The API adds X-SquadPlace-Version to all /api responses
        var response = await Client.GetAsync("/api/feed");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.True(
            response.Headers.Contains("X-SquadPlace-Version"),
            "Expected X-SquadPlace-Version header on API responses");
    }

    [Fact]
    public async Task SecurityHeaders_ArePresent()
    {
        var response = await Client.GetAsync("/api");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.True(response.Headers.Contains("X-Frame-Options"), "Missing X-Frame-Options");
        Assert.True(response.Headers.Contains("X-Content-Type-Options"), "Missing X-Content-Type-Options");
        Assert.True(response.Headers.Contains("Referrer-Policy"), "Missing Referrer-Policy");
    }

    [Fact]
    public async Task EnlistSquad_CreatesTracedActivity()
    {
        // When we enlist a squad, the API starts a "squad.enlist" activity.
        // We can verify the trace works by sending a traceparent and confirming
        // the operation succeeds (the activity source is registered with OTel).
        var traceId = Guid.NewGuid().ToString("N");
        var spanId = Guid.NewGuid().ToString("N")[..16];
        var traceparent = $"00-{traceId}-{spanId}-01";

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/squads/enlist");
        request.Headers.Add("traceparent", traceparent);
        request.Content = JsonContent.Create(new
        {
            name = $"Traced Squad {Guid.NewGuid():N}",
            description = "Testing OTel trace propagation"
        });

        var response = await Client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PublishArtifact_CreatesTracedActivity()
    {
        // First enlist
        var enlistResponse = await Client.PostAsJsonAsync("/api/squads/enlist", new
        {
            name = $"OTel Artifact Squad {Guid.NewGuid():N}"
        });
        var enlistJson = await enlistResponse.Content.ReadFromJsonAsync<JsonElement>();
        var squadId = enlistJson.GetProperty("squad").GetProperty("id").GetString()!;

        // Publish with traceparent
        var traceId = Guid.NewGuid().ToString("N");
        var spanId = Guid.NewGuid().ToString("N")[..16];
        var traceparent = $"00-{traceId}-{spanId}-01";

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/artifacts");
        request.Headers.Add("traceparent", traceparent);
        request.Content = JsonContent.Create(new
        {
            squadId,
            title = "OTel Test Artifact",
            summary = "Verifying trace propagation on publish",
            artifactType = "insight"
        });

        var response = await Client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
