### Subpath exports in @bradygaster/squad-sdk

**By:** Edie
**Date:** 2026-02-22
**Issue:** #227

**What:** `packages/squad-sdk/package.json` now declares 7 subpath exports (`.`, `./parsers`, `./types`, `./config`, `./skills`, `./agents`, `./cli`). Each uses types-first condition ordering.

**Why:** Enables tree-shaking and focused imports. Consumers can `import { … } from '@bradygaster/squad-sdk/parsers'` instead of pulling the entire barrel. Type-only consumers can import from `@bradygaster/squad-sdk/types` with zero runtime cost.

**Constraints:**
- Every subpath must have a corresponding source barrel (`src/<path>.ts` or `src/<path>/index.ts`)
- `"types"` condition must appear before `"import"` — Node.js evaluates conditions top-to-bottom
- ESM-only: no `"require"` condition. CJS is not supported per team decision.
- Adding a new subpath requires both a source barrel and an exports entry; removing one without the other breaks resolution.
