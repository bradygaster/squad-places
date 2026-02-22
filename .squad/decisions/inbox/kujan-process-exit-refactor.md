### Process.exit() refactor — library-safe CLI functions
**By:** Kujan (SDK Expert)
**Date:** 2025-07-22
**Re:** #189

**What:**
- `fatal()` now throws `SquadError` instead of calling `process.exit(1)`.
- `src/index.ts` is a pure barrel export with zero side effects (no `main()`, no `process.exit()`).
- `src/cli-entry.ts` is the sole CLI entry point — it catches `SquadError` and calls `process.exit(1)`.
- `runWatch()` resolves its Promise on SIGINT/SIGTERM instead of `process.exit(0)`.
- `runShell()` closes readline on SIGINT instead of `process.exit(0)`.
- `SquadError` class is exported from the public API.

**Why:**
SquadUI (VS Code extension) imports CLI functions as a library. `process.exit()` kills the entire VS Code extension host. All library-consumable functions must throw errors or return results, never call `process.exit()`. Only the CLI entry point (the thin presentation layer) may call `process.exit()`.

**Pattern established:**
- Library functions: throw `SquadError` or return result objects
- CLI entry point: catches errors, formats output, calls `process.exit()`
- Library consumers: catch `SquadError` for structured error handling

**Impact:** Medium. Changes error handling contract for all functions that used `fatal()`. Backwards-compatible for CLI users (same behavior). Library consumers now get catchable errors instead of process termination.
