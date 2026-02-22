# Decision: OTel Metric Wiring Pattern (#261, #263)

**Author:** Edie  
**Date:** 2026-02-22  
**Status:** Implemented  

## Context

Issues #261 and #263 required wiring pre-existing metric functions from `otel-metrics.ts` into the runtime (`StreamingPipeline`) and adapter (`SquadClient`).

## Decision

- **Token usage metrics** (`recordTokenUsage`) are recorded in `StreamingPipeline.processEvent()` AFTER dispatching to user-registered handlers. This ensures user handlers see the event before OTel instrumentation, and handler failures don't block metric recording.
- **Session pool metrics** are recorded at the innermost success/error boundary in `SquadClient`:
  - `recordSessionCreated()` after successful `client.createSession()` return
  - `recordSessionClosed()` after successful `client.deleteSession()` return
  - `recordSessionError()` at the top of inner catch blocks — recorded for EVERY failed attempt, including ones that trigger reconnection. This is intentional: a reconnect-eligible failure is still an error worth counting.
- No new exports needed — barrel and subpath exports were already wired in the Phase 1 otel-metrics scaffold.

## Rationale

Metric calls are no-ops when OTel is not configured (the meter returns no-op instruments), so this adds zero overhead for users without OTel. Recording errors before reconnect checks gives accurate failure counts without double-counting successes (the recursive retry gets its own `recordSessionCreated()` on success).
