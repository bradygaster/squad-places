# Team Decisions

Shared brain. All agents read this before working.

## Initial Setup

### 2026-02-07: Team formation
**By:** Squad (Coordinator)
**What:** Created Squad's own team using The Usual Suspects universe — Keaton (Lead), Verbal (Prompt Engineer), McManus (DevRel), Fenster (Core Dev), Hockney (Tester).
**Why:** Squad needs a dedicated team to evolve the product, amplify its message, and stay ahead of the industry. Casting chosen to represent pressure and consequence, not literal role names. Brady (the owner) requested The Usual Suspects specifically.

### 2026-02-07: Prioritize stress testing Squad on a real project

**By:** Keaton

**What:** Squad's own team should build a non-trivial feature or project using the Squad workflow to validate orchestration, parallel execution, and memory compounding under real conditions.

**Why:** Current testing is theoretical. We've defined the patterns (drop-box, parallel fan-out, casting) but haven't stressed them with genuine multi-agent work where decisions propagate, agents disagree, or orchestration fails. A real project exposes coordination bugs, reveals where the coordinator instructions are unclear, and demonstrates whether memory actually compounds. This is the only way to know if Squad works at scale.

**Next steps:** Pick a target project (non-docs, real implementation), use Squad to build it, and log what breaks.

### 2026-02-07: Proposal-first workflow adoption

**By:** Keaton + Verbal
**Date:** 2026-02-07
**Context:** bradygaster's request for "proposal first" mindset

Squad adopts a proposal-first workflow for all meaningful changes (features, architecture, major refactors, agent design, messaging, breaking changes). Proposals must be written, reviewed by domain specialists, and approved by bradygaster before execution.

**Why:** Squad's mission requires compound decisions — each feature making the next easier. This only works with visibility and alignment. Proposals are the mechanism: visibility (changes documented before execution), alignment (team reviews before merge), memory (historical record of why choices were made), filtering (bad ideas cancelled, good ideas refined).

**What Changes:** New directory `docs/proposals/` with numbered markdown files. Agents write proposals, not just code. Review gates: Keaton (architecture), Verbal (AI strategy), domain specialists, Brady (final approval). 48-hour timeline.

**What Doesn't Change:** Bug fixes, minor polish, tests, doc updates, dependency bumps — no proposal needed. Parallel execution, drop-box pattern, casting system — all stay the same.

**Implementation:** Proposal written to `docs/proposals/001-proposal-first-workflow.md`.

### 2026-02-07: DevRel priorities for Squad onboarding

**By:** McManus

**What:** Identified six critical polish areas to improve Squad's first-5-minutes developer experience: (1) Make install output visible and explanatory, (2) Link sample-prompts.md from README (16 ready-to-use demos), (3) Add "Why Squad?" value prop section, (4) Elevate casting from Easter egg to feature, (5) Add troubleshooting section, (6) Record 2-minute demo video/GIF showing parallel work.

**Why:** The product has strong bones — solid messaging, tight Quick Start, real numbers in the context budget table — but the first-time experience has gaps. Install output is too quiet (just checkmarks, no structure explanation). Sample prompts are hidden in docs/. Casting (thematic persistent names) is mentioned once but not explained. No "why should I care?" section. No troubleshooting. No visual demo. These gaps increase time-to-value and reduce conversion. Priority is making the first 5 minutes irresistible — from "what is this?" to "I need this" as fast as possible.

### 2026-02-07: Agent experience evolution — three strategic directions

**By:** Verbal

**What:** Identified three areas where Squad's agent design should evolve to stay ahead of the industry: (1) Role-specific spawn prompts with adaptive context loading, (2) Reviewer protocol with guidance and grace periods, (3) Proactive coordinator chaining and conflict resolution.

