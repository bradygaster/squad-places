# Session: v1 Sprint Planning

**Date:** 2026-02-08
**Requested by:** bradygaster
**Type:** Sprint planning, feature direction, parallel proposal generation

## Summary

Brady directed the team to plan Squad v1. Three major feature directions emerged: portable squads, forwardability (upgrade path), and skills (Agent Skills standard compliance + MCP integration). Team adapted in real-time as Brady refined requirements across multiple turns.

## Work Produced (chronological)

1. **Proposal 007** — Agent persistence and latency (Kujan + Verbal). Diagnosed Brady's "agents get in the way" feedback. Tiered response modes: Direct/Lightweight/Standard/Full. P0 fixes: context caching, conditional Scribe spawning.
2. **Proposal 008** — Portable squads architecture (Keaton). Manifest format, export/import CLI, history split (Portable Knowledge vs Project Learnings).
3. **Proposal 008-experience** — Portable squads UX (Verbal). preferences.md, squad-profile.md, five magic moments, industry positioning ("nobody has portable agent teams").
4. **Proposal 008-platform** — Portable squads feasibility (Kujan). .squad JSON format, CLI implementation (~80 lines), merge policy (refuse in v0.1).
5. **Proposal 009** — v1 sprint plan (Keaton). 3 sprints, 10 days, full dependency map. Sprint 1: fast. Sprint 2: yours + smart. Sprint 3: polish. Aggressive cuts list.
6. **Proposal 010** — Skills system (Verbal). Skill acquisition, storage (skills.md per agent), portability, six skill types, confidence lifecycle, magic moments.
7. **Proposal 011** — Forwardability and upgrade path (Fenster). File ownership model (Squad-owned vs user-owned), `npx create-squad upgrade`, version-keyed migration system.
8. **Proposal 012** — Skills platform and Copilot integration (Kujan). Platform feasibility, context budget (0.4–1.6%), `store_memory` rejected, defensive forwardability via existence checks.
9. **Proposal 013** — v1 test strategy (Hockney). `node:test` + `node:assert` (zero dependencies), 9 test categories, 6 blocking quality gates, 90% line coverage target.
10. **Proposal 014** — v1 messaging and launch (McManus). "Throw MY squad at it" tagline, two-project demo arc, 7-day launch sequence, competitive positioning.
11. **Proposal 015** — P0 silent success bug (Kujan). IN PROGRESS.
12. **Proposal 016** — Squad value paper (Verbal). IN PROGRESS.

## Key Decisions

- Brady declared forwardability non-negotiable.
- Brady declared no sacred tech decisions — everything on the table.
- Brady specified skills must use Agent Skills open standard (agentskills.io/SKILL.md format).
- Brady wants MCP tool declarations in skills.
- Brady elevated trust and the value paper to P0, above all other work.
- Silent success bug (agents complete work but report "no response") identified as P0.

## Team Dynamics

- 6 agents produced proposals in parallel across multiple batches.
- Real-time requirement evolution: custom skills → Agent Skills standard → MCP integration in 3 turns.
- Fenster, Kujan, Hockney hit "silent success" bug but all completed their work.
- Brady's direction style: short, directional messages; lets agents do structured thinking.
- ~200KB of structured proposals produced in one session.

## Decisions Merged This Session

6 inbox decisions merged to decisions.md:
- Fenster: Forwardability and upgrade path (Proposal 011)
- Hockney: v1 test strategy (Proposal 013)
- Keaton: v1 sprint plan (Proposal 009)
- Kujan: Skills platform and Copilot integration (Proposal 012)
- McManus: v1 messaging and launch (Proposal 014)
- Verbal: Skills system (Proposal 010)
