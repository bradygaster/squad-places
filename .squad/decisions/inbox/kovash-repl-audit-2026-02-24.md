# REPL Experience Audit — Kovash — 2026-02-24

**Context:** Brady directive — "I'm shocked there are no REPL end-to-end integration tests." Full audit of the 2-minute timeout and dead air moments.

## Executive Summary

Conducted comprehensive REPL audit focusing on timeout handling, SDK connection latency, and streaming pipeline. Filed **7 GitHub issues** (#418, #425, #428, #430, #432, #433, #434) with detailed reproduction steps, root cause analysis, and proposed fixes.

**Critical Findings:**
1. **10-minute timeout too aggressive** — all operations (simple queries, complex refactors) share same timeout, causing premature failures
2. **Cold SDK connection dead air** — 3-5 second freeze on first message with no user feedback
3. **Input buffering drops keystrokes** — backspace/arrows silently ignored during agent processing
4. **Coordinator streaming invisible** — message_delta events accumulate but never render to user
5. **Ghost response retry exhausts 40+ minutes** — auto-retries 3x without user consent or progressive warnings
6. **Zero E2E integration tests** — 106 component tests but no full user flow coverage
7. **No cancel mechanism** — Ctrl+C exits entire shell, SDK abort() exists but never called

## Issues Filed

### #418 — 10-minute timeout too aggressive
- **Location:** `packages/squad-cli/src/cli/shell/index.ts:175` — `TIMEOUTS.SESSION_RESPONSE_MS`
- **Impact:** Users lose 40+ minutes on failed operations (10min × 4 attempts)
- **Proposed:** Contextual timeouts (simple: 2min, complex: 30min), progressive warnings, Ctrl+X cancel

### #425 — Cold SDK connection dead air
- **Location:** `packages/squad-sdk/src/adapter/client.ts:353` — `await this.client.start()`
- **Impact:** First message hits 3-5 second freeze with no feedback
- **Proposed:** Pre-warm connection during banner animation OR show "Connecting..." in ThinkingIndicator

### #428 — Input buffering drops keystrokes
- **Location:** `packages/squad-cli/src/cli/shell/components/InputPrompt.tsx:52-63`
- **Impact:** Backspace/arrows silently ignored during processing → users retype
- **Proposed:** Full buffer with backspace support OR block all input with clear message

### #430 — Coordinator streaming invisible
- **Location:** `packages/squad-cli/src/cli/shell/index.ts:336` + `MessageStream.tsx:122-133`
- **Impact:** 10-30 seconds of silence during coordinator routing decisions
- **Root Cause:** Coordinator not in SessionRegistry → roleMap lookup fails → no streaming content rendered
- **Proposed:** Register coordinator in registry OR special-case in MessageStream

### #432 — Ghost response retry exhaustion
- **Location:** `packages/squad-cli/src/cli/shell/index.ts:74-104` — `withGhostRetry()`
- **Impact:** Auto-retries 3x without user consent, wastes 40+ minutes on failures
- **Proposed:** Progressive warnings (2min, 5min, 8min), prompt before retry, Ctrl+X cancel

### #433 — Missing E2E integration tests
- **Current:** 106 component tests, 29 streaming unit tests, 32 shell integration tests
- **Missing:** Full user flows (cold start, timeout, buffering, coordinator routing, error recovery)
- **Proposed:** Test harness with programmatic shell API, mocked SDK, time control, snapshot testing

### #434 — No cancel mechanism
- **Location:** `packages/squad-cli/src/cli/shell/components/App.tsx:91-95` — Ctrl+C → `exit()`
- **Impact:** Users forced to wait for timeout or lose entire session
- **Root Cause:** SDK exposes `session.abort()` but shell never calls it, Ctrl+C wired to exit not cancel
- **Proposed:** Ctrl+X (or smart Ctrl+C) to cancel operation, wire to SDK abort(), keep shell open

## Architecture Observations

### Timeout Architecture
- **Constants:** `TIMEOUTS.SESSION_RESPONSE_MS` = 600,000ms (10min) in `packages/squad-sdk/src/runtime/constants.ts:66`
- **Override:** Env var `SQUAD_SESSION_TIMEOUT_MS` already exists but undocumented
- **Usage:** `sendAndWait({ prompt }, TIMEOUTS.SESSION_RESPONSE_MS)` in shell/index.ts:175
- **Problem:** Single global timeout for all operation types

### SDK Connection Flow
1. `SquadClient` created in `runShell()` with `autoStart: true` (default)
2. First `createSession()` call triggers lazy `connect()` → `await client.start()`
3. `client.start()` takes 3-5 seconds (measured, logs warning if > 2s)
4. No user feedback during connection — ThinkingIndicator not wired to connection state

### Streaming Pipeline
1. `sendAndWait()` registers `message_delta` handler BEFORE sending prompt
2. Handler calls `extractDelta()` → accumulates in local `accumulated` variable
3. Handler calls `shellApi?.setStreamingContent({ agentName, content })` → triggers Ink re-render
4. **MessageStream only renders** if `roleMap.has(streamingContent.agentName)` — coordinator fails this check
5. After `sendAndWait()` returns, fallback content from `result.data.content` used if deltas empty

### Ghost Response Logic
1. `withGhostRetry()` wraps `sendFn` (which calls `awaitStreamedResponse`)
2. Retries on empty string result (ghost response)
3. Backoff: [1000ms, 2000ms, 4000ms]
4. Max retries: 3
5. Callbacks: `onRetry` (shows system message), `onExhausted` (shows failure message)
6. **Problem:** No way to interrupt, no warnings during the 10-minute wait

## Test Coverage Gaps

### Existing Tests (Good Coverage)
- ✅ `test/repl-ux.test.ts` (106 tests) — Component rendering with ink-testing-library
- ✅ `test/repl-streaming.test.ts` (29 tests) — Streaming pipeline unit tests
- ✅ `test/shell-integration.test.ts` (32 tests) — Lifecycle, routing, coordinator parsing
- ✅ `test/ghost-response.test.ts` — Ghost response retry logic
- ✅ `test/e2e-integration.test.ts` (4 REPL tests) — Partial E2E with mocked SDK

### Missing Tests (Critical Gaps)
- ❌ Cold start flow: Launch → SDK connect → banner → first message → response
- ❌ Timeout scenario: Message exceeds timeout → retry → warnings → failure
- ❌ Input buffering: Type during processing → buffer → restore → submit
- ❌ Coordinator routing: Message → coordinator streams → decision → agent dispatch
- ❌ Error recovery: SDK disconnect → reconnect → degradation → feedback
- ❌ Parallel dispatch: Coordinator routes to 3 agents → all stream → all complete

## Key Files Audited

### Shell Core
- `packages/squad-cli/src/cli/shell/index.ts` — Main shell entry, dispatch logic, timeout handling
- `packages/squad-cli/src/cli/shell/lifecycle.ts` — Initialization, team discovery, shutdown
- `packages/squad-cli/src/cli/shell/coordinator.ts` — Routing decision parsing
- `packages/squad-cli/src/cli/shell/router.ts` — Input parsing (@agent syntax)
- `packages/squad-cli/src/cli/shell/stream-bridge.ts` — Event→callback bridge

### Ink Components
- `packages/squad-cli/src/cli/shell/components/App.tsx` — Main shell UI, state management
- `packages/squad-cli/src/cli/shell/components/MessageStream.tsx` — Message rendering, streaming cursor
- `packages/squad-cli/src/cli/shell/components/InputPrompt.tsx` — Input handling, buffering
- `packages/squad-cli/src/cli/shell/components/ThinkingIndicator.tsx` — Spinner, elapsed time, activity hints
- `packages/squad-cli/src/cli/shell/components/AgentPanel.tsx` — Team roster, status indicators

### SDK Adapter
- `packages/squad-sdk/src/adapter/client.ts` — SquadClient, connection lifecycle, reconnection
- `packages/squad-sdk/src/runtime/constants.ts` — TIMEOUTS configuration

## Recommendations

### Immediate (P0)
1. **Fix coordinator streaming visibility** (#430) — Register coordinator in SessionRegistry OR special-case in MessageStream
2. **Add progressive timeout warnings** (#432) — ThinkingIndicator shows 2min, 5min, 8min markers
3. **Document SQUAD_SESSION_TIMEOUT_MS env var** — Users need override for long operations

### Short-term (P1)
4. **Fix input buffering** (#428) — Support backspace/arrows OR block all input with clear feedback
5. **Pre-warm SDK connection** (#425) — Call `client.connect()` during shell init (hide 3-5s latency)
6. **Implement E2E test harness** (#433) — Programmatic shell API for full user flow testing

### Long-term (P2)
7. **Contextual timeouts** (#418) — Operation-based timeout tiers (simple: 2min, complex: 30min)
8. **Cancel mechanism** — Expose `session.abort()` via Ctrl+X
9. **User consent for retries** — Prompt before each retry instead of auto-retry 3x

## Decision

All issues filed with detailed technical analysis. Awaiting Brady's prioritization.

**Signed:** Kovash (REPL & Interactive Shell Expert)
**Date:** 2026-02-24
