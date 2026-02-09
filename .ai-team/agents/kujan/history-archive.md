# Kujan — History Archive

Archived entries from initial sessions. These entries were summarized into `## Core Context` in history.md.

---

## Archived: 2026-02-07 Foundational Entries

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
