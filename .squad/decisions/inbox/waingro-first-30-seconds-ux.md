# Decision: First 30 Seconds UX Standards

**Date:** 2026-02-24  
**Author:** Waingro (Hostile QA)  
**Status:** Proposed

## Context

During hostile QA testing of the first-time user journey (`squad --help` → `squad init` → `squad` → first message), I identified 3 P1 friction points that create bailout risk in the first 30 seconds:

1. **Stale root bundle** — `cli.js` is v0.6.0-alpha.0, contradicts documented behavior
2. **Help wall of text** — 44 lines, 16 commands, user must scan to find 2 essential ones
3. **Dead air on launch** — 2-4 seconds of silence before welcome banner, no loading indicator

## Decision

Establish UX standards for the critical first 30 seconds:

### Standard 1: No Stale Entry Points

**Rule:** All CLI entry points must run from a single source of truth.

**Rationale:** Having multiple entry points (`cli.js`, `packages/squad-cli/dist/cli-entry.js`) with different versions creates confusion and violates principle of least surprise.

**Action:**
- Remove root `cli.js` OR regenerate from latest source on every build
- Document canonical entry point clearly
- Add CI check to prevent version drift

### Standard 2: Help Respects Impatient Users

**Rule:** Default help shows ≤10 lines, covers 80% use case (getting started).

**Rationale:** First-time users need exactly 2 commands (`init` and shell). Advanced commands (`scrub-emails`, `aspire`, `plugin marketplace`) are noise at this stage.

**Action:**
- Split help into quick (default) and extended (`--all`)
- Quick help format:
  ```
  squad v0.8.5.1

  Get started:
    squad init     Create your agent team
    squad          Launch interactive shell

  More:
    squad help --all    All commands
    squad doctor        Check setup
  ```

**Comparison:** GitHub CLI `gh help` shows 8 core commands, hides 20+ additional ones.

### Standard 3: No Dead Air Exceeding 1 Second

**Rule:** Any operation taking >1s must show progress feedback within 500ms.

**Rationale:** Dead silence creates "is this broken?" anxiety. Every second of silence increases bailout risk.

**Action:**
- Shell launch: print `◆ Loading squad...` or spinner before Ink render
- First message: pre-warm SDK connection during shell initialization
- Init: skip animations in non-TTY, cap TTY animations to 500ms total

**Measurement:** Use speed gate tests to enforce thresholds.

## Issues Filed

- #417 (P1) — Stale root bundle
- #424 (P1) — Help wall of text
- #427 (P1) — Shell launch dead air
- #429 (P2) — Version format inconsistency
- #431 (P2) — Empty args behavior (defensible, low priority)

## Related Work

- #387, #395, #397, #399, #401 — Speed gates filed 2025-07-25
- Speed gate tests at `test/speed-gates.test.ts`

## Success Metrics

**Before:**
- Help: 44 lines, 1331ms
- Shell launch: 2-4s dead air
- Root entry point: runs wrong command

**After:**
- Help: ≤10 lines (quick), 1331ms acceptable (Node.js startup floor)
- Shell launch: <1s to first feedback
- Single entry point, always correct behavior

## Review Requested

- **Marquez** (UX design) — approve help tier structure
- **Keaton** (Architect) — approve entry point consolidation strategy
- **Cheritto** (CLI lead) — implement loading indicators
