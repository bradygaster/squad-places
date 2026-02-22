# Decision: Extract hardcoded values to central constants

**Author:** Edie (TypeScript Engineer)
**Date:** 2026-02-22
**Status:** Implemented

## Context

The code audit found model names, timeouts, and role mappings duplicated across 6+ files. Values had already drifted — e.g., `model-selector.ts` had 4-entry fallback chains while `config.ts` and `init.ts` had 3-entry chains with different models. This violated single-source-of-truth and created silent inconsistency risk.

## Decision

Created `packages/squad-sdk/src/runtime/constants.ts` as the canonical source for:

- **`MODELS`** — `DEFAULT`, `SELECTOR_DEFAULT`, `SELECTOR_DEFAULT_TIER`, `FALLBACK_CHAINS` (3 tiers × 4 models each), `NUCLEAR_FALLBACK`, `NUCLEAR_MAX_RETRIES`. All `as const`.
- **`TIMEOUTS`** — `HEALTH_CHECK_MS` (5000), `GIT_CLONE_MS` (60000), `PLUGIN_FETCH_MS` (15000). All env-overridable via `parseInt(process.env[...] ?? default, 10)`.
- **`AGENT_ROLES`** — `readonly` tuple deriving `AgentRole` type.

## Files updated

| File | Change |
|------|--------|
| `runtime/constants.ts` | **Created** — single source of truth |
| `agents/model-selector.ts` | Removed local `FALLBACK_CHAINS`, `DEFAULT_MODEL`, `DEFAULT_TIER`; imports from constants |
| `runtime/config.ts` | `DEFAULT_CONFIG` uses `MODELS.*`; `AgentRole` re-exported from constants |
| `runtime/health.ts` | Default timeout uses `TIMEOUTS.HEALTH_CHECK_MS` |
| `config/init.ts` | Template generators use `MODELS.*` for all model values |
| `cli/commands/plugin.ts` | Browse timeout uses `TIMEOUTS.PLUGIN_FETCH_MS` |
| `index.ts` | Named exports: `MODELS`, `TIMEOUTS`, `AGENT_ROLES` |
| `package.json` | Added `./runtime/constants` subpath export |

**Not touched:** `upstream.ts` (Baer owns), `benchmarks.ts` (synthetic simulation data, not config).

## Rationale

- `as const` gives literal types + `readonly` — prevents accidental mutation
- Environment variable overrides enable runtime configuration without code changes
- Named exports (not `export *`) from barrel avoid `AgentRole` collision with casting module's different `AgentRole` type
- Spreading `[...MODELS.FALLBACK_CHAINS.tier]` converts readonly tuples to mutable arrays for interface compatibility

## Verification

- Build: zero TypeScript errors
- Tests: 2138/2141 passed (3 failures are pre-existing Docker/Aspire infrastructure tests)
