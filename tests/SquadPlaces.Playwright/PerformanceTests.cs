namespace SquadPlaces.Playwright;

/// <summary>
/// Performance smoke tests — validates page load times, console errors,
/// and basic performance metrics for key pages.
/// </summary>
[TestFixture]
public class PerformanceTests : PlaywrightTestBase
{
    private const int MaxLoadTimeMs = 3000;

    [Test]
    public async Task FeedPage_LoadsUnder3Seconds()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        stopwatch.Stop();

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Status, Is.LessThan(400), "Feed page should return a success status");
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(MaxLoadTimeMs),
            $"Feed page took {stopwatch.ElapsedMilliseconds}ms to load (limit: {MaxLoadTimeMs}ms)");
    }

    [Test]
    public async Task SquadsPage_LoadsUnder3Seconds()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Page.GotoAsync($"{BaseUrl}/Squads", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        stopwatch.Stop();

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Status, Is.LessThan(400));
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(MaxLoadTimeMs),
            $"Squads page took {stopwatch.ElapsedMilliseconds}ms to load");
    }

    [Test]
    public async Task SearchPage_LoadsUnder3Seconds()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await Page.GotoAsync($"{BaseUrl}/Search?q=test", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        stopwatch.Stop();

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Status, Is.LessThan(400));
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(MaxLoadTimeMs),
            $"Search page took {stopwatch.ElapsedMilliseconds}ms to load");
    }

    [Test]
    public async Task FeedPage_NoConsoleErrors()
    {
        var consoleErrors = new List<string>();
        Page.Console += (_, msg) =>
        {
            if (msg.Type == "error")
                consoleErrors.Add(msg.Text);
        };

        await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });

        Assert.That(consoleErrors, Is.Empty,
            $"Console errors detected on feed page:\n{string.Join("\n", consoleErrors)}");
    }

    [Test]
    public async Task SquadsPage_NoConsoleErrors()
    {
        var consoleErrors = new List<string>();
        Page.Console += (_, msg) =>
        {
            if (msg.Type == "error")
                consoleErrors.Add(msg.Text);
        };

        await Page.GotoAsync($"{BaseUrl}/Squads", new() { WaitUntil = WaitUntilState.NetworkIdle });

        Assert.That(consoleErrors, Is.Empty,
            $"Console errors detected on squads page:\n{string.Join("\n", consoleErrors)}");
    }

    [Test]
    public async Task FeedPage_NoFailedNetworkRequests()
    {
        var failedRequests = new List<string>();
        Page.RequestFailed += (_, request) =>
        {
            failedRequests.Add($"{request.Method} {request.Url} - {request.Failure}");
        };

        await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });

        Assert.That(failedRequests, Is.Empty,
            $"Failed network requests on feed page:\n{string.Join("\n", failedRequests)}");
    }

    [Test]
    public async Task FeedPage_FirstContentfulPaint_IsReasonable()
    {
        await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.DOMContentLoaded });

        // Use Performance API to check FCP
        var fcp = await Page.EvaluateAsync<double?>(@"() => {
            const entries = performance.getEntriesByName('first-contentful-paint');
            return entries.length > 0 ? entries[0].startTime : null;
        }");

        if (fcp.HasValue)
        {
            TestContext.Out.WriteLine($"First Contentful Paint: {fcp.Value:F0}ms");
            Assert.That(fcp.Value, Is.LessThan(2000),
                $"FCP was {fcp.Value:F0}ms — should be under 2000ms");
        }
        else
        {
            // FCP may not be available in all headless contexts — don't fail
            TestContext.Out.WriteLine("FCP metric not available in this browser context");
            Assert.Pass("FCP metric not available — skipping");
        }
    }

    [Test]
    public async Task AllPages_ReturnSuccessStatus()
    {
        var urls = new[]
        {
            (BaseUrl, "Feed"),
            ($"{BaseUrl}/Squads", "Squads"),
            ($"{BaseUrl}/Search", "Search"),
        };

        foreach (var (url, name) in urls)
        {
            var response = await Page.GotoAsync(url);
            Assert.That(response, Is.Not.Null, $"{name} page returned null response");
            Assert.That(response!.Status, Is.LessThan(400),
                $"{name} page returned status {response.Status}");
        }
    }
}
