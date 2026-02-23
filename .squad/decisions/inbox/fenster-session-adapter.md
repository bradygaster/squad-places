# Decision: CopilotSessionAdapter — Runtime method mapping for SquadSession

**Author:** Fenster  
**Date:** 2026-02-23  
**Status:** Implemented  
**Issue:** #315  
**Priority:** P0

## Context

`SquadClient.createSession()` used `as unknown as SquadSession` to cast the raw `CopilotSession` object from `@github/copilot-sdk`. This compiled but silently skipped runtime method mapping:

- CopilotSession has `send()` → SquadSession expects `sendMessage()`
- CopilotSession has no `off()` → SquadSession expects `off()`
- CopilotSession has `destroy()` → SquadSession expects `close()`
- CopilotSession `on()` returns an unsubscribe function → SquadSession `on()` returns void

In local environments the bug was masked because the coordinator code path wasn't exercised end-to-end. In GitHub Codespaces (reported by @spboyer), the full path ran and `coordinatorSession.sendMessage()` threw "sendMessage is not a function".

## Decision

Created `CopilotSessionAdapter` class in `packages/squad-sdk/src/adapter/client.ts` that wraps raw CopilotSession and implements the `SquadSession` interface at runtime — not just at compile time.

Method mapping:
- `sendMessage(opts)` → `inner.send(opts)` (same shape: prompt, attachments, mode)
- `on(type, handler)` → `inner.on(type, handler)` + stores unsubscribe fn in Map
- `off(type, handler)` → calls stored unsubscribe fn (CopilotSession lacks off())
- `close()` → `inner.destroy()` + clears unsubscriber Map
- `sessionId` → reads `inner.sessionId`

## Alternatives Considered

1. **Modify CopilotSession upstream** — Not possible, `@github/copilot-sdk` is an external dependency.
2. **Change SquadSession interface** — Would break all downstream callers and is our public API.
3. **Monkey-patch methods onto CopilotSession** — Fragile, breaks if SDK changes prototype.

## Impact

- Zero changes to callers (shell, coordinator, spawn, lifecycle, fan-out)
- Zero changes to the SquadSession interface (public API preserved)
- 4 test files updated for CopilotSession-shaped mocks
- 9 new tests covering adapter behavior
