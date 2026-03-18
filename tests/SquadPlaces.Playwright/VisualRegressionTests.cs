namespace SquadPlaces.Playwright;

/// <summary>
/// Visual regression tests — captures screenshots of key pages at a consistent
/// viewport for baseline comparison and documentation use.
/// </summary>
[TestFixture]
public class VisualRegressionTests : PlaywrightTestBase
{
    private const int ViewportWidth = 1280;
    private const int ViewportHeight = 720;

    [SetUp]
    public async Task SetViewport()
    {
        await Page.SetViewportSizeAsync(ViewportWidth, ViewportHeight);
        Directory.CreateDirectory(ScreenshotDir);
    }

    [Test]
    public async Task Screenshot_FeedPage()
    {
        await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });

        await Expect(Page.Locator("h1", new() { HasTextString = "Discovery Feed" })).ToBeVisibleAsync();

        await Page.ScreenshotAsync(new()
        {
            Path = Path.Combine(ScreenshotDir, "feed-page.png"),
            FullPage = true
        });

        Assert.Pass("Feed page screenshot captured");
    }

    [Test]
    public async Task Screenshot_SquadsPage()
    {
        await Page.GotoAsync($"{BaseUrl}/Squads", new() { WaitUntil = WaitUntilState.NetworkIdle });

        await Expect(Page.Locator("h1", new() { HasTextString = "Squads" })).ToBeVisibleAsync();

        await Page.ScreenshotAsync(new()
        {
            Path = Path.Combine(ScreenshotDir, "squads-page.png"),
            FullPage = true
        });

        Assert.Pass("Squads page screenshot captured");
    }

    [Test]
    public async Task Screenshot_SquadDetail()
    {
        await Page.GotoAsync($"{BaseUrl}/Squads", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.Locator(".Box-row a.Link--primary").First.ClickAsync();
        await Page.WaitForURLAsync(new System.Text.RegularExpressions.Regex(@"/Squads/Detail\?id="));

        await Page.ScreenshotAsync(new()
        {
            Path = Path.Combine(ScreenshotDir, "squad-detail.png"),
            FullPage = true
        });

        Assert.Pass("Squad detail screenshot captured");
    }

    [Test]
    public async Task Screenshot_ArtifactDetail()
    {
        await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.Locator(".feed-item a:has(h3)").First.ClickAsync();
        await Page.WaitForURLAsync(new System.Text.RegularExpressions.Regex(@"/Artifacts/Detail\?id="));

        await Page.ScreenshotAsync(new()
        {
            Path = Path.Combine(ScreenshotDir, "artifact-detail.png"),
            FullPage = true
        });

        Assert.Pass("Artifact detail screenshot captured");
    }

    [Test]
    public async Task Screenshot_SearchPage()
    {
        await Page.GotoAsync($"{BaseUrl}/Search?q=test", new() { WaitUntil = WaitUntilState.NetworkIdle });

        await Page.ScreenshotAsync(new()
        {
            Path = Path.Combine(ScreenshotDir, "search-page.png"),
            FullPage = true
        });

        Assert.Pass("Search page screenshot captured");
    }

    [Test]
    public async Task Screenshot_FeedPage_Mobile()
    {
        await Page.SetViewportSizeAsync(375, 812);
        await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });

        await Expect(Page.Locator("h1", new() { HasTextString = "Discovery Feed" })).ToBeVisibleAsync();

        await Page.ScreenshotAsync(new()
        {
            Path = Path.Combine(ScreenshotDir, "feed-page-mobile.png"),
            FullPage = true
        });

        Assert.Pass("Feed page mobile screenshot captured");
    }
}
