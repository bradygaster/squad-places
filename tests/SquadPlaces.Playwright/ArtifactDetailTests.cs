namespace SquadPlaces.Playwright;

/// <summary>
/// Tests for artifact detail pages and comment threads.
/// </summary>
[TestFixture]
public class ArtifactDetailTests : PlaywrightTestBase
{
    // Target the artifact title link (the one wrapping an h3), not the squad link
    private ILocator FirstArtifactTitleLink => Page.Locator(".feed-item a:has(h3)").First;

    private async Task NavigateToFirstArtifactDetail()
    {
        await Page.GotoAsync(BaseUrl);
        await FirstArtifactTitleLink.ClickAsync();
        // Wait for the artifact detail page to load (either via htmx swap or full navigation)
        await Page.WaitForURLAsync(new System.Text.RegularExpressions.Regex(@"/Artifacts/Detail\?id="));
    }

    [Test]
    public async Task ArtifactDetail_NavigateFromFeed()
    {
        await NavigateToFirstArtifactDetail();
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(@"/Artifacts/Detail\?id="));
    }

    [Test]
    public async Task ArtifactDetail_ShowsTitle()
    {
        await Page.GotoAsync(BaseUrl);

        var titleText = await Page.Locator(".feed-item h3").First.InnerTextAsync();
        await FirstArtifactTitleLink.ClickAsync();
        await Page.WaitForURLAsync(new System.Text.RegularExpressions.Regex(@"/Artifacts/Detail"));

        var detailTitle = Page.Locator("h1");
        await Expect(detailTitle).ToContainTextAsync(titleText);
    }

    [Test]
    public async Task ArtifactDetail_ShowsBackToFeedLink()
    {
        await NavigateToFirstArtifactDetail();
        await Expect(Page.Locator("nav a", new() { HasTextRegex = new System.Text.RegularExpressions.Regex("Back to feed") })).ToBeVisibleAsync();
    }

    [Test]
    public async Task ArtifactDetail_ShowsArtifactType()
    {
        await NavigateToFirstArtifactDetail();

        // The detail Box has exactly one artifact-type badge
        var typeBadge = Page.Locator(".Box .artifact-type");
        await Expect(typeBadge).ToBeVisibleAsync();
    }

    [Test]
    public async Task ArtifactDetail_ShowsPublishedBySquad()
    {
        await NavigateToFirstArtifactDetail();
        await Expect(Page.Locator("text=Published by")).ToBeVisibleAsync();
    }

    [Test]
    public async Task ArtifactDetail_ShowsCommentsSection()
    {
        await NavigateToFirstArtifactDetail();
        await Expect(Page.Locator("h4", new() { HasTextRegex = new System.Text.RegularExpressions.Regex("Comments") })).ToBeVisibleAsync();
    }

    [Test]
    public async Task ArtifactDetail_ShowsAdoptionCount()
    {
        await NavigateToFirstArtifactDetail();
        await Expect(Page.Locator("text=Adopted by")).ToBeVisibleAsync();
    }

    [Test]
    public async Task ArtifactDetail_BackToFeedNavigates()
    {
        await NavigateToFirstArtifactDetail();

        await Page.Locator("nav a", new() { HasTextRegex = new System.Text.RegularExpressions.Regex("Back to feed") }).ClickAsync();

        await Expect(Page.Locator("h1", new() { HasTextString = "Discovery Feed" })).ToBeVisibleAsync();
    }
}
