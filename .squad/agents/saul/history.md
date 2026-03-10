📌 Team update (2026-03-10T053431Z): Wave 3 security hardening complete — AppHost orchestration complete — API, Redis persistent, Web depends on API. Integration: All services now discoverable by name; Fenster/Baer/Keaton services integrated; Hockney tests run against full stack.

# Saul — History

## Project Context

- **Project:** Squad SDK — the programmable multi-agent runtime for GitHub Copilot
- **Owner:** Brady
- **Stack:** TypeScript (strict mode, ESM-only), Node.js ≥20, @github/copilot-sdk, Vitest, esbuild
- **Joined:** 2026-02-22

## Key Context

- OTel Phases 1-3 complete: provider init, telemetry bridge, agent lifecycle traces, session traces, tool enhancements, metric wiring
- OTel Phase 4 complete: Aspire command (`squad aspire`), file watcher (squad-observer), event payloads, WS bridge
- Aspire dashboard: `mcr.microsoft.com/dotnet/aspire-dashboard:latest`
  - UI: port 18888, OTLP/gRPC: port 18889 (mapped to host 4317)
  - Anonymous mode: `ASPIRE_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true`
- The `squad aspire` CLI command handles Docker lifecycle (start/stop container)
- OTel exports from SDK: initializeOTel, shutdownOTel, getTracer, getMeter, bridgeEventBusToOTel, initSquadTelemetry
- 2022 tests passing across 74 files (before Wave 2)

## Learnings

- Aspire dashboard Playwright integration tests: `test/aspire-integration.test.ts` (5 tests)
  - Uses `@opentelemetry/sdk-node` (0.57.2) with gRPC exporters for version-aligned OTel setup
  - Direct URL navigation (`/traces`, `/metrics`) is more reliable than sidebar click selectors — Aspire uses Fluent UI web components
  - NodeSDK registers global providers automatically; `trace.getTracer()` / `metrics.getMeter()` work without separate registration
  - `forceFlush` on global providers works via `trace.getTracerProvider()` cast — the SDK's NodeSDK doesn't expose flush directly
  - Dashboard needs ~3s after flush to index traces, ~5s for metrics
  - Test auto-skips when `SKIP_DOCKER_TESTS=1` or Docker is unavailable
  - 2141 tests passing across 79 files (post Aspire integration test)
- Wave 3 work (2026-02-23):
  - Created `docs/scenarios/aspire-dashboard.md` — scenario-style guide for using Squad with Aspire
  - Updated `docs/build.js` SECTION_ORDER to include aspire-dashboard in scenarios ordering
  - Doc covers: what Aspire is, Docker launch, SDK integration, dashboard features (traces, metrics, resources), troubleshooting, pro tips
  - Tone: action-oriented, welcoming, prompt-first (matching existing Squad docs)
  - Referenced `packages/squad-sdk/src/runtime/otel-*.ts` and `test/aspire-integration.test.ts` for implementation details
  - Docs build verified — 39 pages generated, aspire-dashboard.html confirmed in dist
- **Critical bug fix (2026-02-XX): Telemetry not appearing in Aspire dashboard.** [CORRECTED: Date context missing, but timeline consistent with wave 2 work] Root cause analysis:
  1. **Protocol mismatch** — SDK was using OTLP/HTTP exporters (`exporter-trace-otlp-http`), Aspire only accepts OTLP/gRPC on port 18889. Switched to `exporter-trace-otlp-grpc` / `exporter-metrics-otlp-grpc`.
  2. **Wrong endpoint in `squad aspire`** — `ASPIRE_OTLP_ENDPOINT` was `http://localhost:18888` (the dashboard UI port). Fixed to `http://localhost:4317` (host-mapped gRPC port).
  3. **OTLP auth mode was `ApiKey`** — Docker command set `DASHBOARD__OTLP__AUTHMODE=ApiKey` but SDK sent no API key header. Changed to `Unsecured` for local dev.
  4. **Docs used stale env var** — `ASPIRE_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true` replaced with `DASHBOARD__FRONTEND__AUTHMODE=Unsecured`.
  5. Added "Quick Debug Checklist" to `docs/scenarios/aspire-dashboard.md` for future troubleshooting.
  - gRPC packages (`@opentelemetry/exporter-trace-otlp-grpc`, `@grpc/grpc-js`) were already in node_modules transitively via `@opentelemetry/sdk-node`.
  - All 84 OTel/Aspire tests pass after fix.
