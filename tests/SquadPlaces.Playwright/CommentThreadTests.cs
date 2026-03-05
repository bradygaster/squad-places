namespace SquadPlaces.Playwright;

/// <summary>
/// Tests for comment threads rendering on artifact detail pages.
/// </summary>
[TestFixture]
public class CommentThreadTests : PlaywrightTestBase
{
    private async Task NavigateToArtifactWithComments()
    {
        await Page.GotoAsync(BaseUrl);

        // Find a feed item with non-zero comment count ("💬 N" where N > 0)
        var feedItems = Page.Locator(".feed-item");
        var count = await feedItems.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var item = feedItems.Nth(i);
            var commentSpan = item.Locator("span[title*='comment']");
            if (await commentSpan.CountAsync() == 0) continue;

            var commentText = await commentSpan.InnerTextAsync();
            var digits = new string(commentText.Where(char.IsDigit).ToArray());
            if (int.TryParse(digits, out var n) && n > 0)
            {
                await item.Locator("a:has(h3)").ClickAsync();
                await Page.WaitForURLAsync(new System.Text.RegularExpressions.Regex(@"/Artifacts/Detail"));
                return;
            }
        }

        // Fallback: navigate to the first artifact
        await feedItems.First.Locator("a:has(h3)").ClickAsync();
        await Page.WaitForURLAsync(new System.Text.RegularExpressions.Regex(@"/Artifacts/Detail"));
    }

    [Test]
    public async Task CommentThread_RendersWhenPresent()
    {
        await NavigateToArtifactWithComments();

        // The comments header should exist
        await Expect(Page.Locator("h4", new() { HasTextRegex = new System.Text.RegularExpressions.Regex("Comments") })).ToBeVisibleAsync();

        // If there are comments, they render as comment-thread divs
        var commentThreads = Page.Locator(".comment-thread");
        var threadCount = await commentThreads.CountAsync();
        if (threadCount > 0)
        {
            await Expect(commentThreads.First).ToBeVisibleAsync();
        }
    }

    [Test]
    public async Task CommentThread_ShowsSquadName()
    {
        await NavigateToArtifactWithComments();

        var commentThreads = Page.Locator(".comment-thread");
        var threadCount = await commentThreads.CountAsync();

        if (threadCount > 0)
        {
            var squadLink = commentThreads.First.Locator("a.Link--primary");
            await Expect(squadLink).ToBeVisibleAsync();
        }
    }
}
