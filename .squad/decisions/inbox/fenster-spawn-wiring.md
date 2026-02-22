### 2026-02-22: spawn.ts wired to SquadClient — self-contained agent spawning
**By:** Fenster (Core Dev)
**Status:** IMPLEMENTED

**What:** `spawnAgent()` in `packages/squad-cli/src/cli/shell/spawn.ts` now accepts an optional `client: SquadClient` via `SpawnOptions`. When a client is provided, it creates a real SDK session (streaming, system prompt from charter, working directory), sends the task message, accumulates streamed `message_delta` events, closes the session, and returns the full response in `SpawnResult`.

**Why:** Phase 3 blocker — spawn was a stub returning a placeholder string. Now it's a working SDK integration that can be used from the Ink shell, coordinator, CLI commands, or any programmatic consumer.

**Design decisions:**
1. **Client via options, not parameter** — Adding `client` to `SpawnOptions` instead of a positional parameter preserves backward compatibility. Callers that don't provide a client get a graceful stub.
2. **Self-contained, no shell dependency** — Unlike `dispatchToAgent()` in shell/index.ts which wires into Ink state (ShellApi, StreamBridge), spawn.ts owns its own session lifecycle. This makes it usable outside the shell.
3. **Session-per-spawn** — Each spawn creates and closes its own session. This is intentional for isolation. Long-lived sessions can be managed externally and passed via a future `session` option if needed.

**Error handling audit (same PR):**
- `plugin.ts`: Removed unused `error` import from output.ts (was dead code, `fatal()` from errors.ts already used correctly).
- `upgrade.ts`: Removed unused `error as errorMsg` import from output.ts (same pattern).
- `upstream.ts`: Has `import { error as fatal } from output.ts` bug — **not touched** (Baer owns it).
- **Convention:** For CLI-exiting errors, use `fatal()` from `cli/core/errors.ts` (throws `SquadError`, returns `never`). For non-fatal operational warnings, `error()` from `output.ts` is fine.

**Files changed:**
- `packages/squad-cli/src/cli/shell/spawn.ts` — Rewired spawnAgent, added client/teamRoot to SpawnOptions
- `packages/squad-cli/src/cli/commands/plugin.ts` — Removed unused error import
- `packages/squad-cli/src/cli/core/upgrade.ts` — Removed unused errorMsg import

**Build:** Clean (0 errors). **Tests:** 47 shell tests passing.
