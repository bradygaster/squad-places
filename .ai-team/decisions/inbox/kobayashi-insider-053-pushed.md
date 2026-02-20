# Decision: Insider Version Scheme 0.5.3-insiders

**Date:** 2026-02-21  
**Author:** Kobayashi (Git & Release Engineer)  
**Context:** Issue #128, v0.5.3 release, insider program

## Decision

Insider builds are now versioned with the `-insiders` pre-release suffix baked into `package.json` on the `insider` branch.

### Version Scheme

- **Production (main):** `0.5.3` → GitHub Release `v0.5.3` (stable)
- **Preview/staging:** Follows main (no separate versioning)
- **Insider (dev channel):** `0.5.3-insiders` → GitHub Release `v0.5.3-insiders` (pre-release)

### Rationale

1. **Single source of truth** — Version lives in `package.json`, no computed suffixes (short SHA, build number). The literal version string is what LLMs see in the coordinator greeting via `stampVersion()`.

2. **Semantic versioning compliance** — Pre-release suffix `-insiders` follows semver rules: pre-release versions sort lower than release versions (compareSemver() now handles this). Ordering is correct: `0.5.3-insiders < 0.5.3 < 0.6.0`.

3. **Reduced release workflow complexity** — Removed short SHA suffix computation (`${VERSION}-insider+${SHORT_SHA}`). Workflow now simply reads `pkg.version` and tags as `v{version}`. Fewer moving parts = fewer automation bugs.

4. **Clear distribution signal** — Install path `npx github:bradygaster/squad#v0.5.3-insiders` is unambiguous. Version string in coordinator output matches the install ref. No user confusion about "what version am I running?"

5. **GitHub Release API** — Insider releases marked as `--prerelease`, so they don't surface as "latest" in GitHub UI. Stable releases on `main` always take precedence. Tag format `v0.5.3-insiders` is distinct from stable `v0.5.3`, preventing accidental overwrites.

## Implementation

**On insider branch:**
- `package.json`: version = `"0.5.3-insiders"`
- `squad-insider-release.yml`: reads pkg.version directly, creates tag `v${VERSION}` (literally `v0.5.3-insiders`)
- Workflow uses `--no-fail-on-empty-changeset || true` to avoid duplicate-tag errors on re-runs

**On dev branch:**
- `package.json`: version = `"0.5.3"` (stable version)
- When merged to insider, version auto-updates to `"0.5.3-insiders"`

## Testing

- Version stamping verified: stampVersion() replaces `Squad v{version}` with literal string (9 new tests in test/version-stamping.test.js)
- compareSemver() handles pre-release suffixes correctly (tested with `-insiders` suffix)
- Workflow syntax validated (YAML parse successful)
- Pushed successfully: dev (commit 936e371), insider (commit 6fc537a)

## Rollout

- Insider program already live (Phase 1 complete, PR #94)
- v0.5.3-insiders tag will auto-publish on next push to insider branch
- Users upgrading via `npx github:bradygaster/squad#insider` will get `-insiders` suffix in coordinator

## Notes

- This scheme is forward-compatible: v0.6.0-insiders, v0.7.0-insiders, etc. all follow the same pattern
- Backports to v0.5.2 insider: No backport needed (v0.5.2-insiders would require updating that tag; insider program is forward-only from v0.5.3)
- Alternative considered: compute suffix at runtime (short SHA, branch name) — rejected because it breaks determinism and complicates LLM instruction stamping
