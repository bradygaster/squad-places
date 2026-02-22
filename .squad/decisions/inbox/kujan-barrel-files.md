# Decision: Barrel file conventions for parsers and types

**By:** Kujan (SDK Expert)
**Date:** 2025-07-18
**Re:** #225, #226 (Epic #181)

**What:**
- `src/parsers.ts` re-exports all parser functions AND their associated types using `export { ... } from` and `export type { ... } from` syntax.
- `src/types.ts` re-exports ONLY types using `export type { ... } from` syntax exclusively — guaranteed zero runtime imports.
- Both files follow the existing ESM barrel pattern established in `src/index.ts`.

**Why:**
1. **Consumer convenience:** SquadUI and other consumers can import parsers or types from a single module instead of reaching into internal paths.
2. **Separation of concerns:** `types.ts` is safe for type-only contexts (declaration files, type-checking-only imports). `parsers.ts` includes runtime functions for consumers that need them.
3. **No existing file modifications:** Both are additive-only new files — zero risk to existing behavior.

**Impact:** Low. Two new files, no changes to existing source. Build and all 1683 tests pass.
