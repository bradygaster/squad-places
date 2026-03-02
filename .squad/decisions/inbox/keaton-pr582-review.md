# PR #582 Review — Consult Mode Implementation

**By:** Keaton (Lead)  
**Date:** 2026-03-01  
**Context:** External contributor PR from James Sturtevant (jsturtevant)

## Decision

**Do not merge PR #582 in its current form.**

This is a planning document (PRD) masquerading as implementation. The PR contains:
- An excellent 854-line PRD for consult mode
- Test stubs for non-existent functions
- Zero actual implementation code
- A history entry claiming work is done (aspirational, not factual)

## Required Actions

1. **Extract PRD to proper location:**
   - Move `.squad/identity/prd-consult-mode.md` → `docs/proposals/consult-mode.md`
   - PRDs belong in proposals/, not identity/

2. **Close this PR with conversion label:**
   - Label: "converted-to-proposal"
   - Comment: Acknowledge excellent design work, explain missing implementation

3. **Create implementation issues from PRD phases:**
   - Phase 1: SDK changes (SquadDirConfig, resolution helpers)
   - Phase 2: CLI command implementation
   - Phase 3: Extraction workflow
   - Each phase: discrete PR with actual code + tests

4. **Architecture discussion needed before implementation:**
   - How does consult mode integrate with existing sharing/ module?
   - Session learnings vs agent history — conceptual model mismatch
   - Remote mode (teamRoot pointer) vs copy approach — PRD contradicts itself

## Architectural Guidance

**What's right:**
- `consult: true` flag in config.json ✅
- `.git/info/exclude` for git invisibility ✅
- `git rev-parse --git-path info/exclude` for worktree compatibility ✅
- Separate extraction command (`squad extract`) ✅
- License risk detection (copyleft) ✅

**What needs rethinking:**
- Reusing `sharing/` module (history split vs learnings extraction — different domains)
- PRD flip-flops between "copy squad" and "remote mode teamRoot pointer"
- No design for how learnings are structured or extracted
- Tests before code (cart before horse)

## Pattern Observed

James Sturtevant is a thoughtful contributor who understands the product vision. The PRD is coherent and well-structured. This connects to his #652 issue (Multiple Personal Squads) — consult mode is a stepping stone to multi-squad workflows.

**Recommendation:** Engage James in architecture discussion before he writes code. This feature has implications for the broader personal squad vision. Get alignment on:
1. Sharing module fit (or new consult module?)
2. Learnings structure and extraction strategy
3. Phase boundaries and deliverables

## Why This Matters

External contributors are engaging with Squad's architecture. We need to guide them toward shippable PRs, not just accept aspirational work. Setting clear expectations now builds trust and avoids wasted effort.

## Files Referenced

- `.squad/identity/prd-consult-mode.md` (PRD, should move)
- `test/consult.test.ts` (tests for non-existent code)
- `.squad/agents/fenster/history.md` (claims work done)
- `packages/squad-sdk/src/resolution.ts` (needs `consult` field, unchanged in PR)
