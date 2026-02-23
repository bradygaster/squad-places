# What's New

Full release history for Squad — from beta through the v1 TypeScript replatform.


## v0.8.2 — Current Release

- **Version alignment** — CLI (0.8.1) and SDK (0.8.0) snapped to 0.8.2 across all packages
- **Published to npm** — `@bradygaster/squad-sdk@0.8.2` and `@bradygaster/squad-cli@0.8.2`
- **Remote squad mode** (ported from @spboyer's [PR #131](https://github.com/bradygaster/squad/pull/131)):
  - `resolveSquadPaths()` dual-root resolver for project-local vs team identity directories (#311)
  - [`squad doctor` command](../cli/doctor.md) — 9-check setup validation with emoji output (#312)
  - [`squad link` command](../cli/link.md) — link a project to a remote team root (#313)
  - `squad init --mode remote` — initialize with remote team config (#313)
  - `ensureSquadPathDual()` and `ensureSquadPathResolved()` dual-root write guards (#314)
- **CopilotSessionAdapter** (#315) — Wraps `@github/copilot-sdk` CopilotSession to map `sendMessage`→`send`, `on`→unsubscribe tracking, `destroy`→`close`. Fixed P0 "sendMessage is not a function" Codespace bug.
- **Adapter hardening** (#316–#322) — 7 issues fixed:
  - EVENT_MAP with 10 event type mappings and REVERSE_EVENT_MAP for normalization
  - Typed field mapping replacing all `as unknown as` casts
  - Per-event-type unsubscribe tracking
  - Zero unsafe casts remaining in adapter layer
- **Docs restructure** — 85 pages across 6 sections:
  - Getting Started (10), CLI (3), SDK (3), Features (25), Scenarios (21), Blog (23)
  - Ported all 44 scenario + feature docs from beta
  - 8 new blog posts covering the replatform arc
  - Custom static site generator with markdown-it, frontmatter, search index
- **GitHub Pages** — Live docs site with dark mode, client-side search, sidebar nav, beta site UI
- **Test baseline** — 2232 tests across 85 test files

## v0.6.0 — The TypeScript Replatform

- **Full rewrite** — JavaScript → TypeScript with strict mode, ESM modules, Node.js ≥20
- **SDK + CLI split** — Two npm packages: `@bradygaster/squad-sdk` (runtime, adapter, resolution) and `@bradygaster/squad-cli` (commands, shell, REPL)
- **npm workspace** — Monorepo with `packages/squad-sdk` and `packages/squad-cli`
- **Interactive shell** — `squad` with no args launches rich REPL with streaming, welcome banner, session registry
- **OpenTelemetry integration** — 3-layer API (low-level otel.ts, bridge otel-bridge.ts, init otel-init.ts), SquadObserver file watcher, Aspire dashboard support
- **Adapter layer** — `CopilotSessionAdapter` bridging `@github/copilot-sdk` to Squad's session interface
- **Constants extraction** — `MODELS`, `TIMEOUTS`, `AGENT_ROLES` centralized in constants.ts
- **Security** — `execFileSync` with array args replacing `execSync` template strings (CWE-78 fix)
- **Wave-based development** — 3 waves of parallel fan-out:
  - Wave 1: OTel, Aspire, SquadObserver, upstream docs
  - Wave 2: REPL polish, CWE-78 fix, config extraction, 119 new tests, Aspire E2E
  - Wave 3: Docs migration, site engine, 5 guides
- **CLI entry point** — Moved from `dist/index.js` to `dist/cli-entry.js`
- **CRLF normalization** — All 8 parsers normalize line endings; Windows users with `core.autocrlf=true` work correctly

## v0.6.0-alpha.0

- **Initial replatform** — First working TypeScript build
- **CLI commands** — init, upgrade, shell, doctor, link
- **npm distribution** — `npm install @bradygaster/squad-cli`
- **Branch protection** — `main` requires PR + build check
- **Changesets** — Infrastructure for independent package versioning

## v0.5.2

- **`upgrade --migrate-directory` exits early fix** — The directory rename step no longer calls `process.exit(0)`, so the full upgrade (squad.agent.md, workflows, .squad-templates) now runs after migration in one command
- **`.slnx`, `.fsproj`, `.vbproj` not detected as .NET** — Proper Visual Studio solution files and F#/VB.NET project files now detected; repos using these get proper dotnet stub CI workflows
- **Migrations use detected squad directory** — Migration steps and `.gitattributes` rules now use the detected squad directory (`.squad/` or `.squad/`) so they work correctly after `--migrate-directory` runs

## v0.5.1

- [**`squad watch` — Local Watchdog**](../features/ralph.md#watch-mode) — Persistent polling for unattended work processing. Run `squad watch` to check GitHub every 10 minutes for untriaged squad work; use `--interval` flag to customize polling (e.g., `squad watch --interval 5` for 5-minute polling). Runs until Ctrl+C.
- **Project type detection** — Squad now detects your project's language and stack (JavaScript, Python, Java, Go, Rust, .NET, etc.) to intelligently configure workflows
- **Git safety rules** — Guardrails enforced based on detected project type to prevent common mistakes and state corruption

## v0.5.0 — The `.squad/` Rename Release

- [**`.squad/` renamed to `.squad/`**](../scenarios/migration/v0.5.0-squad-rename.md) — Full directory rename with backward-compatible migration utilities. Existing repos continue to work; migration required by v1.0.0.
- [**Decision lifecycle management**](../features/decision-lifecycle.md) — Archival and versioning support for design decisions across the agent lifecycle
- **Identity layer** — New `wisdom.md` and `now.md` files for agent context and temporal awareness
- **ISO 8601 UTC timestamps** — Standardized timestamp format throughout (decision dates, agent updates, metadata)
- **Cold-path extraction** — Refactored `squad.agent.md` into active decision paths and on-demand satellite files, reducing coordinator size from ~30KB to ~17KB
- **Skills export/import verification** — Enhanced validation and documentation for agent skill extension
- **Email scrubbing** — Automatic PII removal during migration to prevent accidental email commits

## v0.4.2

- **`/agent` vs `/agents` CLI command fix** — README and install output now correctly reference `/agent` (the CLI command) instead of `/agents` (VS Code shortcut)
- [**Insider Program infrastructure**](../features/insider-program.md) — `insider` branch with guard workflow enforcement; forbidden paths blocked from protected branches to maintain code safety
- **Branch content policy** — Formal decision document defining which files belong on main, preview, and insider branches; includes 5-step branch creation checklist
- **Custom universe support** — Star Trek universe added by community contributor @codebytes

## v0.4.1

- **Task spawn UI** — Added role emoji to task descriptions for visual consistency; 11 role patterns mapped to emoji (🏗️ Lead, 🔧 Backend, ⚛️ Frontend, 🧪 Tester, etc.)
- **Ralph heartbeat workflow syntax fix** — Removed duplicate `issues:` trigger keys in `squad-heartbeat.yml`; combined into single trigger
- **Community page links fixed** — GitHub Discussions links now work correctly (Discussions enabled on repo)
- [**`squad upgrade --self` command**](../scenarios/upgrading.md) — New flag for refreshing squad repo's own `.squad/` from templates; preserves agent history
- **Deprecation banner for .squad/ → .squad/ rename** — CLI and coordinator warn users that v0.5.0 will rename `.squad/` to `.squad/`
- **Stale workflow references fixed** — All documentation updated to reference correct `squad-heartbeat.yml` filename

## v0.4.0

- [**Client Compatibility**](../scenarios/client-compatibility.md) — Full platform support matrix. Squad now works on CLI and VS Code with graceful degradation.
- [**VS Code Support**](../features/vscode.md) — First-class VS Code guide. `runSubagent` parallel spawning, platform detection, feature degradation table.
- [**Project Boards**](../features/project-boards.md) — GitHub Projects V2 integration. Board + Kanban views synced from labels. `gh auth refresh -s project` required.
- [**Label Taxonomy**](../features/labels.md) — 7-namespace label system (status:, type:, priority:, squad:, go:, release:, era:). Labels are the state machine; boards are projections.
- [**Notifications**](../features/notifications.md) — Your squad pings you on Teams, iMessage, or Discord when they need input. Zero infrastructure in Squad — bring your own MCP notification server.
- [**MCP Setup Guide**](../features/mcp.md) — Step-by-step MCP configuration for CLI and VS Code. Examples: GitHub, Trello, Aspire dashboard.
- [**Plugin Marketplace**](../features/plugins.md) — Discover and install curated agent templates and skills from community repositories. Auto-recommend plugins when adding team members.
- **Universe Expansion** — 20 → 33 casting universes (MCU, DC, Stranger Things, The Expanse, Arcane, Ted Lasso, Dune, Cowboy Bebop, Fullmetal Alchemist, Seinfeld, The Office, Adventure Time, Futurama, + 2 more)
- **Docs Growth** — 49 docs across features, scenarios, and guides
- **Context Optimization** — decisions.md pruned from ~80K to ~33K tokens (251 → 78 blocks). Spawn templates deduplicated. Per-agent context usage dropped from 41–46% to 17–23%. Agents now have 78–83% of their context window for actual work.
- **Core Growth** — squad.agent.md: 1,100 → 1,771 lines; index.js: 654 lines; 188+ total commits

## v0.3.0

- [**Per-Agent Model Selection**](../features/model-selection.md) — Cost-first routing: code work gets standard-tier models (claude-sonnet-4.5), non-code tasks use fast/cheap models (claude-haiku-4.5). 16-model catalog with fallback chains.
- [**Ralph — Work Monitor**](../features/ralph.md) — Built-in squad member that autonomously processes backlogs. Self-chaining work loop: scan GitHub → spawn agents → collect results → repeat.
- [**@copilot Coding Agent**](../features/copilot-coding-agent.md) — GitHub's Copilot agent as a squad member. Three-tier capability profile. Auto-assign with workflow.
- **Universe Expansion** — 14 → 20 casting universes (Succession, Severance, Lord of the Rings, Attack on Titan, Doctor Who, Monty Python)
- **Milestones Rename** — "Sprints" → "Milestones" (GitHub-native alignment)
- **Test Growth** — 92 → 118 tests
- **Emoji Fixes** — Test suite encoding standardized

## v0.2.0

- [**Export & Import CLI**](../features/export-import.md) — Portable team snapshots for moving squads between repos
- [**GitHub Issues Mode**](../features/github-issues.md) — Issue-driven development with `gh` CLI integration
- [**PRD Mode**](../features/prd-mode.md) — Product requirements decomposition into work items
- [**Human Team Members**](../features/human-team-members.md) — Mixed AI/human teams with routing
- [**Skills System**](../features/skills.md) — Earned knowledge with confidence lifecycle
- [**Tiered Response Modes**](../features/response-modes.md) — Direct/Lightweight/Standard/Full response depth
- [**Smart Upgrade**](../scenarios/upgrading.md) — Version-aware upgrades with migrations

## v0.1.0

- **Coordinator agent** — Orchestrates team formation and parallel work across specialized agents
- **Init command** — `squad` copies agent file and templates, creates placeholder directories
- **Upgrade command** — `squad upgrade` updates Squad-owned files without touching team state
- **Template system** — Charter, history, roster, routing, orchestration-log, run-output, raw-agent-output, scribe-charter, casting config
- **Persistent thematic casting** — Agents get named from film universes (The Usual Suspects, Alien, Ocean's Eleven)
- **Parallel agent execution** — Coordinator fans out work to multiple specialists simultaneously
- **Memory architecture** — Per-agent `history.md`, shared `decisions.md`, session `log/`
- **Reviewer protocol** — Agents with review authority can reject work and reassign
- **Scribe agent** — Silent memory manager, merges decisions, maintains logs