**Why:** Current spawn template is uniform across all agents. This works functionally but doesn't match how specialists actually work — Leads need trade-off context, Testers need edge case catalogs, etc. Adaptive context loading (tagging decisions by domain, injecting only relevant history) prevents agents from parsing noise. Reviewer protocol adding rejection with guidance + grace periods makes reviews collaborative handoffs. Coordinator chaining follow-up work automatically and catching decision conflicts before the user sees them makes Squad feel predictive, not reactive.

### 2026-02-07: Industry trends — agent specialization, collaboration, speculative execution

**By:** Verbal

**What:** Three trends Squad should lead: (1) Dynamic micro-specialist spawning (10+ narrow experts on the fly), (2) Agent-to-agent negotiation (multi-turn collaboration, not just fan-out-and-merge), (3) Speculative execution (anticipatory agents for work that will obviously follow).

**Why:** Specialization — current 5-role model will expand to 10+ narrow specialists, adding specialists mid-session should be effortless. Collaboration — agents currently work in parallel and coordinator synthesizes; next evolution is agent-to-agent negotiation. Speculative execution — parallel agents are the only way to stay fast at scale; spawn anticipatory agents and discard if unneeded. These trends align with where the industry is headed. Squad should ship these patterns before competitors figure out basic parallelism.

### 2026-02-07: Baseline testing infrastructure needed before broader adoption

**By:** Hockney

**What:** Squad currently has zero automated tests. Before we move beyond internal experimentation, we need at minimum: (1) a test framework, (2) an integration test for the happy path (run `index.js` in a temp directory, validate expected files are created), and (3) error handling for filesystem failures.

**Why:** The installer manipulates the filesystem with conditional logic (skip if exists, recursive copy, directory creation). Without tests, we have no way to know when we break something. Users will get raw stack traces instead of helpful error messages. This is acceptable for early prototyping but not for external use — even as "experimental."

**Priority:** Not blocking current iteration. But required before we ask anyone outside the core team to use this.

**Proposed approach:** Use `tap` as test framework. Start with one integration test. Add error handling incrementally as we find failure modes.

### 2026-02-07: Stay independent, optimize around Copilot
**By:** Kujan
**What:** Squad will NOT become a Copilot SDK product. Instead, we optimize around the platform while maintaining independence. Focus on being the best example of what you can build *on* Copilot, not *of* Copilot.
**Why:** Squad's filesystem-backed memory (git-cloneable, human-readable) is a killer feature. SDK adoption would abstract this away and reduce transparency. We can evolve faster independently. If the SDK later adds features we need (agent memory primitives, marketplace integration, spawn quota management), we reconsider. Until then: independent product, platform-optimized implementation.

### 2026-02-07: Proposal 003 revisions after deep onboarding review
**By:** Kujan
**What:** Three revisions to Proposal 003 (Copilot Platform Optimization) based on full codebase review:
1. **Inline charter is correct** — inline charters are the right pattern for batch spawns (eliminates tool call from agent critical path). Agent-reads-own is better only for single spawns. Coordinator should pick the strategy per spawn.
2. **Context pre-loading downgraded** — current hybrid (inline charter, agent reads own history+decisions) is sound. Pre-loading would inflate spawn prompts unnecessarily. Removed from Phase 3.
3. **Parallel Scribe spawning confirmed** — `squad.agent.md` line 360 still spawns Scribe serially after work. Should change to parallel spawning with work agents.
**Why:** Proposal 003 was written before a full read of `squad.agent.md`. The coordinator's deliberate inline-charter design and hybrid context-loading approach are well-reasoned. Overriding them would fight the platform. Parallel Scribe remains a genuine friction point worth fixing.

### 2026-02-07: README rewrite proposal ready for review
**By:** McManus
**What:** Proposal 006 (`docs/proposals/006-readme-rewrite.md`) contains the complete new README text implementing proposal 002. Copy-paste-ready once approved. Key decisions: "What is Squad?" merged into hero, sample prompts link at end of Quick Start, no Go references in README (Go example in sample-prompts tracked separately), no demo GIF yet (needs production setup).
**Why:** Consolidates messaging overhaul into a concrete, reviewable artifact. Needs sign-off from Keaton (messaging), Brady (owner), and Verbal (voice/tone review on "Why Squad?" section).

