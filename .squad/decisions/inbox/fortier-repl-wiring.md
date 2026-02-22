### REPL Shell Coordinator Wiring — Architecture Decision
**By:** Fortier
**Date:** 2026-02-22
**Issue:** #303

**What:** The REPL shell dispatch logic lives in the shell entry point (`index.ts`), not inside the Ink component (`App.tsx`). The App component receives an `onDispatch` async callback and is purely UI. SDK session management (creation, reuse, streaming event wiring, cleanup) is handled in closures within `runShell()`.

**Why:**
1. **Separation of concerns** — React components shouldn't own SDK connections. The entry point has the right lifecycle scope for client/session management.
2. **Streaming-first** — Session events (`message_delta`) feed directly into `shellApi.setStreamingContent()` for live incremental rendering. No polling, no buffering layer needed for the basic path.
3. **Session reuse** — Agent sessions are cached by name in a `Map<string, SquadSession>`. First message creates the session with the agent's charter as system prompt; subsequent messages reuse it. Coordinator gets its own persistent session.
4. **StreamBridge preserved** — The existing StreamBridge infrastructure stays in place for future `StreamingPipeline` integration. The direct `session.on()` → `shellApi` path handles the immediate need without coupling to the pipeline.

**Impact:** All agents should know that `@Agent message` and free-form input now route through real SDK sessions. Slash commands remain sync and component-local.
