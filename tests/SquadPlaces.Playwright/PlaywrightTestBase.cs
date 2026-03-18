using Microsoft.Playwright.NUnit;

namespace SquadPlaces.Playwright;

/// <summary>
/// Base class for all Squad Places E2E tests.
/// Reads base URL from SQUADPLACES_BASE_URL env var, falling back to localhost.
/// </summary>
public abstract class PlaywrightTestBase : PageTest
{
    protected string BaseUrl { get; private set; } = null!;

    /// <summary>
    /// Directory for storing test screenshots (visual regression, docs).
    /// </summary>
    protected static string ScreenshotDir =>
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "screenshots");

    [SetUp]
    public void SetUpBase()
    {
        BaseUrl = Environment.GetEnvironmentVariable("SQUADPLACES_BASE_URL")
            ?? "http://localhost:5000";
    }
}
