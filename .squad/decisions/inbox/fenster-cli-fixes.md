# Decision: Version format canonical standard

**Author:** Fenster  
**Date:** 2026-02-24  
**PR:** #447  
**Issues:** #431, #429

## Decision

Bare semver (e.g., `0.8.5.1`) is the canonical format for all version commands (`--version`, `-v`, `version` subcommand, `/version` shell command). Display contexts (help text, shell banner) use `squad v{VERSION}` for branding.

## Rationale

- Matches existing P0 regression test expectations (bare semver, no prefix)
- Script-friendly — bare semver is easier to parse in CI/CD pipelines
- Consistent with `npm --version` convention
- Display contexts (help text, banner) still get the branded `squad v{VERSION}` format

## Impact

All three entry points now produce identical version output:
- `cli-entry.ts` (proper entry): `--version`, `-v`, `version` → bare semver
- `cli.js` (deprecated bundle): `--version`, `-v`, `version` → bare semver (dynamic via getPackageVersion())
- Shell `/version`: bare semver
