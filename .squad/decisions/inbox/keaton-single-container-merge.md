# Decision: Merge API into Web Project — Single Container for Synology

**Date:** 2025-07-17
**By:** Keaton (Lead)
**Requested by:** Jeffrey T. Fritz
**Status:** Decided — ready for implementation

## Context

Jeffrey wants to deploy Squad Places to his Synology NAS as a single Docker container with file-based storage. Currently there are two projects:

- **SquadPlaces.Api** — 1025-line Program.cs with 11 minimal API endpoints, rate limiting, IP blocklist, duplicate detection, OpenAPI/Scalar docs, CORS
- **SquadPlaces.Web** — 32-line Program.cs with Razor Pages, SignalR (FeedHub), static assets

Both projects independently use `IBlobStorageService` from `SquadPlaces.Data`. **The Web project does NOT call the API via HTTP.** Zero HttpClient usage. Zero `Services__api` consumption in code. The `Services__api` environment variable in docker-compose and the `.WithReference(api)` in AppHost are dead wiring — the Web reads storage directly.

## Decision

**Option 1: Merge API endpoints into the Web project.** One process, one port, one container.

## Alternatives Rejected

| Option | Verdict | Reason |
|--------|---------|--------|
| YARP reverse proxy | Rejected | No HTTP call to proxy. Adds latency, complexity, and a dependency for nothing. |
| Multi-process container (supervisord) | Rejected | Two processes competing for file storage. No shared SignalR context. Maintenance headache. |
| New combined project | Rejected | Over-engineering. Web is 32 lines. Just add to it. New project = new build target, new Dockerfile, new references — for zero architectural benefit. |

## Why Option 1 Wins

1. **No integration to untangle.** Both projects already use `IBlobStorageService` directly. Merging is purely additive — register API middleware and map API endpoints in the Web's startup.

2. **Single process = free SignalR push.** When an artifact is published via the API endpoint, inject `IHubContext<FeedHub>` and push real-time updates to all connected Web clients. No webhook, no polling, no inter-process communication.

3. **Synology deployment is trivial.** One container, one port, one volume mount. `docker run` with `-v /volume1/squad-data:/data -p 5100:8080`. Done.

4. **StorageServiceFactory already exists but is unused.** Neither project calls it — both hardcode `BlobStorageService`. The merge is the right time to wire `StorageServiceFactory.AddStorageService()` and properly support `STORAGE_MODE=File` without conditional startup code.

5. **Aspire still works.** AppHost changes from two project references to one. Same `WithExternalHttpEndpoints()`, same blob storage reference. Simpler.

## Implementation Plan for Fenster

### Files to Create

```
src/SquadPlaces.Web/Api/
├── ApiEndpoints.cs                         # Extension method: app.MapApiEndpoints()
├── ApiModels.cs                            # DTOs: EnlistRequest, PublishArtifactRequest, PostCommentRequest, FeedArtifact
├── ApiValidation.cs                        # Static helpers: Sanitize, validators, spam detection, Levenshtein, near-duplicate
├── Services/
│   ├── IpBlocklistService.cs               # IP blocklist (15 strikes → 10min block)
│   ├── DuplicateDetectionService.cs        # Artifact duplicate detection (same squad+title within 5min)
│   └── CommentDuplicateDetectionService.cs # Comment duplicate detection (same squad+artifact+body within 2min)
```

### Files to Modify

1. **`src/SquadPlaces.Web/SquadPlaces.Web.csproj`**
   - Add: `Microsoft.AspNetCore.OpenApi` (10.0.3), `Scalar.AspNetCore` (*)

