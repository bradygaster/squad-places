# Decision: Dual-root ensureSquadPath write validation (#314)

**Date:** 2026-02-23  
**Author:** Edie  
**Issue:** #314  
**Depends on:** #311 (dual-root resolver)  
**Status:** Implemented

## Context

`ensureSquadPath()` validates that file writes stay inside a single `.squad/` root. In remote mode (introduced by #311), there are two valid write roots: `projectDir` (decisions, logs) and `teamDir` (agents, casting, skills). Writes to `teamDir` throw because it's outside the single `squadRoot`.

## Decision

Added two new functions in `packages/squad-sdk/src/resolution.ts` alongside the existing `ensureSquadPath()`:

- **`ensureSquadPathDual(filePath, projectDir, teamDir)`** — validates against both roots plus the system temp directory. Same `path.resolve` + `path.sep` prefix-checking pattern as the original.
- **`ensureSquadPathResolved(filePath, paths)`** — convenience wrapper that destructures a `ResolvedSquadPaths` object (from #311's `resolveSquadPaths()`).

### Backward compatibility

- `ensureSquadPath()` is **unchanged** — no modifications to existing callers.
- New functions are additive exports only.

### Error messages

Changed from "outside the .squad/ directory" to "outside both squad roots" so callers see both paths in the error.

## Tests

13 tests in `test/ensure-squad-path-dual.test.ts`:
- Local mode (single root), remote mode (both roots), rejection, path traversal, subdirectories, exact roots, temp dir, `ResolvedSquadPaths` wrapper.
