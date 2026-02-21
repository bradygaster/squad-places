# CLI Migration Plan — Beta to SDK v1
**Created:** 2026-02-22  
**Author:** Keaton (Lead)  
**Status:** Proposed

---

## Executive Summary

**The Gap:** Squad SDK v1 (v0.6.0-alpha.0) has a full runtime — sessions, tools, hooks, coordinator, casting, ralph — but the CLI is a skeleton. The beta CLI at `C:\src\squad\index.js` is 1,496 lines and handles init, upgrade, watch, export, import, plugin marketplace, copilot agent management, and email scrubbing. The SDK CLI at `src/cli/` has type stubs only.

**The Goal:** Make `squad-sdk` the primary working repository. The team needs to be re-established there (respawn prompt exists at `docs/respawn-prompt.md`), and the CLI must achieve feature parity with beta before the beta repo can be archived.

**What Exists:**
- ✅ SDK Runtime: 13 modules, 16,233 LOC, 1,551 tests
- ✅ Beta CLI: 1,496 lines, 9 commands, zero-dep scaffolding
- ✅ Distribution: `npx github:bradygaster/squad` (GitHub-native)
- ✅ Template system: 33 files in `templates/` directory (beta)
- ✅ Coordinator: `squad.agent.md` (32KB, version-stamped)
- ✅ Respawn DNA: `docs/respawn-prompt.md` for team rebirth