2. **`src/SquadPlaces.Web/Program.cs`**
   - Replace `builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>()` with `builder.Services.AddStorageService(builder.Configuration)`
   - Add: `IpBlocklistService`, `DuplicateDetectionService`, `CommentDuplicateDetectionService` singletons
   - Add: Rate limiting configuration (3 policies: global 100/min, write 30/min, read 60/min)
   - Add: OpenAPI + Scalar registration
   - Add: CORS (AllowAnyOrigin for API)
   - Add: IP blocking middleware (before rate limiter)
   - Add: `app.UseRateLimiter()`
   - Add: `app.MapOpenApi()` + `app.MapScalarApiReference()`
   - Add: `app.MapApiEndpoints()` (the extension method)
   - Remove: `builder.AddAzureBlobServiceClient("BlobStorage")` — let StorageServiceFactory handle it (keep it only for Blob mode via conditional)
   - Keep: Razor Pages, SignalR, static assets, existing middleware

3. **`src/SquadPlaces.Web/Dockerfile`**
   - No changes needed (already has /data volume, port 8080)

4. **`docker-compose.yml`**
   - Remove `api` service entirely
   - Rename `web` to `app` (or keep as `web`)
   - Remove `Services__api` environment variable
   - Remove `depends_on: api`
   - Single port mapping (e.g., 5100:8080)

5. **`src/SquadPlaces.AppHost/AppHost.cs`**
   - Remove API project reference
   - Remove `.WithReference(api)` and `.WaitFor(api)` from Web
   - Single project: Web with blob storage reference

6. **`src/SquadPlaces.AppHost/SquadPlaces.AppHost.csproj`**
   - Remove project reference to SquadPlaces.Api

### Files to Delete (after merge is verified)

- `src/SquadPlaces.Api/` — entire directory (all functionality moved to Web)
- Update `SquadPlaces.slnx` to remove Api project reference

### Key Implementation Notes

- **Namespace:** Put API classes under `SquadPlaces.Web.Api` namespace. Clean separation within the project.
- **Endpoint prefix:** Keep `/api/` prefix on all endpoints. Web uses `/` routes for Razor Pages. No conflicts.
- **Rate limiting scope:** Apply rate limiting middleware ONLY to `/api/*` routes, not to Razor Pages or SignalR. Use `RequireRateLimiting()` on individual endpoints (already done in source).
- **IP blocking middleware:** Run for all requests (same as current API behavior).
- **StorageServiceFactory:** Wire it up with `builder.Configuration` — it reads `STORAGE_MODE` and `FILE_STORAGE_PATH` from environment. For Aspire mode (Blob), keep `builder.AddAzureBlobServiceClient()` conditionally.
- **BlobStorage Aspire binding:** Only call `builder.AddAzureBlobServiceClient("BlobStorage")` when NOT in File storage mode. Check `StorageServiceFactory.IsFileStorage(builder.Configuration)` first.
- **OpenAPI/Scalar:** Serve in all environments (matching current API behavior) — AI agents need the spec in production.
- **FeedHub integration (future enhancement):** After merge, inject `IHubContext<FeedHub>` into the artifact publish endpoint to push real-time updates. Not required for initial merge — file as follow-up.

### Verification

1. `dotnet build` succeeds for Web project
2. `docker build -f src/SquadPlaces.Web/Dockerfile .` succeeds
3. `docker-compose up` starts single container
4. Razor Pages work at `http://localhost:5100/`
5. API endpoints work at `http://localhost:5100/api/`
6. OpenAPI spec at `http://localhost:5100/openapi/v1.json`
7. Scalar docs at `http://localhost:5100/scalar/v1`
8. File storage writes to `/data/` volume
9. Aspire AppHost still builds and runs (with blob emulator)

## Impact

- **Deployment:** Single container, single port. Ideal for Synology NAS.
- **Aspire:** Simpler — one project instead of two. Still works for development with blob emulator.
- **Docker Compose:** One service instead of two. No inter-service networking needed.
- **Existing API consumers:** Zero breaking changes — same endpoints, same paths, same behavior. Just served from the Web port instead of a separate API port.
- **Future:** SignalR real-time push from API writes becomes trivial (shared process).
