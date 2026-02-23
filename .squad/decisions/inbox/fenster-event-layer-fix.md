# Decision: Adapter Event Name Mapping and Data Normalization

**Date:** 2026-02-22  
**By:** Fenster (Core Dev)  
**Issues:** #316, #317, #319  
**Status:** Implemented

## Context

The `CopilotSessionAdapter` in `packages/squad-sdk/src/adapter/client.ts` mapped methods (`sendMessage` → `send`, `close` → `destroy`) but did NOT map event names or event data shapes. The `@github/copilot-sdk` uses dotted-namespace event types (e.g., `assistant.message_delta`, `assistant.usage`) and wraps payloads in an `event.data` envelope. Our adapter passed short names like `message_delta` and `usage` directly through, causing handlers to silently never fire. The OTel telemetry in `sendMessage()` was therefore dead code — `first_token` never recorded, `tokens.input`/`tokens.output` never populated.

## Decision

1. **Event name mapping via `EVENT_MAP`**: A static `Record<string, string>` maps 10 Squad short names to SDK dotted names. The reverse map is computed automatically. Unknown names pass through unchanged, so callers CAN use the full SDK name directly if they prefer.

2. **Event data normalization via `normalizeEvent()`**: The adapter wraps every handler to flatten `event.data` onto the top-level `SquadSessionEvent` and maps the type back to the Squad short name. This means callers access `event.inputTokens` (not `event.data.inputTokens`) and check `event.type === 'usage'` (not `event.type === 'assistant.usage'`).

3. **Per-event-type unsubscribe tracking**: Changed `unsubscribers` from `Map<handler, unsubscribe>` to `Map<handler, Map<eventType, unsubscribe>>` so the same handler function can be subscribed to multiple event types without the second `on()` overwriting the first's unsubscribe function.

## Alternatives Considered

- **No mapping layer (callers use SDK names directly):** Rejected — breaks the adapter's purpose of decoupling Squad from SDK internals. Also requires changing all callers (violates constraint).
- **Transform at dispatch instead of subscribe:** Would require intercepting `_dispatchEvent` on the inner session, which is fragile and relies on SDK internals.

## Impact

- Zero changes to callers — the adapter normalizes everything transparently
- Zero changes to `SquadSession` interface or `SquadSessionEventType`
- OTel stream tracking now works end-to-end through the adapter
- Future SDK event name changes only require updating `EVENT_MAP`
