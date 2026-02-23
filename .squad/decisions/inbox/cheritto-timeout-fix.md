# Decision: Make sendAndWait timeout configurable

**Author:** Cheritto (TUI Engineer)
**Date:** 2026-02-23
**Status:** Implemented
**Issue:** #325
**PR:** #347

## Context

The `awaitStreamedResponse()` function in the shell had a hard-coded `120_000ms` (2-minute) timeout on `session.sendAndWait()`. Long-running agent conversations were being killed prematurely.

## Decision

1. Added `TIMEOUTS.SESSION_RESPONSE_MS` to `packages/squad-sdk/src/runtime/constants.ts` — default 600,000ms (10 minutes), overridable via `SQUAD_SESSION_TIMEOUT_MS` env var.
2. Updated `packages/squad-cli/src/cli/shell/index.ts` to import and use `TIMEOUTS.SESSION_RESPONSE_MS` instead of the magic number.
3. Updated all 6 test assertions in `test/repl-streaming.test.ts` to reference the constant.

## Impact

- Users can now set `SQUAD_SESSION_TIMEOUT_MS=900000` for 15-minute timeouts (or any value).
- Default is 5x longer than before (10 min vs 2 min), eliminating the P0 blocker.
- All 41 streaming tests pass. Both SDK and CLI build clean.
