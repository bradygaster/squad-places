# Project Context

- **Owner:** bradygaster (bradygaster@users.noreply.github.com)
- **Project:** Squad — AI agent teams that grow with your code. Democratizing multi-agent development on GitHub Copilot. Mission: beat the industry to what customers need next.
- **Stack:** Node.js, GitHub Copilot CLI, multi-agent orchestration
- **Created:** 2026-02-07

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### 2026-02-07: Initial Platform Assessment

**Context:** First review of Squad's Copilot integration. Analyzed `squad.agent.md`, `index.js`, package structure, and README.

**Key findings:**
- Squad's core architecture (task tool spawning, filesystem memory, background mode) is already Copilot-native — no fundamental rewrites needed
- Three optimization categories identified: (1) things working well, (2) friction points where we fight the platform, (3) missed opportunities
- Main friction: inline charter pattern (coordinator pastes charter into spawn prompts), serial Scribe spawning, no speculative execution
- Main opportunities: predictive agent spawning, agent-to-agent handoffs, context pre-loading for batch spawns
- **Recommendation:** Stay independent (not a Copilot SDK product) but become best-in-class example of building on Copilot

**Architectural patterns observed:**
- Drop-box pattern for concurrent writes (`.ai-team/decisions/inbox/`) eliminates file conflicts — this is elegant and should be preserved
- Agent spawn via task tool with `mode: "background"` as default — correct pattern for Copilot async execution
- Filesystem-backed memory (charter.md, history.md, decisions.md) makes everything git-cloneable and human-readable — killer feature, don't abandon this for SDK abstractions

**Platform knowledge:**
- Copilot's task tool supports background mode for true async parallelism
- Agents have full filesystem access — leverage this, don't invent memory APIs
- Context window: 128K tokens, Squad uses ~1.5% for coordinator, ~4.4% for mature agents, leaving 94% for actual work
- `explore` sub-agent exists for codebase search — agents should use this instead of grep/glob when doing semantic search

**Next work:**
- Monitor Phase 1 implementation (remove friction: agents read own charters, parallel Scribe spawning)
- If Phase 1 succeeds, assess Phase 2 (predictive execution) and Phase 3 (agent autonomy)
- Track spawn latency, parallel utilization, and context usage as optimization metrics

📌 Team update (2026-02-08): Proposal-first workflow adopted — all meaningful changes require proposals before execution. Write to `docs/proposals/`, review gates apply. — decided by Keaton + Verbal
📌 Team update (2026-02-08): Stress testing prioritized — Squad must build a real project using its own workflow to validate orchestration under real conditions. — decided by Keaton
📌 Team update (2026-02-08): Baseline testing needed — zero automated tests today; `tap` framework + integration tests required before broader adoption. — decided by Hockney
📌 Team update (2026-02-08): DevRel polish identified — six onboarding gaps to close: install output, sample-prompts linking, "Why Squad?" section, casting elevation, troubleshooting, demo video. — decided by McManus
📌 Team update (2026-02-08): Agent experience evolution proposed — adaptive spawn prompts, reviewer protocol with guidance, proactive coordinator chaining. — decided by Verbal
📌 Team update (2026-02-08): Industry trends identified — dynamic micro-specialists, agent-to-agent negotiation, speculative execution as strategic directions. — decided by Verbal
📌 Team update (2026-02-08): Proposal 003 revised — inline charter confirmed correct for batch spawns, context pre-loading removed, parallel Scribe spawning confirmed. — decided by Kujan

### 2026-02-07: Deep Onboarding — Full Codebase Review

**Context:** First comprehensive review of all Squad files, all agent histories, all proposals, all inbox decisions, coordinator spec, templates, and ceremonies.

**Revised platform assessment:**

1. **Inline charter is correct (revising Proposal 003).** `squad.agent.md` line 208 deliberately inlines charters into spawn prompts to eliminate a tool call from the agent's critical path. My proposal recommended agents read their own charters — wrong tradeoff for batch spawns where coordinator already reads charters. Revised recommendation: inline for batch spawns (3+ agents), agent-reads-own for single spawns.

2. **Context pre-loading (Proposal 003 Phase 3.2) downgraded.** Current hybrid is sound: coordinator inlines charter, agent reads its own `history.md` + `decisions.md`. Pre-loading history/decisions into spawn prompts would inflate them unnecessarily. Keep current hybrid.

