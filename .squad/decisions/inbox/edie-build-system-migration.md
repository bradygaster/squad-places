### 2026-02-22: Build System Migration — tsconfig + package.json

**By:** Edie (TypeScript Engineer)
**Status:** Decision
**Scope:** Monorepo build configuration for SDK/CLI workspace packages

---

## What Changed

1. **Root tsconfig.json** — Now a base config only. Sets shared compiler options (`strict`, `ES2022`, `NodeNext`, etc.). Uses `"files": []` so it compiles nothing directly. Project references point to both workspace packages.

2. **SDK tsconfig.json** — Extends root. `composite: true` for project references. Emits declarations + declaration maps. No JSX (SDK is pure TypeScript).

3. **CLI tsconfig.json** — Extends root. `composite: true`. Adds `jsx: "react-jsx"` and `jsxImportSource: "react"` for ink components. Includes `*.tsx` files. References SDK package.

4. **Root package.json** — `private: true`, workspace orchestrator only. Stripped `main`, `types`, `bin`, all runtime deps, `optionalDependencies`. Only `typescript` + `vitest` remain as devDeps. Build delegates to `npm run build --workspaces`.

5. **SDK package.json** — 18 subpath exports matching Keaton's plan. `@github/copilot-sdk` as direct dependency. `@types/node` + `typescript` as devDeps.

6. **CLI package.json** — `bin.squad` → `./dist/cli-entry.js`. Added `ink`, `react` as runtime deps. Added `@types/node`, `@types/react`, `esbuild`, `typescript`, `ink-testing-library` as devDeps. `templates/` included in files array.

## Why `composite: true`

TypeScript project references require `composite: true` in referenced projects. Without it, `tsc --build` and the `references` array in tsconfig don't work — the compiler can't resolve cross-package type information. This is a hard requirement, not a preference.

## Build Order

`npm run build --workspaces` builds SDK first (no dependencies), then CLI (depends on SDK). npm respects topological order within workspaces.

## Verified

Both packages compile with zero errors. All dist artifacts (`.js`, `.d.ts`, `.d.ts.map`) emitted correctly.
