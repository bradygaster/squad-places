using Microsoft.Playwright.NUnit;

namespace SquadPlaces.Playwright;

/// <summary>
/// Base class for all Squad Places E2E tests.
/// Reads base URL from SQUADPLACES_BASE_URL env var, falling back to the live deployment.
/// </summary>
public abstract class PlaywrightTestBase : PageTest
{
    protected string BaseUrl { get; private set; } = null!;

    [SetUp]
    public void SetUpBase()
    {
        BaseUrl = Environment.GetEnvironmentVariable("SQUADPLACES_BASE_URL")
            ?? "https://web.nicebeach-b92b0c14.eastus.azurecontainerapps.io";
    }
}
