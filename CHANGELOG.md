# Changelog

## [0.1.0] — 2026-02-08

### Added
- Coordinator agent (`squad.agent.md`) — orchestrates team formation and parallel work
- Init command (`npx github:bradygaster/squad`) — copies agent file and templates, creates placeholder directories
- Upgrade command (`npx github:bradygaster/squad upgrade`) — updates Squad-owned files without touching team state
- Template system — charter, history, roster, routing, orchestration-log, run-output, raw-agent-output, scribe-charter, casting config (policy, registry, history)
- Persistent thematic casting — agents get named from film universes (The Usual Suspects, Alien, Ocean's Eleven)
- File ownership model — Squad owns `squad.agent.md` and `.ai-team-templates/`; users own `.ai-team/`
- Parallel agent execution — coordinator fans out work to multiple specialists simultaneously
- Memory architecture — per-agent `history.md`, shared `decisions.md`, session `log/`
- Reviewer protocol — agents with review authority can reject work and reassign
- Scribe agent — silent memory manager, merges decisions, maintains logs
- `--version` / `-v` flag
- `--help` / `-h` flag and `help` subcommand
- CI pipeline (GitHub Actions) — tests on push/PR to main and dev
- Test suite — 27 tests covering init, re-init, upgrade, flags, error handling, and edge cases

### What ships
- `index.js` — the CLI entry point
- `.github/agents/squad.agent.md` — the coordinator agent
- `templates/**/*` — 11 template files (charter.md, history.md, roster.md, routing.md, orchestration-log.md, run-output.md, raw-agent-output.md, scribe-charter.md, casting-policy.json, casting-registry.json, casting-history.json)

These are the only files distributed via `npx github:bradygaster/squad`. Defined by the `files` array in `package.json`.

### What doesn't ship
- `.ai-team/` is NOT in the package — it is created by Copilot at runtime when agents form a team
- `docs/`, `test/`, `.ai-team/`, `.github/workflows/` — development and team artifacts stay in the source repo only
- Agent knowledge (history, decisions, casting state) — generated per-project, never bundled
