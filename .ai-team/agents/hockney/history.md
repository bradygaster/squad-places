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
