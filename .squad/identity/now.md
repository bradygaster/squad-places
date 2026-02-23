---
updated_at: 2026-02-23T18:34:00Z
focus_area: Epic #323 — CLI Quality & UX (Testing Wave → Improvement → Breathtaking)
active_issues: [323, 324, 325, 326, 327, 328, 329, 330, 331, 332, 333, 334, 335, 336, 337, 338, 339, 340, 341]
wave: testing
---

# What We're Focused On

**Status:** PRD complete. 19 issues created (1 epic + 18 sub-issues). Ready to execute Phase 1.

**⚠️ Repo: bradygaster/squad-pr ONLY — not bradygaster/squad.**

## Epic #323: CLI Quality & UX

### Phase 1: Testing Wave (ready to start)
- #325 Cheritto → Fix 2-minute timeout (P0 blocker)
- #324 Keaton + Waingro → Dogfood CLI with real repos
- #326 Breedan → Expand E2E test coverage
- #327 Waingro → Hostile QA: break everything
- #328 Nate → Accessibility audit
- #329 Cheritto → P0 UX blockers from Marquez audit

### Phase 2: Improvement (blocked on Phase 1)
- #330–#334: P1 UX polish, thinking feedback, ghost response, bug fixes, error hardening

### Phase 3: Breathtaking (blocked on Phase 2)
- #335–#341: Progress indicators, terminal adaptivity, animations, copy polish, accessibility, P2 UX, wow moment

## Critical Bug
Hard-coded `120_000ms` timeout in `sendAndWait` at `packages/squad-cli/src/cli/shell/index.ts:123`. Must fix first (#325).

## New Agents
Cheritto (TUI), Breedan (E2E), Waingro (hostile QA), Nate (accessibility), Marquez (UX design) — all chartered and ready.
