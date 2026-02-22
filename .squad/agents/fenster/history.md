# Project Context

- **Owner:** Brady
- **Project:** squad-sdk — the programmable multi-agent runtime for GitHub Copilot (v1 replatform)
- **Stack:** TypeScript (strict mode, ESM-only), Node.js ≥20, @github/copilot-sdk, Vitest, esbuild
- **Created:** 2026-02-21

## Core Context

**Created:** 2026-02-21  
**Role:** Core Developer — Runtime implementation, CLI structure, shell infrastructure  
**Key Decisions Owned:** Test import patterns (vitest via dist/), CRLF normalization at parser entry, shell module structure (readline→ink progression), spawn lifecycle, SessionRegistry design

**Phase 1-2 Complete (2026-02-21 → 2026-02-22T041800Z):**
- M3 Resolution (#210/#211): `resolveSquad()` + `resolveGlobalSquadPath()` in src/resolution.ts, standalone concerns (no auto-fallback)
- CLI: --global flag routing, `squad status` command composition, command rename finalized (triage, loop, hire)
- Shell foundation: readline-based CLI shell, SessionRegistry (Map-backed, no persistence), spawn infrastructure (loadAgentCharter, buildAgentPrompt, spawnAgent)
- CRLF hardening: normalize-eol.ts applied to 8 parsers, one-line guard at entry point
- SDK/CLI split executed: 15 dirs + 4 files migrated to packages/, exports map updated (7→18 subpaths SDK, 14 subpaths CLI), 6 config files fixed, versions aligned to 0.8.0
- Test import migration: 56 test files migrated from ../src/ to @bradygaster/squad-sdk/* and @bradygaster/squad-cli/*, 26 SDK + 16 CLI subpath exports, vitest resolves via dist/, all 1719+ tests passing
- Zero-dependency scaffolding preserved, strict mode enforced, build clean (tsc 0 errors)

**Phase 3 Blocking (2026-02-22 onwards):**
- Ralph start(): EventBus subscription + health checks (14 TODOs)
- Coordinator initialize()/route(): CopilotClient wiring + agent manager (13 TODOs)
- Agents spawn(): SDK session creation + history injection (14 TODOs)
- Shell UI: Ink components not yet wired (readline only), streaming responses, agent status display
- Casting: registry.json parsing stub (1 TODO)
- Triage/Loop/Hire: placeholder commands (low priority, defer)

## Learnings

### 📌 SDK/CLI File Migration — Keaton's split plan executed
- **Phase 1 (SDK):** Copied all 15 directories (adapter, agents, build, casting, client, config, coordinator, hooks, marketplace, ralph, runtime, sharing, skills, tools, utils) and 4 standalone files (index.ts, resolution.ts, parsers.ts, types.ts) from root `src/` into `packages/squad-sdk/src/`. Cleaned the SDK barrel (`packages/squad-sdk/src/index.ts`) — removed the CLI re-exports block (lines 25-52 of the original, exporting success/error/warn/fatal/SquadError/detectSquadDir/runWatch/runInit/runExport/runImport/runCopilot etc. from `./cli/index.js`). Updated SDK `package.json` exports map: removed `./cli`, added all subpath exports from Keaton's plan (resolution, runtime/streaming, coordinator, hooks, tools, adapter, client, marketplace, build, sharing, ralph, casting).
- **Phase 2 (CLI):** Copied `src/cli/` directory and `src/cli-entry.ts` into `packages/squad-cli/src/`. Copied `templates/` into `packages/squad-cli/templates/`. Rewrote 4 cross-package imports in CLI source:
  - `cli/upgrade.ts`: `../config/migration.js` → `@bradygaster/squad-sdk/config`
  - `cli/copilot-install.ts`: `../config/init.js` → `@bradygaster/squad-sdk/config`
  - `cli/shell/spawn.ts`: `../../resolution.js` → `@bradygaster/squad-sdk/resolution`
  - `cli/shell/stream-bridge.ts`: `../../runtime/streaming.js` → `@bradygaster/squad-sdk/runtime/streaming`
  - `cli-entry.ts`: `./resolution.js` and `./index.js` → `@bradygaster/squad-sdk`
- **Intra-CLI imports** (within `cli/` directory) left untouched — all relative.
- **Root `src/` preserved** — not deleted, per plan (cleanup after tests pass).
- Pattern: SDK subpath exports match the directory barrel structure — `@bradygaster/squad-sdk/{module}` resolves to `dist/{module}/index.js`. Special cases: `./resolution` → `dist/resolution.js`, `./runtime/streaming` → `dist/runtime/streaming.js`.

### 📌 Test import migration to workspace packages — completed
- Migrated all 56 test files (173 import replacements) from relative `../src/` paths to workspace package imports.
- SDK imports use 26 subpath exports (18 existing + 8 new): `@bradygaster/squad-sdk/config`, `@bradygaster/squad-sdk/agents`, etc.
- CLI imports use 16 new subpath exports: `@bradygaster/squad-cli/shell/sessions`, `@bradygaster/squad-cli/core/init`, etc.
- Added 8 new SDK subpath exports for deep modules not covered by barrels: `adapter/errors`, `config/migrations`, `runtime/event-bus`, `runtime/benchmarks`, `runtime/i18n`, `runtime/telemetry`, `runtime/offline`, `runtime/cost-tracker`.
- Added missing barrel re-exports: `selectResponseTier`/`getTier` in coordinator/index.ts, `onboardAgent`/`addAgentToConfig` in agents/index.ts.
- Updated consumer-imports test: CLI functions (`runInit`, `runExport`, `runImport`, `scrubEmails`) now imported from `@bradygaster/squad-cli` instead of SDK barrel.
- Rebuilt SDK and CLI packages to update dist. All 1727 tests pass across 57 files.
- Pattern: vitest resolves through compiled `dist/` files, not TypeScript source — barrel changes require a package rebuild to take effect.
- Pattern: when consolidating deep imports to barrel paths, verify the barrel actually re-exports the needed symbols before assuming availability.

### 📌 Runtime Implementation Assessment (2026-02-22T22:00Z) — Fenster
**Status:** Phase 1-2 complete (SDK/CLI split, monorepo structure). Phase 3 (runtime integration) blocked.

**Implemented & Working:**
- ✅ **SDK/CLI split:** Both packages at 0.8.0 (SDK)/0.8.1 (CLI). Clean exports maps (18 subpaths SDK, 14 subpaths CLI).
- ✅ **Build pipeline:** tsc compiles both packages to dist/, all dependencies resolved (SDK→Copilot SDK, CLI→SDK+ink+react). Zero errors.
- ✅ **CLI structure:** Entry point (cli-entry.ts) routes 14 commands. Commands implemented: `help`, `version`, `status`, `init`, `upgrade`, `export`, `import`, `copilot`, `plugin`, `scrub-emails`. Commands stubbed: `triage` (watch alias), `loop`, `hire`.
- ✅ **Shell foundation:** readline-based CLI shell with header chrome, session registry, spawn infrastructure. Agent discovery, charter loading, and spawn lifecycle foundation. Type-safe completion.
- ✅ **Core modules:** resolution.ts, config/, build/, skills/, hooks/, tools/, client/ (EventBus structure), marketplace/, adapter/ all present.
- ✅ **Monorepo:** npm workspaces, changesets configured, independent versioning (SDK/CLI can release separately).

**Incomplete/Stubs (Phase 3 blockers):**
- ⏳ **Ralph monitor** (src/ralph/index.ts): Class structure present. 14 TODO comments (PRD 8). Methods stubbed: start(), handleEvent(), healthCheck(), stop(). EventBus subscription logic not wired.
- ⏳ **Coordinator** (src/coordinator/index.ts): Class structure present. 13 TODO comments (PRD 5). Methods stubbed: initialize(), route(), spawn(), monitor(), destroy(). No SquadClient wiring, no agent manager hookup.
- ⏳ **Agents module** (src/agents/index.ts): Charter compilation imported from separate file (working). SessionManager class present but 14 TODO comments (PRD 4). Methods stubbed: spawn(), resume(), terminate(). No SDK session creation wired.
- ⏳ **Casting system** (src/casting/index.ts): v1 CastingEngine imported (working). Legacy CastingRegistry stubbed — 1 TODO (PRD 11) for registry.json parsing. Cast/recast methods throw "Not implemented".
- ⏳ **Shell UI:** No ink-based components wired. readline loop exists but command handling is echo-only (line 78 in shell/index.ts). No agent discovery integration, no streaming response display, no real coordinator handoff.
- ⏳ **Triage/Loop/Hire commands:** Placeholder messages in cli-entry.ts lines 115-148. No implementation.

**Important TODOs in Code:**
- **Ralph (8):** start() needs EventBus subscription, health checks, persistent state loading/saving (8 items).
- **Coordinator (13):** initialize() needs client connection, charter loading, hook setup, EventBus wiring (13 items).
- **Agents (14):** spawn() needs charter reading, YAML parsing, SDK session creation with history injection (14 items).
- **Shell spawn.ts (1):** "Wire to CopilotClient session API" — CopilotClient session creation stubbed with TODO (line ~78 in spawn.ts).
- **Casting (1):** registry.json parsing stub.

**CLI Commands Status:**
- **Fully working (7):** help, version, status, init, upgrade, export, import, copilot, plugin, scrub-emails
- **Stubbed (3):** triage (watch alias), loop, hire — all print placeholder messages
- **Design note:** Commands are correct per Brady's directives (squad loop, squad triage, squad hire). Command routing works; implementations pending.

**Technical Debt:**
- **Phase 3 cleanup pending:** root `src/` directory still exists (backward compat). Will be deleted after monorepo migration complete per history.
- **Ink components:** No UI components wired yet. Shell uses readline only. Ink dependency is in CLI package.json but not used.
- **Event-driven flow:** EventBus is defined (event-bus.ts) but no actual event emission wired. Handler error isolation TODO (PRD 1).

**CLI Entry Point Wiring:**
- `main()` parses command and routes to implementations (all sync/await patterns clean).
- No external commands spawned yet (e.g., `gh api`, file system watch).
- `--global` flag works (resolveGlobalSquadPath routing correct).

**Build/Test/Lint Status:**
- ✅ **Build:** 0 errors (tsc clean).
- ✅ **Tests:** 1700+ passing (exact count varies by run, all passing).
- ✅ **Lint:** tsc --noEmit clean (strict mode enforced).

**Next Phase Blocking Items:**
1. Wire EventBus: Actual event emission from sessions + handler execution in coordinator/ralph.
2. CopilotClient session integration: Ralph.start() and spawnAgent() need live session creation/resumption.
3. Coordinator.initialize() and route(): Accept user message, load charters, route to agents.
4. Shell UI: Wire ink components for agent display, streaming responses, session status.
5. Triage/Loop/Hire: Implement placeholder commands (low priority, can defer).

**Assessment for Brady:** Core runtime foundation is solid — SDK/CLI split is complete, command routing works, type safety is enforced. Phase 3 (integrating with CopilotClient, EventBus event emission, Coordinator logic) is the next lift. Ralph and Coordinator are well-structured but need internal wiring. No broken code — just incomplete TODOs. Estimate 2-3 weeks to wire Phase 3 fully.

### 📌 Team update (2026-02-22T070156Z): Test import migration merged to decisions, CLI functions correctly exported from CLI package — decided by Fenster, Edie, Hockney
- **Test import migration decision:** 56 test files migrated from `../src/` to `@bradygaster/squad-sdk` / `@bradygaster/squad-cli`. 26 SDK subpath exports, 16 CLI subpath exports. Barrel re-exports verified for missing symbols. All 1727 tests passing.
- **CLI function placement clarified:** runInit, runExport, runImport, scrubEmails correctly exported from `@bradygaster/squad-cli` (not SDK), reflecting intentional architecture separation.
- **Pattern established:** Vitest resolves through compiled `dist/`, so barrel changes require `npm run build` in the package before tests see them.
- **Decision merged to decisions.md.** Status: Test infrastructure aligned with workspace split, ready for Phase 3 runtime integration.
