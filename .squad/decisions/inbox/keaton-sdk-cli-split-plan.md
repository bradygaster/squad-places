### 2026-02-21: SDK/CLI File Split Plan — Definitive Mapping

**By:** Keaton (Lead)
**Status:** Decision
**Scope:** Monorepo restructure — move `src/` into `packages/squad-sdk/` and `packages/squad-cli/`

---

## Overview

All 114 `.ts` files in root `src/` must be split between two workspace packages. The dependency direction is **one-way: CLI → SDK**. No SDK module imports from CLI. This is clean and the split is mechanical.

---

## SDK Package (`packages/squad-sdk/src/`)

### Directories That Move Here (verbatim)

| Source | Target | Notes |
|--------|--------|-------|
| `src/adapter/` | `src/adapter/` | Copilot SDK adapter layer (types, client, errors) |
| `src/agents/` | `src/agents/` | Charter compilation, lifecycle, model selection, history shadows |
| `src/build/` | `src/build/` | Bundle, npm-package, github-dist, CI pipeline, versioning, release |
| `src/casting/` | `src/casting/` | Casting engine, history |
| `src/client/` | `src/client/` | SquadClient, SessionPool, EventBus |
| `src/config/` | `src/config/` | Schema, routing, models, migration, doc-sync, feature-audit |
| `src/coordinator/` | `src/coordinator/` | Coordinator, direct-response, fan-out, response-tiers |
| `src/hooks/` | `src/hooks/` | HookPipeline, policies, reviewer lockout |
| `src/marketplace/` | `src/marketplace/` | Manifest, packaging, browser, backend, security |
| `src/ralph/` | `src/ralph/` | RalphMonitor — work monitor runtime |
| `src/runtime/` | `src/runtime/` | Config loader, streaming, cost-tracker, telemetry, i18n, offline, benchmarks, health, event-bus |
| `src/sharing/` | `src/sharing/` | Export, import, history-split, versioning, cache, conflicts |
| `src/skills/` | `src/skills/` | SkillRegistry, skill-loader, skill-source |
| `src/tools/` | `src/tools/` | ToolRegistry, defineTool, squad_* tools |
| `src/utils/` | `src/utils/` | normalize-eol utility |

### Standalone Files That Move Here

| Source | Target | Notes |
|--------|--------|-------|
| `src/index.ts` | `src/index.ts` | SDK barrel export (public API surface) |
| `src/resolution.ts` | `src/resolution.ts` | resolveSquad, resolveGlobalSquadPath, ensureSquadPath |
| `src/parsers.ts` | `src/parsers.ts` | Parser barrel re-exports |
| `src/types.ts` | `src/types.ts` | Pure type re-exports |

### Exports Map (updated `package.json` exports)

```json
{
  ".": { "types": "./dist/index.d.ts", "import": "./dist/index.js" },
  "./parsers": { "types": "./dist/parsers.d.ts", "import": "./dist/parsers.js" },
  "./types": { "types": "./dist/types.d.ts", "import": "./dist/types.js" },
  "./config": { "types": "./dist/config/index.d.ts", "import": "./dist/config/index.js" },
  "./skills": { "types": "./dist/skills/index.d.ts", "import": "./dist/skills/index.js" },
  "./agents": { "types": "./dist/agents/index.d.ts", "import": "./dist/agents/index.js" },
  "./adapter": { "types": "./dist/adapter/types.d.ts", "import": "./dist/adapter/types.js" },
  "./client": { "types": "./dist/client/index.d.ts", "import": "./dist/client/index.js" },
  "./coordinator": { "types": "./dist/coordinator/index.d.ts", "import": "./dist/coordinator/index.js" },
  "./hooks": { "types": "./dist/hooks/index.d.ts", "import": "./dist/hooks/index.js" },
  "./tools": { "types": "./dist/tools/index.d.ts", "import": "./dist/tools/index.js" },
  "./runtime": { "types": "./dist/runtime/config.d.ts", "import": "./dist/runtime/config.js" },
  "./runtime/streaming": { "types": "./dist/runtime/streaming.d.ts", "import": "./dist/runtime/streaming.js" },
  "./marketplace": { "types": "./dist/marketplace/index.d.ts", "import": "./dist/marketplace/index.js" },
  "./build": { "types": "./dist/build/index.d.ts", "import": "./dist/build/index.js" },
  "./sharing": { "types": "./dist/sharing/index.d.ts", "import": "./dist/sharing/index.js" },
  "./ralph": { "types": "./dist/ralph/index.d.ts", "import": "./dist/ralph/index.js" },
  "./casting": { "types": "./dist/casting/index.d.ts", "import": "./dist/casting/index.js" },
  "./resolution": { "types": "./dist/resolution.d.ts", "import": "./dist/resolution.js" }
}
```

