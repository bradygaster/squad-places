# Project Context

- **Owner:** Brady
- **Project:** squad-sdk — the programmable multi-agent runtime for GitHub Copilot (v1 replatform)
- **Stack:** TypeScript (strict mode, ESM-only), Node.js ≥20, @github/copilot-sdk, Vitest, esbuild
- **Created:** 2026-02-21

## Learnings

### From Beta (carried forward)
- Event-driven over polling: always prefer event-based patterns
- Streaming-first: async iterators over buffers — this is a core design principle
- Graceful degradation: if one session dies, others survive
- Node.js ≥20: use modern APIs (structuredClone, crypto.randomUUID, fetch, etc.)
- ESM-only: no CJS shims, no dual-package hazards
- Cost tracking and telemetry: runtime performance is a feature, not an afterthought

### Issue #239: StreamingPipeline Bridge + Console Renderer
- **StreamBridge** (`src/cli/shell/stream-bridge.ts`): Callback-based bridge connecting `StreamingPipeline` events to shell rendering. Accumulates `message_delta` chunks in per-session buffers, dispatches `usage` and `reasoning_delta` events to optional callbacks, and `flush()` finalizes buffered content into `ShellMessage` objects while updating `SessionRegistry` status.
- **ShellRenderer** (`src/cli/shell/render.ts`): Pre-ink console renderer using `process.stdout.write()` for streaming deltas and `console.log()` for complete messages. Tracks current agent to avoid redundant headers during contiguous streaming from the same agent.
- **Design pattern**: The bridge is event-sink only — it receives `StreamingEvent` from the pipeline but does not subscribe itself. The caller (shell entry point) is responsible for wiring `pipeline.onDelta()` → `bridge.handleEvent()`. This keeps the bridge testable without a live pipeline.
- **Key type alignment**: `StreamingEvent` is a union of `StreamDelta | UsageEvent | ReasoningDelta` — no `stream_end` or `stream_error` variants exist. Stream completion is signaled externally via `flush()`.

### Issue #240: Shell Session Lifecycle Management
- **ShellLifecycle** (`src/cli/shell/lifecycle.ts`): Manages the full shell session lifecycle — initialization, message history tracking, state transitions, and graceful shutdown.
- **Initialization**: Verifies `.squad/` exists at `teamRoot`, reads `team.md`, parses the Members markdown table to discover agents (name, role, charter path, status). Registers all `Active` agents in `SessionRegistry`.
- **Message history**: Tracks user, agent, and system messages with timestamps. Supports filtering by agent name. State object always gets a shallow copy of the history array (immutable external view).
- **Team manifest parsing**: `parseTeamManifest()` is a local function that extracts agent rows from the `## Members` markdown table. Handles emoji-prefixed status fields (e.g. "✅ Active" → "Active").
- **State machine**: `initializing` → `ready` (on success) or `error` (on missing `.squad/` or `team.md`). Shutdown transitions back through `initializing` while clearing all state.
- **PR**: #287

### 📌 Team update (2026-02-22T08:50:00Z): Runtime EventBus as canonical bus for orchestration classes — decided by Fortier
runtime/event-bus.ts (colon-notation: session:created, subscribe() API, error isolation) is canonical for orchestration classes. client/event-bus.ts (dot-notation, on() API) remains for backward-compat but shouldn't be used in new code. Coordinator and RalphMonitor now import from runtime/event-bus. All new EventBus consumers follow this pattern.

### Coordinator + Ralph Runtime Stubs
- **Coordinator** (`src/coordinator/index.ts`): Legacy Coordinator class fully implemented — constructor accepts optional `CoordinatorDeps` (client, eventBus, agentManager, hookPipeline, toolRegistry), `initialize()` subscribes to lifecycle events via RuntimeEventBus, `route()` classifies messages by tier (direct/standard/full), `execute()` emits `coordinator:routing` events, `shutdown()` unsubscribes and nulls references.
- **RalphMonitor** (`src/ralph/index.ts`): Event-driven work monitor — `start()` subscribes to session lifecycle + milestone events, `handleEvent()` maintains per-agent work status map, `healthCheck()` flags stale sessions beyond configurable threshold (default 5min), `stop()` persists state to JSON file if `statePath` configured.
- **EventBus import alignment**: Both Coordinator and Ralph switched from `client/event-bus.js` (dot-notation types, no error isolation) to `runtime/event-bus.js` (colon-notation types, `executeHandler()` with try/catch). This aligns with SquadCoordinator tests pattern.
- **CastingRegistry.load()**: Implemented `registry.json` parsing — reads from `castingDir/registry.json`, populates entries map by role.
- **Key pattern**: All EventBus subscriptions return unsubscribe functions stored in `unsubscribers[]` array. Shutdown iterates and calls them all — no dangling listeners.