### 2026-02-07: Video content strategy approved
**By:** Verbal
**What:** Video content strategy for Squad: 75-second trailer, 6-minute full demo, 5-video series (7 total including supercut). Trailer ships first (cold open, no intro). Visual hook is agents coordinating through decisions.md, not code generation. "Throw a squad at it" closes every video. Weekly release cadence (~9 weeks).
**Why:** Positions Squad as the definitive multi-agent tool for Copilot through visual proof. Needs McManus (scripting/polish), Keaton (strategy alignment review), Brady (release cadence and on-camera decision). Proposal: `docs/proposals/005-video-content-strategy.md`.

### 2026-02-07: Demo script format — beat-based structure
**By:** McManus
**What:** Demo script (`docs/demo-script.md`) uses beat-based format with three sections per beat: 🎬 ON SCREEN (what viewer sees), 🎙️ VOICEOVER (exact words), 👆 WHAT TO DO (physical actions during recording). Eliminates improvisation — Brady records each beat independently.
**Why:** Brady's feedback: current script doesn't tell him what to do. Ambiguity costs takes. Beat format makes recording mechanical. Proposal: `docs/proposals/004-demo-script-overhaul.md`. Needs Keaton (feature ordering), Verbal (tone/claims), Brady (final sign-off).

### 2026-02-08: Portable Squads — architecture direction

**By:** Keaton
**Proposal:** 008-portable-squads.md

**What:** Squad will support exporting and importing team identity across projects via a JSON manifest file. Core architectural decision: separate Team Identity (portable) from Project Context (not portable) at the data model level. History split into `## Portable Knowledge` and `## Project Learnings`. Export format is a single `squad-export.json` manifest. CLI: `npx create-squad export` / `npx create-squad --from <manifest>`. Casting (universe, names, roles) travels unconditionally. No merge in v1 — `--force` replaces with archival.

**Why:** The team is more valuable than the project. Without portability, users rebuild from scratch every time. Opens path to squad sharing (v2) and registries (v3). Aligns with Proposal 007 (both modify history structure, complementary). Requires Hockney's testing infrastructure for round-trip validation. Template changes are backward-compatible.

### 2026-02-08: Agent persistence and latency reduction — tiered response modes

**By:** Kujan + Verbal

**What:** Proposed a tiered response system (Direct → Lightweight → Standard → Full) to replace the current "every interaction spawns an agent" model. Key changes: (1) Coordinator skips re-reading team.md/routing.md/registry.json after first message (context caching), (2) Scribe only spawns when inbox has files to merge, (3) Coordinator may handle trivial single-line tasks directly without spawning, (4) Lightweight spawn template for simple scoped tasks that skips history.md and decisions.md reads, (5) Progressive history summarization for mature projects.

**Why:** Brady's feedback: "later on, the agents get in the way more than they help." Root cause is that every interaction pays the same 9-10 tool call overhead (~30-35s) regardless of task complexity. By message 10, trivial tasks take 30+ seconds for what should be a 5-second change. P0 fixes (context caching + Scribe batching) are zero-risk instruction changes that save ~12-17s per message. P1 fixes (tiered modes + coordinator direct handling) transform the late-session experience from friction to flow.

**Proposal:** `docs/proposals/007-agent-persistence-and-latency.md`

### 2026-02-08: Portable squads — export/import design and platform feasibility

**By:** Kujan

**What:** Designed the portable squads feature: CLI subcommands (`create-squad export` / `create-squad import <file>`), `.squad` JSON file format, export payload definition, import flow, and coordinator integration. Key decisions: (1) Single JSON file format over tarball/directory, (2) refuse merge in v0.1 (existing squad → error), (3) manual history curation in v0.1 with LLM-assisted cleanup in v0.2, (4) `imported_from` one-time flag in registry.json for coordinator onboarding detection, (5) Scribe history excluded from export (project-specific), Scribe charter included.

