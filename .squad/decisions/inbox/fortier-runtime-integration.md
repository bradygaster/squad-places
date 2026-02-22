# Decision: Runtime EventBus as canonical bus for orchestration classes

**By:** Fortier
**Date:** 2026-02-21
**Scope:** Coordinator, Ralph, and future orchestration components

## Decision
The `runtime/event-bus.ts` (colon-notation: `session:created`, `subscribe()` API, built-in error isolation via `executeHandler()`) is the canonical EventBus for all orchestration classes. The `client/event-bus.ts` (dot-notation: `session.created`, `on()` API) remains for backward-compat but should not be used in new code.

## Rationale
- Runtime EventBus has proper error isolation — one handler failure doesn't crash others
- SquadCoordinator (M3-1) tests already use RuntimeEventBus
- Consistent API surface (`subscribe`/`subscribeAll`/`unsubscribe`) is cleaner than `on`/`onAny`
- Event type strings use colon-notation which avoids ambiguity with property access patterns

## Impact
- Coordinator and RalphMonitor now import from `../runtime/event-bus.js`
- All new EventBus consumers should follow this pattern
- Client EventBus remains exported for external consumers
