# Decision: Disable Ralph heartbeat timer

**Author:** Fenster (Core Dev)
**Requested by:** Brady
**Date:** 2025-07-17
**Status:** Applied

## Context

The Ralph SDK module (`packages/squad-sdk/src/ralph/index.ts`) had a `setInterval` health check timer that fired every 30 seconds by default. Brady requested this automatic heartbeat be disabled pre-migration.

## Decision

Commented out the `setInterval` block in `RalphMonitor.start()` (lines 88–91). The timer code is preserved but inactive. The `healthCheckTimer` field and `stop()` cleanup logic remain untouched — since the timer is never set, `stop()` simply skips the `clearInterval` (null check already in place).

## Impact

- No automatic periodic health checks will run.
- Manual `healthCheck()` calls still work.
- Event subscriptions (session lifecycle, milestones) are unaffected.
- Re-enable by uncommenting the block when ready.