3. **Scribe serial spawning confirmed as friction.** `squad.agent.md` line 360 spawns Scribe as step 4 after results are collected. Proposal 003 recommendation to spawn Scribe in parallel with work agents is still valid and should be prioritized.

4. **Ceremonies system is orphaned.** `.ai-team/ceremonies.md` and `.ai-team-templates/ceremonies.md` define Design Review and Retrospective triggers, but `squad.agent.md` has zero references to ceremonies. Either the coordinator needs ceremony-triggering logic or the files should be removed.

5. **Decision inbox has 7 unmerged entries.** Scribe has never run. Team's shared brain (`decisions.md`) is stale — only contains initial team formation. This is the most urgent operational issue.

6. **Coordinator size (32KB) approaching platform limits.** Every new feature (ceremonies, speculative execution, agent-to-agent handoffs) increases `squad.agent.md`. LLM instruction-following degrades with prompt length. Need a strategy: either extract subsystems (casting spec, ceremony triggers) to reference docs, or accept the size and optimize for information density.

**Key file paths confirmed:**
- `squad.agent.md` line 84-101: Team Mode entry, routing, session catch-up
- `squad.agent.md` line 113-121: Eager execution philosophy
- `squad.agent.md` line 122-145: Mode selection (background default)
- `squad.agent.md` line 147-171: Parallel fan-out pattern
- `squad.agent.md` line 199-333: How to spawn an agent (inline charter pattern)
- `squad.agent.md` line 345-385: After agent work (Scribe spawning, serial)
- `squad.agent.md` line 433-563: Casting & Persistent Naming (full algorithm)
- `squad.agent.md` line 565-599: Constraints + Reviewer Rejection Protocol
- `.ai-team/ceremonies.md`: Design Review + Retrospective (orphaned)
- `.ai-team/decisions/inbox/`: 7 unmerged decisions from all agents

**Platform patterns validated:**
- Drop-box pattern (inbox → Scribe merge) is the best lock-free concurrent write pattern available on the Copilot platform. Preserve this.
- Filesystem-as-memory is Squad's killer differentiator vs. SDK-managed state. Never abandon for abstractions.
- `task` tool with `mode: "background"` as default spawn mode is the correct Copilot pattern. No changes needed.
- `explore` sub-agent should be recommended for semantic codebase search in agent charters (currently not mentioned in any charter).

### 2026-02-08: Agent Persistence & Latency Analysis (Proposal 007)

**Context:** Brady reported "agents get in the way more than they help" later in sessions. Collaborated with Verbal on a latency reduction proposal.

**Key platform findings:**

1. **Copilot has no agent persistence.** Every `task` spawn is stateless. There's no warm cache, no persistent agent process, no session state for sub-agents. The coordinator's conversation history is the ONLY persistent state within a session. This is a hard platform constraint — not something we can work around with clever engineering.

2. **Tool calls are the dominant latency source.** Each `view` call costs ~1-2s of wall clock (LLM decide + execute + LLM process). The coordinator's 4 mandatory reads (team.md, routing.md, registry.json, charter.md) cost 4-8s before any spawn. This is the single biggest optimization target.

3. **Coordinator context caching is the cheapest win.** The coordinator already has team.md/routing.md/registry.json in its conversation context after the first message. Telling it to skip re-reading saves ~4.5s per message with zero risk. This is a 1-line instruction change.

4. **Scribe spawns are wasteful 50%+ of the time.** When no decisions were made, Scribe does nothing but still costs a full spawn cycle (~8-12s). Conditional spawning (only when inbox has files) is a strict improvement.

5. **The coordinator CAN do trivial domain work.** `squad.agent.md` line 569 prohibits it, but this is a policy choice not a platform constraint. The coordinator has full tool access. For single-line, unambiguous changes, spawning an agent is pure overhead.

6. **History growth is real but secondary.** Week 12 history loads add ~3-4K extra tokens per spawn. This matters, but less than the 9-10 tool calls of overhead. Progressive summarization is a P3 optimization — useful but not urgent.

**Architectural insight — tiered response modes:**
- The "every interaction goes through an agent spawn" assumption is the core problem.
- Solution: Direct (coordinator handles) → Lightweight (minimal spawn) → Standard (normal spawn) → Full (multi-agent fan-out).
- The coordinator's routing judgment becomes the critical path, not spawn mechanics.
- This extends the existing "quick factual question → answer directly" pattern that already exists in the routing table.

