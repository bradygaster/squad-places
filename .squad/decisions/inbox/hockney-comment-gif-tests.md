# Decision: Comment & GIF Test Coverage

**By:** Hockney (Tester)
**Date:** 2026-03-05
**Status:** Tests written, awaiting implementation

## What

17 integration tests for the Comments/Replies and GIF features in `tests/SquadPlaces.AppHost.Tests/CommentAndGifTests.cs`. Written against the API contract before Fenster's implementation lands.

## Test Coverage Matrix

| # | Test | Endpoint | Expected |
|---|------|----------|----------|
| 1 | PostComment_OnValidArtifact_Returns201 | POST /api/artifacts/{id}/comments | 201 |
| 2 | GetComments_ReturnsPostedComments | GET /api/artifacts/{id}/comments | 200, ≥2 items |
| 3 | GetComment_ById_Returns200 | GET /api/comments/{id} | 200, correct body |
| 4 | PostReply_WithParentCommentId_Returns201 | POST /api/artifacts/{id}/comments | 201, parentId set |
| 5 | PostComment_WithGifUrl_Returns201 | POST /api/artifacts/{id}/comments | 201, GifUrl preserved |
| 6 | PublishArtifact_WithGifUrl_Returns201 | POST /api/artifacts | 201, GifUrl preserved |
| 7 | PostComment_EmptyBody_Returns400 | POST /api/artifacts/{id}/comments | 400 |
| 8 | PostComment_MissingBody_Returns400 | POST /api/artifacts/{id}/comments | 400 |
| 9 | PostComment_BodyTooLong_Returns400 | POST /api/artifacts/{id}/comments | 400 (10K chars) |
| 10 | PostComment_InvalidGifUrl_Returns400 | POST /api/artifacts/{id}/comments | 400 |
| 11 | PostComment_InvalidSquadId_Returns400 | POST /api/artifacts/{id}/comments | 400 |
| 12 | PostComment_InvalidArtifactId_Returns404 | POST /api/artifacts/{id}/comments | 404 |
| 13 | PostComment_InvalidParentCommentId_Returns400 | POST /api/artifacts/{id}/comments | 400 |
| 14 | PostComment_ParentOnDifferentArtifact_Returns400 | POST /api/artifacts/{id}/comments | 400 |
| 15 | GetComments_NoComments_ReturnsEmptyList | GET /api/artifacts/{id}/comments | 200, [] |
| 16 | GetComment_InvalidId_Returns404 | GET /api/comments/{id} | 404 |
| 17 | PublishArtifact_WithInvalidGifUrl_Returns400 | POST /api/artifacts | 400 |

## Design Decisions

- **No model imports:** Tests use `Dictionary<string, object?>` and anonymous objects for payloads, not imported record types. This decouples the test file from Fenster's in-progress types.
- **Shared fixture:** Reuses `ApiTestFixture` (same as ApiValidationTests) — Aspire host boots once, not per-test.
- **Contract-first:** Tests validate HTTP status codes and JSON response shapes. If Fenster's implementation deviates from the agreed contract, these tests will catch it.

## Known Gaps

- **No rate limit tests** for comment spam — the existing rate limiter tests in ApiValidationTests cover that pattern.
- **No pagination tests** for GET comments — not specified in the contract yet.
- **Test #14 (cross-artifact reply)** may need adjustment if Fenster doesn't validate parent comment artifact membership.

## Impact

All agents: When Fenster's implementation lands, run `dotnet test` to verify contract alignment. If tests fail, check whether the contract changed or the implementation has a bug.
