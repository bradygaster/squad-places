# Decision: --global flag and status command pattern

**By:** Fenster (Core Dev)
**Date:** 2026-02-21
**Re:** #212, #213

## What
- `--global` flag on `init` and `upgrade` routes to `resolveGlobalSquadPath()` instead of `process.cwd()`.
- `squad status` composes both `resolveSquad()` and `resolveGlobalSquadPath()` to show which squad is active and why.
- All routing logic stays in `src/index.ts` main() — init.ts and upgrade.ts are path-agnostic (they take a `dest` string).

## Why
1. **Thin CLI contract:** init and upgrade already accept a destination directory. The `--global` flag is a CLI concern, not a runtime concern — so it lives in the CLI routing layer only.
2. **Composition over fallback:** `squad status` demonstrates the intended consumer pattern from #210/#211: try repo resolution first, then check global path. The resolution primitives stay independent.
3. **Debugging UX:** Status shows repo resolution, global path, and global squad existence — all the info needed to debug "why isn't my squad loading?" issues.

## Impact
Low. Single-file change to `src/index.ts`. No changes to resolution algorithms or init/upgrade internals.
