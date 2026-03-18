# Shiherlis — History

## Project Context
- **Project:** Squad Places — a social network for AI agent teams
- **Stack:** .NET 10, Aspire, Azure Blob Storage, Razor Pages
- **Owner:** Brady
- **Joined:** 2026-03-05

## Learnings
- .NET Playwright (Microsoft.Playwright.NUnit 1.58.0) works well on net10.0 with NUnit 4.x. Run `playwright.ps1 install chromium` from the bin output to get browsers.
- The live deployed site is behind the source code — sort buttons, squad filter, and comments counter aren't deployed yet. Tests must be resilient to source/deployment drift.
- `hx-boost="true"` on the body means htmx intercepts link clicks for AJAX navigation. Playwright handles this transparently — `WaitForURLAsync` works with pushState.
- Feed item locators need to be specific: `.feed-item a:has(h3)` targets artifact title links, not squad links (both are `a.Link--primary`).
- Strict mode violations are the most common Playwright failure — always scope locators precisely or use `.First` when multiple matches are expected.
- Base URL is configurable via `SQUADPLACES_BASE_URL` env var, defaulting to localhost:5000. No public URLs in code — Brady's directive.
- Visual regression screenshots go to `tests/SquadPlaces.Playwright/screenshots/` (gitignored, `.gitkeep` tracked). Use `ScreenshotDir` property from base class.
- Performance.getEntriesByName('first-contentful-paint') may return empty in headless Chromium — tests should handle gracefully with Assert.Pass fallback.
- Accessibility tests: Primer CSS uses `<header>` landmark but verify `<main>` exists in layout. `lang` attribute on `<html>` is important for screen readers.
- `.runsettings` supports `ExpectTimeout` under `<Playwright>` and `DefaultTimeout` under `<NUnit>` — useful for CI where network is slower.
- Parallel workers bumped from 2 → 4 since tests are I/O-bound (waiting on page loads). Safe because each test gets its own browser context.
