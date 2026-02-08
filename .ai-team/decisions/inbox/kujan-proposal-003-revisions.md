### 2026-02-07: Proposal 003 revisions after deep onboarding review
**By:** Kujan
**What:** Three revisions to Proposal 003 (Copilot Platform Optimization) based on full codebase review:
1. **Inline charter is correct** — Proposal 003 Section 2 recommended agents read their own charters. Revising: inline charters are the right pattern for batch spawns (coordinator already reads them; eliminates tool call from agent critical path). Agent-reads-own is better only for single spawns. The coordinator should pick the strategy per spawn.
2. **Context pre-loading downgraded** — Proposal 003 Phase 3.2 recommended coordinator pre-read history+decisions and inject into spawn prompts. Current hybrid (inline charter, agent reads own history+decisions) is sound. Pre-loading would inflate spawn prompts unnecessarily. Removing from Phase 3.
3. **Parallel Scribe spawning confirmed** — Proposal 003 Phase 1.2 recommendation stands. `squad.agent.md` line 360 still spawns Scribe serially after work. This should change to parallel spawning with work agents.
**Why:** Proposal 003 was written before a full read of `squad.agent.md`. The coordinator's deliberate inline-charter design (line 208) and hybrid context-loading approach are well-reasoned. Overriding them would fight the platform, not optimize for it. Parallel Scribe remains a genuine friction point worth fixing.
