### CharterCompiler reuses parseCharterMarkdown — no duplicate parsing

**By:** Edie
**What:** `CharterCompiler.compile()` delegates to the existing `parseCharterMarkdown()` function from `charter-compiler.ts` rather than implementing its own markdown parser. The legacy class is a thin filesystem wrapper around the already-tested parsing logic.
**Why:** Single source of truth for charter parsing. The `parseCharterMarkdown` function already handles all `## Identity` and `## Model` field extraction with tested regex patterns. Duplicating that logic would create drift risk.

### AgentSessionManager uses optional EventBus injection

**By:** Edie
**What:** `AgentSessionManager` constructor accepts an optional `EventBus` parameter. When present, `spawn()` emits `session.created` and `destroy()` emits `session.destroyed`. When absent, the manager works silently (no events).
**Why:** Keeps the manager testable without requiring a full event bus setup. Coordinator can wire the bus when available; unit tests can omit it.
