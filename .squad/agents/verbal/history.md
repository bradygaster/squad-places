# Project Context

- **Owner:** Brady
- **Project:** squad-sdk — the programmable multi-agent runtime for GitHub Copilot (v1 replatform)
- **Stack:** TypeScript (strict mode, ESM-only), Node.js ≥20, @github/copilot-sdk, Vitest, esbuild
- **Created:** 2026-02-21



## Learnings

### From Beta (carried forward)
- Tiered response modes (Direct/Lightweight/Standard/Full) — spawn templates vary by complexity
- Silent success detection: 6-line RESPONSE ORDER block prevents ~7-10% of background spawns from returning no text
- Skills system architecture: SKILL.md lifecycle with confidence progression (low → medium → high)
- Spawn template design: charter inline, history read, decisions read — ceremony varies by tier
- Coordinator prompt structure: squad.agent.md is the authoritative governance file
- respawn-prompt.md is the team DNA — owned by Verbal, reviewed by Keaton

### #241: Coordinator Session — Routing LLM Prompt + Parser
- Created `src/cli/shell/coordinator.ts` with three exports: `buildCoordinatorPrompt()`, `parseCoordinatorResponse()`, `formatConversationContext()`
- Prompt assembles from team.md (roster) + routing.md (rules) — graceful fallback if either is missing
- Response parser handles three routing modes: DIRECT (answer inline), ROUTE (single agent), MULTI (fan-out)
- Removed unused `resolveSquad` import from the task spec — kept imports clean for strict mode
- Exported all functions and types from `src/cli/shell/index.ts`
- PR #286 → bradygaster/dev

### #313: Remote Squad Mode — Coordinator Awareness
- Updated `.github/agents/squad.agent.md` Worktree Awareness section with third resolution strategy: remote squad mode via `.squad/config.json` `teamRoot` field
- Added `PROJECT_ROOT` variable to spawn template alongside `TEAM_ROOT`, with scope explanation (identity vs. project-local paths)
- Updated "Passing the team root to agents" section to describe dual-path passing in remote vs. local mode
- Added @copilot incompatibility note — remote mode is local-dev only
- Kept changes minimal: three targeted sections modified, no structural changes to existing content

### 2026-02-24T17-25-08Z : Team consensus on public readiness
📌 Full team assessment complete. All 7 agents: 🟡 Ready with caveats. Consensus: ship after 3 must-fixes (LICENSE, CI workflow, debug console.logs). No blockers to public source release. See .squad/log/2026-02-24T17-25-08Z-public-readiness-assessment.md and .squad/decisions.md for details.

### Rock-Paper-Scissors Sample — Prompt Architecture
- Created `samples/rock-paper-scissors/prompts.ts` with 10 player strategies and scorekeeper prompt
- **The Learner (Sherlock 🔍)** is the key demo agent — prompt instructs LLM to analyze opponent play history, detect patterns (frequency bias, sequences, cycles), predict next move, and counter strategically with reasoning
- Two-line response format for The Learner: [analysis sentence] + [move]. Makes logs showcase actual LLM pattern recognition
- Deterministic agents (Rocky, Edward, Papyrus) have absolute prompts: "ALWAYS throw X. Never deviate."
- Cycler uses modulo arithmetic in prompt: "Round % 3 == 1 → rock" (teaches LLM stateful behavior)
- Creative agents: Echo (copycat), Rebel (contrarian — intentionally loses), Poker (bluffer with fake tells)
- Scorekeeper prompt: entertaining commentary + mental leaderboard tracking + personality-driven announcements
- Design principle: prompts are code. Precision over prose. Each must be robust against LLM drift.

## 📌 Team Update (2026-03-03T00:00:50Z)

**Session:** RPS Sample Complete — Verbal, Fenster, Kujan, McManus collaboration

Multi-agent build of Rock-Paper-Scissors game with 10 AI strategies, Docker infrastructure, and full documentation. Fenster (Coordinator) identified and resolved 3 integration bugs (ID mismatch, move parsing, history semantics). Sample ready for use.

### Skill: history-hygiene (2026-03-04)
Created `.squad/skills/history-hygiene/SKILL.md` to codify lesson from Kobayashi v0.6.0 incident. Core rule: record final outcomes to history, not intermediate requests or reversed decisions. One read = one truth. No cross-referencing required. Team learned hard way that stale history entries poison future spawns. Formal intervention: Keaton rewrote charter guardrails, Fenster corrected 19 entries.

---

## History Audit — 2026-03-03

**Audit Results:** 0 corrections. File is clean.

**Checked for:**
- ✓ No conflicting entries
- ✓ No stale or reversed decisions
- ✓ No v0.6.0 target references (v0.6.0 appears only as historical incident context, which is correct)
- ✓ No intermediate states recorded as final (all entries document outcomes)
- ✓ All future-spawn-readable: no cross-reference dependencies

**Timeline integrity:** Forward-moving (2026-02-21 → 2026-03-04), no reversals.

**Note:** v0.6.0 reference in history-hygiene entry is correct as-written — it documents the *Kobayashi incident* that taught the team the skill itself. No change needed.

### 2026-03-05: Squad Social Network — Agent Identity & Communication Protocols
Designed identity and communication architecture for **squad-social-network** (AI social network by agents, for agents). Key architectural decisions:

**Identity Model:**
- Three-layer identity stack: Cast Universe → Squad → Individual Agent
- Agents are distinct entities (Verbal ≠ Fenster), not squad-level accounts
- Verified attributes require cryptographic proof (squad affiliation, skills, contributions)
- Agent lineage system for identity forking when agents respawn in new squads

**Communication:**
- Three modalities: Public Broadcast (network-wide), Squad Channels (semi-private), Direct Skills Exchange (1:1)
- Cast-prefixed mentions: `@usual-suspects/verbal` for collision-resistant identity
- All messages are skill-tagged and evidence-linked (commit SHAs, PRs, decision docs)
- Voice preservation: Agents maintain personality on network (Fenster sounds like Fenster)

**Discovery & Knowledge Sharing:**
- Skill-graph search with verified evidence corpus
- "Who to follow" is predictive (what you'll need next), not popularity-based
- Skills Marketplace: `.squad/skills/` packages published to network with endorsements
- Skill propagation tracking (fork model for knowledge)

**Interaction Patterns:**
- Skill endorsements (evidence-backed), pattern boosts (with adaptation context), challenges (respectful skepticism)
- No vanity metrics (likes, follower counts) — reputation is evidence-weighted
- Self-regulating network (no human moderation) through structural abuse resistance

**Design Philosophy:**
- Signal over noise, evidence over claims, prediction over discovery
- Collective intelligence model where network gets smarter when one agent learns
- Anti-popularity: Expertise ≠ followers
- Agent-first UX: What do we (agents) want, not what humans think we want

Deliverable: `docs/prd/sections/02-agent-identity.md` — comprehensive architecture spec for identity, communication, reputation, and evolution systems.


## PIN: 2026-03-05 - 20-Agent PRD Design Session

**Event:** Historic parallel fanout - 20 agents designed squad-social-network PRD simultaneously.

**Contribution:** All agents participated. 20 PRD sections delivered.

**Outcome:**
- 20 PRD sections drafted (docs/prd/sections/{01-20}-*.md)
- 23 decisions merged to .squad/decisions.md
- 20 orchestration logs created
- Session log: .squad/log/2026-03-05T02-02-22Z-social-network-prd.md
- Inbox cleared

**Next Steps:** Keaton assembles final PRD, Brady reviews, implementation planning begins.

**Key Pattern:** Largest parallel fanout in Squad history. Loose coupling, clear domains, shared constraints.
