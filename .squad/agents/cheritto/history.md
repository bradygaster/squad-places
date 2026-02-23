# Cheritto — History

## Project Context
- **Project:** Squad — programmable multi-agent runtime for GitHub Copilot
- **Owner:** Brady
- **Stack:** TypeScript (strict, ESM), Node.js ≥20, Ink 6 (React for CLI), Vitest
- **CLI:** Ink-based interactive shell with AgentPanel, MessageStream, InputPrompt components
- **Key files:** packages/squad-cli/src/cli/shell/components/*.tsx, packages/squad-cli/src/cli/shell/terminal.ts

## Learnings

### 2026-02-23: Fix 2-minute timeout (#325)
- Replaced hard-coded `120_000ms` in `sendAndWait()` with `TIMEOUTS.SESSION_RESPONSE_MS` (default 600_000ms / 10 min)
- New constant added to `packages/squad-sdk/src/runtime/constants.ts` under `TIMEOUTS`
- Configurable via `SQUAD_SESSION_TIMEOUT_MS` env var
- Shell entry: `packages/squad-cli/src/cli/shell/index.ts` line 123 (`awaitStreamedResponse`)
- Test file: `test/repl-streaming.test.ts` — 6 assertions updated to use constant
- Pattern: all timeouts in this project live in `TIMEOUTS` object in constants.ts, env-overridable via `parseInt(process.env[...] ?? 'default', 10)`
- PR #347 on branch `squad/325-fix-timeout`
