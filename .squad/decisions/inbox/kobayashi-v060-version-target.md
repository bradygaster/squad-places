# Decision: v0.6.0 Public Release Version Target

**Date:** 2025  
**Decided By:** Brady (Product), Kobayashi (Git & Release)  
**Status:** ✅ DECIDED

## Context

The squad-pr private repository has undergone significant development from v0.5.4 (public baseline) through internal versions 0.6.x through 0.8.x. The team needed to establish a clear version target for the public migration from squad-pr → squad (bradygaster/squad).

### Previous Recommendation

Kobayashi had recommended v0.8.17 as the target version for public distribution.

### Brady's Direction

Brady decided that **v0.6.0** is the target public release version:
- Public repo (bradygaster/squad) is currently at v0.5.4
- v0.6.0 represents a clean minor version bump for public users
- Internal versions (0.6.x through 0.8.x) were development milestones in the private repo and do not need to be published

## Decision

### Public Migration Version: v0.6.0

**Public Distribution:**
- Jump from v0.5.4 → v0.6.0 (clean minor version bump per semver)
- v0.6.0 tag will be created at the migration merge commit on the public repo
- Package names remain: `@bradygaster/squad-cli` + `@bradygaster/squad-sdk`

### Internal Development Continuity

Origin (squad-pr) continues development:
- Origin remains at 0.8.18-preview.x for continued internal development
- Public repo isolated at v0.6.0 release
- Origin packages published to npm as v0.6.0 (not 0.8.x)

## Updated Documentation

The following migration documentation has been updated to reflect v0.6.0 as the target:

1. **docs/migration-checklist.md** — Updated all phases to reference v0.6.0:
   - Phase 2: Removed duplicate tagging logic (v0.6.0 tags at merge commit on public repo only)
   - Phase 4: PR title and body reference v0.5.4 → v0.6.0
   - Phase 5: Consolidated to single decision (no Option A/B) with v0.6.0 as target
   - Phase 7: User upgrade path references v0.6.0
   - Phase 8-13: All npm, release, and verification steps use v0.6.0

2. **docs/migration-guide-private-to-public.md** — Updated 45+ version references:
   - User upgrade path: v0.5.4 → v0.6.0
   - All migration steps reference v0.6.0
   - GitHub Release and npm package versions updated
   - Breaking changes documentation reflects v0.6.0 migration

3. **docs/launch/migration-guide-v051-v060.md** — No changes needed (internal SDK migration)

4. **docs/migration-github-to-npm.md** — No changes needed (distribution method, no version refs)

5. **docs/cookbook/migration.md** — No changes needed (internal SDK, references 0.5.1 → 0.6.0 correctly)

## Rationale

- **Semver Clarity:** v0.5.4 → v0.6.0 is a clear, standard minor version bump for public users
- **No Skipping Versions:** Public users see contiguous version progression (unlike internal dev milestones)
- **User Communication:** Clear "before/after" marker for migration announcement
- **npm Distribution:** v0.6.0 is the stable release version for npm packages `@bradygaster/squad-cli` and `@bradygaster/squad-sdk`

## Related Issues & PRs

- Migration initiated by Brady's direction (private conversation)
- Supersedes Kobayashi's v0.8.17 recommendation
- docs/migration-checklist.md is the authoritative execution guide

## Sign-Off

- ✅ **Brady** — Approves v0.6.0 as public migration target
- ✅ **Kobayashi** — Updates all migration documentation and processes
