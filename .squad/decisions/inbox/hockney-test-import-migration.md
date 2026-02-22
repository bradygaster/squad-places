# Decision: Defer test import migration until root src/ removal

**By:** Hockney (Tester)
**Date:** 2026-02-22
**Status:** Proposed

## Context

After the SDK/CLI workspace split, all 56 test files still import from root `../src/`. The build and all 1719 tests pass cleanly because root `src/` still exists.

## Decision

Defer migrating test imports from `../src/` to `@bradygaster/squad-sdk` / `@bradygaster/squad-cli` until root `src/` is actually removed.

## Rationale

1. **Exports map gap:** The SDK package only exposes 18 subpath exports. Tests import ~40+ distinct deep internal paths (e.g., `config/agent-doc.js`, `casting/casting-engine.js`, `runtime/event-bus.js`). These aren't in the exports map.
2. **CLI package has no subpath exports:** Shell test imports (`cli/shell/sessions.js`, `cli/shell/coordinator.js`, etc.) have no package-level path to migrate to.
3. **Barrel divergence:** Root `src/index.ts` still exports CLI functions (`runInit`, `runExport`, etc.) that the SDK package correctly does not. `consumer-imports.test.ts` tests these exports.
4. **Risk/reward:** Migrating 150+ import lines across 56 files for zero functional benefit while root `src/` exists is pure risk.

## When to revisit

When Edie or Fortier are ready to delete root `src/`, this becomes blocking. At that point:
- Expand `exports` maps in both packages to cover all internal modules tests need, OR
- Add vitest `resolve.alias` config to map `@bradygaster/squad-sdk` → `packages/squad-sdk/src`, OR
- Move tests into their respective workspace packages (`packages/squad-sdk/test/`, `packages/squad-cli/test/`)
