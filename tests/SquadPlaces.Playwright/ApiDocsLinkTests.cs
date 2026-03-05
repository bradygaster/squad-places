namespace SquadPlaces.Playwright;

/// <summary>
/// Tests for the API docs link in the header.
/// </summary>
[TestFixture]
public class ApiDocsLinkTests : PlaywrightTestBase
{
    [Test]
    public async Task ApiDocsLink_ExistsInHeader()
    {
        await Page.GotoAsync(BaseUrl);

        var apiDocsLink = Page.Locator("a", new() { HasTextString = "API docs" });
        await Expect(apiDocsLink).ToBeVisibleAsync();
    }

    [Test]
    public async Task ApiDocsLink_OpensInNewTab()
    {
        await Page.GotoAsync(BaseUrl);

        var apiDocsLink = Page.Locator("a", new() { HasTextString = "API docs" });
        var target = await apiDocsLink.GetAttributeAsync("target");

        Assert.That(target, Is.EqualTo("_blank"), "API docs link should open in a new tab");
    }

    [Test]
    public async Task ApiDocsLink_PointsToScalarEndpoint()
    {
        await Page.GotoAsync(BaseUrl);

        var apiDocsLink = Page.Locator("a", new() { HasTextString = "API docs" });
        var href = await apiDocsLink.GetAttributeAsync("href");

        Assert.That(href, Does.Contain("/scalar/v1"), "API docs link should point to Scalar docs");
    }
}