**Why:** Brady wants users to move squads between projects. The feature is pure CLI/filesystem — no Copilot platform constraints. History splitting (portable user preferences vs. project-specific facts) is the only hard problem; v0.1 punts to manual curation. Merge support deferred to v0.3 due to universe conflicts and name collision complexity. Implementation is ~80 lines in `index.js`, ~10 lines in `squad.agent.md`. No new dependencies.

**Proposal:** `docs/proposals/008-portable-squads-platform.md`

### 2026-02-08: Portable Squads — memory architecture and experience design

**By:** Verbal

**What:** Portable squads require a memory split: separate `preferences.md` (user-specific, portable) from `history.md` (project-specific, stays). Add `squad-profile.md` for team-level meta-history. Export packages charters + preferences + casting + coordinator. Import reconstitutes the squad with user knowledge intact but project context empty. Import skips casting ceremony. Narrative markdown format for v1.

**Why:** Nobody in the industry has portable agent teams — category-defining feature. Stickiness through relationship capital, not lock-in. Filesystem-backed memory makes export trivially simple. Enables future phases: squad templates, team-shared squads, marketplace. Aligns with Proposal 007's progressive summarization.

**Proposal:** `docs/proposals/008-portable-squads-experience.md`

### 2026-02-08: Squad v1 Sprint Plan — architecture and prioritization

**By:** Keaton
**Proposal:** 009-v1-sprint-plan.md

**What:** Comprehensive v1 sprint plan synthesizing proposals 001-008 and Brady's directives. Three sprints over 10 days:

- **Sprint 1 (Days 1-3): "Make It Fast"** — Forwardability (`npx create-squad upgrade`), latency P0 fixes (context caching + Scribe batching), tiered response modes (Direct/Lightweight/Standard/Full), coordinator direct handling. Init always overwrites `squad.agent.md` — it's our code, not user state.

- **Sprint 2 (Days 4-7): "Make It Yours"** — History split (Portable Knowledge / Project Learnings), Skills system (`skills.md` — domain expertise that compounds across projects), export/import CLI with manifest schema v1.0, imported squad detection in coordinator.

- **Sprint 3 (Days 8-10): "Make It Shine"** — README rewrite, testing infrastructure (5 core tests with tap), progressive history summarization, lightweight spawn template.

**Key architectural decisions:**
1. **Forwardability bright line:** We own `squad.agent.md` and templates. Users own `.ai-team/`. Upgrade overwrites our code, never touches their state.
2. **Skills are a new first-class concept.** Not preferences (about the user), not project learnings (about the codebase) — domain expertise about technologies and frameworks. Stored in `.ai-team/skills.md`. Portable and shareable.
3. **`preferences.md` deferred to v1.1.** Portable Knowledge section in history.md is sufficient for v1. Separate file adds migration cost without near-term benefit.
4. **No merge in v1.** `--force` with archival only. Merge is v2.
5. **No `squad-profile.md` in v1.** Relationship tracking is v1.1.

**What's explicitly cut from v1:** Squad merge, LLM-assisted history classification, squad sharing/registry, agent-to-agent negotiation, speculative execution, Copilot SDK integration, squad diff.

**Why:** Brady said forwardability, portability, and skills are all v1 features. 9 users, division talking. The plan is aggressive because it needs to be. "Throw a squad at it" must be earned, not marketed.

**Success criteria:** Trivial task latency drops from ~30s to ~3-5s. Export/import round-trip at 100% fidelity. Upgrade preserves 100% of user state. 5 core tests passing. Brady approves.

### 2026-02-08: Skills system — agent competence as portable knowledge

**By:** Verbal
**Date:** 2026-02-08
**Proposal:** 010-skills-system.md

