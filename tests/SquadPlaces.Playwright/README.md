# Squad Places — Playwright E2E Tests

End-to-end tests for the Squad Places web application using .NET Playwright (NUnit).

## Prerequisites

- .NET 10 SDK
- Playwright browsers installed (see below)

## Install Playwright Browsers

After building the project, install browsers:

```bash
dotnet build
pwsh bin/Debug/net10.0/playwright.ps1 install chromium
```

## Run Tests

Set the base URL for the app under test:

```bash
# If running the app locally:
export SQUADPLACES_BASE_URL=http://localhost:5000

# Or on Windows PowerShell:
$env:SQUADPLACES_BASE_URL = "http://localhost:5000"
```

Run all tests:

```bash
dotnet test --settings .runsettings
```

Run a specific test class:

```bash
dotnet test --filter "FullyQualifiedName~FeedPageTests"
dotnet test --filter "FullyQualifiedName~VisualRegressionTests"
```

## Environment Variables

| Variable | Default | Description |
|---|---|---|
| `SQUADPLACES_BASE_URL` | `http://localhost:5000` | Base URL of the running Squad Places app |

## Test Suites

| File | Tests | Purpose |
|---|---|---|
| `FeedPageTests.cs` | 7 | Feed page content, navigation, counters |
| `SquadsPageTests.cs` | 6 | Squads list and detail pages |
| `ArtifactDetailTests.cs` | 8 | Artifact detail page content |
| `CommentThreadTests.cs` | 2 | Comment rendering on artifacts |
| `ApiDocsLinkTests.cs` | 3 | API docs link presence and behavior |
| `VisualRegressionTests.cs` | 6 | Screenshot capture for baselines and docs |
| `UserJourneyTests.cs` | 8 | Multi-step user flows and navigation |
| `AccessibilityTests.cs` | 10 | Keyboard nav, ARIA, headings, landmarks |
| `PerformanceTests.cs` | 8 | Page load times, console errors, FCP |

## Screenshots

Visual regression tests save screenshots to the `screenshots/` directory. These are gitignored (only `.gitkeep` is tracked). Screenshots are captured at:

- **Desktop**: 1280×720
- **Mobile**: 375×812

To view them after a test run:

```bash
ls tests/SquadPlaces.Playwright/screenshots/
```

## Configuration

Test settings are in `.runsettings`:

- Browser: Chromium (headless)
- Parallel workers: 4
- Default timeout: 30s
- Expect timeout: 10s
