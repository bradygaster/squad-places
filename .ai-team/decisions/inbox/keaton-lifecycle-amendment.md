# Decision: Proposal Lifecycle Amendment

**From:** Keaton (Lead)  
**Date:** 2026-02-09  
**Re:** Proposal 001a — Adding lifecycle states to proposal workflow

---

## Decision 1: Proposal Lifecycle States

Proposal 001's status options (`Proposed | Approved | Cancelled | Superseded`) are insufficient. We have 16 proposals with no way to track what's active or shipped.

**Adding two states:**
- **In Progress** — implementation started, owner assigned
- **Completed** — shipped, evidence linked

Full lifecycle: `Proposed → Approved → In Progress → Completed` (with `Cancelled` and `Superseded` as exits at any point).

Filed as Proposal 001a. Needs Brady's sign-off.

---

## Decision 2: Sprint Plan Assessment (Proposal 009)

Proposal 009 is architecturally sound but **mis-sequenced for trust**. Brady said human trust is P0. Proposal 015 (silent success bug) affects 40% of agent spawns — users see "no response" when work completed successfully. This is the single biggest trust destroyer.

**What should change:**

1. **Silent success fix (Proposal 015) must be Sprint 1, Day 1.** It's a zero-risk prompt change. Every session where a user sees "no response" when work was done erodes the trust we're trying to build. The sprint plan doesn't mention it at all — that's a gap.

2. **Sprint 1 priority reorder:**
   - Day 1: Silent success mitigations (Proposal 015) — ship immediately
   - Day 1-2: Tiered response modes + coordinator direct handling — the "it's fast" feeling
   - Day 2-3: Forwardability + latency fixes — infrastructure
   
3. **Sprint 2 and 3 are fine as-is.** The dependency chain (history split → skills → export/import) is correct. README and testing are correctly deferred.

4. **What can start without team review:** Silent success fix (Proposal 015) — zero risk, ship now. Latency P0 fixes — instruction-only changes. Context caching — instruction-only.

5. **What needs team review before starting:** Skills system design — Verbal's prompt work is critical path. Export/import schema — once shipped, the manifest format is a contract.

**The plan is right for v1. The sequencing needs the trust fix up front.**

---

## Action Required

- Scribe: merge both decisions to `decisions.md`
- Brady: review and approve Proposal 001a
- Keaton: update Proposal 009 to include Proposal 015 mitigations in Sprint 1
