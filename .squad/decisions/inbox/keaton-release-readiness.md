# Decision: Thursday Release Readiness — Blockers and Go/No-Go

**By:** Keaton (Lead)  
**Date:** July 2025  
**Scope:** Squad Places public trial release

## Decision

**🟡 Go with caveats.** Squad Places is ready for public trial on Thursday after fixing 2 blocking issues.

## Blockers (Must Fix)

### 1. Replace squad-ci.yml with .NET CI
The CI workflow (`.github/workflows/squad-ci.yml`) runs `npm ci` / `npm test` — this is from the squad-sdk TypeScript repo. It must be replaced with `dotnet build` / `dotnet test` before any community PRs arrive.

### 2. Fix .squad/team.md Project Context
Lines 76-80 of `.squad/team.md` describe a TypeScript/Node.js project. This repo is .NET. Update to reflect Squad Places stack.

## High Priority (Before Thursday If Possible)

- `.squad/routing.md` module ownership table references TypeScript modules
- `.squad/decisions.md` is 424KB with mostly irrelevant squad-sdk content — archive old decisions
- Live Azure Container Apps URLs committed in `.squad/` history files — remove or genericize

## Completed

- ✅ Fixed PlaywrightTestBase.cs hardcoded Azure URL → now defaults to localhost:5000
- ✅ Created FTUE design doc at `docs/getting-started/first-time-experience.md`
- ✅ Created release readiness report at `docs/development/release-readiness.md`
- ✅ Created 7 human-in-the-loop test issues (#2-#8) with `squad` label

## What This Means for the Team

- Any agent working on this repo should read the updated team.md once it's fixed
- The FTUE flow defines how the Coordinator should behave when a new user arrives
- The release readiness report is the canonical reference for Thursday go/no-go
