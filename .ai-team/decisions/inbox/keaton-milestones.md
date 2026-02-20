# Decision: SDK Replatform Milestones & Work Items Structure

**Author:** Keaton (Lead)  
**Date:** 2026-02-21  
**Status:** PROPOSED  
**Priority:** GATE (blocks implementation)  

---

## Problem

Brady requested comprehensive milestones and work items documentation before any implementation begins. The 14 PRDs provide strategic direction but lack:
1. Concrete work breakdown (tasks, owners, effort estimates)
2. Execution sequencing (critical path, parallelization, phases)
3. Dependencies (what blocks what, where are the gates)
4. Exit criteria per milestone (how do we know it's done)
5. Risk mitigation (Brady checkpoints, real-repo testing)

Without this, the team lacks clarity on:
- How long the replatform takes
- Who does what, in what order
- What happens if M0 (SDK viability) fails
- How to coordinate parallel work in M3–M5

## Solution

**Created `.ai-team/docs/milestones.md`** — comprehensive planning document organizing the SDK replatform into 6 shippable milestones across 3 phases (~32 weeks).

### Milestone Structure

| M# | Name | Weeks | Owner | Exit Criteria |
|----|----|-------|-------|---------------|
| **M0** | Foundation | 1–4 | Fenster | SDK connection works, session pool proven, PRD 1 viability gate |
| **M1** | Core Runtime | 3–9 | Fenster, Baer, Verbal | Tools, hooks, lifecycle; ready for M2 |
| **M2** | Config & Init | 5–12 | Keaton, Fenster, Verbal | Config-driven architecture; new init flow; migration registry |
| **M3** | Feature Parity | 10–18 | Fenster, Verbal, Kujan | Coordinator runs on SDK; routing matches v0.4.1; 80%+ tests |
| **M4** | Distribution | 19–26 | Kujan | npm + GitHub distro; install/upgrade work; ~5MB bundle |
| **M5** | Agent Repository | 22–32 | Fenster, Verbal, Kujan | Export/import; remote agents; marketplace MVP |
| **M6** | Polish & Launch | 28–32 | Keaton, Verbal, Kujan | Docs, migration guide, v0.6.0-rc.1, Brady approval, launch |

### Critical Path

```
M0 (viability) → M1 (tools/hooks/lifecycle) 
              → M2 (config/init)
              → M3 (feature parity) ╮
                                    ├→ M4 (distro) ╮
              → M5 (agent repo)     ╭────────────┬→ M6 (launch)
                                    ╰────────────╯
```

**M0 is the gate.** If SDK orchestration (PRD 1) isn't viable, stop after 2 days — no sunk cost.

### Key Design Decisions Embedded

The milestones incorporate all 27 resolved decisions:
- **Q19:** SDK-free init (M2-6)
- **Q20:** Exact SDK version pinning (M0, M4-5)
- **Q23:** Config-driven architecture (M2, entire milestone)
- **Q24:** Export/import in marketplace context (M5)
- **Q25:** Kobayashi workflow migration (deferred, noted in M3-13)
- **Q27:** Planning first, then implementation (this document IS the gate)

Plus full coverage of Kujan's feature risk punch list:
- **14 GRAVE items:** All addressed in milestone work items or explicitly flagged as intentional drops
- **12 AT RISK items:** Clarification and PRD updates planned across milestones

### Work Item Granularity

Each milestone has 10–14 detailed work items:
- Specific title (not vague)
- PRD reference(s)
- Detailed description (what needs to be built, how)
- Owner assigned (single person accountable)
- Effort estimate (days)
- Exit criteria (how we know it's done)

Example (M0-3): "SquadClient wrapper" — wraps CopilotClient, manages connection lifecycle, error recovery, auto-reconnect, protocol version validation. 4d effort. Fenster owns. Exit: works with real CLI, < 2s init, tests pass.

### Risk Mitigation Built In

1. **PRD 1 viability gate** (M0 exit criteria) — If SDK doesn't work, stop. If it works, proceed to M1.
2. **Real repo testing** — Every milestone includes testing against 3–5 real user repos, not just synthetic tests.
3. **Brady checkpoints** — Brady reviews after M0 (viability), M3 (parity), M5 (marketplace) before next phase.
4. **Rollback at every phase** — Template system unchanged. Features behind flags. Migration registry supports atomic rollback.
5. **Effort estimates** — Every task estimated in days. Accumulated by owner to assess overload.

## Trade-offs

### What We Accept

- **Timeline:** ~32 weeks (7.5 months) from M0 start to v0.6.0 release (vs. shorter if cutting corners)
- **Fenster load:** Heavy during critical path (M0–M3). But evens out with Kujan taking M4–M6.
- **Brady involvement:** 3 explicit checkpoints (M0, M3, M5). Approval delays could slip dates.
- **Real repo dependency:** Testing against real user repos takes time but prevents silent regressions.

### What We Avoid

- **Vague timelines:** Instead of "several months," we have 7.5 months with phased releases.
- **Unclear ownership:** Each milestone has explicit owner(s). No ambiguity on who's accountable.
- **Hidden dependencies:** All blocking relationships documented. No surprises mid-execution.
- **Feature loss:** All 54 current features (covered + at-risk + grave) are explicitly addressed in milestones.
- **Migration pain:** M2 includes migration registry (atomic, reversible). M6 includes migration guide for existing users.

## Alternatives Considered & Rejected

### 1. "Single big-bang milestone"
**Rejected:** Too risky. Combines M0–M6 into one 6-month effort with no intermediate validation. If M0 (SDK viability) fails at month 2, sunk cost is massive.

**Why milestones are better:** M0 fails → stop in 4 weeks, ~$10K sunk cost. Fail at month 6 → ~$120K sunk cost.

### 2. "Defer testing to end"
**Rejected:** M0–M3 feature parity relies on real-repo validation. If we defer testing to M6, migration regressions won't be caught until launch (too late).

**Why embedded testing:** Every milestone tests against real user repos. Regressions surfaced immediately, not at launch.

### 3. "Faster timeline (20 weeks instead of 32)"
**Rejected:** Compressing critical path (M0–M3) risks quality. M0 needs time to prove SDK viability. M3 feature parity needs exhaustive testing.

**Why 32 weeks is right:** Allows parallel work (M3, M4, M5), proper testing, and Brady checkpoints. Faster would require cutting corners or parallelizing proven-impossible tasks.

## Acceptance Criteria

✅ **Document complete:** `.ai-team/docs/milestones.md` is readable, specific, actionable.  
✅ **All PRDs covered:** Every PRD (1–15) has explicit work items in milestones.  
✅ **All decisions embedded:** Q19–Q27 + Kujan's feature risk list addressed.  
✅ **Ownership clear:** Every milestone and every work item has explicit owner.  
✅ **Effort estimated:** Every task has effort estimate (days), accumulated per owner.  
✅ **Sequencing documented:** Critical path, parallel work, gates, and checkpoints all clear.  
✅ **Risk mitigated:** M0 viability gate, Brady checkpoints, real-repo testing, rollback plans.  

---

## Brady Review & Approval

**This document is the planning gate. Implementation does NOT begin until Brady approves.**

- [ ] Brady reviews milestones document
- [ ] Brady approves timeline (7.5 months)
- [ ] Brady confirms owner assignments
- [ ] Brady agrees to checkpoint schedule (M0, M3, M5)
- [ ] Brady signals: "Go implement M0" (or asks for changes)

---

## Next Steps (Brady Approval → Implementation)

1. **Brady reviews + approves** (1–2 days)
2. **Team sync:** Discuss milestones, answer questions (1 hour)
3. **M0 kickoff:** Fenster begins SDK integration (Week 1)
4. **Weekly syncs:** Milestone owner(s) + Keaton review progress
5. **M0 viability check** (Week 4): If SDK works, proceed. If not, pivot.

---

## References

- `.ai-team/docs/milestones.md` — Full planning document
- `.ai-team/docs/prds/00-index.md` — PRD registry
- `.ai-team/docs/feature-risk-punchlist.md` — Feature coverage analysis (Kujan)
- `.ai-team/decisions.md` — Q19–Q27 resolved decisions

---

*This decision is the planning gate for the SDK replatform. Sent to Brady for approval.*
