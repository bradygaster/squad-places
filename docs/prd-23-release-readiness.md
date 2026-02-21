# PRD 23: Release Readiness — Walk-Up-and-Try-It Parity

**Status:** Draft
**Priority:** P0 — Blocks public release
**Author:** Keaton (Lead)

---

## Problem

squad-sdk has all 9 CLI commands implemented, 1,551 tests passing, and a bundled `cli.js` for distribution. But several gaps remain before Brady can hand this to users as a drop-in replacement for the beta.

## Current State (What Works Now)

| Capability | Status |
|-----------|--------|
| `node cli.js` runs all 9 commands | ✅ |
| Init creates all 55 files in clean directory | ✅ |
| Templates tracked in git (34 files) | ✅ |
| 1,551 tests across 45 files | ✅ |
| CLI bundled as single 76KB cli.js | ✅ |
| README has walk-up-and-try-it quick start | ✅ |
| .squad/ cleaned for Brady's respawn | ✅ |
| Help output matches beta (all 9 commands) | ✅ |

## Gaps Remaining

### Gap 1: npx End-to-End Verification

**Risk:** High
**Effort:** Small

`cli.js` is committed and `bin` points to it, but `npx github:bradygaster/squad-sdk` hasn't been tested from another machine. The package.json dependency on `@github/copilot-sdk: file:../copilot-sdk/nodejs` is a local file reference — npx from GitHub won't resolve this.

**Fix:**
- The CLI (`cli.js`) is zero-dep — it does NOT import the SDK. Only `dist/index.js` (the library) needs the SDK.
- Verify: `npx github:bradygaster/squad-sdk` should run `cli.js` which never touches the SDK import.
- If npm tries to install deps before running bin, the `file:` reference will fail. Need to either:
  - (a) Move SDK dep to `optionalDependencies` or `peerDependencies`
  - (b) Add an `.npmrc` with `optional=true`
  - (c) Add a `preinstall` script that skips SDK install for CLI-only usage
- **Test plan:** Push to master, run `npx github:bradygaster/squad-pr` from a different directory.

### Gap 2: Integration Test Suite for CLI

**Risk:** Medium
**Effort:** Medium

The 1,551 tests cover SDK core modules. The CLI commands (init, upgrade, export, import, plugin, copilot, watch, scrub-emails) have no automated end-to-end tests.

**Fix:**
- Create `test/cli/` directory with integration tests
- Test init in temp directory (verify all files created)
- Test upgrade (modify file, run upgrade, verify overwritten)
- Test scrub-emails (add email to file, run, verify removed)
- Test export/import round-trip
- Can use Node's built-in `node --test` runner or Vitest

### Gap 3: Insider Branch Setup

**Risk:** Low
**Effort:** Small

The beta has an `insider` branch with pre-release workflow. Squad-sdk needs the same for `npx github:bradygaster/squad-sdk#insider`.

**Fix:**
- Create `insider` branch from master
- Verify insider release workflow (already in templates)
- Document in README (already referenced)

### Gap 4: Version Bumping Workflow

**Risk:** Low
**Effort:** Small

No mechanism to bump version across `package.json`, `cli.js`, and `src/cli-entry.ts` (VERSION constant is hardcoded in 2 places).

**Fix:**
- Extract version from package.json at runtime (already done in `getPackageVersion()`)
- Remove hardcoded VERSION constant from `src/cli-entry.ts` and `src/index.ts`
- Use `getPackageVersion()` everywhere
- Add `npm version` hook to rebuild `cli.js` after bump

### Gap 5: Package Name in npx URL

**Risk:** Low
**Effort:** Small

The npx URL is `npx github:bradygaster/squad-sdk` but the repo is named `squad-pr` on GitHub. If the repo is renamed to `squad` (replacing beta), the URL changes.

**Decision needed from Brady:** What will the final repo name and npx URL be?

## Out of Scope

- Fritz preparation (separate workstream)
- SquadUI SDK proposal (separate workstream)
- Editless extension updates (separate workstream)
- Public docs site (v1 docs are internal only per standing decision)

## Definition of Done

1. `npx github:bradygaster/squad-pr` (or final URL) creates a working squad in a clean repo
2. CLI integration tests pass in CI
3. Insider branch exists and insider release workflow is functional
4. Version can be bumped with a single `npm version` command
5. Brady can open squad-sdk in VS Code, select Squad agent, and run the respawn prompt