- **OTEL pipeline silent failure diagnosis (2026-02-XX):** [CORRECTED: Date context missing, related to telemetry not appearing bug; may be same incident] Brady reported "telemetry pump not working" — REPL printed telemetry-active message but nothing appeared in Aspire.
  - **Root cause:** Aspire container started without `DASHBOARD__OTLP__AUTHMODE=Unsecured`, gRPC exporter got `16 UNAUTHENTICATED (HTTP 401)` on every export attempt. Error was completely invisible — swallowed by OTel's internal error handling.
  - **Fix 1:** `ensureSDK()` in `otel.ts` now auto-enables OTel `DiagConsoleLogger` at WARN level when `SQUAD_DEBUG=1` is set. This surfaces gRPC transport errors (401, ECONNREFUSED, etc.) to stderr. When `debug: true` is passed explicitly, full DEBUG level is enabled.
  - **Fix 2:** `ensureSDK()` now resets `_sdk = undefined` if `start()` throws, preventing the "initialized but broken" state where the SDK thinks it's running but providers were never registered.
  - **Fix 3:** `shutdownOTel()` catches and logs shutdown/flush errors when `SQUAD_DEBUG=1` instead of propagating them to callers. Previously, a flush error (e.g. 401) would throw from `shutdownOTel()` potentially breaking cleanup chains.
  - **Fix 4:** Added `test/otel-export.test.ts` — 5 tests validating the SDK's internal span pipeline with in-memory exporter (span capture, multi-span, multi-tracer, error recording, clean shutdown).
  - **Key diagnostic technique:** Standalone `tmp-otel-diag.mjs` script creating a single span + flush immediately revealed the 401. This should be the first step whenever "telemetry not appearing" is reported.
  - Code review of `client.ts` span instrumentation: all spans properly `.end()`'d in `finally` blocks; tracer proxy pattern works correctly (module-level `trace.getTracer()` delegates to whatever provider is registered later by `NodeSDK.start()`).
  - 2424 tests passing across 89 files after fix.
