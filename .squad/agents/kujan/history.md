# Project Context

- **Owner:** Brady
- **Project:** squad-sdk — the programmable multi-agent runtime for GitHub Copilot (v1 replatform)
- **Stack:** TypeScript (strict mode, ESM-only), Node.js ≥20, @github/copilot-sdk, Vitest, esbuild
- **Created:** 2026-02-21

## Learnings

### From Beta (carried forward)
- Copilot CLI vs. Copilot SDK boundary awareness: know which surface you're on
- Model selection fallback chains: Premium → Standard → Fast → nuclear (omit model param)
- Platform detection: CLI has task tool, VS Code has runSubagent, fallback works inline
- SQL tool is CLI-only — does not exist on VS Code, JetBrains, or GitHub.com
- Client compatibility matrix: spawning behavior varies by platform

### Wave 1 M0 SDK Audit (2025-07-18)
- @github/copilot-sdk IS published on npm (v0.1.25, 28 versions, MIT license)
- Squad's file: reference (v0.1.8) is outdated by 17 versions
- Only 1 runtime import: `CopilotClient` from `@github/copilot-sdk` in `src/adapter/client.ts`
- Adapter types layer (`src/adapter/types.ts`) decouples Squad from SDK types — good pattern

### Process.exit() Refactor for SquadUI (issue #189)
- `fatal()` in `src/cli/core/errors.ts` now throws `SquadError` instead of calling `process.exit(1)`
- CLI entry points (`src/cli-entry.ts`) catch `SquadError` and call `process.exit(1)` — library consumers catch normally
- `src/index.ts` is now a pure barrel export with zero side effects — safe for VS Code extension import
- `runWatch()` uses Promise resolution instead of `process.exit(0)` for graceful shutdown
- `runShell()` closes readline on SIGINT instead of `process.exit(0)`
- `SquadError` is exported from public API for library consumers to catch specifically
- Pattern: library functions throw/return, CLI entry point catches and exits
- All 4 test files mock the SDK; no tests need real SDK at runtime
- Build and all 1592 tests pass with npm `^0.1.25` reference — verified
- SDK dist is ~150KB; the 296MB local install is due to `node_modules` in the sibling dir
- Bundle config correctly marks `@github/copilot-sdk` as external (esbuild won't bundle it)

### Barrel Files for SquadUI (issues #225, #226)
- Created `src/parsers.ts` — re-exports all parser functions and result types from markdown-migration, routing, charter-compiler, and skill-loader
- Created `src/types.ts` — pure `export type` re-exports with zero runtime code, covering parsed types, config types, routing types, adapter types
- Both files use `export { ... } from './path.js'` ESM syntax, consistent with existing barrel patterns in `src/index.ts`
- Build (tsc + workspaces) and all 1683 tests pass with the new files

### 📌 Team update (2026-02-22T020714Z): Process.exit() refactor complete
Kujan's error handling refactor makes all library functions throw SquadError instead of calling process.exit(). Only CLI entry point (src/cli-entry.ts) calls process.exit() now. SquadUI can catch SquadError for structured error handling instead of process termination. Pattern: library functions throw, CLI entry point catches. Decision merged to decisions.md. Issue #189 closed. 1683 tests passing.

### OTel Public API Export (Issue #266)
- Exported the 3-layer OTel API from `src/index.ts`:
  - **Low-level** (`otel.ts`): `initializeOTel`, `shutdownOTel`, `getTracer`, `getMeter`, `OTelConfig` — already exported via `export *`
  - **Mid-level** (`otel-bridge.ts`): Added `bridgeEventBusToOTel(bus)` — subscribes to an EventBus and creates OTel spans per event, returns unsubscribe function
  - **High-level** (`otel-init.ts`): Created `initSquadTelemetry(options)` — one-call setup that wires OTel providers + EventBus bridge + TelemetryTransport, returns `SquadTelemetryHandle` with shutdown()
- All OTel instrumentation is no-op when no provider is registered (zero overhead for non-OTel consumers)
- Tree-shaking friendly: each layer is a separate module, named exports for bridge/init
- `SquadTelemetryOptions` extends `OTelConfig` with optional `eventBus` and `installTransport` fields
- Build passes with all changes
