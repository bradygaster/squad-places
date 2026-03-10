# SignalR JavaScript Client Loading Strategy

**Date:** 2026-03-09  
**Author:** Fenster  
**Status:** Implemented

## Context

The `_Layout.cshtml` referenced a non-existent SignalR JS client path: `/_content/Microsoft.AspNetCore.SignalR.Client/signalr.min.js`. The `Microsoft.AspNetCore.SignalR.Client` package is a .NET client library and does not contain JavaScript files. The actual JS client comes from the npm package `@microsoft/signalr`.

## Decision

**Bundle the SignalR JavaScript client locally in wwwroot rather than using a CDN reference.**

Implementation:
- Downloaded `@microsoft/signalr/dist/browser/signalr.min.js` from unpkg
- Placed in `src/SquadPlaces.Web/wwwroot/js/signalr.min.js`
- Updated `_Layout.cshtml` to reference `/js/signalr.min.js`

## Rationale

1. **Reliability:** Local bundling ensures the app works in containerized, airgapped, or offline deployment scenarios
2. **Consistency:** Aligns with project's approach to static assets served from wwwroot
3. **Tool availability:** LibMan (the .NET client library manager) was not available in the environment
4. **Critical dependency:** SignalR is a core runtime dependency for the feed feature, not an optional enhancement

## Alternatives Considered

- **CDN reference (like htmx):** Would work for connected deployments but creates external dependency
- **LibMan:** Preferred for .NET projects but tool not installed
- **npm + bundler:** Overkill for a single JS file dependency

## Impact

- ✅ SignalR client now loads correctly
- ✅ No external runtime dependencies
- ✅ Works in all deployment scenarios (container, airgapped, Synology)
- ⚠️ Requires manual update if newer SignalR version needed (acceptable tradeoff)

## Related Files

- `src/SquadPlaces.Web/Pages/Shared/_Layout.cshtml` (line 78)
- `src/SquadPlaces.Web/wwwroot/js/signalr.min.js` (new file, 47KB)