### Issue #255 + #256: OTel Provider Init & Bridge
- **otel.ts** (`src/runtime/otel.ts`): Full OTel provider initialization using `@opentelemetry/sdk-node`'s `NodeSDK`. Exposes `initializeTracing()`, `initializeMetrics()`, `initializeOTel()`, `shutdownOTel()`, `getTracer()`, `getMeter()`. Config priority: explicit config → `OTEL_EXPORTER_OTLP_ENDPOINT` env var → disabled (no-op). Uses `NodeSDK` as unified provider manager to avoid OTel sub-package version skew.
- **otel-bridge.ts** (`src/runtime/otel-bridge.ts`): `createOTelTransport()` returns a `TelemetryTransport` function that converts `TelemetryEvent` instances to OTel spans. Maps all five event types (`squad.init`, `squad.agent.spawn`, `squad.error`, `squad.run`, `squad.upgrade`) to named spans. Error events get `SpanStatusCode.ERROR` + exception event. Bridge is additive — existing transport pipeline untouched.
- **OTel version skew**: The OTel npm ecosystem has pervasive version fragmentation between `sdk-trace-base`, `sdk-trace-node`, `sdk-metrics`, and `resources`. Using `NodeSDK` from `@opentelemetry/sdk-node` and re-exporting its bundled `Resource` and `PeriodicExportingMetricReader` avoids type conflicts. Do NOT install `@opentelemetry/sdk-trace-base`, `@opentelemetry/sdk-metrics`, or `@opentelemetry/resources` as direct deps — let `sdk-node` manage them.
- **Resource attributes**: `service.name` (hardcoded string, not `ATTR_SERVICE_NAME` — avoids `@opentelemetry/semantic-conventions` version issues) and `squad.version` (read from package.json via `createRequire`).

### Issues #259 + #264: Session Traces + Response Latency Metrics
- **SquadClient.sendMessage()** (`src/adapter/client.ts`): New method wrapping `session.sendMessage()` with `squad.session.message` parent span and `squad.session.stream` child span. Stream span emits `first_token`, `last_token`, `stream_error` events and records `tokens.input`, `tokens.output`, `duration_ms` attributes. Temporary event listeners on the session track first delta and usage data, cleaned up in `finally`.
- **SquadClient.closeSession()** (`src/adapter/client.ts`): Traced alias for `deleteSession` — creates a `squad.session.close` span wrapping the existing delete span. Provides semantic clarity for callers closing sessions vs deleting them.
- **StreamingPipeline latency wiring** (`src/runtime/streaming.ts`): `markMessageStart(sessionId)` records a high-res timestamp. On first `message_delta` with `index === 0`, calls `recordTimeToFirstToken()`. On `usage` event, calls `recordResponseDuration()` and `recordTokensPerSecond()` (output tokens / elapsed * 1000). Tracking state auto-cleans after usage event. `clear()` resets tracking maps.
- **Design decision**: Latency tracking is opt-in via `markMessageStart()` — pipeline doesn't assume a send happened. This keeps the pipeline testable without mocking timers and avoids coupling to SquadClient.
- **Tests**: 15 new tests in `test/session-traces.test.ts` covering sendMessage span creation, closeSession alias, TTFT recording, duration recording, tokens/sec calculation, and cleanup semantics.

### 📌 Team update (2026-02-22T093300Z): OTel Phase 2 complete — session traces, latency metrics, tool enhancements, agent metrics, token usage wiring, metrics tests — decided by Fortier, Fenster, Edie, Hockney
All four agents shipped Phase 2 in parallel: Fortier wired TTFT/duration/throughput metrics. Fenster established tool trace patterns and agent metric wiring conventions. Edie wired token usage and session pool metrics. Hockney created spy-meter test pattern (39 new tests). Total: 1940 tests passing, metrics ready for production telemetry.

