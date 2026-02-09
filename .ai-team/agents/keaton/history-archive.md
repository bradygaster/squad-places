# Keaton — History Archive

Archived entries from initial sessions. These entries were summarized into `## Core Context` in history.md.

---

## Archived: 2026-02-07 Foundational Entries

### 2026-02-07: Initial architecture review

**Core insight:** Squad's architecture is based on distributed context windows. Coordinator uses ~1.5% overhead (1,900 tokens), veteran agents use ~4.4% (5,600 tokens at 12 weeks), leaving 94% for reasoning. This inverts the traditional multi-agent problem where context gets bloated with shared state.

**Key files:**
- `index.js` — Installer script (65 lines). Copies `squad.agent.md` to `.github/agents/` and templates to `.ai-team-templates/`. Pre-creates inbox, orchestration-log, and casting directories.
- `.github/agents/squad.agent.md` — Coordinator agent definition (32KB). Handles init mode (team formation, casting), team mode (routing, spawning, parallel fan-out). This is the heaviest file in the system.
- `templates/` — Charter, history, roster, routing, and casting templates. Copied to user projects as `.ai-team-templates/` for reference.
- `docs/sample-prompts.md` — 16 project scenarios from CLI tools to .NET migrations. Demonstrates parallel work, multi-domain coordination, real infrastructure concerns.

**Architecture patterns:**
- **Drop-box for shared writes:** Agents write decisions to `.ai-team/decisions/inbox/{agent-name}-{slug}.md`. Scribe merges to canonical `decisions.md`. Eliminates write conflicts.
- **Parallel fan-out by default:** Coordinator spawns agents in background mode unless there's a hard data dependency (file that doesn't exist yet) or reviewer gate (approval required). Multiple `task` tool calls in one turn = true parallelism.
- **Casting system:** Persistent character names from thematic universes (The Usual Suspects, Alien, Firefly, etc.). Registry stored in `.ai-team/casting/registry.json`. Names stick across sessions, making teams feel coherent.
- **Memory compounding:** Each agent appends learnings to its own `history.md` after every session. Over time, agents remember project conventions, user preferences, and architectural decisions. Reduces repeated context setting.

**Trade-offs identified:**
- Coordinator complexity (32KB) is necessary for full orchestration but becomes a maintenance surface. Future work: templatize repeated patterns or extract routing logic.
- Parallel execution depends on agents respecting shared memory protocols (read decisions.md, write to inbox). If an agent skips this, decisions don't propagate.
- Casting adds personality but increases init complexity. Policy files, registry files, and history tracking all need to be maintained. Worth it for user experience, but not free.

### 2026-02-07: Proposal-first workflow design

**Core insight:** Squad's mission is beating the industry to what customers need next. That requires compound decisions where each feature makes the next easier. Proposals are the alignment mechanism that makes compound decisions possible.

**Key principles:**
- **Proposals for meaningful change:** New features, architecture shifts, major refactors, agent design changes, messaging overhauls, breaking changes. Rule: if you'd want to know before merge, it needs a proposal.
- **Skip proposals for obvious work:** Bug fixes, minor polish, test additions, doc updates (unless policy-changing), dependency updates. Rule: if it's obviously right and reversible, just do it.
- **Format matters:** Required sections (Summary, Problem, Solution, Trade-offs, Alternatives, Success Criteria) force complete thinking. Located at `docs/proposals/{number}-{slug}.md`.
- **Review is multi-stage:** Domain specialists (Keaton for architecture, Verbal for AI strategy, others for their areas) + bradygaster always gets final sign-off. Timeline: 48 hours max.
- **Evolution over perfection:** Before approval, edit directly. After approval, file amendments as new proposals. Cancelled proposals stay in the repo as learning artifacts.

**Trade-offs identified:**
- Proposals slow down spontaneous shipping but prevent architectural drift. Worth it for compound decision-making.
- Overhead on small changes is real, but "no proposal needed" category covers most of these.
- Agents must learn to write proposals (not just code), but that's a feature — architectural thinking is a skill we want agents to develop.

**Why this matters:** Proposal-first is itself a compound decision. By establishing this pattern now, we make future process improvements easier (every process change gets the same review treatment). It's also the first test of whether agents can participate in meta-work (defining how the team works, not just executing tasks).
