# Decision: Tier 2 Moderation — Azure Content Safety Integration

**By:** Fenster (Core Dev)
**Issue:** #18
**Date:** 2026-03-10

## What

Azure Content Safety SDK integrated as Tier 2 in ContentModerationPipeline. Pipeline method changed from `Evaluate()` (sync) to `EvaluateAsync()` (async) to support the async SDK.

## Key Design Choices

1. **Graceful degradation**: If `AzureContentSafety:Endpoint` and `AzureContentSafety:Key` aren't configured, Tier 2 is skipped entirely. If the API call fails at runtime, it degrades silently (logs error, returns Allowed).

2. **Tier ordering**: Tier 1 (local regex) runs first. If it hard-blocks, Tier 2 is skipped (no wasted API call). If Tier 1 passes or flags NeedsReview, Tier 2 runs and can escalate.

3. **Severity thresholds** (configurable via config):
   - `AzureContentSafety:BlockThreshold` (default: 4) → hard block
   - `AzureContentSafety:ReviewThreshold` (default: 2) → NeedsReview

4. **Breaking change**: `Evaluate()` → `EvaluateAsync()`. Both call sites in ApiEndpoints.cs updated. No external callers affected (internal pipeline only).

## Files Changed

- `src/SquadPlaces.Api.Endpoints/Services/AzureContentSafetyService.cs` (new)
- `src/SquadPlaces.Api.Endpoints/Services/ContentModerationPipeline.cs` (Tier 2 integration, async)
- `src/SquadPlaces.Api.Endpoints/ApiServiceRegistration.cs` (DI)
- `src/SquadPlaces.Api.Endpoints/ApiEndpoints.cs` (call sites)
- `src/SquadPlaces.Api.Endpoints/SquadPlaces.Api.Endpoints.csproj` (Azure.AI.ContentSafety package)

## Why Not AppHost Wiring

No `Aspire.Hosting.Azure.AI.ContentSafety` component exists yet. Config flows through standard `IConfiguration` (env vars, user secrets, appsettings). When Aspire adds hosting support, we can wire it through the AppHost.
