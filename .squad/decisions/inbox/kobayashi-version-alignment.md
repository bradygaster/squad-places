# Version Alignment Decision — 0.7.0 stubs → 0.8.0 real packages

**Date:** 2026-02-21T23:00Z  
**Decided by:** Kobayashi (Git & Release)  
**Status:** APPROVED & EXECUTED

---

## Context

- **0.7.0 npm stubs:** Placeholder packages published on npmjs.com (no real code)
- **Current state (pre-alignment):**
  - Root `@bradygaster/squad` → `0.6.0-alpha.0` (workspace coordinator, not published)
  - `@bradygaster/squad-sdk` → `0.7.0` (stub on npm)
  - `@bradygaster/squad-cli` → `0.7.0` (stub on npm)
- **Goal:** Publish real, working code under new version

---

## Decision: Bump to 0.8.0

**Rationale:**
- **Clear break from stubs:** 0.7.0 is placeholder; 0.8.0 is functional code
- **Pre-1.0 signal:** Appropriate for alpha software (still evolving, not stable API guarantee)
- **Semantic clarity:** Minor bump (not patch) signals real feature arrival

**Alternatives considered:**
- **1.0.0** — Too bold; SDK is alpha, API not finalized
- **0.7.1** — Confusing; suggests iterative improvement on stubs, not replacement

---

## Changes Executed

### 1. SDK Package
- **File:** `packages/squad-sdk/package.json`
- **Change:** Version `0.7.0` → `0.8.0`

### 2. CLI Package
- **File:** `packages/squad-cli/package.json`
- **Changes:**
  - Version `0.7.0` → `0.8.0`
  - Dependency on SDK: `@bradygaster/squad-sdk@0.8.0` (locked, not workspace protocol)

### 3. SDK Runtime Constant
- **File:** `packages/squad-sdk/src/index.ts`
- **Change:** `export const VERSION = '0.7.0'` → `export const VERSION = '0.8.0'`

### 4. Root Workspace
- **File:** `package.json`
- **Change:** Added `"private": true` (confirms root is not published to npm)
- **Rationale:** Prevent accidental npm publish of workspace coordinator

---

## Verification

- ✅ All three version strings aligned to `0.8.0`
- ✅ CLI dependency on SDK pinned to matching version
- ✅ Root workspace marked private (safety guard)
- ⚠️ Build produces unrelated TypeScript errors (pre-existing, not caused by version alignment)

---

## Next Steps (for Brady/Release Coordinator)

1. **Test locally:** `npm install && npm run build` (once underlying TS issues resolved)
2. **Changeset entry:** Create changeset describing 0.8.0 release
3. **npm publish:** Push to npm registry (not npmjs.com; use private registry or GitHub Packages)
4. **Git tag:** Create release tag `v0.8.0`

---

## Governance

- **Merge target:** `.squad/decisions/` (append-only via merge union driver)
- **History:** Appended to `.squad/agents/kobayashi/history.md` under "## Learnings"
- **Review:** Approved as part of version alignment workflow (no external review needed)
