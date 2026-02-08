### 2026-02-09: Proposal 023 v2 — Incoming Queue architecture decisions
**By:** Verbal (Prompt Engineer)
**What:** Three architecture decisions for the incoming queue proposal, incorporating Brady's feedback and Kujan's platform assessment:

1. **SQL hot layer + filesystem durable store.** SQL `todos` table is the queryable working set within a session. `.ai-team/backlog.md` is the durable source of truth across sessions. Writes go to both. Session start rehydrates SQL from filesystem. Filesystem always wins on conflict.

2. **Team backlog as first-class feature.** Auto-populated from conversation extraction, explicit adds supported, drop-box pattern for agent writes. Third memory channel alongside decisions and history. Proactive surfacing after agent work completes.

3. **Agent cloning is architecturally ready.** Same agent identity can spawn multiple times in parallel — each clone in its own worktree, writing to separate inbox files. No infrastructure changes needed. Coordinator prompt needs spawn limit relaxation for parallelizable backlog items. Key risk: file conflicts (mitigated by coordinator-assigned non-overlapping scopes).

**Why:** Brady's explicit architecture direction (SQL as cache, filesystem as truth, team can clone). Kujan's assessment confirmed platform constraints that validate this approach. Proposal 023 updated to Revised Draft status.

**Recommendation:** Move to implementation. Phase 1 (extraction + dual-layer writes) is ~40 lines in squad.agent.md and can ship independently. Cloning (Phase 3) should be tested conservatively — start with Fenster×2 on clearly separate tasks before scaling.
