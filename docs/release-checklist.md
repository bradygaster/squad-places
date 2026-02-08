# Release Checklist

> The release ritual. We evolve this, we don't break it.
> **Owner:** Kobayashi (Git & Release Engineer)

---

## Phase 1: Pre-Release

Everything in this section must be true before anyone touches the release workflow.

### Code Readiness

- [ ] **All tests pass on `dev`** — run `npm test` locally and confirm green. **HUMAN**
- [ ] **No open "blocks-release" issues** — check the issue tracker. **HUMAN**
- [ ] **Version bumped in `package.json`** — the `"version"` field is the single source of truth. Bump it on `dev` and commit: `git commit -m "chore: bump version to X.Y.Z"`. **HUMAN**
- [ ] **Version follows semver** — breaking = minor (pre-v1), feature = minor, fix = patch. See Proposal 021 §2. **HUMAN**
- [ ] **No uncommitted changes on `dev`** — `git status` should be clean. **HUMAN**

### Content Readiness

- [ ] **README.md is current** — install command, usage examples, and feature list reflect what's shipping. **HUMAN**
- [ ] **Release notes drafted** — know what you're going to say. Bullet points are fine. GitHub auto-generates a commit changelog, but the human summary matters. **HUMAN**
- [ ] **CHANGELOG.md updated** (when it exists — McManus owns this, future wave). **TEAM** (McManus)
- [ ] **Breaking changes documented** — if any behavior changed, call it out explicitly. Users read release notes. **HUMAN**

### State Integrity

- [ ] **`upgrade` preserves `.ai-team/`** — run the upgrade path manually: init a fresh project, add a file to `.ai-team/`, run upgrade, confirm the file survives. **HUMAN**
- [ ] **Squad-owned files update on upgrade** — confirm `squad.agent.md` and `.ai-team-templates/` are overwritten. **HUMAN**
- [ ] **CI enforces this** — the state integrity test in CI covers sentinel file survival. **AUTOMATED** (CI workflow)

---

## Phase 2: Release Execution

### Trigger the Release

Two options — pick one:

**Option A: Workflow Dispatch (Recommended)**
- [ ] Go to **Actions → Release → Run workflow** on GitHub. **HUMAN**
- [ ] Enter the version (e.g., `0.2.0`) — must match `package.json` exactly. **HUMAN**

**Option B: Tag Push**
```bash
git checkout dev
git pull origin dev
git tag -a v0.2.0 -m "Release v0.2.0"
git push origin v0.2.0
```
- [ ] Tag pushed to `dev`. **HUMAN**

### What the Workflow Does

These happen automatically — watch for failures in the Actions tab:

- [ ] **Tests run on `dev`** — gate. If tests fail, the release stops. **AUTOMATED**
- [ ] **Version validated** — tag/input version must match `package.json`. Mismatch = hard failure. **AUTOMATED**
- [ ] **Product files filtered** — only product files are staged (no `.ai-team/`, `docs/`, `test/`, workflows). **AUTOMATED**
- [ ] **Commit to `main`** — filtered content replaces `main` entirely. Commit message: `release: vX.Y.Z`. **AUTOMATED**
- [ ] **Tag created on `main`** — annotated tag `vX.Y.Z` on the release commit. Immutable once pushed. **AUTOMATED**
- [ ] **`main` and tag pushed** — both pushed to origin. **AUTOMATED**
- [ ] **GitHub Release created** — includes install/upgrade/pin commands. Pre-v1 versions are marked `prerelease: true`. Auto-generated release notes from commits. **AUTOMATED**

### Watch For

- [ ] **Workflow completes green** — all three jobs (test, release, verify) must pass. **HUMAN**
- [ ] **No error annotations** — check the workflow run summary for warnings or errors. **HUMAN**

---

## Phase 3: Post-Release Validation

Do not skip this. A release that doesn't work isn't a release.

### Automated Verification

- [ ] **`npx` resolution verified** — the release workflow runs `npx -y github:bradygaster/squad --version` and confirms output. **AUTOMATED**

### Manual Verification

- [ ] **Verify default install** — run `npx -y github:bradygaster/squad --version` from a clean terminal. Confirm the new version number. **HUMAN**
- [ ] **Verify pinned install** — run `npx -y github:bradygaster/squad#vX.Y.Z --version`. Confirm exact version. **HUMAN**
- [ ] **Verify upgrade path** — in an existing Squad project, run `npx github:bradygaster/squad upgrade`. Confirm Squad-owned files update and `.ai-team/` is untouched. **HUMAN**
- [ ] **GitHub Release exists** — visit the Releases page. Confirm the release is listed, has the right tag, and shows install/upgrade/pin instructions. **HUMAN**
- [ ] **`main` branch is clean** — `main` HEAD should be the release commit. No stray files, no Squad Squad artifacts. **HUMAN**
- [ ] **`dev` branch is unaffected** — no unexpected commits or changes on `dev`. **HUMAN**

---

## Phase 4: Communication

### Always

- [ ] **GitHub Release notes are readable** — edit the auto-generated notes if the commit messages are unclear. Add a human-written summary at the top. **HUMAN**
- [ ] **Team notified** — update `.ai-team/` history files if the release changes how agents work. **TEAM**

### When Applicable

- [ ] **Blog post** — for minor+ releases, McManus writes a post. Not required for patch releases. **TEAM** (McManus)
- [ ] **README badge updated** — if there's a version badge, confirm it reflects the new version. **HUMAN**
- [ ] **Breaking change announcement** — if anything broke backward compatibility, call it out loudly in release notes AND any relevant docs. **HUMAN**

---

## Phase 5: Rollback Plan

Things go wrong. Here's what to do.

### If the release workflow fails mid-run

- **Nothing shipped.** The workflow is atomic — if tests fail or version validation fails, `main` is untouched. Re-run after fixing the issue on `dev`. **HUMAN**

### If `main` has bad content after release

1. **Do NOT force-push `main`.** Force-push destroys history and breaks pinned users.
2. Fix the issue on `dev`.
3. Bump to a patch version (e.g., `0.2.0` → `0.2.1`).
4. Cut a new release. The new release commit on `main` replaces the broken content.
5. **The bad tag stays.** Tags are immutable. The new release supersedes it.

### If `npx` serves stale content

1. npm caches GitHub tarballs aggressively. Wait 5-15 minutes.
2. Users can force-refresh: `npx --yes github:bradygaster/squad` or clear their npm cache.
3. Verify with `npx -y github:bradygaster/squad#vX.Y.Z --version` (pinned version bypasses default-branch caching).

### If `.ai-team/` state was corrupted

This should never happen — it's a P0 incident.

1. The release workflow doesn't touch user repos. Only the `upgrade` subcommand runs in user repos.
2. If `upgrade` corrupted state, the bug is in `index.js`. Fix on `dev`, patch release immediately.
3. Affected users can recover from git: `git checkout -- .ai-team/`.

---

## Quick Reference

| Step | Who | Can it fail silently? |
|------|-----|----------------------|
| Version bump | HUMAN | No — workflow validates |
| Tests | AUTOMATED | No — gates the release |
| Filtered copy to main | AUTOMATED | No — visible in commit |
| Tag creation | AUTOMATED | No — visible in git |
| GitHub Release | AUTOMATED | Yes — check Releases page |
| npx verification | AUTOMATED | Yes — check workflow logs |
| Upgrade path works | HUMAN | Yes — test manually |

---

*This checklist is a living document. Evolve it when the process changes. Don't skip steps because they feel redundant — redundancy is the point.*
