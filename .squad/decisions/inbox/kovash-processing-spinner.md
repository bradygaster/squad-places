# Decision: Expose setProcessing on ShellApi

**By:** Kovash
**Date:** 2026-03-01
**Context:** Init auto-cast path bypassed App.tsx handleSubmit, so processing state was never set — spinner invisible during team casting.

## Decision

`ShellApi` now exposes `setProcessing(processing: boolean)` so that any code path in index.ts that triggers async work outside of `handleSubmit` can properly bracket it with processing state. This enables ThinkingIndicator and InputPrompt spinner without duplicating React state management.

## Rule

Any new async dispatch path in index.ts that bypasses `handleSubmit` **must** call `shellApi.setProcessing(true)` before the async work and `shellApi.setProcessing(false)` in a `finally` block covering all exit paths.

## Files Changed

- `packages/squad-cli/src/cli/shell/components/App.tsx` — added `setProcessing` to ShellApi interface + wired in onReady
- `packages/squad-cli/src/cli/shell/index.ts` — added setProcessing calls in handleInitCast (entry, pendingCastConfirmation return, finally block)
