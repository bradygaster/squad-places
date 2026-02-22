### OpenTelemetry version alignment: pin core packages to 1.30.x line
**By:** Edie
**Issue:** #254
**What:** All OTel optional dependencies in `packages/squad-sdk/package.json` must stay version-aligned: `sdk-node@^0.57.x` requires `sdk-trace-base@^1.30.0`, `sdk-trace-node@^1.30.0`, `sdk-metrics@^1.30.0`, `resources@^1.30.0`. These must be explicit optional dependencies, not left to transitive resolution.
**Why:** Without explicit pins, npm hoists the latest versions (2.x) of `sdk-trace-base`, `sdk-metrics`, and `resources` to the top-level `node_modules`. The 2.x types are structurally incompatible with the 1.x types that `sdk-node@0.57.x` transitively depends on, causing TS2345/TS2741 type errors (e.g., missing `instrumentationScope` on `ReadableSpan`, missing `getRawAttributes` on `Resource`). Explicit pins at `^1.30.0` force npm to deduplicate on the 1.x line, eliminating the type conflicts.