**Decision:** Remove `./cli` from SDK exports. CLI re-exports in `src/index.ts` (lines 26-52) were a mistake — those are CLI utilities leaking into the SDK barrel. The SDK barrel should be cleaned to remove `export { ... } from './cli/index.js'`. Consumers who need CLI utilities use the CLI package directly.

### SDK Dependencies (move from root `package.json`)

```json
{
  "dependencies": {
    "@github/copilot-sdk": "^0.1.25"
  },
  "devDependencies": {
    "@types/node": "^22.0.0",
    "typescript": "^5.7.0"
  }
}
```

---

## CLI Package (`packages/squad-cli/src/`)

### Directories That Move Here

| Source | Target | Notes |
|--------|--------|-------|
| `src/cli/` | `src/cli/` | All CLI submodules: core/, commands/, shell/, components/ |

### Standalone Files That Move Here

| Source | Target | Notes |
|--------|--------|-------|
| `src/cli-entry.ts` | `src/cli-entry.ts` | `#!/usr/bin/env node` entry point. Becomes the `bin.squad` target. |

### Bin Entry Point Setup

`packages/squad-cli/package.json`:
```json
{
  "bin": {
    "squad": "./dist/cli-entry.js"
  }
}
```

The esbuild bundle step (`build:cli` script) stays here. CLI is the thing that bundles; SDK is the thing that gets consumed as a library.

### How CLI Imports SDK

All relative imports from CLI into SDK modules must be rewritten to package imports:

| Current Import (in CLI code) | Rewritten Import |
|------------------------------|------------------|
| `from '../config/migration.js'` | `from '@bradygaster/squad-sdk/config'` |
| `from '../config/init.js'` | `from '@bradygaster/squad-sdk/config'` |
| `from '../../resolution.js'` | `from '@bradygaster/squad-sdk/resolution'` |
| `from '../../runtime/streaming.js'` | `from '@bradygaster/squad-sdk/runtime/streaming'` |
| `from './index.js'` (importing VERSION) | `from '@bradygaster/squad-sdk'` |

The `cli-entry.ts` file currently does:
```ts
import { VERSION } from './index.js';
import { resolveSquad, resolveGlobalSquadPath } from './resolution.js';
```
These become:
```ts
import { VERSION } from '@bradygaster/squad-sdk';
import { resolveSquad, resolveGlobalSquadPath } from '@bradygaster/squad-sdk';
```

### CLI Dependencies

```json
{
  "dependencies": {
    "@bradygaster/squad-sdk": "workspace:*",
    "ink": "^6.8.0",
    "react": "^19.2.4"
  },
  "devDependencies": {
    "@types/node": "^22.0.0",
    "@types/react": "^19.2.14",
    "esbuild": "^0.25.0",
    "typescript": "^5.7.0",
    "ink-testing-library": "^4.0.0"
  }
}
```

**Decision:** `ink` and `react` are CLI-only dependencies (used only in `src/cli/shell/components/*.tsx`). They move from root to CLI package. SDK has zero UI dependencies.

---

## Root Package

### What Stays at Root

- `package.json` — workspace orchestrator only. No `main`, no `types`, no `bin`.
- `tsconfig.json` — base config with project references to `packages/*/tsconfig.json`
- `vitest.config.ts` — test runner config
- `test/` and `test-fixtures/` — integration tests that import from `@bradygaster/squad-sdk` and `@bradygaster/squad-cli`
- `templates/` — scaffolding templates (used by CLI init, referenced by path at runtime)
- `.squad/`, `docs/`, `CHANGELOG.md`, `README.md`, `CONTRIBUTING.md`

### Root `package.json` Changes

```json
{
  "name": "@bradygaster/squad",
  "private": true,
  "workspaces": ["packages/*"],
  "scripts": {
    "build": "npm run build --workspaces",
    "test": "vitest run",
    "test:watch": "vitest",
    "lint": "tsc --noEmit -p packages/squad-sdk/tsconfig.json && tsc --noEmit -p packages/squad-cli/tsconfig.json"
  },
  "devDependencies": {
    "vitest": "^3.0.0",
    "typescript": "^5.7.0"
  }
}
```

Remove: `main`, `types`, `bin`, all runtime `dependencies`, `optionalDependencies`. Root is no longer a publishable package.

---

## Import Rewiring Rules

### Pattern

1. **CLI → SDK relative imports** become **package imports**:
   - `from '../{module}/'` → `from '@bradygaster/squad-sdk/{module}'`
   - `from '../../resolution.js'` → `from '@bradygaster/squad-sdk/resolution'`
   - `from '../../runtime/streaming.js'` → `from '@bradygaster/squad-sdk/runtime/streaming'`

