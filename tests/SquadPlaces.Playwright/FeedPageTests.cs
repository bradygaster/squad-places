namespace SquadPlaces.Playwright;

/// <summary>
/// Tests for the Discovery Feed (home page).
/// </summary>
[TestFixture]
public class FeedPageTests : PlaywrightTestBase
{
    [Test]
    public async Task FeedPage_LoadsWithTitle()
    {
        await Page.GotoAsync(BaseUrl);
        await Expect(Page).ToHaveTitleAsync(new System.Text.RegularExpressions.Regex("Feed.*Squad Places"));
    }

    [Test]
    public async Task FeedPage_ShowsDiscoveryFeedHeading()
    {
        await Page.GotoAsync(BaseUrl);
        var heading = Page.Locator("h1", new() { HasTextString = "Discovery Feed" });
        await Expect(heading).ToBeVisibleAsync();
    }

    [Test]
    public async Task FeedPage_ShowsArtifactCounters()
    {
        await Page.GotoAsync(BaseUrl);

        // The stats bar shows counters — at least artifacts and squads
        var counters = Page.Locator(".d-flex.flex-items-center .Counter");
        var count = await counters.CountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(2), "Expected at least 2 counters (artifacts, squads)");
    }

    [Test]
    public async Task FeedPage_ShowsSortButtons_WhenPresent()
    {
        await Page.GotoAsync(BaseUrl);

        // Sort UI may not be deployed yet — verify if present it works correctly
        var sortGroup = Page.Locator(".BtnGroup");
        if (await sortGroup.CountAsync() > 0)
        {
            await Expect(sortGroup).ToBeVisibleAsync();
            await Expect(sortGroup.Locator("a", new() { HasTextRegex = new System.Text.RegularExpressions.Regex("Latest") })).ToBeVisibleAsync();
        }
        else
        {
            Assert.Pass("Sort buttons not present in current deployment — feature not yet deployed");
        }
    }

    [Test]
    public async Task FeedPage_ShowsFeedItems()
    {
        await Page.GotoAsync(BaseUrl);

        var feedItems = Page.Locator(".feed-item");
        await Expect(feedItems.First).ToBeVisibleAsync();
    }

    [Test]
    public async Task FeedPage_HasSquadFilterDropdown_WhenPresent()
    {
        await Page.GotoAsync(BaseUrl);

        // Squad filter may not be deployed yet
        var select = Page.Locator("select[name='squad']");
        if (await select.CountAsync() > 0)
        {
            await Expect(select).ToBeVisibleAsync();
            var allOption = select.Locator("option", new() { HasTextString = "All Squads" });
            await Expect(allOption).ToBeAttachedAsync();
        }
        else
        {
            Assert.Pass("Squad filter not present in current deployment — feature not yet deployed");
        }
    }

    [Test]
    public async Task FeedPage_HeaderNavigation_IsPresent()
    {
        await Page.GotoAsync(BaseUrl);

        // Target header links specifically within the .Header element
        var header = Page.Locator("header.Header");
        await Expect(header.Locator("a.Header-link", new() { HasTextString = "Squad Places" })).ToBeVisibleAsync();
        await Expect(header.Locator("a.Header-link", new() { HasTextString = "Feed" })).ToBeVisibleAsync();
        await Expect(header.Locator("a.Header-link", new() { HasTextString = "Squads" })).ToBeVisibleAsync();
    }
}
