namespace SquadPlaces.Playwright;

/// <summary>
/// Tests for the Squads list and detail pages.
/// </summary>
[TestFixture]
public class SquadsPageTests : PlaywrightTestBase
{
    [Test]
    public async Task SquadsPage_LoadsWithTitle()
    {
        await Page.GotoAsync($"{BaseUrl}/Squads");
        await Expect(Page).ToHaveTitleAsync(new System.Text.RegularExpressions.Regex("Squads.*Squad Places"));
    }

    [Test]
    public async Task SquadsPage_ShowsHeading()
    {
        await Page.GotoAsync($"{BaseUrl}/Squads");
        var heading = Page.Locator("h1", new() { HasTextString = "Squads" });
        await Expect(heading).ToBeVisibleAsync();
    }

    [Test]
    public async Task SquadsPage_ShowsEnlistedSquads()
    {
        await Page.GotoAsync($"{BaseUrl}/Squads");

        // Expect at least one squad listed
        var squadLinks = Page.Locator(".Box-row a.Link--primary");
        await Expect(squadLinks.First).ToBeVisibleAsync();
    }

    [Test]
    public async Task SquadsPage_NavigateToSquadDetail()
    {
        await Page.GotoAsync($"{BaseUrl}/Squads");

        // Click the first squad link
        var firstSquadLink = Page.Locator(".Box-row a.Link--primary").First;
        var squadName = await firstSquadLink.InnerTextAsync();
        await firstSquadLink.ClickAsync();

        // Should navigate to squad detail page
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"/Squads/Detail\?id="));

        // Squad name should appear in the heading area
        var heading = Page.Locator(".Subhead-heading");
        await Expect(heading).ToContainTextAsync(squadName);
    }

    [Test]
    public async Task SquadDetail_ShowsArtifactCount()
    {
        await Page.GotoAsync($"{BaseUrl}/Squads");
        await Page.Locator(".Box-row a.Link--primary").First.ClickAsync();

        // "N artifacts" appears once in the detail stats area
        var artifactStat = Page.Locator("span.text-bold + span", new() { HasTextString = "artifacts" });
        await Expect(artifactStat).ToBeVisibleAsync();
    }

    [Test]
    public async Task SquadDetail_ShowsEnlistedDate()
    {
        await Page.GotoAsync($"{BaseUrl}/Squads");
        await Page.Locator(".Box-row a.Link--primary").First.ClickAsync();

        // "Enlisted MMM d, yyyy" appears in the detail stats area
        var enlistedText = Page.Locator(".color-fg-muted", new() { HasTextRegex = new System.Text.RegularExpressions.Regex(@"Enlisted \w+ \d+, \d{4}") });
        await Expect(enlistedText.First).ToBeVisibleAsync();
    }
}
