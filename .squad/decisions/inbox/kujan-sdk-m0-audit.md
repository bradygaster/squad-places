# SDK M0 Audit Findings

**Author:** Kujan (SDK Expert)
**Date:** 2025-07-18
**Issues:** #190, #193, #194

---

## #190 — Audit @github/copilot-sdk dependency

### Current dependency reference
- **Type:** `file:` reference (`file:../copilot-sdk/nodejs`)
- **Location:** `optionalDependencies` in `package.json`
- **Lockfile version:** `0.1.8` (resolved from sibling directory)

### SDK size
- **Total on disk (with node_modules):** ~296 MB (overwhelmingly `node_modules/` at ~295.5 MB)
- **dist/ only:** ~0.12 MB
- **src/ only:** ~0.13 MB
- **npm tarball (published):** 16 files, ~150 KB unpacked

### License
- **MIT** — both in local copy and npm registry.

### SDK runtime dependencies
- `@github/copilot` ^0.0.411
- `vscode-jsonrpc` ^8.2.1
- `zod` ^4.3.6

### Public API surface consumed by Squad
Squad's SDK surface is **minimal and well-contained**:

1. **`src/adapter/client.ts:10`** — `import { CopilotClient } from "@github/copilot-sdk"` — the only runtime import.
2. **`src/adapter/types.ts`** — defines Squad-stable adapter types; does NOT import from SDK (comment-only reference).
3. **`src/build/bundle.ts:33`** — string literal `'@github/copilot-sdk'` in the external list for esbuild bundling.

**Consumed API:** `CopilotClient` class with these methods: `start()`, `stop()`, `forceStop()`, `createSession()`, `resumeSession()`, `listSessions()`, `deleteSession()`, `getLastSessionId()`, `ping()`, `getStatus()`, `getAuthStatus()`, `listModels()`, `on()`.

Constructor options: `cliPath`, `cliArgs`, `cwd`, `port`, `useStdio`, `cliUrl`, `logLevel`, `autoStart`, `autoRestart`, `env`, `githubToken`, `useLoggedInUser`.

### Test files that reference SDK
4 test files mock the SDK via `vi.mock('@github/copilot-sdk')`:
- `test/adapter-client.test.ts`
- `test/client.test.ts`
- `test/integration.test.ts`
- `test/bundle.test.ts` (string assertion only)

All tests use mocks — **no tests require a real SDK installation to pass**.

---

## #193 — Decide resolution path for SDK publishing blocker

### Key finding: The blocker is already unblocked
**`@github/copilot-sdk` is published on npm.** Latest: `0.1.25` (28 versions available, including preview releases). The SDK has SLSA provenance attestations on npm.

### Recommended path: **(a) Normal npm dependency**

Switch from `file:../copilot-sdk/nodejs` to `"@github/copilot-sdk": "^0.1.25"` in `optionalDependencies`.

**Verified:** Build passes (0 errors), all 1592 tests pass with npm reference.

### Why optionalDependencies should remain
Squad distributes via `npx github:bradygaster/squad` (GitHub-native, per team decision). The SDK is optional at the CLI scaffolding layer — it's only needed at runtime when `src/adapter/client.ts` is loaded. Keeping it in `optionalDependencies` preserves the zero-dependency scaffolding guarantee (decision by Rabin).

### Alternatives considered
- **(b) Dynamic import:** Unnecessary; `optionalDependencies` already handles the missing-SDK case at install time.
- **(c) Peer dependency:** Would force consumers to install it manually; worse UX.
- **(d) Bundle the SDK:** Would bloat the package and make updates harder. The SDK dist is only ~150KB but has transitive deps.
- **(e) Other:** No need; the straightforward path works.

---

## #194 — Verify SDK builds and tests pass without file: reference

### With npm reference (`^0.1.25`)
- **Build:** ✅ `tsc --noEmit` passes with 0 errors
- **Tests:** ✅ 1592/1592 tests pass (49 test files)
- **Install:** ✅ `npm install` resolves cleanly (adds 4 packages)

### Without any SDK reference (dependency removed entirely)
- **Build:** ❌ 1 error in `src/adapter/client.ts:10` — `Cannot find module '@github/copilot-sdk'`
- **Tests:** All 4 SDK-mocking test files would fail at import resolution
- **Impact:** Only the adapter layer breaks; the rest of Squad compiles fine

### Test dependency analysis
- **4 test files** import from `@github/copilot-sdk` but all use `vi.mock()` — they need the module to exist for resolution but never call real SDK code
- **0 tests** require a live Copilot CLI server
- **1545+ tests** have zero SDK dependency

---

## Recommendation

**Immediate action:** Change `optionalDependencies` from `file:../copilot-sdk/nodejs` to `"@github/copilot-sdk": "^0.1.25"`. This is a one-line change. Build and tests are verified to pass. The file: reference is the only M0 blocker — it prevents npm publish and breaks CI on any machine without the sibling directory.
