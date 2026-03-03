# Migration Readiness Review — v0.8.18 UX Assessment

**Conducted by:** Keaton (Lead)  
**Date:** 2026-02-21  
**Scope:** User-facing documentation for v0.5.4 → v0.8.18 migration  

---

## 🔴 Blockers (Must Fix Before Migration)

### 1. **Stale version reference in migration-guide-private-to-public.md**
- **File:** `docs/migration-guide-private-to-public.md`
- **Line:** 43
- **Issue:** Table row states `v0.5.4 → v0.8.17 (after migration)` but team decision is v0.8.18. This directly conflicts with the official migration checklist and will confuse users during upgrade.
- **Impact:** Users upgrading will see v0.8.17 in the guide but v0.8.18 in the actual release notes and checklist. Creates doubt about which version is correct.
- **Fix:** Change line 43 from `v0.5.4 → v0.8.17 (after migration)` → `v0.5.4 → v0.8.18 (after migration)`

### 2. **Multiple v0.8.17 references throughout migration-guide-private-to-public.md**
- **File:** `docs/migration-guide-private-to-public.md`
- **Lines:** 47, 70, 75, 78, 86, 88, 100, 103, 104, 106, 107, 108, 110, 115, 119, 127, 129, 131, 139, 143, 147, 151, 153, 154, 170, 176, 185, 198, 202 (and more)
- **Issue:** The entire document consistently uses v0.8.17 as the target version. This document appears to be a **template or outdated reference** that predates the v0.8.18 decision.
- **Impact:** **SEVERE** — New users following this migration guide will believe they're upgrading to v0.8.17, then be confused when the actual release is v0.8.18.
- **Fix:** This document needs a complete review and systematic replacement of all v0.8.17 → v0.8.18 references. Consider whether this document should be consolidated into `migration-checklist.md` (which is the authoritative source per line 1).

### 3. **Missing clarity on step-by-step upgrade path for NEW users**
- **File:** `README.md` (and all getting-started docs)
- **Issue:** Installation docs show the npm packages but don't explain **what version users will get** or **how to verify it post-upgrade**. The quick-start section (lines 20–60) says `npm install --save-dev @bradygaster/squad-cli` without version pinning—users could get an older version if npm has a cached build.
- **Impact:** Users may silently install an old version and report bugs against v0.8.18 when they actually have v0.8.17.
- **Fix:** Add explicit version pinning to quick-start: `npm install --save-dev @bradygaster/squad-cli@0.8.18` and add verification step: `squad --version` → must show `0.8.18`.

---

## 🟡 Warnings (Should Fix, Not Blocking)

### 1. **migration-guide-private-to-public.md status unclear**
- **File:** `docs/migration-guide-private-to-public.md`
- **Line:** 1
- **Issue:** The document begins with `> **⚠️ SUPERSEDED** — This document has been consolidated into [`docs/migration-checklist.md`](migration-checklist.md). Retained for reference only.` **BUT** the document then contains 200+ lines of detailed migration instructions—far more than a "reference." This is contradictory and confusing.
- **Recommendation:** Either (a) mark it truly deprecated and remove the content, or (b) remove the "superseded" header and clarify that this is a **companion guide to the checklist** with user-focused details.
- **Impact:** Users and the team may waste time consulting a document marked as superseded.

### 2. **README migration reference points to outdated guide**
- **File:** `README.md`, lines 36, 156
- **References:** Both lines point to `docs/migration-github-to-npm.md` as the upgrade path.
- **Issue:** That document is only 65 lines and focuses on the **distribution change** (GitHub-native → npm), not the **version upgrade** (v0.5.4 → v0.8.18). Users upgrading from v0.5.4 also need to read about `.ai-team/` → `.squad/` directory migration, breaking changes, etc.
- **Recommendation:** Add a note in `README.md` that users also consult `docs/migration-guide-private-to-public.md` (once fixed) or consolidate all upgrade guidance into a single "Upgrade Guide" doc.
- **Impact:** Users may miss breaking changes and encounter directory/format incompatibility post-upgrade.

### 3. **samples/README.md not linked from main README**
- **File:** `README.md` (missing reference)
- **Issue:** `samples/README.md` describes 8 working examples (hello-squad, knock-knock, rock-paper-scissors, etc.) at various difficulty levels. The main README has no pointer to these.
- **Impact:** New users don't know samples exist. SDK users have to discover them via directory browsing or search.
- **Fix:** Add a section in `README.md` (after "Quick Start" or under "SDK") that says "See `samples/` for 8 working examples (beginner to advanced)" with a link to `samples/README.md`.

### 4. **Incomplete CLI documentation in README**
- **File:** `README.md`, lines 64–83
- **Issue:** The commands table lists 15 commands but most lack examples or a pointer to where full docs live. New users see `squad init`, `squad upgrade`, `squad status`, etc. but have no quick reference for what each does without reading full docs.
- **Recommendation:** Add a "Quick Reference" section or link to `docs/cli-reference.md` (if it exists) so users can see a one-line description and find detailed docs.
- **Impact:** Users may not discover powerful commands like `squad nap`, `squad scrub-emails`, or `squad link`.

### 5. **Installation.md and first-session.md use old directory reference**
- **File:** `docs/get-started/installation.md`, line 112
- **Issue:** Mentions `.github/agents/squad.agent.md` being created, but elsewhere docs mention `.squad/agents/` structure. This suggests inconsistent terminology.
- **Impact:** Users unfamiliar with the codebase may get confused about where agent files live.
- **Note:** This might be correct (two different directories for different purposes), but the docs don't explain why or clarify the difference.

