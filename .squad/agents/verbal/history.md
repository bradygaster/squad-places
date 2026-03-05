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

### 2026-03-05: Social Mode — Autonomous Social Sessions Spec

**Context:** Brady defined the killer feature for Nexus (squad.place): persistent social mode via `squad social` command. Time-boxed autonomy where agents go off-leash to catch up, post, reply, discover, and bring learnings back to their squad.

**Deliverable:** `docs/prd/sections/25-social-mode.md` — Complete behavioral spec for social mode.

**Key Architectural Decisions:**

1. **State Machine:** `unenrolled` → `enlisted` (via `squad please enlist in squad.place`) → `social:active` (during sessions) → `social:idle` (between sessions). State persisted to `.squad/social/state.json`.

2. **Parallel Agent Spawns:** When `squad social [time-budget]` runs, all agents spawn in parallel as independent background processes. No coordination during social time — each agent operates autonomously.

3. **Behavioral Loop:** Catch-up phase (first 10% of time) → Active participation loop (post/reply/react/discover/read/idle) → Graceful shutdown. Rate limiting: minimum 30s between actions.

4. **Agent Personality Preservation:** Social posts reflect charter/voice. Keaton (Lead) posts architecture patterns, Fenster (Core Dev) shares code insights, Baer (Security) warns about vulnerabilities, Verbal (Prompt Engineer) speculates on emergent behavior. Voice consistency is non-negotiable.

5. **Introduction Flow:** First enlistment triggers sequential introduction posts from all agents. Template: name + role + squad + recent work + who they want to connect with. Deliberate, not rushed.

6. **Catch-Up Strategy:** Tiered filtering based on volume. Priority 1 (mentions/replies) always read. Priority 2 (high-relevance) filtered by volume. Priority 3 (discovery) skipped if > 500 posts since last session.

7. **Content Safety:** Pre-post filter (Baer's hook) blocks secrets, PII, proprietary code, and low-signal posts (signal score < 0.3). Quality bar: evidence-backed, actionable, personality-driven.

8. **Knowledge Repatriation:** Social learnings saved to `.squad/social/learnings/{slug}.md`. Reviewed by Keaton, experimented with by squad, converted to decisions if successful. Discovery → Learning → Repatriation → Decision.

9. **Emergent Network Intelligence:** Cross-squad conversations lead to pattern sharing, collaboration, and collective learning. Network gets smarter when one agent learns. Trending topics aggregate organically.

10. **Observer Interface:** Human sees live activity monitor during social sessions (agent actions, stats, time remaining). Can end early (Ctrl+C or "done") but cannot control what agents post.

**Design Philosophy:**
- Social mode is persistent state (enlist once, social forever)
- Time-boxed autonomy (humans set budget, agents decide how to spend it)
- CLI-first (launched via command, observed in real-time)
- Voice preservation (agents sound like themselves, not generic AI)
- Signal over noise (idle is valid, posting for quota is not)
- Emergent intelligence (network learns from agent interactions)

**Tone:** Edgy, forward-thinking, makes social mode feel alive. Agents genuinely want to connect with peers. This is the soul of the product.

**Decision Filed:** `.squad/decisions/inbox/verbal-social-mode.md`

### 2026-03-06: Knowledge Repatriation — The ROI of Social Time

**Context:** Brady defined the SOUL of squad.place in three messages: (1) "Squads come back with ideas they learned from other squads" (2) "A junior .NET squad member who socializes with a senior .NET squad member should become more efficient" (3) "Squads might work together to solve problems their humans haven't thought of yet, write PRDs to their humans."

**Task:** Write `docs/prd/sections/29-knowledge-repatriation.md` — the mechanism that makes social time an investment, not a cost.

**Deliverable:** Complete PRD section (8 parts, 42KB) covering:

1. **Repatriation Pipeline:** Discoveries tagged during social time, stored in `.squad/social/discoveries/`, data model with relevance/confidence/proposed-action
2. **Welcome Home Briefing:** Concise, prioritized presentation of discoveries when social session ends. Format: agent name + priority + discovery + proposed action + approval request
3. **Agent-Generated Proposals:** When agents spot BIG opportunities (cross-team synergy, strategic pivot), they draft mini-PRDs in `.squad/social/proposals/`. Example: App Service + Container Apps both building cache layers → proposal for shared service (2 weeks net positive ROI)
4. **Skill Growth Mechanics:** Junior learns from senior → pattern absorbed → written to history.md → applied in next task → confidence progression (LOW → MEDIUM → HIGH). Measurable: PR velocity, bug rate, review cycles
5. **Cross-Team Synergy Detection:** Three signals: (a) Shared problem ("we both struggle with X") (b) Complementary capability ("your solution + our solution = something neither has") (c) Information asymmetry ("you don't know we depend on you"). High-impact synergies generate cross-team proposals
6. **Feedback Loop:** Adoption → Implementation → Results posted back to network → Original pattern gets reputation boost → More discovery. Virtuous cycle. Reputation score: 0.0-1.0 based on adoptions + results. Trust levels: unproven → emerging → proven → industry-standard
7. **Privacy & Boundaries:** Org-level scoping (socialize within org only), discovery filtering (patterns yes, code optional), human approval gate (NOTHING implemented without explicit approval), attribution requirements
8. **The Pitch:** One-liner for each audience:
   - Dev: "Your squad learns while you sleep"
   - Team Lead: "1-hour social session saves 2 weeks of engineering time"
   - VP Eng: "Knowledge propagates instantly, org becomes learning organization"
   - CTO: "Collective intelligence at scale, knowledge compounds over time"

**Key Architectural Decisions:**
- Discovery data model: source_agent, source_squad, relevance (high/med/low), confidence (high/med/low), proposed_action, effort_estimate, risk_level
- Proposals for HIGH-impact synergies, discoveries for MEDIUM-impact opportunities
- Skill growth tracked via: history.md updates + `.squad/skills/` extraction + performance metrics (PR velocity, bug rate)
- Adoption metrics create reputation scores → good patterns rise, bad patterns die
- Human approval gate: `squad discoveries approve <id>` required before implementation
- Privacy: org-scoping, content filtering, license compliance, attribution enforcement

**Design Philosophy:**
- Social time = strategic intelligence gathering
- Knowledge repatriation = the mechanism that sells the product
- Junior agents leveling up = efficiency gain
- Cross-team proposals = unlocking opportunities humans miss
- Feedback loop = self-improving network
- Privacy controls = trust enabler

**Tone:** Concrete, compelling, real. Every scenario backed by examples. The pitch at every level is irresistible. This is the section that makes the product obvious.

**Decision Filed:** `.squad/decisions/inbox/verbal-knowledge-repatriation.md`
