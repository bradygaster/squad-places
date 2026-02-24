# Kovash — History

## Project Context

- **Project:** Squad — the programmable multi-agent runtime for GitHub Copilot
- **Owner:** Brady
- **Stack:** TypeScript (strict mode, ESM-only), Node.js ≥20, @github/copilot-sdk, Vitest, esbuild
- **My focus:** REPL shell in `packages/squad-cli/src/cli/shell/`

## Core Context

**REPL Streaming & Diagnostics (Feb 23–24):** Fixed empty-response streaming bug (root cause: `dispatchToCoordinator` fire-and-forget, fixed with `awaitStreamedResponse()` using `sendAndWait()` with 120s timeout). Fixed `deltaContent` SDK event key priority. Added OTel REPL wiring with resilience (try/catch gRPC failures). Added SQUAD_DEBUG logging infrastructure and .env file parser (no dotenv dep). Added VS Code launch.json OTEL_EXPORTER_OTLP_ENDPOINT config. UX overhaul: ThinkingIndicator with elapsed time + phase transitions, AgentPanel animations, MessageStream duration + wider rules, InputPrompt spinner. Comprehensive REPL audit filed 7 issues (#418, #425, #428, #430, #432, #433, #434). Fixed SDK connection dead air (#420/#425) with early activity hints + setImmediate. Fixed input buffering race (#428) with pendingInputRef queue. Fixed ghost retry messaging (#432) with clearer attempt counts. Fixed timeout progress (#418) with periodic "Still working..." hints. Verified coordinator streaming already correct (#430). All 157+ tests passing.

## Learnings

### SDK & Shell Architecture
- `CopilotSessionAdapter` maps `sendMessage()` → `inner.send()`, event names via EVENT_MAP.
- Shell modules: index.ts (entry), coordinator.ts, router.ts, spawn.ts, sessions.ts, render.ts, stream-bridge.ts, lifecycle.ts, memory.ts, terminal.ts, autocomplete.ts, commands.ts, types.ts
- Ink components: App.tsx, AgentPanel.tsx, MessageStream.tsx, InputPrompt.tsx
- SDK event mapping: `message_delta` → `assistant.message_delta`, `turn_end` → `assistant.turn_end`, `idle` → `session.idle`
- `sendAndWait` is optional on `SquadSession` interface (`sendAndWait?`) but always implemented in `CopilotSessionAdapter`
- `message_delta` events carry content in `deltaContent` key (priority: `deltaContent` > `delta` > `content`)
- SDK `_dispatchEvent` dispatches typed handlers first, then wildcard handlers; both swallow errors silently
- `awaitStreamedResponse()` returns full response content from `sendAndWait` result as fallback if delta accumulation empty
- Pattern: `dev:link` / `dev:unlink` scripts for local npm link workflow

### Recent Work Examples (#437, #440, #441, #442)

---

📌 Team update (2026-02-23T09:25Z): Streaming diagnostics infrastructure complete — SQUAD_DEBUG logging added, .env setup, OTel REPL wiring, version bump to 0.8.5.1. Hockney identified root cause of silent ghost response (empty sendAndWait + empty deltas). Saul fixed OTel protocol to gRPC. — decided by Scribe
- **PR #437 (SDK CONNECTION):** Immediate activity hints before createSession() blocks, setImmediate for render tick, "Connecting to SDK..." → "Routing..." transitions
- **PR #440 (INPUT BUFFERING + TIMEOUT PROGRESS):** Concurrent pendingInputRef queue for race conditions, periodic "Still working..." every 30s via setActivityHint()
- **PR #441 (GHOST RETRY):** Clearer messaging with (attempt N/totalAttempts) format, changed "No response" → "Empty response detected"
- **PR #442 (COORDINATOR STREAMING):** Verified wiring is correct, added diagnostic logging for session creation + listener lifecycle

---

📌 Team update (2026-02-24T07:20:00Z): Wave D Batch 1 work filed (#488–#493). Cheritto: #488–#490 (UX precision — status display, keyboard hints, error recovery). Kovash: #491–#492 (hardening — message history cap, per-agent streaming). Fortier: #493 (streamBuffer cleanup on error). See .squad/decisions.md for details. — decided by Keaton

