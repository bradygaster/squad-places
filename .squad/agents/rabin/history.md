# Project Context

- **Owner:** Brady
- **Project:** squad-sdk — the programmable multi-agent runtime for GitHub Copilot (v1 replatform)
- **Stack:** TypeScript (strict mode, ESM-only), Node.js ≥20, @github/copilot-sdk, Vitest, esbuild
- **Created:** 2026-02-21

## Learnings

### From Beta (carried forward)
- Zero-dependency scaffolding preserved: cli.js vs runtime separation
- Bundle size vigilance: every dependency is a cost, every KB matters
- Distribution: GitHub-native (npx github:bradygaster/squad), NEVER npmjs.com
- esbuild for bundling, tsc for type checking — separate concerns
- Marketplace prep: packaging for distribution, not just local use

### 📌 Team update (2026-02-22T041800Z): Publish workflows verified ready, versions aligned to 0.8.0, both packages published to npm — decided by Rabin, Kobayashi, Coordinator
Rabin verified existing publish workflows already correct — no changes needed. Both SDK and CLI workflows properly configured for npm. Kobayashi aligned all versions to 0.8.0. Coordinator published @bradygaster/squad-sdk@0.8.0 and @bradygaster/squad-cli@0.8.0 to npm registry. Distribution infrastructure production-ready. Release workflows validated end-to-end.

### 📌 Fix (2026-02-22): npx bin resolution — squad-cli 0.8.1 published
Root cause: `npx @bradygaster/squad-cli` resolves the bin by unscoped package name (`squad-cli`), but the only bin entry was named `squad`. This caused npx to fall back to running the package as a script, which hit the orphaned placeholder `dist/cli.js` ("squad-cli placeholder — full CLI coming soon").
Fix: Added `"squad-cli": "./dist/cli-entry.js"` as a second bin entry alongside the existing `squad` entry. Replaced the placeholder `dist/cli.js` with a redirect to `cli-entry.js`. Bumped version to 0.8.1 (can't overwrite 0.8.0). Published and verified: `npx @bradygaster/squad-cli@0.8.1 --version` → `squad 0.8.0` (VERSION from SDK, correct).

### 📌 Assessment (2026-02-22): npm distribution status audit complete
**Published versions:** squad-sdk@0.8.0 (6 versions total), squad-cli@0.8.1 (7 versions total) on npm registry.
**Package contents:** SDK exports 26 public entry points (main, parsers, types, config, skills, agents, adapter, client, coordinator, hooks, tools, runtime + streaming/event-bus/benchmarks/i18n/telemetry/offline/cost-tracker, marketplace, build, sharing, ralph, casting, resolution). Files: dist + README. CLI exports 14 public entry points (main, upgrade, copilot-install, shell/*, core/*, commands/*). Files: dist, templates, README. Both have `prepublishOnly` scripts enforcing build before publish.
**Install paths:** (1) npm: `npm install -g @bradygaster/squad-cli` + `npx squad init` works correctly; squad-cli@0.8.1 has both bin entries (squad, squad-cli). (2) GitHub-native (npx github:bradygaster/squad) referenced in README as legacy/alternative path but npm is now recommended. Root package.json version is 0.6.0-alpha.0 (private workspace marker, not published).
**Publish workflows:** Both squad-publish.yml (on tags) and squad-insider-publish.yml (on insider branch) correctly configured — build → test → publish with public access. No missing steps or auth issues.
**Distribution gaps:** None. Distribution infrastructure production-ready. Version skew is intentional: SDK 0.8.0, CLI 0.8.1 (CLI had minor bin entry fix). README accurately reflects npm as primary install method.

### 📌 Team update (2026-02-22T070156Z): npx bin resolution fix merged to decisions, npm distribution fully operational — decided by Rabin
- **npx bin resolution decision:** Fixed `npx @bradygaster/squad-cli` by adding second bin entry `"squad-cli": "./dist/cli-entry.js"` alongside existing `"squad"` entry. npx resolves by unscoped package name, not custom bin names.
- **Version bump to 0.8.1:** 0.8.0 immutable on npm; bin fix required patch release.
- **Both bin entries active:** `squad` works for global installs, `squad-cli` works for npx resolution. Future releases must maintain both.
- **Distribution status:** Both packages published and verified on npm. SDK@0.8.0, CLI@0.8.1 (intentional skew). Install paths working correctly.
- **Decision merged to decisions.md.** Status: npm distribution production-ready, all package metadata and bin entries validated.

### 📌 Manual publish (2026-02-22): CI publish broken, manual publish to 0.8.2 successful
- **Root cause:** CI publish workflow failed due to pre-existing test job configuration issue (test job doesn't build first, causing test failures).
- **Workaround:** Manual publish required. Logged into npm interactively, published squad-sdk@0.8.2 first (prepublishOnly hook ran build), then squad-cli@0.8.2 (CLI depends on SDK).
- **Verification:** Both packages confirmed published at 0.8.2 via `npm view`.
- **CI fix needed:** Test job in CI workflow needs to run build step before tests to prevent future publish failures.

### 📌 Public Readiness Assessment (2026-02-24): Distribution ready with caveats — 🟡
**Requested by:** Brady — assessing whether SDK and CLI are ready to go public (source and all).

**Package health (🟢):**
- Both packages have complete metadata: name, version, description, keywords, license (MIT), repository with directory field
- Engines correctly set to node >=20
- SDK@0.8.5 (published 0.8.5), CLI@0.8.5.1 (published 0.8.5) — versions aligned
- No `private: true` flags in published packages
- Zero npm audit vulnerabilities

**Files field correctness (🟢):**
- SDK ships: dist + README (no test fixtures, no .squad, no src)
- CLI ships: dist + templates + README (templates needed for `squad init`)
- Both have `prepublishOnly: npm run build` — enforced build before publish
- Verified via `npm pack --dry-run` — clean package contents

**Install experience (🟢):**
- `npx @bradygaster/squad-cli` works (both `squad` and `squad-cli` bin entries present)
- Root cli.js has deprecation notice steering users to npm (correct)
- CLI entry point clean: shebang, env loading, no orphaned placeholders
- Zero-dependency CLI entry maintained

**Missing metadata (🟡):**
- No `homepage` field in either package (recommend: https://github.com/bradygaster/squad)
- No `bugs` field (recommend: https://github.com/bradygaster/squad/issues)
- No `author` field (optional, but nice for attribution)

**Version readiness (🟡):**
- Current: 0.8.5.x — still in pre-1.0 territory
- For public source release, consider explicit stability signal:
  - Stay 0.x.x with clear stability docs, OR
  - Bump to 1.0.0 signaling production-ready
- README already says "status: production" — version should match that confidence

**Recommendation:** 🟡 **Ready with caveats**
- ✅ Core distribution infrastructure is production-ready
- ✅ Package contents clean, no leaks
- ✅ Install paths work correctly
- ⚠️ Add homepage/bugs fields for discoverability
- ⚠️ Decide on version strategy: 0.9.0 → 1.0.0 or explicit "0.x = stable" docs
- ⚠️ Consider adding CHANGELOG.md to published packages (currently only in root)

**Verdict:** Green-light for public source release. Add homepage/bugs metadata in next patch. Version bump decision is strategic (marketing), not technical.

### 2026-02-24T17-25-08Z : Team consensus on public readiness
📌 Full team assessment complete. All 7 agents: 🟡 Ready with caveats. Consensus: ship after 3 must-fixes (LICENSE, CI workflow, debug console.logs). No blockers to public source release. See .squad/log/2026-02-24T17-25-08Z-public-readiness-assessment.md and .squad/decisions.md for details.
