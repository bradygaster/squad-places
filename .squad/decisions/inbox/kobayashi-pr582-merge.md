# Decision: PR #582 Merge into origin/migration (Local)

**Date:** 2026-02-24  
**By:** Kobayashi (Git & Release)  
**Status:** Decided  
**Affected:** Brady (requester), James Sturtevant (PR author), team

---

## Summary

PR #582 ("Consult mode implementation" by James Sturtevant) must be merged into the local `origin/migration` branch BEFORE pushing to the public beta repo. This consolidates the consult mode feature into the migration payload and ensures the feature ships with the v0.8.17 public release.

---

## Context

- **PR:** #582 (consult-mode-impl branch)
- **Author:** James Sturtevant (@jsturtevant)
- **Scope:** 11 commits, 57 files changed
- **Timeline:** Before Phase 3 (push to beta/migration)
- **Requestor:** Brady

---

## Key Changes in PR #582

### Files Modified (High Risk)
- `package.json` — dependency updates, consult mode additions
- `packages/squad-cli/package.json` — version/dependencies
- `packages/squad-sdk/package.json` — version/dependencies
- `packages/squad-cli/src/cli-entry.ts` — new consult mode commands
- `packages/squad-sdk/src/index.ts` — new SDK exports (⚠️ merge conflict likely)
- `packages/squad-sdk/src/resolution.ts` — core logic
- `squad.config.ts` — config structure
- Test files (parallel test additions expected)
- Templates and documentation

### Files Modified (Medium Risk)
- Various test files with new consult mode tests
- Template files with consult mode scaffolds

---

## Conflict Resolution Plan

### Version Conflicts (CRITICAL)
**Rule:** Keep 0.8.18-preview everywhere. This is our current development version.
- Do NOT accept PR's version strings (which may differ)
- Use `git merge --ours package.json packages/*/package.json`
- If conflict occurs: `git checkout --ours <file>`
- Verify post-merge: `grep '"version"' package.json packages/*/package.json | grep -v 0.8.18-preview` → must be empty

### SDK Exports (index.ts)
- **Likely conflict:** Both PR and migration branch have new exports
- **Strategy:** Manual merge — ensure ALL exports from both branches are retained
- **Validation:** `npm run build` must pass post-merge

### package-lock.json
- **Strategy:** Regenerate from scratch post-merge
- **Commands:** 
  ```
  git merge --abort  # if conflict in package-lock.json
  git merge origin/consult-mode-impl --no-ff -m "Merge PR #582: Consult mode implementation"
  npm install  # regenerates lock file
  git add package-lock.json
  git commit --amend --no-edit
  ```

### Test Files
- **Strategy:** Manual merge — ensure both old tests and new tests are included
- **Validation:** `npm test` must pass post-merge

---

## Merge Commands

```bash
# Fetch PR #582 branch
git fetch origin consult-mode-impl

# Ensure on migration branch
git checkout migration

# Merge with no-ff (preserves PR history)
git merge origin/consult-mode-impl --no-ff -m "Merge PR #582: Consult mode implementation"

# Handle conflicts as documented above
# ...resolve conflicts...

# Regenerate lock file if needed
npm install

# Verify merge succeeded
git log migration --oneline -5

# Verify versions (all must be 0.8.18-preview)
grep '"version"' package.json packages/*/package.json
```

---

## Rollback

If merge fails irreparably:
```bash
# Abort merge
git merge --abort

# Return to clean migration branch
git status  # verify clean
```

Then escalate to Brady and James Sturtevant for resolution.

---

## Validation Checklist

- [ ] Merge completes without fatal conflicts
- [ ] All version strings are 0.8.18-preview (no 0.6.0)
- [ ] `npm install` succeeds (no unresolved dependency errors)
- [ ] `npm run build` succeeds
- [ ] `npm test` passes
- [ ] Migration branch HEAD is the merge commit
- [ ] No uncommitted changes remain

---

## Timing

This merge happens **immediately before Phase 3** (push to beta/migration). It is **not** executed until Brady says "banana" (per BANANA RULE in migration-checklist.md).

---

## Why This Matters

1. **Feature inclusion:** Consult mode ships with v0.8.17 public release
2. **State integrity:** Single, coherent migration payload (not multiple merges on beta)
3. **Version safety:** Version conflicts caught and resolved locally (before pushing to public repo)
4. **Risk isolation:** If merge fails, only origin/migration is affected (beta/public repo untouched)

---

**Decision Owner:** Kobayashi  
**Next Step:** Execute when Brady authorizes (BANANA RULE)