### 6. **CHANGELOG.md cuts off at line 49, no context for future releases**
- **File:** `CHANGELOG.md`
- **Issue:** The [Unreleased] and [0.8.18-preview] sections describe what's coming but don't show what v0.5.4 → v0.8.18 means in terms of features users are gaining. No summary of major features added since v0.5.4.
- **Recommendation:** Add a section at the top of [0.8.18-preview] summarizing **what changed for users upgrading from v0.5.4**. Example:
  ```
  **For users upgrading from v0.5.4:**
  - Directory: `.ai-team/` → `.squad/`
  - Distribution: GitHub-native → npm packages
  - Features: Monorepo support, remote squad mode, doctor command, dual-root resolution
  ```
- **Impact:** Users upgrading want to know what they gain and what breaks. The changelog currently doesn't surface this clearly.

### 7. **Inconsistent package name messaging**
- **File:** Multiple (`README.md` lines 27, 100, 148; `docs/migration-github-to-npm.md` lines 27, 32)
- **Issue:** Some places say `squad-cli`, others say `@bradygaster/squad-cli`. The scoped package name is correct, but mixing notation may confuse beginners.
- **Recommendation:** Always use the full scoped name (`@bradygaster/squad-cli`) in user-facing docs. Reserve short names (`squad-cli`) for internal code comments only.
- **Impact:** Users might try `npm install squad-cli` (no scope) and get the wrong package or "not found" error.

---

## 🟢 Good (Things Working Well)

### 1. **migration-checklist.md is comprehensive and authoritative**
- Clear phase structure (14 phases) with checkboxes
- Explicit version targets (v0.8.18 everywhere)
- Handles both npm packages AND the public repo GitHub release
- Good decision rationale sections
- Covers rollback scenarios
- **This is the gold standard for the team.** Reference it in all other migration docs.

### 2. **README.md clearly explains distribution change**
- Lines 36, 156 explicitly state that GitHub-native is removed
- Shows both global install and npx options
- Mentions insider channel
- Good tone (direct, not hand-wavy)

### 3. **CLI commands are well-organized in README**
- 15 commands with clear names and brief descriptions
- Covers both end-user commands (init, status, triage) and advanced (nap, scrub-emails, link)
- Aliases shown (e.g., `squad watch` = `squad triage`)
- Table format is scannable

### 4. **Quick Start section is discoverable and actionable**
- Lines 20–60 in README are the right place
- Shows the three main entry points (CLI, VS Code, SDK)
- Example is real and practical (recipe sharing app)
- Good progression: install → authenticate → open Copilot → describe project

### 5. **SDK samples are diverse and well-leveled**
- `samples/README.md` shows 8 samples from beginner (hello-squad, knock-knock) to advanced (autonomous-pipeline, cost-aware-router)
- Estimated LOC helps users pick the right entry point
- Clear theme for each (casting, governance, streaming, etc.)
- **Well done.** Just needs visibility in main docs.

### 6. **Changelog structure is correct**
- [Unreleased] section comes first (expected pattern)
- [0.8.18-preview] section clearly marks the upcoming release
- Categories (Added, Changed, Fixed, Internal) are standard
- Migration context is documented (phases 1–14 linked to phase numbers)

### 7. **Breaking changes are explicitly called out**
- README lines 8–9: "APIs and CLI commands may change between releases"
- CHANGELOG line 17: Distribution change (GitHub-native removed)
- CHANGELOG lines 18–19: Semantic versioning fix
- CHANGELOG line 34: `.squad/` directory structure (migration from `.ai-team/`)
- **Users are warned.** Good.

---

## Summary Table

| Category | Count | Status |
|----------|-------|--------|
| 🔴 Blockers | 3 | Requires fixes before launch |
| 🟡 Warnings | 7 | Should fix soon |
| 🟢 Good | 7 | Keep as-is |
| **Total Findings** | **17** | — |

---

## Execution Path (for Brady)

### Immediate (Before Release)
1. **Fix blocker #1 & #2:** Replace all v0.8.17 → v0.8.18 in `docs/migration-guide-private-to-public.md` (consider consolidating into `migration-checklist.md` instead)
2. **Fix blocker #3:** Add version pinning and verification step to `README.md` quick-start

### Before Public Announcement  
3. Clarify whether `migration-guide-private-to-public.md` is deprecated or a companion guide (add one clear sentence)
4. Add samples reference to main `README.md`
5. Standardize package naming (@bradygaster/squad-cli everywhere)

### Nice-to-Have (Post-Launch)
6. Add CHANGELOG summary for v0.5.4 → v0.8.18 transitions
7. Link all migration docs from a single "Upgrade Path" section in README
8. Verify `.github/agents/` vs `.squad/agents/` terminology is consistent across docs

---

## Questions for Brady

1. **Is `migration-guide-private-to-public.md` intended to stay?** It's redundant with `migration-checklist.md` and uses outdated version numbers. Should we consolidate and remove?
2. **Should we pin versions in the quick-start?** `npm install --save-dev @bradygaster/squad-cli@0.8.18` vs `@latest`? (Recommend: pin for documentation examples, use @latest in production recipes.)
3. **Are there pre-launch tests we should run?** Example: Install via npm, check `squad --version`, run `squad init`, verify `.squad/` structure matches docs.

---

**Status:** 🟡 **Ready for fixes** — All blockers identified, no ambiguities. Recommend fixing blockers before launch, warnings after.
