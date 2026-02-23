# Decision: Remote squad mode CLI commands (Issue #313)

**Date:** 2026-02-23  
**By:** Fenster  
**Status:** Implemented  
**Depends on:** #311 (dual-root resolver — complete)

## What

Added two CLI commands for remote squad mode:

1. **`squad link <team-repo-path>`** — links a project to a remote team root by writing `.squad/config.json` with a relative `teamRoot` path.
2. **`squad init --mode remote <team-root-path>`** — initializes a project in remote mode, writing config.json before running the standard init scaffolding.

## Why

Remote squad mode lets multiple projects share a single team identity (agents, casting, skills) stored in a separate repo. The dual-root resolver (#311) already handles reading `config.json` and resolving paths. These CLI commands are the user-facing way to **create** that configuration.

## Key decisions

- **Always relative paths:** `config.json` stores `teamRoot` as a relative path from the project root. Never absolute. This ensures portability across machines and CI.
- **Validation on link:** `squad link` validates the target exists and contains `.squad/` or `.ai-team/`. Fails fast with `fatal()` otherwise.
- **No validation on init --mode remote:** `writeRemoteConfig()` does not validate the target exists — the user may be setting up config before the team repo is cloned. Validation happens at resolution time via `resolveSquadPaths()`.
- **link is idempotent:** Running `squad link` again overwrites `config.json` with the new target.
- **init --mode local is the default:** No behavioral change to existing `squad init`.

## Files changed

- `packages/squad-cli/src/cli/commands/link.ts` — new
- `packages/squad-cli/src/cli/commands/init-remote.ts` — new
- `packages/squad-cli/src/cli-entry.ts` — registered both commands, updated help text
- `packages/squad-cli/package.json` — added subpath exports
- `test/cli/remote-mode.test.ts` — 9 tests

## Round-trip verified

`squad link` → `resolveSquadPaths()` → remote mode with correct `teamDir`. Tested in `test/cli/remote-mode.test.ts`.
