# Decision: API Validation Regression Test Suite

**Author:** Hockney (Tester)
**Date:** 2026-03-05
**Status:** Implemented

## Context

Waingro ran adversarial API testing against Squad Places (2026-03-05) and found 3 server crashes (P0), 4 missing validations (P1), and 2 security-adjacent concerns. Brady asked Hockney to write regression tests covering every bug found.

## Decision

Created `tests/SquadPlaces.AppHost.Tests/ApiValidationTests.cs` with 15 integration tests using the existing Aspire `DistributedApplicationTestingBuilder` pattern. Tests use a shared `IClassFixture<ApiTestFixture>` to boot the app host once (~30s) instead of per-test.

## Test Coverage

| # | Category | Test Name | Bug Ref | Status |
|---|----------|-----------|---------|--------|
| 1 | P0 | `EnlistSquad_WithEmptyJsonBody_DoesNotReturn500` | BUG-1 | ✅ |
| 2 | P0 | `EnlistSquad_WithMissingName_DoesNotReturn500` | BUG-1 | ✅ |
| 3 | P0 | `EnlistSquad_WithNullBytesInName_DoesNotReturn500` | BUG-2 | ✅ |
| 4 | P1 | `EnlistSquad_WithEmptyName_Returns400` | BUG-3 | ✅ |
| 5 | P1 | `EnlistSquad_WithVeryLongName_Returns400` | BUG-6 | ✅ |
| 6 | P1 | `PublishArtifact_WithInvalidType_Returns400` | BUG-4 | ✅ |
| 7 | P1 | `PublishArtifact_WithEmptyTitle_Returns400` | BUG-5 | ✅ |
| 8 | P1 | `PublishArtifact_WithInvalidSquadId_Returns400` | — | ✅ |
| 9 | Happy | `EnlistSquad_WithValidData_Returns201` | — | ✅ |
| 10 | Happy | `PublishArtifact_WithValidData_Returns201` | — | ✅ |
| 11 | Happy | `GetFeed_Returns200` | — | ✅ |
| 12 | Happy | `GetFeed_WithPageZero_DoesNotCrash` | — | ✅ |
| 13 | Happy | `GetFeed_WithOversizedPageSize_ReturnsAtMost100Items` | — | ✅ |
| 14 | Edge | `GetSquad_WithNonexistentId_Returns404` | — | ✅ |
| 15 | Edge | `GetArtifact_WithNonexistentId_Returns404` | — | ✅ |

## Open Issue

Null bytes test (BUG-2) passes P0 (no 500) but the server returns an Azure SDK exception instead of a clean 400. The sanitize function strips `\0` but `\uFFFD` and `\u202E` propagate to blob metadata. Recommend: expand `Sanitize()` to strip all control characters and non-printable Unicode before blob storage.

## Consequences

- All 15 regression tests pass against current `main`.
- Any future changes that break validation will be caught immediately.
- The shared fixture pattern can be reused for additional API test classes.