**What's Missing:**
- ❌ CLI entry point with subcommand routing
- ❌ Init command (template copying, squad.agent.md installation, version stamping)
- ❌ Upgrade command (overwrite squad-owned files, `--migrate-directory` flag)
- ❌ Watch command (Ralph's local polling process via `gh` CLI)
- ❌ Export/import commands (squad-export.json format)
- ❌ Plugin marketplace CLI (add/remove/list/browse)
- ❌ Copilot agent CLI (add/remove, auto-assign config)
- ❌ Email scrubbing utility (PII cleanup)
- ❌ Project type detection (npm/go/python/java/dotnet/unknown)
- ❌ Workflow generation (project-type-aware stubs for non-npm)
- ❌ Migration runner (version-based additive migrations)
- ❌ Template infrastructure in SDK repo

**Success Criteria:**
1. `npx github:bradygaster/squad` runs from squad-sdk and has CLI feature parity
2. Squad team exists in squad-sdk (`.squad/` directory, self-referential)
3. Beta repo archived with redirect notice
4. All 9 beta commands work in SDK with zero regressions
5. TypeScript strict mode, ESM-only, Node ≥20
6. Zero-dep scaffolding preserved (init/upgrade use no SDK deps)

---

## CLI Feature Inventory

| Feature | Beta (index.js) | SDK Status | Gap | Lines |
|---------|----------------|------------|-----|-------|
| **Entry point** | ✅ main() with --version, --help, -v, -h | 🟡 Stub only | Full routing logic | 84 |
| **Init command** | ✅ Template copy (33 files), squad.agent.md, .gitattributes, .gitignore, version stamp | ❌ Type stubs only | Complete implementation | ~600 |
| **Upgrade command** | ✅ Overwrite squad-owned files, migrations, version compare | ❌ Type stubs only | Complete implementation | ~400 |
| **--migrate-directory** | ✅ Rename .ai-team/ → .squad/, email scrub, .gitattributes/.gitignore update | ❌ Not present | Complete implementation | ~100 |
| **Watch command** | ✅ Ralph local polling (gh CLI), issue triage, @copilot auto-assign | ❌ Not present | Complete implementation | ~160 |
| **Export command** | ✅ squad-export.json format, --out flag | ❌ Not present | Complete implementation | ~80 |
| **Import command** | ✅ Import from export, --force, conflict detection, history split | ❌ Not present | Complete implementation | ~130 |
| **Plugin marketplace** | ✅ add/remove/list/browse, marketplaces.json registry | ❌ Not present | Complete implementation | ~130 |
| **Copilot agent CLI** | ✅ copilot [--off] [--auto-assign], team.md manipulation | ❌ Not present | Complete implementation | ~120 |
| **scrub-emails** | ✅ PII cleanup from .squad/ or .ai-team/ | ❌ Not present | Complete implementation | ~120 |
| **Project type detection** | ✅ Detect npm/go/python/java/dotnet/unknown | ❌ Not present | Complete implementation | ~20 |
| **Workflow generation** | ✅ Project-type-aware stubs for CI/release/docs | ❌ Not present | Complete implementation | ~160 |
| **Version stamping** | ✅ Replace {version} in squad.agent.md | ❌ Not present | Complete implementation | ~20 |
| **Migration runner** | ✅ Version-based additive migrations (skills/, plugins/, email scrub) | ❌ Not present | Complete implementation | ~50 |
| **Directory detection** | ✅ .squad/ first, fall back to .ai-team/ with deprecation warning | ❌ Not present | Complete implementation | ~30 |
| **Color/emoji output** | ✅ GREEN/RED/YELLOW/DIM/BOLD/RESET + emoji | ❌ Not present | Complete implementation | ~10 |
| **--self flag** | ✅ Refresh squad repo's own .ai-team/ from templates | ❌ Not present | Complete implementation | ~40 |
| **Total beta CLI** | **1,496 lines** | **65 lines (stubs)** | **~1,431 lines** | |

---

## Architecture Decision: Port vs Rewrite

### Option A: Line-by-Line Port to TypeScript
**Approach:** Preserve beta CLI structure, translate JS → TS, add types.

**Pros:**
- Minimal risk (behavior is known, tested in production)
- Fast to ship (mechanical translation)
- Easy to verify (line-by-line diffing)

**Cons:**
- Carries over tech debt (1,496-line single file)
- Misses opportunity to leverage SDK primitives
- No type safety for config/agent operations

### Option B: Full Rewrite Using SDK Primitives
**Approach:** Rewrite using typed config, hooks, casting, SDK tools.

**Pros:**
- Type-safe config operations (SquadConfig, agent specs)
- Leverage existing SDK infrastructure (sharing, marketplace, config modules)
- Clean module boundaries (cli/init.ts, cli/upgrade.ts, cli/watch.ts)

**Cons:**
- High risk (new code, unknown unknowns)
- Breaks zero-dep constraint (SDK is a dependency)
- Long timeline (3-4 weeks vs 1 week)

### Option C: Hybrid (RECOMMENDED)
**Approach:**
1. **Port the scaffolding** (init, upgrade, version stamping, template copying, project detection) with **zero deps** — pure Node.js stdlib (fs, path, child_process)
2. **Rewrite the runtime commands** (watch, export/import, plugin) to **use SDK primitives** (config, sharing, marketplace, ralph)

**Pros:**
- Preserves zero-dep constraint where it matters (init/upgrade)
- Leverages SDK for runtime features (type-safe, tested)
- Reasonable risk profile (scaffolding is mechanical, runtime is greenfield)
- Allows phased delivery (scaffolding first, runtime second)

**Cons:**
- Two dependency profiles (scaffolding vs runtime) — needs clear separation

**DECISION: Hybrid (Option C).**

**Implementation Strategy:**
- `src/cli/core/` — zero-dep scaffolding (init, upgrade, templates, version, migrations)
- `src/cli/commands/` — SDK-dependent runtime commands (watch, export, import, plugin, copilot)
- Entry point (`src/index.ts`) routes to appropriate layer based on command

---

## PRD Breakdown

### PRD 15: CLI Entry Point & Subcommand Router
**Scope:**
- Wire `main()` to real subcommand routing (not just stubs)
- Preserve `--version`, `--help`, `-v`, `-h` flags
- Route to zero-dep core (init, upgrade) or SDK commands (watch, export, etc.)
- Color/emoji output utilities (GREEN/RED/YELLOW/DIM/BOLD/RESET)
- Error handling with `fatal()` helper
- `bin` field in package.json points to `dist/index.js`

**Dependencies:** None (foundation)

**Estimated Effort:** 2-3 hours

**Files:**
- `src/index.ts` (main entry point, rewrite from stub)
- `src/cli/core/output.ts` (color/emoji utilities)
- `src/cli/core/errors.ts` (fatal() helper)

---

### PRD 16: Init Command (npx github:bradygaster/squad)
**Scope:**
- Template copying (33 files from `templates/` directory)
  - Copy to `.squad/` (new installs) or detect `.ai-team/` (legacy support)
  - Files: team.md, routing.md, ceremonies.md, skills/, workflows/, copilot-instructions.md, etc.
- squad.agent.md installation to `.github/agents/`
- Version stamping (replace `{version}` placeholders in agent doc)
- .gitattributes creation (linguist overrides for .squad/)
- .gitignore creation (.squad/orchestration-log/, .squad/log/)
- Directory structure creation (decisions/inbox/, casting/, skills/, plugins/, identity/)
- Project type detection (npm/go/python/java/dotnet/unknown)
- Workflow generation (project-type-aware stubs for non-npm projects)
- Starter skills copying (if skills/ is empty)
- Identity files (now.md, wisdom.md)
- Squad directory detection with deprecation warning for .ai-team/
- Skip existing files (idempotent init)
- Output formatting (colors, emoji, progress)

**Dependencies:** PRD 15 (CLI router)

**Estimated Effort:** 8-10 hours

**Files:**
- `src/cli/core/init.ts` (port from beta index.js lines 1098-1496, zero-dep)
- `src/cli/core/templates.ts` (template copying logic)
- `src/cli/core/project-type.ts` (project detection)
- `src/cli/core/workflows.ts` (workflow generation)
- `templates/` directory (moved from beta repo to SDK repo)

**Template Migration:**
- Move `C:\src\squad\templates\` → `C:\src\squad-sdk\templates\`
- Move `C:\src\squad\.github\agents\squad.agent.md` → `C:\src\squad-sdk\templates\squad.agent.md`
- Update beta repo to point to SDK repo for template source (transition period)

---

### PRD 17: Upgrade Command
**Scope:**
- squad.agent.md overwrite (always overwrites, version-stamped)
- Template directory overwrite (`.squad-templates/` or `.ai-team-templates/`)
- Workflow file overwrite (project-type-aware)
- copilot-instructions.md refresh (if @copilot is enabled)
- Version comparison (skip if already current, but run missing migrations)
- Migration runner (version-based additive operations)
  - v0.2.0: Create skills/ directory
  - v0.4.0: Create plugins/ directory
  - v0.5.0: Scrub email addresses (PII cleanup)
- `--migrate-directory` flag: rename .ai-team/ → .squad/, scrub emails, update .gitattributes/.gitignore
- `--self` flag: refresh squad repo's own .ai-team/ from templates (meta-upgrade)
- Never touch `.squad/` or `.ai-team/` state files (team.md, decisions/, agents/*/history.md)
- Output formatting (colors, emoji, version delta)

**Dependencies:** PRD 15 (CLI router)

**Estimated Effort:** 6-8 hours

**Files:**
- `src/cli/core/upgrade.ts` (port from beta index.js lines 1115-1405, zero-dep)
- `src/cli/core/migrations.ts` (migration runner)
- `src/cli/core/version.ts` (semver comparison, version stamping)
- `src/cli/core/email-scrub.ts` (PII cleanup)

---

### PRD 18: Watch Command (Ralph CLI)
**Scope:**
- Standalone polling process (not inside Copilot)
- `--interval` flag (default: 10 minutes)
- GitHub issue scanning via `gh` CLI
  - `gh issue list --label "squad" --state open --json number,title,labels,assignees`
- Auto-triage based on team roles (parse team.md roster)
  - Frontend/UI → frontend agent
  - Backend/API → backend agent
  - Test/bug/fix → QA agent
  - No match → Lead
- Label assignment (`gh issue edit <number> --add-label "squad:<agent>"`)
- @copilot auto-assignment (if enabled in team.md)
  - `gh issue list --label "squad:copilot" --state open --json number,title,assignees`
  - `gh issue edit <number> --add-assignee copilot-swe-agent`
- Ctrl+C to stop gracefully
- Output formatting (timestamp, colors, emoji)

**Dependencies:** PRD 15 (CLI router)

**Estimated Effort:** 4-5 hours

**Files:**
- `src/cli/commands/watch.ts` (port from beta index.js lines 104-265, SDK-optional)
- `src/cli/core/gh-cli.ts` (gh CLI wrapper utilities)

---

### PRD 19: Export/Import Commands
**Scope:**

**Export:**
- squad-export.json format (v1.0 schema)
  - Fields: version, exported_at, squad_version, casting, agents, skills
- `--out` flag to override default path (default: `squad-export.json`)
- Read from casting/ (registry.json, policy.json, history.json)
- Read from agents/*/charter.md, agents/*/history.md
- Read from skills/*/SKILL.md
- Warning: "Review agent histories before sharing — may contain PII"

**Import:**
- squad-export.json validation (version 1.0, required fields)
- `--force` flag to replace existing squad (archives current to `.ai-team-archive-<timestamp>/`)
- Conflict detection (squad already exists → error unless --force)
- History split: portable knowledge vs project learnings
  - Preserve: learnings, portable knowledge, team updates
  - Separate: key file paths, sprint history, session logs
- Write casting state, agents, skills to `.ai-team/` or `.squad/`
- Output formatting (colors, emoji, file counts)

**Dependencies:** PRD 15 (CLI router), SDK sharing module (optional)

**Estimated Effort:** 6-7 hours

**Files:**
- `src/cli/commands/export.ts` (port from beta index.js lines 836-914, SDK-integrated)
- `src/cli/commands/import.ts` (port from beta index.js lines 917-1096, SDK-integrated)
- `src/cli/core/history-split.ts` (port from beta index.js lines 1032-1096)
- Wire to existing `src/sharing/` module (export.ts, import.ts, history-split.ts exist)

---

### PRD 20: Plugin Marketplace CLI
**Scope:**
- `squad plugin marketplace add <owner/repo>` — register marketplace
- `squad plugin marketplace remove <name>` — unregister marketplace
- `squad plugin marketplace list` — show registered marketplaces
- `squad plugin marketplace browse <name>` — list plugins in marketplace via gh CLI
  - `gh api repos/{owner/repo}/contents --jq "[.[] | select(.type == \"dir\") | .name]"`
- marketplaces.json registry (`.squad/plugins/marketplaces.json`)
  - Schema: `{ marketplaces: [{ name, source, added_at }] }`
- Error handling (gh CLI not found, API rate limits, invalid repo)
- Output formatting (colors, emoji, plugin counts)

**Dependencies:** PRD 15 (CLI router), SDK marketplace module (optional)

**Estimated Effort:** 3-4 hours

**Files:**
- `src/cli/commands/plugin.ts` (port from beta index.js lines 716-833, SDK-integrated)
- Wire to existing `src/marketplace/` module (browser.ts, packaging.ts exist)

---

### PRD 21: Copilot Agent CLI
**Scope:**
- `squad copilot` — add @copilot to team roster
  - Insert "Coding Agent" section into team.md (before "Project Context")
  - Copy copilot-instructions.md to `.github/`
  - Capability profiles (🟢 good fit, 🟡 needs review, 🔴 not suitable)
- `squad copilot --off` — remove @copilot from team roster
  - Remove "Coding Agent" section from team.md
  - Delete `.github/copilot-instructions.md`
- `squad copilot --auto-assign` — enable auto-assignment
  - Update team.md: `<!-- copilot-auto-assign: true -->`
- team.md manipulation (parse, edit, write)
- Guidance for COPILOT_ASSIGN_TOKEN (classic PAT with repo scope)
- Output formatting (colors, emoji, status)

**Dependencies:** PRD 15 (CLI router)

**Estimated Effort:** 3-4 hours

**Files:**
- `src/cli/commands/copilot.ts` (port from beta index.js lines 598-713, zero-dep)
- `src/cli/core/team-md.ts` (team.md parser/editor utilities)

---

### PRD 22: Repo Independence — Working Inside squad-sdk
**Scope:**
- Team respawn in squad-sdk (use `docs/respawn-prompt.md`)
  - Create `.squad/` directory in squad-sdk repo
  - Spawn team with accumulated knowledge from beta
  - Self-referential: squad-sdk's own Squad team managing squad-sdk
- Template files embedded in SDK repo (`templates/` directory)
- squad.agent.md bundled with SDK (not external fetch)
- Distribution: `npx github:bradygaster/squad` now points to `bradygaster/squad-sdk` (not `bradygaster/squad`)
  - Update beta repo README with redirect notice
  - Archive beta repo (mark as read-only, point to squad-sdk)
- CI/CD in squad-sdk (GitHub Actions for tests, build, release)
- Documentation update (README, docs/ site)

**Dependencies:** PRD 16 (init command must work to spawn team)

**Estimated Effort:** 4-6 hours (mostly coordination + respawn)

**Files:**
- `C:\src\squad-sdk\.squad\` directory created
- `C:\src\squad-sdk\templates\` directory populated
- `C:\src\squad\README.md` updated with redirect notice

---

## Dependency Graph

```
PRD 15 (CLI router)
├─→ PRD 16 (init) ──→ PRD 22 (repo independence)
├─→ PRD 17 (upgrade)
├─→ PRD 18 (watch)
├─→ PRD 19 (export/import)
├─→ PRD 20 (plugin)
└─→ PRD 21 (copilot)
```

**Critical Path:** PRD 15 → PRD 16 → PRD 22

**Parallel Work (after PRD 15):**
- PRD 17, 18, 19, 20, 21 can all proceed independently once CLI router exists

---

## Milestone Proposal

### M7: CLI Foundation (2-3 weeks)
**Goal:** "npx can init again"

**PRDs:** 15, 16
**Deliverables:**
- CLI entry point with subcommand routing
- Init command (template copy, squad.agent.md, version stamping, project detection)
- Templates moved to squad-sdk repo
- Zero-dep scaffolding preserved
- Basic tests (init creates expected files, version stamping works)

**Success Criteria:**
- `npx github:bradygaster/squad-sdk` runs from main branch
- `squad init` creates .squad/ directory with 33+ files
- squad.agent.md is version-stamped correctly
- Project type detection works (npm/go/python/java/dotnet/unknown)
- Workflow stubs generated for non-npm projects

---

### M8: CLI Parity (3-4 weeks)
**Goal:** "Every beta command works in v1"

**PRDs:** 17, 18, 19, 20, 21
**Deliverables:**
- Upgrade command (migrations, --migrate-directory, --self)
- Watch command (Ralph local polling, gh CLI integration)
- Export/import commands (squad-export.json format, history split)
- Plugin marketplace CLI (add/remove/list/browse)
- Copilot agent CLI (add/remove, auto-assign)
- Email scrubbing utility
- Comprehensive tests (all commands, error cases, edge cases)

**Success Criteria:**
- All 9 beta commands ported with zero regressions
- `squad upgrade --migrate-directory` works (renames .ai-team/ → .squad/)
- `squad watch` polls GitHub issues via gh CLI
- `squad export` + `squad import` round-trips with 100% fidelity
- `squad plugin marketplace browse <name>` lists plugins
- `squad copilot --auto-assign` enables auto-assignment
- Test coverage ≥80% for CLI commands

---

### M9: Repo Independence (1-2 weeks)
**Goal:** "Team lives in squad-sdk, beta is archived"

**PRDs:** 22
**Deliverables:**
- Squad team spawned in squad-sdk (`.squad/` directory)
- Respawn using `docs/respawn-prompt.md` (team DNA transferred)
- Beta repo archived with redirect notice
- Documentation updated (README, docs/ site)
- CI/CD in squad-sdk (tests, build, release)
- Distribution points to squad-sdk (npx github:bradygaster/squad → squad-sdk)

**Success Criteria:**
- Squad team exists in squad-sdk and is self-managing
- Beta repo README says "This repo is archived — use squad-sdk"
- `npx github:bradygaster/squad` resolves to squad-sdk main branch
- All squad-sdk tests pass (1,551 existing + new CLI tests)
- Documentation reflects squad-sdk as canonical repo

---

## What Stays in Beta

**Transition Strategy:**
1. **Immediate (M7):** Beta repo stays active, continues to serve `npx github:bradygaster/squad`
2. **M7 Complete:** Beta README updated with notice: "CLI is moving to squad-sdk — install from there for latest"
3. **M8 Complete:** Beta repo marked as "maintenance mode" — bug fixes only
4. **M9 Complete:** Beta repo archived, README redirects to squad-sdk

**Post-Archive:**
- Beta repo becomes read-only
- GitHub redirects `bradygaster/squad` → `bradygaster/squad-sdk` (optional, requires repo rename)
- npx continues to work via GitHub tags (`npx github:bradygaster/squad#v0.5.3` for legacy)

---

## Risk Register

### Risk 1: Template Drift
**Description:** Beta templates continue to evolve while CLI is being ported. SDK templates become stale.

**Likelihood:** MEDIUM  
**Impact:** MEDIUM  

**Mitigation:**
- Freeze beta template changes during M7 (init command porting)
- Copy templates to SDK repo in M7 kickoff (not M7 end)
- Lock beta to v0.5.x (no new features, only bug fixes)
- Use git submodule for templates (beta and SDK share source) — REJECTED (adds complexity)
- **Decision:** One-time copy in M7, then SDK is source of truth

---

### Risk 2: npx Distribution Change
**Description:** Moving from `npx github:bradygaster/squad` (beta) to `npx github:bradygaster/squad-sdk` breaks user installs.

**Likelihood:** HIGH  
**Impact:** HIGH  

**Mitigation:**
- Keep both repos active during transition (M7-M8)
- Beta README redirects to squad-sdk in M7
- Beta repo stays at v0.5.x (stable channel)
- squad-sdk uses v0.6.0+ (insider channel)
- Rename squad-sdk → squad (GitHub repo rename) in M9 — RISKY
- **Decision:** Communicate change via README, GitHub Discussions, and release notes. Accept breaking change for GitHub-native distribution.

---

### Risk 3: Squad.agent.md Bundling
**Description:** squad.agent.md is 32KB and must travel with CLI. Embedding in source adds maintenance overhead.

**Likelihood:** LOW  
**Impact:** LOW  

**Mitigation:**
- Store in `templates/squad.agent.md` (like beta)
- Copy to `.github/agents/` during init/upgrade (existing pattern)
- Version stamp at copy time (existing pattern)
- No change from beta — risk is continuity, not new problem

---

### Risk 4: Zero-Dep Constraint vs SDK Dependency
**Description:** Init/upgrade must be zero-dep (can't install SDK to run init). But watch/export/import need SDK. Two dependency profiles in one CLI.

**Likelihood:** HIGH  
**Impact:** MEDIUM  

**Mitigation:**
- Split CLI into two layers:
  - `src/cli/core/` — zero-dep scaffolding (init, upgrade, templates)
  - `src/cli/commands/` — SDK-dependent runtime (watch, export, plugin)
- Entry point conditionally loads SDK only for runtime commands
- Init/upgrade ship as pure Node.js scripts (no compilation needed)
- **Decision:** Hybrid architecture (PRD 15 enforces this split)

---

### Risk 5: Team Respawn Fidelity
**Description:** Respawning team in squad-sdk may lose context/knowledge from beta team.

**Likelihood:** MEDIUM  
**Impact:** HIGH  

**Mitigation:**
- `docs/respawn-prompt.md` already exists with full team DNA
- Agent histories preserved in beta repo (copy to SDK agent directories)
- Skills transferred (export from beta, import to SDK)
- Casting policy preserved (universe, model selection)
- **Decision:** Use respawn-prompt.md verbatim, manual history transfer for core agents (Keaton, Kujan, McManus, Fenster, Verbal)

---

### Risk 6: Workflow Stubs for Non-NPM Projects
**Description:** Generating project-type-aware workflow stubs is complex. Risk of incorrect detection or broken stubs.

**Likelihood:** MEDIUM  
**Impact:** LOW  

**Mitigation:**
- Port existing beta logic (tested in production)
- Project detection is heuristic-based (package.json → npm, go.mod → go, etc.)
- Workflow stubs are minimal (TODO comments for user to fill in)
- Users can delete/customize workflows (non-critical path)
- **Decision:** Port beta logic verbatim, accept TODO stubs as good enough

---

## Timeline & Effort Estimate

| Milestone | PRDs | Effort | Duration | Target |
|-----------|------|--------|----------|--------|
| M7: CLI Foundation | PRD 15, 16 | 10-13 hours | 2-3 weeks | Week of Mar 10 |
| M8: CLI Parity | PRD 17, 18, 19, 20, 21 | 22-27 hours | 3-4 weeks | Week of Apr 7 |
| M9: Repo Independence | PRD 22 | 4-6 hours | 1-2 weeks | Week of Apr 21 |
| **Total** | **7 PRDs** | **36-46 hours** | **6-9 weeks** | **End of April 2026** |

**Assumptions:**
- Single engineer (Brady or delegated team member)
- Part-time work (10-15 hours/week)
- No major blockers (gh CLI available, GitHub API stable)
- Test writing included in effort estimates

---

## Conclusion

**Recommendation:** Proceed with Hybrid Architecture (Option C). Port scaffolding (init/upgrade) as zero-dep, rewrite runtime commands (watch/export/plugin) using SDK. Deliver in 3 milestones (M7, M8, M9) over 6-9 weeks.

**Next Steps:**
1. **Approve this plan** (Brady review + decision)
2. **Create GitHub issues** for PRD 15-22 in squad-sdk repo
3. **Create milestones** M7, M8, M9 in squad-sdk repo
4. **Copy templates** from beta to SDK (kickoff M7)
5. **Spawn team in squad-sdk** (use respawn-prompt.md)
6. **Lock beta repo** to maintenance mode (v0.5.x bug fixes only)
7. **Start PRD 15** (CLI router) immediately

**Owner:** Brady (delegation TBD)  
**Timeline:** 6-9 weeks (March 10 - April 21, 2026)  
**Risk Level:** MEDIUM (mitigated via hybrid architecture + phased delivery)

---

**Document Metadata:**
- **Version:** 1.0
- **Last Updated:** 2026-02-22
- **Status:** Proposed (awaiting Brady approval)
- **Related Docs:** `docs/respawn-prompt.md`, `docs/codebase-comparison.md`, `docs/team-to-brady.md`
