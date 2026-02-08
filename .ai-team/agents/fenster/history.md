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

### Key File Paths
- `.github/agents/squad.agent.md` — authoritative governance (32KB spec, source of truth)
- `index.js` — installer entrypoint (65 lines, copies manifest + templates)
- `.ai-team/casting/registry.json` — persistent agent-to-name mappings
- `.ai-team/casting/history.json` — universe usage history, assignment snapshots
- `.ai-team/casting/policy.json` — universe allowlist, capacity limits
- `.ai-team/decisions/inbox/` — drop-box for parallel decision writes (merged by Scribe)
- `templates/` — copied to `.ai-team-templates/` as format guides