### Issue #303: REPL Shell Coordinator Wiring
- **App.tsx** (`src/cli/shell/components/App.tsx`): Replaced placeholder stubs for `direct_agent` and `coordinator` routing paths with real `onDispatch` callback prop. The component is now purely UI — all SDK integration lives in the shell entry point. Graceful degradation: when no dispatch function is available, shows a clear error message instead of silent no-op.
- **Shell entry point** (`src/cli/shell/index.ts`): Full coordinator wiring — creates `SquadClient`, manages per-agent `SquadSession` instances (lazy creation, reuse on subsequent messages), and a coordinator session for unaddressed messages. Streaming is event-driven: `session.on('message_delta')` feeds accumulated deltas to `shellApi.setStreamingContent()` for live rendering, with cleanup in `finally` blocks.
- **Coordinator routing**: After coordinator session completes, `parseCoordinatorResponse()` classifies the response (DIRECT/ROUTE/MULTI). ROUTE decisions auto-dispatch to agent sessions. MULTI decisions dispatch in parallel via `Promise.allSettled()`.
- **Lifecycle initialization**: `ShellLifecycle.initialize()` now runs on shell start to discover agents from `team.md` and populate the `SessionRegistry`, enabling `@Agent` direct addressing.
- **Cleanup**: On shell exit, all SDK sessions are closed and client disconnected (best-effort, errors swallowed).
- **TypeScript closure caveat**: TS control flow analysis can't track `coordinatorSession` mutation across async closures. Worked around with explicit `as SquadSession | null` assertion at cleanup point.
- **PR**: #303

### Wave 2: REPL Shell Polish — "The Moment"
- **Welcome Header** (`App.tsx`): Replaced bare `Squad v{version}` header with a rich welcome banner using `borderStyle="round"`. Shows brand mark (◆ SQUAD), version, project description parsed from `team.md`, full team roster with role emoji, current focus from `.squad/identity/now.md`, and quick-start hints. Data loaded via new `loadWelcomeData()` in lifecycle.ts — filesystem reads in a `useEffect`, non-blocking, fail-safe.
- **lifecycle.ts additions**: `getRoleEmoji(role)` maps 17 role strings to emoji with `🔹` fallback. `loadWelcomeData(teamRoot)` reads `team.md` + `now.md` in one call, returns `WelcomeData` (projectName, description, agents with emoji, focus). Both exported for use in UI components. Reuses existing private `parseTeamManifest()` internally.
- **AgentPanel** (`AgentPanel.tsx`): Replaced vertical status-dot list with compact inline layout using `flexWrap="wrap"`. Each agent shows role emoji + name, active agents are bold green with `●` indicator, error agents shown in red with `✖`. Streaming agents get a "💭 responding..." line. Idle state shows "Team idle — ready for work".
- **MessageStream** (`MessageStream.tsx`): Three-tier message styling — user messages are bold cyan (`❯ you:`), system messages are dimmed (`⚙ system:`), agent messages get role emoji prefix in green. Thin `─` separator lines between conversation turns. New `ThinkingIndicator` component: animated braille spinner (⠋⠙⠹⠸⠼⠴⠦⠧⠇⠏) at 80ms interval, shown when processing but no streaming content yet. Detects @Agent from last user message to show "Fenster is thinking..." or "Routing your request..." for coordinator paths.
- **InputPrompt** (`InputPrompt.tsx`): Dynamic prompt — `squad>` when idle, `squad (streaming)>` when processing. Yellow color during processing, cyan when idle. Added `marginTop={1}` for visual breathing room.
- **Design decisions**: Kept all data loading inside components (no index.ts changes). Welcome data loaded once in `useEffect` — synchronous filesystem reads are fine for one-time startup in a Node.js CLI. ThinkingIndicator uses `setInterval` at 80ms (~12 fps) — minimal event loop impact. Agent role-to-emoji mapping lives in lifecycle.ts alongside team manifest parsing, keeping all team-data concerns in one module.
