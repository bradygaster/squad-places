# Project Context

- **Owner:** bradygaster (bradygaster@users.noreply.github.com)
- **Project:** Squad — AI agent teams that grow with your code. Democratizing multi-agent development on GitHub Copilot. Mission: beat the industry to what customers need next.
- **Stack:** Node.js, GitHub Copilot CLI, multi-agent orchestration
- **Created:** 2026-02-07

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### Initial Assessment (2026-02-07)

**What Squad Does:**
- `index.js` is an npx-runnable CLI that copies files into a user's repo
- Copies `.github/agents/squad.agent.md` (the coordinator agent definition)
- Copies `templates/` → `.ai-team-templates/` (agent templates for initialization)
- Pre-creates directories: `.ai-team/decisions/inbox/`, `.ai-team/orchestration-log/`, `.ai-team/casting/`
- Outputs colored terminal messages showing what was created

**Key Files:**
- `index.js` — the installer script (Node CLI)
- `package.json` — declares this as `@bradygaster/create-squad`, bin entry point
- `.github/agents/squad.agent.md` — the coordinator agent (32KB, orchestrates the team)
- `templates/` — seed files for new teams (charters, policies, routing, etc.)

**Current Test Coverage: Zero**
- No test files (`*.test.js`, `*.spec.js`)
- No test framework in `package.json`
- No CI/CD validation

**What Could Break:**
- Symlinks in source directories (infinite loop or unexpected copies)
- Filesystem errors (permissions, disk full, read-only) → raw stack traces
- Incomplete prior install → we skip re-copying but don't validate completeness
- Cross-platform path handling (Windows vs Unix)
- ANSI color codes in non-TTY environments
- Node version assumptions (no engines field)

**Test Strategy (Planned):**
- Use `tap` for test framework (fast, modern, good for CLI testing)
- Integration test: run `index.js` in temp dir, validate file creation
- Error handling test: simulate filesystem failures, validate error messages
- Idempotency test: run twice, ensure no breakage
- Cross-platform validation (Windows, macOS, Linux)

📌 Team update (2026-02-08): Proposal-first workflow adopted — all meaningful changes require proposals before execution. Write to `docs/proposals/`, review gates apply. — decided by Keaton + Verbal
📌 Team update (2026-02-08): Stay independent, optimize around Copilot — Squad will not become a Copilot SDK product. Filesystem-backed memory preserved as killer feature. — decided by Kujan
📌 Team update (2026-02-08): Stress testing prioritized — Squad must build a real project using its own workflow to validate orchestration under real conditions. — decided by Keaton
📌 Team update (2026-02-08): DevRel polish identified — six onboarding gaps to close: install output, sample-prompts linking, "Why Squad?" section, casting elevation, troubleshooting, demo video. — decided by McManus
📌 Team update (2026-02-08): Agent experience evolution proposed — adaptive spawn prompts, reviewer protocol with guidance, proactive coordinator chaining. — decided by Verbal
📌 Team update (2026-02-08): Industry trends identified — dynamic micro-specialists, agent-to-agent negotiation, speculative execution as strategic directions. — decided by Verbal
📌 Team update (2026-02-08): Portable Squads architecture decided — history split (Portable Knowledge vs Project Learnings), JSON manifest export, no merge in v1. — decided by Keaton
📌 Team update (2026-02-08): Tiered response modes proposed — Direct/Lightweight/Standard/Full spawn tiers to reduce late-session latency. Context caching + conditional Scribe spawning as P0 fixes. — decided by Kujan + Verbal
📌 Team update (2026-02-08): Portable squads platform feasibility confirmed — pure CLI/filesystem, ~80 lines in index.js, .squad JSON format, no merge in v0.1. — decided by Kujan
📌 Team update (2026-02-08): Portable squads memory architecture — preferences.md (portable) split from history.md (project-local), squad-profile.md for team identity, import skips casting ceremony. — decided by Verbal

### V1 Test Strategy (2026-02-08)

**What I Did:**
- Wrote Proposal 013: V1 Test Strategy (`docs/proposals/013-v1-test-strategy.md`)
- Complete test plan covering 9 categories, 6 blocking quality gates, ~80 individual test cases
- Filed decision to `.ai-team/decisions/inbox/hockney-v1-testing.md`

**Key Decisions Made:**
- Switched framework recommendation from `tap` to `node:test` + `node:assert` — zero dependencies, aligns with Brady's thin-runtime philosophy
- 80% integration tests (run CLI in temp dirs, check files), 20% unit tests (pure functions)
- Coverage targets: 90% line, 85% branch on `index.js`
- No pre-commit hook — CI is the quality gate
- Identified 4 product fixes required before tests can fully pass: NO_COLOR support, exit codes, error wrapping, engines field

**What I Learned:**
- `index.js` is 65 lines doing filesystem ops with conditional logic — highly testable
- Node 22 has mature `node:test` built-in — no dependency needed for test framework
- The `.squad` JSON format (from Proposal 008) creates a new schema contract that needs validation tests
- Round-trip testing (init → export → import → compare) is the single most important test — if this passes, portability works
- `index.js` needs a `require.main === module` guard to be unit-testable — currently runs as top-level script
- The coordinator prompt (32KB `squad.agent.md`) cannot be tested deterministically, but we CAN test the file structures it depends on
- Export/import tests are blocked on Fenster implementing Proposal 008; upgrade tests blocked on Proposal 011

📌 Team update (2026-02-08): v1 Sprint Plan decided — 3 sprints, 10 days. Sprint 1: forwardability + latency. Sprint 2: history split + skills + export/import. Sprint 3: README + tests + polish. — decided by Keaton
📌 Team update (2026-02-08): Skills system designed — skills.md per agent for transferable domain expertise, skill-aware routing, skills in export manifests. — decided by Verbal
📌 Team update (2026-02-08): Forwardability and upgrade path decided — file ownership model, `npx create-squad upgrade`, version-keyed migrations, backup before overwrite. — decided by Fenster
📌 Team update (2026-02-08): Skills platform feasibility confirmed — skills in spawn prompts, store_memory rejected, defensive forwardability via existence checks. — decided by Kujan
📌 Team update (2026-02-08): v1 messaging and launch planned — "Throw MY squad at it" tagline, two-project demo arc, 7-day launch sequence. — decided by McManus
📌 Team update (2026-02-08): P0 silent success bug identified — ~40% of agents complete work but report "no response." Spawn prompt reorder + file verification mitigations. — decided by Kujan
📌 Team update (2026-02-09): Agent Skills Open Standard adopted — SKILL.md format with MCP tool declarations, built-in vs learned skills, progressive disclosure. Replaces flat skills.md. — decided by Kujan

**What Could Still Break:**
- Symlinks in `.ai-team/` — `copyRecursive` follows them, could infinite loop
- Windows paths with >260 chars — Node handles this but old Windows APIs don't
- UTF-8 BOM in `.squad` files — `JSON.parse` chokes on BOM prefix
- Concurrent init processes writing to same directory — no locking
- `cleanTeamMd` regex could strip too much or too little depending on markdown structure
