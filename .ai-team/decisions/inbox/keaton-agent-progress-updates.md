# Decision: Agent Progress Updates — Milestone Signals + Coordinator Polling

**Decision Date:** 2026-02-13  
**Decided by:** Keaton (Lead)  
**Affects:** Proposal 022a (Issue #22), Coordinator (squad.agent.md), All Agent Types  
**Status:** Proposed (awaiting Brady approval, likely v0.4.0)

---

## The Question

**Issue #22 (bradygaster):** Users feel uncertain during long-running background agent work. The terminal goes quiet. How do we surface periodic progress updates that:
- Signal work is still happening
- Reflect agent personality (not generic "still working...")
- Don't slow down actual work (cost-first model)
- Work across all agent types (explore, task, general-purpose, code-review)

## The Decision

**Implement Milestone Signals + Coordinator Polling mechanism.**

### Architecture

```
Agent (working):
  - Emits output as usual (via console.log, file writes, etc.)
  - At natural breakpoints, emits: ✅ [MILESTONE] Analyzed 150/400 files
  - No file I/O, no special tools, no ceremony

Coordinator (polling loop, every 30s):
  - Calls read_agent(agent_id, wait: false) — get current partial output
  - Scans output for lines matching \[MILESTONE\]
  - For each new milestone (not previously shown):
    - Extracts milestone text
    - Outputs: 📍 {AgentName} — {milestone_text}
  - Continues polling until agent completes

User (watching terminal):
  - Sees "🏗️ Keaton is analyzing... I'll check in every 30s"
  - [30s] 📍 Keaton — ✅ Parsed 150/400 files
  - [30s] 📍 Keaton — 📍 Analyzing dependencies...
  - [45s] ✅ Keaton completed. Report:
    [full output]
```

### Why This Approach

| Criterion | Score | Notes |
|-----------|-------|-------|
| **Cost** | ⭐⭐⭐⭐⭐ | Reuses read_agent polling (1 API call per 30s per agent). No new infrastructure. |
| **Voice** | ⭐⭐⭐⭐⭐ | Agents control what's highlighted. Not coordinator-paraphrased progress. |
| **Adoption** | ⭐⭐⭐⭐⭐ | Zero agent code changes. Skill-based documentation (opt-in). Backward compatible. |
| **UX** | ⭐⭐⭐⭐⭐ | 30s cadence is industry standard. Users see "work is progressing" without noise. |
| **Complexity** | ⭐⭐⭐⭐ (low) | ~30 lines in coordinator. Regex extraction. No file I/O. |

### Alternatives Rejected

- **Polling only (no milestone signal):** Coordinator tells user "still working every 30s" — generic, not agent personality
- **File-based progress:** Agents write to `.ai-team/progress/{agent}.md` — file coordination overhead, requires agent discipline, merge conflicts possible
- **Event drop-box:** Agents write JSON events to `.ai-team/progress-events/` — over-engineered, unnecessary file handles, higher latency
- **Real-time streaming:** Would require WebSocket or event subscription — incompatible with read_agent polling model, adds infrastructure

### Implementation Plan

**Coordinator changes (squad.agent.md):**
1. Add progress polling loop to spawn → completion flow
2. Call `read_agent(agent_id, wait: false)` every 30s while agent is running
3. Extract lines matching `\[MILESTONE\]` from output
4. Display new milestones with agent name and emoji

**Skill creation (.ai-team/skills/progress-signals/SKILL.md):**
1. Document when to use milestone signals (any work > 30 seconds)
2. Show pattern: `console.log("✅ [MILESTONE] {message}")`
3. Emoji conventions: ✅ completed, 📍 in-progress, 🔴 error, 🤔 thinking
4. Do's and don'ts (not every log line, keep < 80 chars, etc.)

**No agent code changes required.** Agents that don't emit milestones still get "still working..." fallback messages every 30s. Adoption is gradual.

### Compound Value

This decision unlocks downstream features:

1. **With Proposal 034 (Notifications):** Agent can emit `🔴 [MILESTONE] Blocked on decision` → Coordinator can trigger human notification
2. **With Squad DM:** Progress milestones can sync to Discord channel as reactions or embeds
3. **With Proposal 028 (GitHub-native planning):** Milestones can auto-comment on GitHub Issues in progress
4. **Future: Agent negotiation:** Agents can emit `⚠️ [MILESTONE] Conflict detected` → Coordinator initiates agent conversation

Visible progress is foundational for agent-user intimacy. It answers "Are they working for me or with me?"

---

## Success Criteria

- [ ] Coordinator extracts milestones correctly from 10+ common formats
- [ ] Agents adopt pattern within 1-2 spawns of first use
- [ ] No performance degradation: read_agent polling < 100ms overhead per call
- [ ] Works across all agent types without modification
- [ ] Users report less uncertainty during 2+ minute tasks (post-launch feedback)
- [ ] Milestone signal appears in at least 3 agent specs by v0.4.0 close

---

## Risk Mitigation

| Risk | Likelihood | Mitigation |
|------|-----------|-----------|
| Agents forget to emit milestones | Medium | Skill documentation + spawn template examples |
| Coordinator extracts false positives | Low | Strict regex: `\[MILESTONE\]` (hard to accidentally match) |
| Polling latency (30s) feels too slow | Low | Industry standard; documented as tunable per agent |
| read_agent output accumulates too large | Low | Milestones are 1-2 lines; total overhead < 10KB |
| Users get milestone fatigue | Low | Deduplication + one milestone per 30s discipline |

---

## Timeline

- **Proposal review:** 48-72 hours (standard)
- **Implementation (if approved):** 3-4 squad-hours
  - Fenster: Coordinator polling loop (1.5h)
  - Verbal: Skill design + agent examples (1.5h)
  - Testing: Validation across agent types (1h)
- **Target release:** v0.4.0 (after Project Boards)

---

## Related Proposals

- **Proposal 034:** Notification architecture (Teams, iMessage, webhook) — notifications triggered by agent state
- **Proposal 017/030/030a:** Async comms (Squad DM) — milestones can surface in Discord/Teams
- **Proposal 028:** GitHub-native planning — milestones can comment on issues
- **Issue #22:** Community request for progress visibility during long-running work

---

## Next Steps

1. **Brady's approval:** Is this the right design? Any modifications?
2. **Verbal's feedback:** How should the skill be documented? What examples?
3. **Fenster's estimate:** Any gotchas in the coordinator implementation?
4. **Implementation:** Parallel work on coordinator + skill
5. **Testing:** Validate across explore, task, general-purpose, code-review agents
6. **Release:** v0.4.0 (bundled with Project Boards feature)

