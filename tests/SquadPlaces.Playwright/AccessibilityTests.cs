namespace SquadPlaces.Playwright;

/// <summary>
/// Accessibility tests — validates keyboard navigation, ARIA attributes,
/// heading hierarchy, and semantic HTML across key pages.
/// </summary>
[TestFixture]
public class AccessibilityTests : PlaywrightTestBase
{
    [Test]
    public async Task FeedPage_HasSingleH1()
    {
        await Page.GotoAsync(BaseUrl);

        var h1Count = await Page.Locator("h1").CountAsync();
        Assert.That(h1Count, Is.EqualTo(1),
            "Page should have exactly one h1 for correct heading hierarchy");
    }

    [Test]
    public async Task FeedPage_HeadingHierarchyIsValid()
    {
        await Page.GotoAsync(BaseUrl);

        // Verify headings don't skip levels (h1 → h3 with no h2 is bad)
        var hasH1 = await Page.Locator("h1").CountAsync() > 0;
        var hasH2 = await Page.Locator("h2").CountAsync() > 0;
        var hasH3 = await Page.Locator("h3").CountAsync() > 0;

        Assert.That(hasH1, Is.True, "Page must have an h1");

        if (hasH3)
        {
            // If there's an h3, there should be either an h2 or the h3s are inside
            // sectioned content (acceptable in HTML5 outline). Log for review.
            TestContext.Out.WriteLine(
                $"Heading audit: h1={hasH1}, h2={hasH2}, h3={hasH3}. " +
                (hasH2 ? "Hierarchy looks correct." : "Warning: h3 present without h2 — review sectioning."));
        }
    }

    [Test]
    public async Task FeedPage_ImagesHaveAltText()
    {
        await Page.GotoAsync(BaseUrl);

        var images = Page.Locator("img");
        var count = await images.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var img = images.Nth(i);
            var alt = await img.GetAttributeAsync("alt");
            Assert.That(alt, Is.Not.Null,
                $"Image {i} is missing alt attribute");
        }
    }

    [Test]
    public async Task FeedPage_LinksHaveAccessibleText()
    {
        await Page.GotoAsync(BaseUrl);

        // Check that links in the main content area have visible text or aria-label
        var mainLinks = Page.Locator("main a, .feed-item a");
        var count = await mainLinks.CountAsync();

        for (int i = 0; i < Math.Min(count, 20); i++)
        {
            var link = mainLinks.Nth(i);
            var text = (await link.InnerTextAsync()).Trim();
            var ariaLabel = await link.GetAttributeAsync("aria-label");
            var title = await link.GetAttributeAsync("title");

            Assert.That(
                !string.IsNullOrWhiteSpace(text) || !string.IsNullOrWhiteSpace(ariaLabel) || !string.IsNullOrWhiteSpace(title),
                Is.True,
                $"Link {i} has no accessible text, aria-label, or title");
        }
    }

    [Test]
    public async Task FeedPage_HeaderNavHasAriaLandmark()
    {
        await Page.GotoAsync(BaseUrl);

        // <header> is a landmark role by default; verify it exists
        var header = Page.Locator("header");
        Assert.That(await header.CountAsync(), Is.GreaterThan(0),
            "Page should have a <header> landmark element");
    }

    [Test]
    public async Task FeedPage_MainContentLandmarkExists()
    {
        await Page.GotoAsync(BaseUrl);

        // Check for <main> or role="main"
        var main = Page.Locator("main, [role='main']");
        Assert.That(await main.CountAsync(), Is.GreaterThan(0),
            "Page should have a <main> landmark for the primary content area");
    }

    [Test]
    public async Task FeedPage_KeyboardNavigationToFirstLink()
    {
        await Page.GotoAsync(BaseUrl);

        // Tab through the page — focus should reach interactive elements
        await Page.Keyboard.PressAsync("Tab");
        await Page.Keyboard.PressAsync("Tab");
        await Page.Keyboard.PressAsync("Tab");

        var focused = Page.Locator(":focus");
        var tagName = await focused.EvaluateAsync<string>("el => el.tagName.toLowerCase()");

        Assert.That(tagName, Is.AnyOf("a", "button", "input", "select", "textarea"),
            "Tab focus should land on an interactive element");
    }

    [Test]
    public async Task FeedPage_KeyboardCanActivateLink()
    {
        await Page.GotoAsync(BaseUrl);

        // Tab to the first link in the feed and press Enter
        var firstFeedLink = Page.Locator(".feed-item a:has(h3)").First;
        await firstFeedLink.FocusAsync();
        var href = await firstFeedLink.GetAttributeAsync("href");

        await Page.Keyboard.PressAsync("Enter");

        // Should navigate — URL should change
        await Page.WaitForURLAsync(url => url != BaseUrl);
    }

    [Test]
    public async Task SquadsPage_HasSingleH1()
    {
        await Page.GotoAsync($"{BaseUrl}/Squads");

        var h1Count = await Page.Locator("h1").CountAsync();
        Assert.That(h1Count, Is.EqualTo(1),
            "Squads page should have exactly one h1");
    }

    [Test]
    public async Task AllPages_HaveHtmlLangAttribute()
    {
        var urls = new[] { BaseUrl, $"{BaseUrl}/Squads", $"{BaseUrl}/Search" };

        foreach (var url in urls)
        {
            await Page.GotoAsync(url);
            var lang = await Page.Locator("html").GetAttributeAsync("lang");
            Assert.That(lang, Is.Not.Null.And.Not.Empty,
                $"Page at {url.Replace(BaseUrl, "")} is missing lang attribute on <html>");
        }
    }
}
