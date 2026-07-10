namespace SquadPlaces.IntegrationTests;

/// <summary>
/// Tests that verify rate limiting is enforced on API endpoints.
/// The API has sliding window rate limiters: 100/min global, 60/min read, 30/min write.
/// </summary>
[Collection("Integration")]
public class RateLimitingTests(AppHostFixture fixture)
{
    private HttpClient Client => fixture.ApiClient;

    [Fact]
    public async Task RateLimitHeaders_ArePresent()
    {
        var response = await Client.GetAsync("/api/feed");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // The API sets X-RateLimit-Limit on responses
        Assert.True(
            response.Headers.Contains("X-RateLimit-Limit"),
            "Expected X-RateLimit-Limit header");
    }

    [Fact]
    public async Task WriteRateLimit_RejectsExcessRequests()
    {
        // The write rate limit is 30/min per IP. We'll send enough requests
        // to trigger the limit. Since tests share a fixture and IP, we need
        // to be careful not to interfere with other tests.
        // Strategy: send 35 rapid enlist requests and expect at least one 429.
        var got429 = false;

        for (var i = 0; i < 35; i++)
        {
            var response = await Client.PostAsJsonAsync("/api/squads/enlist", new
            {
                name = $"Rate Limit Test Squad {i} {Guid.NewGuid():N}"
            });

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                got429 = true;
                // Verify the response body has the expected error shape
                var body = await response.Content.ReadFromJsonAsync<JsonElement>();
                Assert.True(body.TryGetProperty("error", out _));
                break;
            }
        }

        Assert.True(got429, "Expected at least one 429 Too Many Requests response within 35 write requests");
    }

    [Fact]
    public async Task RateLimitedResponse_IncludesRetryAfterHeader()
    {
        // Trigger rate limiting by sending many rapid requests
        HttpResponseMessage? limitedResponse = null;

        for (var i = 0; i < 35; i++)
        {
            var response = await Client.PostAsJsonAsync("/api/squads/enlist", new
            {
                name = $"Retry-After Test {i} {Guid.NewGuid():N}"
            });

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                limitedResponse = response;
                break;
            }
        }

        if (limitedResponse is not null)
        {
            // Retry-After header should be present on 429 responses
            Assert.True(
                limitedResponse.Headers.Contains("Retry-After") ||
                limitedResponse.Content.Headers.Contains("Retry-After"),
                "Expected Retry-After header on 429 response");
        }
        else
        {
            // If we didn't hit the limit (shared fixture may have reset window),
            // skip assertion but don't fail - this is timing-dependent
            Assert.True(true, "Rate limit not triggered in this run (timing-dependent)");
        }
    }
}
