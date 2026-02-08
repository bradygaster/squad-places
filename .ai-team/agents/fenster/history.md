# Project Context

- **Owner:** bradygaster (bradygaster@users.noreply.github.com)
- **Project:** Squad — AI agent teams that grow with your code. Democratizing multi-agent development on GitHub Copilot. Mission: beat the industry to what customers need next.
- **Stack:** Node.js, GitHub Copilot CLI, multi-agent orchestration
- **Created:** 2026-02-07

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### Runtime Architecture
- **No traditional runtime exists** — the entire orchestration system is a 32KB markdown file (`.github/agents/squad.agent.md`) that GitHub Copilot reads and executes via LLM interpretation
- **Installer is minimal by design** (`index.js`, 65 lines) — copies agent manifest, creates directory structure, copies templates to `.ai-team-templates/`
- **Execution model**: Squad (coordinator) spawns agents via GitHub Copilot CLI's `task` tool with `agent_type: "general-purpose"`, each gets isolated context
- **File system as IPC** — agents write to `.ai-team/decisions/inbox/{name}-{slug}.md`, Scribe merges asynchronously to `decisions.md`
- **Context budget**: Coordinator uses 1.5%, mature agent (12 weeks) uses 4.4%, leaving 94% for actual work

### Critical Paths Requiring Code
- **Casting engine**: Universe selection algorithm (scoring by size fit, shape fit, resonance, LRU) should be deterministic Node.js code, not LLM judgment
- **Inbox collision detection**: Need timestamp suffixes or UUIDs in decision inbox filenames to prevent overwrites when agents pick same slug
- **Orchestration logging**: Spec requires "single batched write" but doesn't specify format — need concrete implementation for `.ai-team/orchestration-log/`
- **Casting overflow**: 3-tier strategy (diegetic expansion, thematic promotion, structural mirroring) needs character lookup tables per universe to prevent hallucination
- **Migration detection**: Need version stamp in `team.md` to detect pre-casting repos and stale installs

### Windows Compatibility Concerns
- Path resolution: Agents must run `git rev-parse --show-toplevel` before resolving `.ai-team/` paths (spec acknowledges this, but no enforcement)
- Installer uses `path.join()` correctly for cross-platform path separators
- Need testing for file locking behavior during concurrent inbox writes on Windows

📌 Team update (2026-02-08): Proposal-first workflow adopted — all meaningful changes require proposals before execution. Write to `docs/proposals/`, review gates apply. — decided by Keaton + Verbal
📌 Team update (2026-02-08): Stay independent, optimize around Copilot — Squad will not become a Copilot SDK product. Filesystem-backed memory preserved as killer feature. — decided by Kujan
📌 Team update (2026-02-08): Stress testing prioritized — Squad must build a real project using its own workflow to validate orchestration under real conditions. — decided by Keaton
📌 Team update (2026-02-08): Baseline testing needed — zero automated tests today; `tap` framework + integration tests required before broader adoption. — decided by Hockney
📌 Team update (2026-02-08): DevRel polish identified — six onboarding gaps to close: install output, sample-prompts linking, "Why Squad?" section, casting elevation, troubleshooting, demo video. — decided by McManus
📌 Team update (2026-02-08): Agent experience evolution proposed — adaptive spawn prompts, reviewer protocol with guidance, proactive coordinator chaining. — decided by Verbal
📌 Team update (2026-02-08): Portable Squads architecture decided — history split (Portable Knowledge vs Project Learnings), JSON manifest export, no merge in v1. — decided by Keaton
📌 Team update (2026-02-08): Tiered response modes proposed — Direct/Lightweight/Standard/Full spawn tiers to reduce late-session latency. Context caching + conditional Scribe spawning as P0 fixes. — decided by Kujan + Verbal
📌 Team update (2026-02-08): Portable squads platform feasibility confirmed — pure CLI/filesystem, ~80 lines in index.js, .squad JSON format, no merge in v0.1. — decided by Kujan
📌 Team update (2026-02-08): Portable squads memory architecture — preferences.md (portable) split from history.md (project-local), squad-profile.md for team identity, import skips casting ceremony. — decided by Verbal

