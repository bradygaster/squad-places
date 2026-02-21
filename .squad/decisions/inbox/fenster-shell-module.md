# Decision: Shell module structure and entry point routing

**By:** Fenster (Core Dev)
**Date:** 2026-02-21
**Re:** #234, #235
**PR:** #282

## What
- `src/cli/shell/` module created with `index.ts`, `types.ts`, and `components/` placeholder directory.
- `squad` with no args now launches `runShell()` (interactive shell) instead of `runInit()`.
- `squad init` remains available as an explicit subcommand — no functionality removed.

## Why
1. **Entry point change:** Brady's directive (#decisions.md) makes the interactive shell the primary UX. Running `squad` with no args should enter the shell, not re-run init. Init is still available via `squad init`.
2. **Placeholder over premature implementation:** `runShell()` is console.log-only. Ink dependency is handled separately (#233). This keeps the shell module structure ready without coupling to the UI library.
3. **Types first:** `ShellState`, `ShellMessage`, and `AgentSession` interfaces define the shell's data model before any UI code exists. This lets other agents (ink wiring, agent spawning) code against stable types.

## Impact
- Low. No existing tests broken (1621/1621 pass). The only behavior change is `squad` (no args) prints a shell header and exits instead of running init.
- `squad init` and `squad --help` / `squad help` continue to work as before.
