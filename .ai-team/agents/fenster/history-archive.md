# Fenster — History Archive

Archived entries from initial sessions. These entries were summarized into `## Core Context` in history.md.

---

## Archived: 2026-02-07 Foundational Entries

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