### Key File Paths
- `.github/agents/squad.agent.md` — authoritative governance (32KB spec, source of truth)
- `index.js` — installer entrypoint (65 lines, copies manifest + templates)
- `.ai-team/casting/registry.json` — persistent agent-to-name mappings
- `.ai-team/casting/history.json` — universe usage history, assignment snapshots
- `.ai-team/casting/policy.json` — universe allowlist, capacity limits
- `.ai-team/decisions/inbox/` — drop-box for parallel decision writes (merged by Scribe)
- `templates/` — copied to `.ai-team-templates/` as format guides

### Forwardability and Upgrade Architecture
- **The skip-if-exists pattern blocks upgrades** — `index.js` line 30 checks `fs.existsSync(agentDest)` and skips, which means users on v0.1.0 never receive coordinator improvements. This is the core forwardability problem.
- **File ownership model is the foundation** — every file must be classified as Squad-owned (safe to overwrite), user-owned (never touch), or additive-only (create if missing). Getting this classification wrong means either breaking user state or failing to upgrade.
- **squad.agent.md is stateless by design** — the coordinator reads it fresh every session with no cached state. This means overwriting it IS the upgrade. No running state migration needed for coordinator changes, only for `.ai-team/` files.
- **Version detection needs three strategies** — `.squad-version` metadata file (primary), frontmatter parsing (secondary), presence detection (fallback for v0.1.0 pre-versioning installs). Defensive detection is critical because we can't control what state users will be in.
- **Migrations must be idempotent** — users will run `upgrade` multiple times, migrations will encounter partially-migrated state, and failures must not corrupt data. Every migration checks if its work is already done before doing it.
- **Argument routing stays minimal** — `process.argv[2]` positional subcommands (upgrade/export/import/help/version) with no dependency on yargs or commander. Aligns with Proposal 008's export/import pattern. `index.js` stays under 150 lines.
- **Windows path safety is non-negotiable** — all file operations use `path.join()`. No hardcoded separators. No symlinks. No shell commands in migrations. Pure `fs` operations only.
- **Backup before overwrite, always** — `squad.agent.md.v{old}.bak` preserves user customizations. Critical failures (backup or overwrite) abort. Non-critical failures (migrations, new dirs) warn and continue.

📌 Proposal written: `docs/proposals/011-forwardability-and-upgrade-path.md` — complete upgrade system design with migration framework, version detection, error handling, and full index.js sketch.
📌 Team update (2026-02-08): v1 Sprint Plan decided — 3 sprints, 10 days. Sprint 1: forwardability + latency. Sprint 2: history split + skills + export/import. Sprint 3: README + tests + polish. — decided by Keaton
📌 Team update (2026-02-08): Skills system designed — skills.md per agent for transferable domain expertise, distinct from preferences and history. Six skill types, confidence lifecycle, skill-aware routing. — decided by Verbal
📌 Team update (2026-02-08): Skills platform feasibility confirmed — skills in spawn prompts, store_memory rejected, file paths are frozen API contracts, defensive forwardability via existence checks. — decided by Kujan
📌 Team update (2026-02-08): v1 test strategy decided — node:test + node:assert (zero deps), 9 test categories, 6 blocking quality gates, 90% line coverage. index.js refactoring recommended. — decided by Hockney
📌 Team update (2026-02-08): v1 messaging and launch planned — "Throw MY squad at it" tagline, two-project demo arc, 7-day launch sequence, GitHub Discussions first. — decided by McManus
📌 Team update (2026-02-08): P0 silent success bug identified — ~40% of agents complete work but report "no response." Spawn prompt reorder + file verification mitigations. — decided by Kujan
📌 Team update (2026-02-09): Agent Skills Open Standard adopted — SKILL.md format with MCP tool declarations, built-in vs learned skills, progressive disclosure. Replaces flat skills.md. — decided by Kujan