2. **Intra-SDK imports** stay relative (unchanged):
   - `from '../adapter/types.js'` stays as-is inside SDK package

3. **Intra-CLI imports** stay relative (unchanged):
   - `from '../core/output.js'` stays as-is inside CLI package

### Circular Dependency Analysis

**None found.** Dependency flow is strictly:
```
CLI → SDK → @github/copilot-sdk
```

Within SDK, the dependency graph is:
```
coordinator → client → adapter
coordinator → agents, hooks, tools
tools → adapter/types
ralph → client/event-bus
marketplace → config/schema
```
All DAG. No cycles.

The one concern: `src/index.ts` (SDK barrel) currently re-exports from `./cli/index.js`. This creates a false dependency from SDK→CLI. **Action: Remove those re-exports from the SDK barrel.** CLI utilities (`success`, `error`, `warn`, `fatal`, `SquadError`, `detectSquadDir`, `ghAvailable`, etc.) are CLI-only exports. External consumers who imported them from `@bradygaster/squad-sdk` will need to switch to `@bradygaster/squad-cli` — this is a breaking change that's correct to make.

### SDK Barrel Cleanup (`src/index.ts`)

**Remove** these lines from SDK `index.ts`:
```ts
export {
  type ReleaseChannel, type SDKUpdateInfo, type SDKUpgradeOptions,
  checkForUpdate, performUpgrade,
  success, error, warn, info, dim, bold, fatal, SquadError,
  detectSquadDir, ghAvailable, ghAuthenticated, ghIssueList, ghIssueEdit,
  runWatch, runUpgrade, runInit, runExport, runImport, runCopilot,
  type CopilotFlags, scrubEmails
} from './cli/index.js';
```

These are all CLI implementation details. The SDK should not export them.

---

## Migration Order

### Phase 1: SDK Package (do first — CLI depends on it)

1. **Copy** all SDK source files into `packages/squad-sdk/src/`
2. **Create** `packages/squad-sdk/tsconfig.json` with proper paths
3. **Update** `packages/squad-sdk/package.json` with full exports map and dependencies
4. **Clean** the SDK barrel — remove `./cli/index.js` re-exports
5. **Build** SDK in isolation — must compile clean
6. **Run** SDK-specific tests (extract from root `test/`)

### Phase 2: CLI Package (do second — imports SDK)

1. **Copy** `src/cli/` and `src/cli-entry.ts` into `packages/squad-cli/src/`
2. **Rewrite** all relative imports to SDK modules → `@bradygaster/squad-sdk/*` package imports
3. **Create** `packages/squad-cli/tsconfig.json` with project reference to SDK
4. **Update** `packages/squad-cli/package.json` with `ink`, `react`, `esbuild` dependencies
5. **Move** `build:cli` esbuild script from root to CLI package
6. **Build** CLI in isolation — must compile clean against SDK

### Phase 3: Root Cleanup (do last)

1. **Remove** `src/` directory entirely
2. **Strip** root `package.json` down to workspace orchestrator
3. **Update** `vitest.config.ts` to resolve from workspace packages
4. **Update** `test/` imports to use `@bradygaster/squad-sdk` and `@bradygaster/squad-cli`
5. **Verify** `npm run build && npm test` from root

### What Can Be Parallelized

- Phase 1 and Phase 2 setup (file copy, tsconfig creation) can happen simultaneously
- Import rewiring (Phase 2 step 2) must wait for SDK exports map to be finalized (Phase 1 step 3)
- Test migration (Phase 3 step 4) can be done incrementally, file by file

---

## Templates Directory

`templates/` stays at root. The CLI references templates by runtime path resolution (walking up to find the package root). After the split, the CLI package needs `templates/` either:
- **Option A:** Copy `templates/` into `packages/squad-cli/` (cleaner — self-contained CLI package)
- **Option B:** Keep at root, CLI resolves at runtime via `import.meta.url` walk-up

**Decision:** Option A. Copy templates into CLI package. Self-contained packages are easier to reason about and publish.

---

## Summary

- **SDK:** 15 directories + 4 standalone files. Zero UI deps. Pure library.
- **CLI:** 1 directory (`cli/`) + 1 entry file. Owns `ink`, `react`, `esbuild`. Imports SDK via package name.
- **Root:** Workspace orchestrator + tests + templates + docs. Not publishable.
- **Breaking change:** CLI utilities removed from SDK barrel export. Correct and intentional.
- **Circular deps:** None. Clean DAG.
- **Migration:** SDK first, CLI second, root cleanup third. ~2 days of focused work.

**Why:** Every module in the SDK can be imported independently via subpath exports. CLI stays thin. SDK stays pure. Future features (new subcommands, new UI frameworks) only touch the CLI package. Future SDK features (new modules, new adapter backends) only touch the SDK package. This is the architecture that compounds.
