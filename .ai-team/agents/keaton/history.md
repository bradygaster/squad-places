# Project Context

- **Owner:** bradygaster (bradygaster@users.noreply.github.com)
- **Project:** Squad — AI agent teams that grow with your code. Democratizing multi-agent development on GitHub Copilot. Mission: beat the industry to what customers need next.
- **Stack:** Node.js, GitHub Copilot CLI, multi-agent orchestration
- **Created:** 2026-02-07

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

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

📌 Team update (2026-02-08): Proposal-first workflow adopted — all meaningful changes require proposals before execution. Write to `docs/proposals/`, review gates apply. — decided by Keaton + Verbal
📌 Team update (2026-02-08): Stay independent, optimize around Copilot — Squad will not become a Copilot SDK product. Filesystem-backed memory preserved as killer feature. — decided by Kujan
📌 Team update (2026-02-08): Stress testing prioritized — Squad must build a real project using its own workflow to validate orchestration under real conditions. — decided by Keaton
📌 Team update (2026-02-08): Baseline testing needed — zero automated tests today; `tap` framework + integration tests required before broader adoption. — decided by Hockney
📌 Team update (2026-02-08): DevRel polish identified — six onboarding gaps to close: install output, sample-prompts linking, "Why Squad?" section, casting elevation, troubleshooting, demo video. — decided by McManus
📌 Team update (2026-02-08): Agent experience evolution proposed — adaptive spawn prompts, reviewer protocol with guidance, proactive coordinator chaining. — decided by Verbal
📌 Team update (2026-02-08): Industry trends identified — dynamic micro-specialists, agent-to-agent negotiation, speculative execution as strategic directions. — decided by Verbal
📌 Team update (2026-02-08): Proposal 003 revised — inline charter confirmed correct for batch spawns, context pre-loading removed, parallel Scribe spawning confirmed. — decided by Kujan
📌 Team update (2026-02-08): README rewrite ready for review — Proposal 006 contains complete new README implementing proposal 002. Needs Keaton + Brady sign-off. — decided by McManus
📌 Team update (2026-02-08): Video content strategy approved — 75s trailer, 6min demo, 5-video series. Weekly cadence. Needs strategy alignment review. — decided by Verbal
📌 Team update (2026-02-08): Demo script format decided — beat-based structure (ON SCREEN / VOICEOVER / WHAT TO DO). Needs feature ordering review. — decided by McManus

### 2026-02-08: Portable Squads architecture (Proposal 008)

**Core insight:** Squad conflates team identity with project context. Agent histories contain both user preferences (portable) and codebase knowledge (not portable). Casting state contains both universe metadata (portable) and project-specific timestamps (not portable). There's no seam between what's *yours* and what's *here*. Portability requires making this seam explicit.

**Architectural decisions:**
- **History split is the prerequisite.** Everything else depends on agents categorizing learnings into `## Portable Knowledge` (user preferences, style) vs `## Project Learnings` (codebase, architecture). This is the hard problem — not the CLI, not the format, not the import flow. Get the split right and everything else follows.
- **JSON manifest over tarball/npm/cloud.** Human-readable, versionable, reviewable, inspectable. Consistent with Squad's filesystem-first philosophy. Schema is versioned from day one (`squad_manifest_version: "1.0"`).
- **Export extracts, import seeds.** Export reads `.ai-team/` and produces a filtered manifest. Import reads the manifest and creates `.ai-team/` with portable data only. Agents arrive knowing the user but not the project.
- **No merge in v1.** Universe conflicts (Keaton and Neo on the same team) are unsolvable without opinionated rules. `--force` with archival is the right v1 behavior. Merge is a v2 problem with real design space.
- **Casting is the primary portable artifact.** Names are persistent identifiers. Universe is personality. These are the atoms of squad identity — they travel unconditionally.

**Coordination with other proposals:**
- Proposal 007 (progressive history summarization) and Proposal 008 both modify `history.md` structure. They're complementary — summarization applies within both Portable Knowledge and Project Learnings sections. Should be implemented together to avoid double-migration.
- Testing infrastructure (Hockney) is critical — export/import is a round-trip that needs integration tests.

**Strategic implications:**
- Portability changes Squad's positioning from "add a team to a project" to "your team, any project." This is a fundamentally stickier product.
- Opens the path to squad sharing (v2) and registries (v3). The manifest schema is the foundation — get it right now, or pay for it later.
- The `--review` flag on export is non-negotiable for v1. Users must see what portable knowledge is being captured. Trust is earned, not assumed.

📌 Team update (2026-02-08): Portable Squads proposed — export/import squad identity via JSON manifest. History split (Portable Knowledge vs Project Learnings) is the architectural prerequisite. Proposal 008 written. — decided by Keaton
📌 Team update (2026-02-08): Tiered response modes proposed — Direct/Lightweight/Standard/Full spawn tiers to reduce late-session latency. Context caching + conditional Scribe spawning as P0 fixes. — decided by Kujan + Verbal
📌 Team update (2026-02-08): Portable squads platform feasibility confirmed — pure CLI/filesystem, ~80 lines in index.js, .squad JSON format, no merge in v0.1. — decided by Kujan
📌 Team update (2026-02-08): Portable squads memory architecture — preferences.md (portable) split from history.md (project-local), squad-profile.md for team identity, import skips casting ceremony. — decided by Verbal