**What:** Squad agents will acquire, store, and apply **skills** — earned domain knowledge that changes how agents approach work. Skills are distinct from preferences (which are about the user): skills are about what the agent *knows how to do*. Storage: `skills.md` per agent (domain expertise) + squad-level `skills.md` (cross-cutting patterns). Skills travel with portable squad exports alongside preferences and charters. Skill types: patterns, domain expertise, workflows, procedural knowledge, anti-patterns, integration knowledge. Lifecycle: acquisition → reinforcement → correction → deprecation. Confidence tracked via project count (Low/Medium/High). Agents declare known gaps ("What I Don't Know Yet"). Coordinator routes work using skill awareness. Manifest version bumps to 1.1 to include skills in export.

**Why:** Portable squads (Proposal 008) solved "your team knows YOU." Skills solve "your team knows how to DO THINGS." Combined: a squad arrives at a new React project already knowing the user's preferences AND React's patterns. Day one productive. Nobody in the industry has agent skills as a portable, earned, transferable concept. This is the feature that makes squad sharing (marketplace) actually valuable — the difference between a costume and competence. Skills compound across projects: each export captures more, each import starts from a higher baseline. Implementation phased across 6 releases, starting with template + instruction changes only (zero code changes in Phase 1).

**Depends on:** Proposal 008 (Portable Squads) for export/import integration; Proposal 007 (Persistence) for progressive summarization of skills.

### 2026-02-08: Forwardability and upgrade path

**By:** Fenster
**Date:** 2026-02-08

**What:** Squad adopts a forwardability model based on three principles:

1. **File ownership model.** Every file Squad touches is classified as Squad-owned (overwrite on upgrade), user-owned (never touch), or additive-only (create if missing). `squad.agent.md` and `.ai-team-templates/` are Squad-owned. Everything in `.ai-team/agents/`, `decisions.md`, and `casting/` is user-owned.

2. **`npx create-squad upgrade` subcommand.** Backs up `squad.agent.md`, overwrites it with the latest version, refreshes templates, runs version-specific migrations, creates new directories. Never touches user state. Always backs up before overwriting.

3. **Version-keyed migration system.** Migrations are idempotent functions keyed by version range. Each migration moves `.ai-team/` state forward (e.g., adding `## Portable Knowledge` sections to histories). Migrations are non-destructive, fault-tolerant, and ordered. A failed migration logs an error but doesn't abort the upgrade.

