# Decision: Eliminate unsafe casts in adapter layer

**By:** Edie
**Date:** 2026-02-23
**Closes:** #318, #320, #321, #322

## Context

The adapter layer (`packages/squad-sdk/src/adapter/client.ts`) had multiple type-safety gaps:

1. `listSessions()` used `as unknown as SquadSessionMetadata[]` — passing SDK data through without runtime mapping
2. `getStatus()`, `getAuthStatus()`, `listModels()` returned SDK results directly, relying on implicit `any`-to-typed coercion
3. `SquadClient.on()` used `as any` casts to bridge SDK and Squad event handler types
4. `SquadClientWithPool.on()` was fully untyped `(any, any)`
5. Dead `_squadOnMessage` reference in `sendMessage()` — never set, never used
6. `CopilotSessionAdapter` lacked `sendAndWait()`, `abort()`, `getMessages()` — SDK methods with no adapter surface

## Decision

### Field-mapping over unsafe casts (#318)

All SDK-to-Squad data boundaries now use explicit field-picking:

```typescript
const sessions = await this.client.listSessions();
return sessions.map((s): SquadSessionMetadata => ({
  sessionId: s.sessionId,
  startTime: s.startTime,
  ...
}));
```

This catches SDK type drift at compile time. Applied to `listSessions()`, `getStatus()`, `getAuthStatus()`, `listModels()`.

### Client event types (#321)

Created `SquadClientEventType`, `SquadClientEvent`, `SquadClientEventHandler` in `adapter/types.ts` — distinct from session-level event types. These mirror the SDK's `SessionLifecycleEventType` union (`session.created | session.deleted | session.updated | session.foreground | session.background`). Structural compatibility with the SDK means zero `as any` casts at the boundary.

Pool events in `SquadClientWithPool` now use a typed mapping (`poolToSquadEvent`) instead of `as any`.

### Adapter completeness (#322)

Added `sendAndWait()`, `abort()`, `getMessages()` as optional methods on `SquadSession` interface and implemented them in `CopilotSessionAdapter`. Removed dead `_squadOnMessage` reference. Fixed `(event as any).inputTokens` with typed index access via `SquadSessionEvent`'s `[key: string]: unknown` signature.

### resumeSession() already correct (#320)

Verified: `resumeSession()` already wraps in `CopilotSessionAdapter`. No change needed.

## Result

- Zero `as any` or `as unknown as` casts remain in `adapter/client.ts` and `client/index.ts`
- 2219 tests pass, zero regressions
- Build clean under `strict: true`
