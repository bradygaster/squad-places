### SDK dependency: npm registry over file: reference
**By:** Edie
**Date:** 2025-07-24
**What:** `@github/copilot-sdk` is sourced from npm (`^0.1.25`) in `optionalDependencies`, not a local `file:` path.
**Why:** The `file:` reference was an M0 publishing blocker — it breaks `npm publish` because the path doesn't exist on consumer machines. The SDK is published on npm with MIT license and SLSA provenance. Kept in `optionalDependencies` to respect the zero-dependency CLI scaffolding decision.
**PR:** #271
**Closes:** #190, #193, #194
