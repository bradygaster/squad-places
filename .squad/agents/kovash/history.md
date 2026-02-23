# Kovash — History

## Project Context

- **Project:** Squad — the programmable multi-agent runtime for GitHub Copilot
- **Owner:** Brady
- **Stack:** TypeScript (strict mode, ESM-only), Node.js ≥20, @github/copilot-sdk, Vitest, esbuild
- **My focus:** REPL shell in `packages/squad-cli/src/cli/shell/`

## Learnings

- Joined session 2026-02-23. Hired to fix REPL bug: messages immediately show "coordinator:" with empty content.
- Known issue: `dispatchToCoordinator` calls `sendMessage()` (wraps SDK `send()`) which may be fire-and-forget, not blocking until streaming completes.
- `CopilotSessionAdapter` maps `sendMessage()` → `inner.send()`, event names via EVENT_MAP.
- Shell modules: index.ts (entry), coordinator.ts, router.ts, spawn.ts, sessions.ts, render.ts, stream-bridge.ts, lifecycle.ts, memory.ts, terminal.ts, autocomplete.ts, commands.ts, types.ts
- Ink components: App.tsx, AgentPanel.tsx, MessageStream.tsx, InputPrompt.tsx
- **2026-02-23 FIX:** Streaming dispatch bug resolved. Root cause confirmed: `sendMessage()` wraps SDK `send()` which is fire-and-forget — resolves before streaming completes. Fix: introduced `awaitStreamedResponse()` helper that uses `sendAndWait()` (blocks until session idle, 120s timeout). Fallback path listens for `turn_end`/`idle` events when `sendAndWait` is unavailable.
- SDK event mapping: `message_delta` → `assistant.message_delta`, `turn_end` → `assistant.turn_end`, `idle` → `session.idle` (see CopilotSessionAdapter.EVENT_MAP).
- `sendAndWait` is optional on `SquadSession` interface (`sendAndWait?`) but always implemented in `CopilotSessionAdapter`.
- `message_delta` events carry content in `deltaContent` key (SDK actual field from `assistant.message_delta`). `extractDelta()` checks `deltaContent` > `delta` > `content` for backward compat.
- Test file `test/repl-streaming.test.ts` covers the streaming pipeline end-to-end (29 tests).
- **FIX (deltaContent):** `extractDelta` was checking `event['delta']` and `event['content']`, but the SDK's `assistant.message_delta` after `normalizeEvent()` spreads `data.deltaContent` onto the event. Fixed priority to `deltaContent` > `delta` > `content`.
- **FIX (sendAndWait fallback):** `awaitStreamedResponse` now returns the full response content from `sendAndWait` result (`result.data.content`) as a fallback string. Both `dispatchToCoordinator` and `dispatchToAgent` use this fallback if delta accumulation yielded empty string.
- Added `dev:link` / `dev:unlink` scripts to root `package.json` for local npm link workflow.
- **OTEL REPL wiring:** `runShell()` in `packages/squad-cli/src/cli/shell/index.ts` now calls `initSquadTelemetry({ serviceName: 'squad-cli' })` at startup and `telemetry.shutdown()` at cleanup. Telemetry active message goes to stderr via `console.error` to avoid interfering with Ink rendering. ShellRenderer has no `info` method — only `renderDelta`, `renderMessage`, `renderSystem`, `renderError`.
- **OTEL resilience:** Wrapped `_sdk.start()` in `packages/squad-sdk/src/runtime/otel.ts` with try/catch so gRPC exporter connection failures (e.g. Aspire not running) are non-fatal.
- **VS Code launch.json:** Added `OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317` env var to "Squad REPL" and "Squad REPL (rebuild first)" configurations. Not added to npm scripts — env is set per-launch or via shell profile.
- **2026-02-23 DIAGNOSTIC SESSION:** Brady reports REPL still broken despite `deltaContent` fix. Compiled dist verified — `deltaContent` IS present in `extractDelta`. Deep-dived into SDK internals (`@github/copilot-sdk/dist/session.js`): `sendAndWait` uses catch-all `this.on((event) => ...)` to track `assistant.message` and `session.idle`, returns `lastAssistantMessage`. SDK's `_dispatchEvent` silently swallows handler errors (try/catch with empty catch). Both typed and wildcard handlers fire for each event.
- **SQUAD_DEBUG logging:** Added `debugLog()` helper gated on `process.env['SQUAD_DEBUG'] === '1'`. Logs to stderr at: dispatchToCoordinator entry, onDelta fire, extractDelta key inspection, awaitStreamedResponse result shape, accumulated vs fallback lengths, parseCoordinatorResponse decision type. Same for dispatchToAgent.
- **.env loading:** Added lightweight `.env` file parser to `cli-entry.ts` (no dotenv dependency). Reads `key=value` pairs from `.env` in cwd, only sets if key not already in `process.env`. Runs before all other imports.
- **VS Code launch.json:** Added `envFile` property pointing to `${workspaceFolder}/.env` for both "Squad REPL" and "Squad REPL (rebuild first)" debug configurations.
- **SDK event internals:** SDK `_dispatchEvent` dispatches typed handlers first (from `typedEventHandlers` Map), then wildcard handlers (from `eventHandlers` Set). Both paths swallow errors silently. The `sendAndWait` return value is `lastAssistantMessage` (the `assistant.message` event) — can be `undefined` if `session.idle` fires before `assistant.message` or if no message is produced.

---

📌 Team update (2026-02-23T09:25Z): Streaming diagnostics infrastructure complete — SQUAD_DEBUG logging added, .env setup, OTel REPL wiring, version bump to 0.8.5.1. Hockney identified root cause of silent ghost response (empty sendAndWait + empty deltas). Saul fixed OTel protocol to gRPC. — decided by Scribe
- **2026-02-24 UX OVERHAUL:** Overhauled all 4 Ink components for "never feel dead" UX per Brady's request. ThinkingIndicator now shows elapsed time (updated every second), phase transitions (Connecting→Routing→Streaming), and color cycling (cyan→yellow→magenta). AgentPanel has PulsingDot animation on active agents, per-agent elapsed time, count summary, and dotted separator. MessageStream shows response duration timestamps, wider horizontal rules (50 chars), system messages with ◇ icon, user messages in full cyan. InputPrompt has integrated spinner during processing (◆ squad ⠸>), placeholder hint when empty. App header shows team count + active count, keyboard shortcuts, fallback tagline for no-squad. All changes confined to `components/` — no external API changes. Build clean, 157/157 tests pass.