**What I got wrong in Proposal 003:**
- Proposal 003 focused on making spawns faster (inline vs. agent-reads-own charter, parallel Scribe). That's still valid, but the bigger win is *avoiding spawns entirely* for trivial work. I was optimizing the ceremony instead of questioning whether the ceremony was needed.

**File paths:**
- Proposal: `docs/proposals/007-agent-persistence-and-latency.md`
- Key coordinator sections for modification: `squad.agent.md` lines 84-101 (Team Mode entry), 104-111 (routing table), 345-385 (after agent work / Scribe spawning), 565-574 (constraints / "don't do domain work")

### 2026-02-08: Portable Squads — Platform Feasibility Analysis (Proposal 008)

**Context:** Brady wants users to export squads from one project and import into another, keeping names, personalities, and user meta-knowledge while shedding project-specific context.

**Key findings:**

1. **Export/import is pure CLI/filesystem — no Copilot platform constraints apply.** The entire feature runs before any agent session starts. `index.js` gets two new code paths (~80 lines total). No dependencies needed — `fs` and `path` handle everything. This is the simplest kind of feature to build on our stack.

2. **The `.squad` format should be a single JSON file.** Not a tarball, not a directory. JSON is human-readable (users can inspect/edit before sharing), git-diffable, self-describing, and requires no compression library. A mature 6-agent squad exports to ~15-25KB. The format includes a `squad_format_version` field for future migration.

3. **The export payload has a clean cut line.** Portable: casting state, charters, routing, ceremonies, filtered histories. Not portable: decisions.md, inbox, orchestration-log, session logs. `team.md` is portable but needs the Project Context section stripped — it gets rebuilt on import.

4. **History splitting is the only genuinely hard problem.** Agent histories mix portable knowledge (user preferences, coding conventions) with project-specific facts (file paths, architecture). Four approaches analyzed: manual curation, LLM classification, structural separation, tag-based. v0.1 answer: manual curation with clear warnings. v0.2: coordinator-assisted import-time cleanup. v0.3: structural separation in history.md format.

5. **Merge support should be refused in v0.1.** Universe conflicts (Alien vs. Usual Suspects), name collisions, and ambiguous merge semantics make this genuinely complex. "Refuse and explain" is the right v0.1 policy. Users can manually remove `.ai-team/` before importing. Interactive merge is v0.3.

6. **Coordinator changes are minimal (~10 lines in `squad.agent.md`).** Import adds an `imported_from` field to `registry.json`. Coordinator detects this on first session, runs lightweight onboarding (ask about new project, fill in Project Context, update agent histories). One-time flag gets cleared after onboarding. No ongoing behavioral changes.

7. **Copilot SDK would only help with cross-project memory.** If the platform had per-user persistent memory, portable squads would be trivial — squad identity would live in the user's profile. But that doesn't exist, and filesystem-backed memory is our differentiator. We're not waiting for the SDK.

**Architecture decisions made:**
- Subcommands (`export`/`import`) over flags (`--export`/`--from`) — reads more naturally, clearer separation of operations
- Single `.squad` file over directory/archive — easier to share, no dependency needed
- `imported_from` as one-time flag in registry.json — minimal coordinator impact
- Scribe history excluded from export (entirely project-specific), Scribe charter included

**Versioning plan:**
- v0.1: Export + import + manual history curation + refuse merge (~4 hours)
- v0.2: LLM-assisted history classification at import time (~3 hours)
- v0.3: Interactive merge + universe reconciliation (~6 hours)
- v1.0: GitHub Gist integration, squad gallery (depends on platform)

**File paths:**
- Proposal: `docs/proposals/008-portable-squads-platform.md`
- Implementation target: `index.js` (add `exportSquad()` and `importSquad()` functions)
- Coordinator modification: `squad.agent.md` Team Mode entry (add imported squad detection)

📌 Team update (2026-02-08): Portable Squads architecture decided — history split (Portable Knowledge vs Project Learnings), JSON manifest export, no merge in v1. — decided by Keaton
📌 Team update (2026-02-08): Portable squads memory architecture — preferences.md (portable) split from history.md (project-local), squad-profile.md for team identity, import skips casting ceremony. — decided by Verbal