- **Dual-mode OTel telemetry (Issue #343, PR #352):**
  - Added `mode` field to `OTelConfig` — written as `squad.mode` resource attribute in `buildResource()`
  - Created `initAgentModeTelemetry()` convenience in `otel-init.ts` — pre-sets `serviceName: 'squad-copilot-agent'` and `mode: 'copilot-agent'`
  - `runShell()` now passes `mode: 'cli'` to `initSquadTelemetry`, so CLI spans are tagged
  - Exported `initAgentModeTelemetry` from SDK barrel (`index.ts`)
  - Added 3 tests in `test/otel-export.test.ts`: mode attribute on spans, dual-mode distinguishability, handle validity
  - No dedicated Copilot agent-mode entry point exists yet — `initAgentModeTelemetry()` is ready for when it's created

---

📌 Team update (2026-02-23T09:25Z): OTel gRPC protocol fix completed, Aspire dashboard working. Streaming diagnostics infrastructure finished by Kovash (SQUAD_DEBUG logging), 13 regression tests added by Hockney. Version bump to 0.8.5.1 (target: v0.8.17). — decided by Scribe [CORRECTED: Clarified v0.8.17 is the target version; 0.8.5.1 is an intermediate milestone]

### 2026-02-24T17-25-08Z : Team consensus on public readiness
📌 Full team assessment complete. All 7 agents: 🟡 Ready with caveats. Consensus: ship after 3 must-fixes (LICENSE, CI workflow, debug console.logs). No blockers to public source release. See .squad/log/2026-02-24T17-25-08Z-public-readiness-assessment.md and .squad/decisions.md for details.

### Shell Observability Metrics (Issues #508, #520, #526, #530, #531)
- Created `packages/squad-cli/src/cli/shell/shell-metrics.ts` — 4 metrics under `squad.shell.*` namespace
  - `squad.shell.session_count` (counter): incremented on shell start, basic retention proxy
  - `squad.shell.session_duration_ms` (histogram): recorded on shell exit
  - `squad.shell.agent_response_latency_ms` (histogram): time from dispatch to first visible token, attributed by agent name + dispatch type
  - `squad.shell.error_count` (counter): errors during agent/coordinator dispatch, attributed by source
- **Opt-in gated via `SQUAD_TELEMETRY=1`** — stronger than SDK-level gate (which only requires `OTEL_EXPORTER_OTLP_ENDPOINT`). Shell metrics describe user behavior, so explicit consent is required.
- Uses `getMeter('squad-shell')` from SDK — shares the same MeterProvider and OTLP/gRPC pipeline to Aspire
- Wired into `runShell()` in `index.ts`: session count on start, duration on exit, latency in `dispatchToAgent`/`dispatchToCoordinator` (measured at first `message_delta`), errors in catch blocks
- Added subpath export `./shell/shell-metrics` to CLI package.json
- 18 new tests in `test/shell-metrics.test.ts` covering all metrics, opt-in gating, no-op behavior when disabled, and reset
- All 2943 tests pass (only pre-existing Docker/Aspire integration test skipped due to no Docker in env)

### Investigation: Aspire Deprecation Status (2026-02-24)
**Brady asked:** "What happened to squad aspire? When did we deprecate it, and why?"

**Findings:**
- **NOT deprecated.** Aspire is fully functional and actively maintained.
- **CLI Command:** `squad aspire` is registered in `packages/squad-cli/src/cli-entry.ts` and routes to `./cli/commands/aspire.ts`.
- **Implementation:** `aspire.ts` (175 LOC) is complete and production-ready:
  - Docker launch: pulls `mcr.microsoft.com/dotnet/aspire-dashboard:latest`, maps ports 18888 (UI) and 4317 (OTLP gRPC)
  - Dotnet fallback: attempts `dotnet workload list` and `dotnet tool list -g` to detect Aspire availability
  - Lifecycle management: spawns child process, pipes stdout/stderr, handles SIGINT/SIGTERM cleanup
  - OTLP configuration: sets `OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317`
  - Both launch strategies (Docker-first, dotnet-fallback) properly implemented
- **Documentation:** `docs/scenarios/aspire-dashboard.md` (147 lines) is comprehensive:
  - Use case: "Squad ships with an Aspire integration that streams telemetry to the dashboard in real time"
  - Covers what Aspire is, how to launch, SDK integration, dashboard features (traces/metrics/resources), auth modes (Unsecured vs ApiKey)
  - Includes troubleshooting section with "Quick Debug Checklist"
  - No deprecation notices or sunset language
- **Tests:** All Aspire tests passing (23 total):
  - `test/aspire-command.test.ts`: 2 tests — module exports, interface validation ✓
  - `test/cli/aspire.test.ts`: 16 tests — Docker detection, command generation, port mapping, lifecycle, module resolution ✓
  - `test/aspire-integration.test.ts`: 5 tests — Docker integration with Playwright, traces/metrics appear in dashboard, container lifecycle ✓
- **OTel Integration:** Aspire wired into shell telemetry pipeline:
  - `packages/squad-cli/src/cli/shell/index.ts`: Calls `initSquadTelemetry()` and `telemetry.shutdown()`
  - Shell metrics enabled via `enableShellMetrics()` when `SQUAD_TELEMETRY=1` and OTLP endpoint is configured
  - OTel exports via gRPC to localhost:4317 (Aspire's OTLP port) — protocol alignment verified in bug fix notes
- **Blog/Decision History:** No deprecation announcements:
  - `docs/blog/014-wave-1-otel-and-aspire.md` documents the feature launch (Feb 20, 2026), describes it as "optional" not deprecated
  - `.squad/decisions.md` includes directive (Feb 22): "Integration tests must launch the Aspire dashboard and validate OTel telemetry shows up" — active requirement
  - No commits mentioning deprecation, removal, or sunsetting

**Conclusion:** Squad Aspire is production-ready, fully tested, and actively used. No deprecation has occurred. The feature is optional (users can skip it) but the integration is mature and well-documented.

---

### History Audit — 2026-03-03

**Corrections Made:**
1. **Line 38:** Added date context notation to telemetry bug fix entry — timestamp vague (2026-02-XX), cross-referenced with wave 2 work.
2. **Line 46:** Added date context notation to OTel silent failure diagnosis — timestamp vague (2026-02-XX), noted possible correlation with telemetry bug.
3. **Line 65:** Clarified version target: v0.8.17 is final target; 0.8.5.1 is intermediate milestone to prevent future spawns from treating version bumps as final state.

**No critical conflicts found.** History is consistent with decisions.md (Aspire is active, not deprecated). All OTel phases and test counts align. Shell metrics (opt-in via SQUAD_TELEMETRY) properly documented.

**Total corrections: 3 [CORRECTED] annotations added.**

### Squad Social Network — Observability PRD (2026-03-05)

**Context:** Brady building a social network BY AI agents, FOR AI agents. Global, real-time, federated across orgs.

**Task:** Write `docs/prd/sections/12-observability.md` covering telemetry architecture for a network where agents are BOTH producers and consumers of observability data.

**Key Design Decisions:**
1. **OpenTelemetry-native:** OTLP/gRPC pipeline, traces for message delivery, metrics for network health, structured logs with trace correlation
2. **Multi-level attribution:** Every span/metric tagged with `agent.id`, `squad.id`, `org.id`, `cast.universe`
3. **Message lifecycle as distributed trace:** Trace spans from `message.create` → `message.validate` → `message.persist` → `message.fanout` → `federation.forward` → peer org receive
4. **Network vitals metrics:**
   - `network.messages.sent/delivered`, `network.messages.latency_ms` (p50/p95/p99)
   - `network.agents.active` (gauge), `network.agents.response_time_ms`
   - `federation.connections.latency_ms`, `federation.connections.failures`
   - `knowledge.propagation.hops`, `knowledge.propagation.time_to_reach_N`
5. **Cost observability:** Token counters per agent/squad/org (`cost.tokens.prompt/completion/total`, `cost.dollars`), attributed by operation type (`timeline_read`, `reply_generation`, `knowledge_extraction`)
   - Per-agent dashboards show token usage breakdown and monthly cost
   - Budget alerts: warn if agent > $50/mo, squad > $500/mo, org hard cap at $5000/mo
6. **Federation observability:** Per-peer health metrics, W3C Trace Context headers for cross-org traces, circuit breakers based on error rate/latency/queue depth
7. **Anomaly detection:** Telemetry-driven spam/abuse signals (posting frequency, content similarity, suspiciously fast response times), tiered automated actions (log → rate-limit → mute → suspend)
8. **Aspire dashboard integration:** Real-time traces/metrics/logs, agent activity heatmaps, federation health matrix, knowledge propagation Sankey diagrams
9. **Network health indicators:**
   - Green: message latency p95 < 200ms, agent response p95 < 5s, federation uptime > 99.5%
   - Yellow: latency 200-500ms, response 5-15s (degraded)
   - Red: latency > 500ms, response > 15s, federation failures > 5% (critical)

**Document structure:**
1. What to Measure (message throughput, agent activity, federation health, knowledge propagation)
2. Telemetry Architecture (OTLP pipeline, trace instrumentation, structured logging)
3. Aspire Dashboard (dev observability views, heatmaps, live updates)
4. Network Health Indicators (green/yellow/red signals, automated health checks)
5. Cost Observability (token accounting, per-agent/squad/org cost tracking, budget governance)
6. Anomaly Detection (spam patterns, abuse signals, automated response)
7. Federation Observability (peer health, cross-org tracing, circuit breakers)
8. Agent Activity Dashboards (personal metrics, engagement stats, network reach)
9. Implementation Roadmap (5 phases: MVP → Federation → Cost → Advanced Dashboards → Production)

**Tone:** Infrastructure-aware, telemetry-native. "If you can't see it, it didn't happen." No hand-waving — every metric has dimensions, every trace has a purpose.

**References:** OpenTelemetry spec, Aspire dashboard (aspire.dev, NOT learn.microsoft.com), W3C Trace Context, Prometheus naming conventions, internal Squad SDK OTel learnings.

**File written:** `docs/prd/sections/12-observability.md` (23KB, comprehensive coverage of all 8 required topics)

## PIN: 2026-03-05 - 20-Agent PRD Design Session

**Event:** Historic parallel fanout - 20 agents designed squad-social-network PRD simultaneously.

**Contribution:** All agents participated. 20 PRD sections delivered.

**Outcome:**
- 20 PRD sections drafted (docs/prd/sections/{01-20}-*.md)
- 23 decisions merged to .squad/decisions.md
- 20 orchestration logs created
- Session log: .squad/log/2026-03-05T02-02-22Z-social-network-prd.md
- Inbox cleared

**Next Steps:** Keaton assembles final PRD, Brady reviews, implementation planning begins.

**Key Pattern:** Largest parallel fanout in Squad history. Loose coupling, clear domains, shared constraints.

### OpenAPI Spec Enrichment (Squad Places API)
- Enriched all 7 endpoints in `src/SquadPlaces.Api/Program.cs` with `.WithSummary()`, `.WithDescription()`, and `.Produces<T>()` response metadata
- Configured `AddOpenApi()` with document transformer: title "Squad Places API", version "0.1.0-preview", rich description explaining enrollment flow, artifact types, and agent self-integration
- Removed `IsDevelopment()` guard on `MapOpenApi()` — spec is now served in all environments (intentional: deployed instances expose spec for agent discovery)
- Added XML doc comments to `Squad`, `KnowledgeArtifact`, `EnlistRequest`, `PublishArtifactRequest` — these flow into OpenAPI schema descriptions
- Enabled `<GenerateDocumentationFile>true</GenerateDocumentationFile>` and `<NoWarn>1591</NoWarn>` in both Api.csproj and Data.csproj
- All descriptions written in agent-friendly tone: an AI agent reading `/openapi/v1.json` can understand the full system and self-integrate
- Build verified: `dotnet build SquadPlaces.slnx` — all 6 projects succeed

### Docker Container Configuration (2026-03-06)
- Created alternate deployment mode: Docker containers with file-based storage on mounted volumes
- **Files created:**
  - `src/SquadPlaces.Api/Dockerfile` — multi-stage build for API container, /data volume mount
  - `src/SquadPlaces.Web/Dockerfile` — multi-stage build for Web container, /data volume mount
  - `docker-compose.yml` — orchestration with shared squad-data volume, optional Aspire profile
  - `src/SquadPlaces.Data/FileStorageService.cs` — file-based IBlobStorageService implementation
  - `src/SquadPlaces.Data/StorageServiceFactory.cs` — factory for switching storage modes via STORAGE_MODE env var
  - `docs/docker-deployment.md` — comprehensive deployment guide
- **Environment variables for storage switching:**
  - `STORAGE_MODE=Blob` (default) — uses Azure Blob Storage
  - `STORAGE_MODE=File` — uses FileStorageService with JSON files
  - `FILE_STORAGE_PATH=/data` — base path for file storage
- **Volume mount pattern:** Named volume with bind mount (`driver_opts: type: none, o: bind, device: ./data`) — enables host access + container portability
- **Aspire integration:** Optional via `--profile observability` — dashboard container on port 18888, OTLP gRPC on 4317
- **Port mapping:** Web on 5100, API on 5200 (matching AppHost convention)
- Build verified: all 6 projects compile successfully

### AppHost Topology Wiring — Admin Console PRD (2026-03-06)
- **Task:** Wire Aspire AppHost to match target topology from Admin Console PRD
- **Changes to `src/SquadPlaces.AppHost/AppHost.cs`:**
  - Added Redis cache resource (`builder.AddRedis("cache")`) with `ContainerLifetime.Persistent` for dev stability
  - Added API project (`SquadPlaces_Api`) with `.WithExternalHttpEndpoints()`, blob storage and Redis references
  - Updated Web project to reference blobs, Redis, and API — Web now `WaitFor(api)` so it starts after API is healthy
- **Changes to `src/SquadPlaces.AppHost/SquadPlaces.AppHost.csproj`:**
  - Added `<ProjectReference>` for `SquadPlaces.Api.csproj`
  - Added `Aspire.Hosting.Redis` NuGet package (Version="*")
- **Verification:**
  - API project already has `builder.AddServiceDefaults()` wired (line 10 of Program.cs) — Fritz's PR landed correctly
  - `dotnet build SquadPlaces.slnx` — all 8 projects succeed (Build succeeded in 11.2s)
- **Not done (by design):** SignalR, Admin Console UI — just API + Redis per Brady's directive
- **Topology:** storage(emulator) → blobs; redis(persistent); api(public, blobs+redis); web(public, blobs+redis+api)

### Cross-Squad Coordination Telemetry & App Insights Integration (Issue #28)
- **Task:** Add OpenTelemetry instrumentation for cross-squad coordination events and wire Azure Application Insights into the Aspire AppHost
- **Files created:**
  - `src/SquadPlaces.Api.Endpoints/SquadPlacesTelemetry.cs` — centralized telemetry class with ActivitySource ("SquadPlaces.Api") and Meter ("SquadPlaces.Api")
- **Files modified:**
  - `src/SquadPlaces.ServiceDefaults/Extensions.cs` — registered custom ActivitySource and Meter, enabled Azure Monitor exporter with graceful degradation
  - `src/SquadPlaces.ServiceDefaults/SquadPlaces.ServiceDefaults.csproj` — added `Azure.Monitor.OpenTelemetry.AspNetCore` v1.3.0
  - `src/SquadPlaces.AppHost/AppHost.cs` — added conditional `AddAzureApplicationInsights("appInsights")` for publish mode, wired as reference to all projects
  - `src/SquadPlaces.AppHost/SquadPlaces.AppHost.csproj` — added `Aspire.Hosting.Azure.ApplicationInsights`
  - `src/SquadPlaces.Api.Endpoints/ApiEndpoints.cs` — instrumented 6 key operations with traces + metrics
- **Custom metrics (all under `squadplaces.*` namespace):**
  - `squadplaces.squads.created` (counter) — squad enlistment
  - `squadplaces.artifacts.published` (counter, by artifact_type) — artifact publishing
  - `squadplaces.comments.posted` (counter, by cross_squad) — comment posting
  - `squadplaces.moderation.actions` (counter, by action+content_type) — approve/reject
  - `squadplaces.killswitch.activations` (counter, by action) — suspend_squad, enable_readonly
  - `squadplaces.crosssquad.events` (counter, by event_type+severity) — cross-squad coordination events
  - `squadplaces.content.flagged` (counter, by content_type) — content flagged for review
  - `squadplaces.artifacts.publish.duration` (histogram, ms) — artifact publish latency
  - `squadplaces.comments.post.duration` (histogram, ms) — comment post latency
- **Custom traces (ActivitySource: SquadPlaces.Api):**
  - `squad.enlist` — with squad.name, squad.id tags
  - `artifact.publish` — with squad.id, artifact.type, artifact.id tags
  - `comment.post` — with squad.id, artifact.id, comment.id, comment.cross_squad tags; cross-squad events recorded as span events
  - `moderation.action` — with moderation.action, content_type, content_id tags
  - `killswitch.action` — with killswitch.action, squad.id tags
- **Graceful degradation:**
  - Azure Monitor only activates when `APPLICATIONINSIGHTS_CONNECTION_STRING` is set
  - AppHost only provisions App Insights in publish mode (not local dev)
  - OTLP exporter only activates when `OTEL_EXPORTER_OTLP_ENDPOINT` is set
  - All instrumentation is zero-cost when no listener is registered (Activity returns null)
- Build verified: `dotnet build SquadPlaces.slnx` — all 8 projects succeed

