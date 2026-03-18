# Decision: Playwright Test Suite Expansion

**By:** Shiherlis (Playwright Test Engineer)
**Date:** 2026-03-06
**Status:** Implemented

## What

Expanded the Playwright E2E test suite from 26 tests across 6 files to ~58 tests across 10 files. Added four new test categories:

- **VisualRegressionTests.cs** (6 tests): Screenshot capture for feed, squads, squad detail, artifact detail, search, and mobile feed — stored in `screenshots/` for docs and baselines.
- **UserJourneyTests.cs** (8 tests): Multi-step user flows, cross-page navigation, responsive layout at mobile/tablet, error handling for bad routes and invalid IDs.
- **AccessibilityTests.cs** (10 tests): Heading hierarchy, ARIA landmarks, keyboard navigation, image alt text, link accessible names, `lang` attribute.
- **PerformanceTests.cs** (8 tests): Page load under 3s, no console errors, no failed network requests, First Contentful Paint, status code checks.

## Critical Fix

Removed hardcoded public Azure Container Apps URL from `PlaywrightTestBase.cs`. Fallback is now `http://localhost:5000`. Env var `SQUADPLACES_BASE_URL` remains the primary source.

## Infrastructure Changes

- `.runsettings`: Bumped parallel workers from 2 → 4, added ExpectTimeout (10s) and DefaultTimeout (30s).
- `.gitignore`: Added rule for `screenshots/` (only `.gitkeep` tracked).
- Added `screenshots/.gitkeep`.
- Added `README.md` for the test suite with run instructions.

## Why

Thursday release needs broader test coverage. These tests catch real regressions: broken navigation, missing landmarks, slow pages, console errors, visual drift.
