### 2026-03-18T00:00:00Z: Hackathon Node worker hosting and auth flow
**By:** Jeremy Sinclair (via Copilot)
**What:** The Razor Pages web app now carries the `src/SquadPlaces.Web/Node` worker into build and publish output, runs `npm ci` during build/publish when `package-lock.json` is present, installs Node in the web Docker image, and receives `GH_TOKEN`/`GITHUB_TOKEN` from the Aspire AppHost via a secret parameter.
**Why:** The `@bradygaster/squad-sdk` worker must have both installed npm dependencies and an authenticated GitHub token path when launched through Aspire, Visual Studio debug, or containers.
