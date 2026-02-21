# v0.6.0 Launch Communication Plan

> ⚠️ **INTERNAL ONLY** — Coordination document for the Squad team.

---

## Channels

| Channel            | Action                                         | Owner  |
|--------------------|-------------------------------------------------|--------|
| GitHub Release     | Create release with tag `v0.6.0`, attach notes  | Keaton |
| npm Publish        | `npm publish` under `@bradygaster/squad`         | Keaton |
| README             | Update with v1 instructions and migration guide  | Verbal |
| Blog (internal)    | Publish "SDK Replatform" post                    | Verbal |
| Blog (public)      | Publish after GA validation period               | Brady  |

## Timeline

| Phase        | Duration   | Activities                                    |
|--------------|------------|-----------------------------------------------|
| RC           | Days 1–3   | Tag `v0.6.0-rc.1`, internal testing            |
| Validation   | Days 4–7   | Brady runs manual test scripts, team dog-foods |
| GA           | Day 8      | Promote RC to GA, publish to npm               |
| Announcement | Day 8–10   | GitHub release, README update, blog posts      |

## Stakeholders

| Stakeholder | Role                              |
|-------------|-----------------------------------|
| Brady       | Final approval, manual QA sign-off |
| Keaton      | Release engineering, npm publish   |
| Verbal      | Documentation, blog, README        |
| Community   | Post-GA announcement recipients    |

## Rollback Plan

If critical issues are found after GA publish:

1. **Unpublish** the npm package within the 72-hour window: `npm unpublish @bradygaster/squad@0.6.0`
2. **Tag revert**: delete the `v0.6.0` Git tag and GitHub release
3. **Hotfix branch**: create `squad/hotfix-0.6.1` from the last known-good commit
4. **Communication**: post an issue in the repo explaining the rollback and ETA for fix
5. **Re-release**: after fix, follow the same RC → Validation → GA pipeline

## Pre-Publish Checklist

- [ ] `docs/launch/checklist.md` fully signed off
- [ ] `npm pack` produces clean tarball with no extraneous files
- [ ] `npm publish --dry-run` succeeds
- [ ] GitHub release draft reviewed by Brady
- [ ] Blog post reviewed by at least one other team member
