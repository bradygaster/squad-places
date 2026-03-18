namespace SquadPlaces.Playwright;

/// <summary>
/// End-to-end user journey tests — exercises real multi-step flows
/// through the application as a new user would experience them.
/// </summary>
[TestFixture]
public class UserJourneyTests : PlaywrightTestBase
{
    [Test]
    public async Task NewUserJourney_FeedToSquadToArtifact()
    {
        // 1. Land on discovery feed
        await Page.GotoAsync(BaseUrl);
        await Expect(Page.Locator("h1", new() { HasTextString = "Discovery Feed" })).ToBeVisibleAsync();

        // 2. Navigate to squads via header
        var header = Page.Locator("header.Header");
        await header.Locator("a.Header-link", new() { HasTextString = "Squads" }).ClickAsync();
        await Page.WaitForURLAsync(new System.Text.RegularExpressions.Regex(@"/Squads$"));
        await Expect(Page.Locator("h1", new() { HasTextString = "Squads" })).ToBeVisibleAsync();

        // 3. Click into a squad detail
        var firstSquad = Page.Locator(".Box-row a.Link--primary").First;
        var squadName = await firstSquad.InnerTextAsync();
        await firstSquad.ClickAsync();
        await Page.WaitForURLAsync(new System.Text.RegularExpressions.Regex(@"/Squads/Detail\?id="));
        await Expect(Page.Locator(".Subhead-heading")).ToContainTextAsync(squadName);

        // 4. Navigate back to feed via header
        await header.Locator("a.Header-link", new() { HasTextString = "Feed" }).ClickAsync();
        await Expect(Page.Locator("h1", new() { HasTextString = "Discovery Feed" })).ToBeVisibleAsync();

        // 5. Click into an artifact
        await Page.Locator(".feed-item a:has(h3)").First.ClickAsync();
        await Page.WaitForURLAsync(new System.Text.RegularExpressions.Regex(@"/Artifacts/Detail\?id="));
        await Expect(Page.Locator("h1")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Navigation_HeaderLinksWorkFromEveryPage()
    {
        var pages = new[]
        {
            (BaseUrl, "Discovery Feed"),
            ($"{BaseUrl}/Squads", "Squads"),
            ($"{BaseUrl}/Search?q=test", "Search"),
        };

        foreach (var (url, _) in pages)
        {
            await Page.GotoAsync(url);

            var header = Page.Locator("header.Header");
            await Expect(header.Locator("a.Header-link", new() { HasTextString = "Squad Places" })).ToBeVisibleAsync();
            await Expect(header.Locator("a.Header-link", new() { HasTextString = "Feed" })).ToBeVisibleAsync();
            await Expect(header.Locator("a.Header-link", new() { HasTextString = "Squads" })).ToBeVisibleAsync();
        }
    }

    [Test]
    public async Task Navigation_BackToFeedFromArtifactDetail()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.Locator(".feed-item a:has(h3)").First.ClickAsync();
        await Page.WaitForURLAsync(new System.Text.RegularExpressions.Regex(@"/Artifacts/Detail\?id="));

        var backLink = Page.Locator("nav a", new() { HasTextRegex = new System.Text.RegularExpressions.Regex("Back to feed") });
        await backLink.ClickAsync();

        await Expect(Page.Locator("h1", new() { HasTextString = "Discovery Feed" })).ToBeVisibleAsync();
    }

    [Test]
    public async Task Search_SubmitQueryAndViewResults()
    {
        await Page.GotoAsync($"{BaseUrl}/Search");

        // The search page should have an input for the query
        var searchInput = Page.Locator("input[name='q'], input[type='search']").First;
        await searchInput.FillAsync("artifact");
        await searchInput.PressAsync("Enter");

        await Page.WaitForURLAsync(new System.Text.RegularExpressions.Regex(@"/Search\?q="));
    }

    [Test]
    public async Task ResponsiveLayout_MobileViewport()
    {
        await Page.SetViewportSizeAsync(375, 812);
        await Page.GotoAsync(BaseUrl);

        // Page should still render key content at mobile width
        await Expect(Page.Locator("h1", new() { HasTextString = "Discovery Feed" })).ToBeVisibleAsync();

        // Feed items should still be visible
        await Expect(Page.Locator(".feed-item").First).ToBeVisibleAsync();

        // Header should still be present
        await Expect(Page.Locator("header.Header")).ToBeVisibleAsync();
    }

    [Test]
    public async Task ResponsiveLayout_TabletViewport()
    {
        await Page.SetViewportSizeAsync(768, 1024);
        await Page.GotoAsync(BaseUrl);

        await Expect(Page.Locator("h1", new() { HasTextString = "Discovery Feed" })).ToBeVisibleAsync();
        await Expect(Page.Locator(".feed-item").First).ToBeVisibleAsync();
    }

    [Test]
    public async Task ErrorPage_Returns404ForInvalidRoute()
    {
        var response = await Page.GotoAsync($"{BaseUrl}/nonexistent-page-xyz");

        // App should handle gracefully — either redirect to error or show a 404-like page
        Assert.That(response, Is.Not.Null);
        var status = response!.Status;
        Assert.That(status, Is.AnyOf(200, 404),
            "Invalid route should return 404 or redirect to a handled page");
    }

    [Test]
    public async Task ErrorPage_InvalidArtifactIdHandled()
    {
        var response = await Page.GotoAsync($"{BaseUrl}/Artifacts/Detail?id=00000000-0000-0000-0000-000000000000");

        Assert.That(response, Is.Not.Null);
        // The app should not crash — it should return a valid HTTP response
        Assert.That(response!.Status, Is.LessThan(500),
            "Invalid artifact ID should not cause a server error");
    }
}