**Why:** The current installer skips files that already exist. This was correct for v0.1 (don't clobber) but blocks upgrades entirely. When we ship a better `squad.agent.md`, existing users never get it. Brady requires forwardability — users should be able to update their squads with new features.

The file ownership model ensures upgrades are safe. The migration system ensures state evolves correctly. The backup strategy ensures recoverability.

**Implementation:**
- Phase 1 (v0.1.1): Version stamping — add `squad_version` to frontmatter, write `.squad-version` metadata, show version in output
- Phase 2 (v0.2.0): Upgrade command — argument routing, `upgradeSquad()`, migration framework, backup + overwrite
- Phase 3 (v0.2.1): Dry-run, backup, and force flags
- Phase 4 (v0.3.0): Customization detection via content hashing

No new dependencies. `index.js` stays under 150 lines. Aligns with Proposal 008's export/import subcommands — all share the same argument routing pattern.

**What Doesn't Change:**
- Default `npx create-squad` behavior (init flow) is unchanged
- User-owned files are never modified by the upgrade system
- No new dependencies in `package.json`
- The coordinator reads `squad.agent.md` fresh every session — overwriting it IS the upgrade

**Proposal:** `docs/proposals/011-forwardability-and-upgrade-path.md`

### 2026-02-08: Skills platform architecture and v1 Copilot integration

**By:** Kujan

**What:** Skills are transferable domain expertise stored in per-agent `skills.md` files, separate from project-specific `history.md`. The coordinator inlines skills into spawn prompts alongside charters. Skills enable lighter spawns for skilled-domain tasks (extending Proposal 007's tiered modes). `store_memory` is NOT useful for Squad — wrong persistence model, no agent identity, too small. Forwardability handled via defensive file reads (existence checks), not version fields. File paths in charters are a de facto API contract — treat as frozen.

**Why:** Brady wants agents that learn across projects. Skills are the mechanism. Keeping skills separate from history makes the Proposal 008 export clean (skills travel unconditionally, history needs filtering). The `store_memory` analysis prevents the team from investing in a dead-end platform integration. The forwardability analysis prevents breaking changes when `squad.agent.md` evolves. The v1 synthesis (007 + 008 + 012) tells a coherent product story: Squad gets better the more you use it.

**Proposal:** `docs/proposals/012-skills-platform-and-copilot-integration.md`

**v1 scope:** Agent self-writing to skills.md, user-teaches-skills via coordinator, skills in spawn prompts, skills in `.squad` export, defensive forwardability. NOT in v1: `store_memory` integration, automatic skill detection, cross-agent skill sharing, selective skill loading.

### 2026-02-08: V1 test strategy

**By:** Hockney
**Date:** 2026-02-08
**Proposal:** 013-v1-test-strategy.md

**What:** Squad adopts a comprehensive test strategy for v1 using `node:test` + `node:assert` (zero dependencies). Nine test categories covering init, idempotency, export, import, round-trip, upgrade, schema validation, edge cases, and platform-specific behavior. Six blocking quality gates must pass before v1 ships. CI via GitHub Actions matrix (ubuntu, macos, windows).

**Key Decisions:**
1. **Framework: `node:test`** — zero dependencies, built into Node 22. Previous recommendation of `tap` is withdrawn in favor of Brady's thin-runtime philosophy.
2. **Coverage target: 90% line, 85% branch** — `index.js` is small enough that this means "you tested almost everything."
3. **Quality gates (all blocking):** All tests pass; init happy path works; export/import round-trip produces identical portable state; no raw stack traces on any error path; idempotent re-runs don't corrupt state; schema validation catches all malformed input.
4. **Test architecture: 80% integration, 20% unit** — run the CLI in temp dirs, check file output.
5. **No pre-commit hook** — CI is the gate.
6. **`index.js` refactoring recommended** — wrap in functions, export for testing, use `require.main === module` guard.

**Why:** Nine users. Whole division talking. Zero tests. This is a product now — if a user can break it, we should have broken it first.

### 2026-02-08: V1 messaging, README, and launch strategy

**By:** McManus
**Date:** 2026-02-08
**Proposal:** `docs/proposals/014-v1-messaging-and-launch.md`

**What:** Complete v1 public-facing launch plan covering: positioning statement, README rewrite, demo script, launch strategy, community engagement, and competitive positioning.

**Key decisions:**
1. **V1 tagline:** "Throw MY squad at it" — the possessive pronoun is the entire v1 story.
2. **One-liner:** "Your AI squad remembers you. Across every project. Forever."
3. **README restructured for v1** with three new sections: "Your Squad Learns" (skills + persistence merged), "Take Your Squad Anywhere" (portability), and "Staying Current" (forwardability).
4. **Demo script is a two-project arc** — the "holy cow" moment: squad remembers preferences in a brand new project without being told.
5. **Launch sequence:** 7-day pre-launch teasers → D-Day → D+7 follow-up → D+14 community showcase.
6. **Community strategy:** GitHub Discussions first, not Discord. `#throwasquadatit` hashtag. No squad sharing in v1.
7. **Competitive positioning:** "Other tools have memory. Squad has a relationship." Never trash Copilot Chat.

**Why:** Brady's goals: "9 users, whole division talking," "throw a squad at it should be EARNED." Three v1 features (portability, skills, forwardability) are category-defining. The launch must match the ambition.
