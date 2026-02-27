# Repository Cleanup Analysis & Plan

**Decision ID:** keaton-repo-cleanup  
**Date:** 2026-02-24  
**Decider:** Keaton (Lead)  
**Requested by:** Brady  
**Status:** Proposed

## Context

Brady is preparing to work on squad-pr tasks from the `dev` branch but discovered the repository is in a messy state:

- **main** has 1 unpushed commit (fc9d7db) adding `.ai-team/skills/` files (daily squad report, squadified-repo discovery/report)
- **main** and **dev** have diverged significantly: 308 files, 45K+ insertions difference
- **dev** has all the squad-sdk planning work (.ai-team/ docs, decisions, orchestration logs)
- **main** has release/promotion commits (v0.5.0-v0.5.2) that dev doesn't have
- 32 local squad/* branches exist (18 merged, 14 to evaluate)
- 27 remote squad/* branches (mostly merged)
- 11 accumulated stashes
- Tests passing: 86/86

## Analysis

### 1. Branch Divergence Root Cause

The merge base is `d405871`. Since then:
- **main** got 21 non-merge commits (mostly release work, docs updates, guard cleanups)
- **dev** got extensive squad-sdk planning work (session logs, decisions, orchestration logs)

The branches effectively represent two different workstreams that happened in parallel:
- **main**: Release management (v0.5.x), guard cleanup, archive notices
- **dev**: Squad-SDK planning, CLI migration logs, PRD work

### 2. The Unpushed Commit (fc9d7db)

**Commit:** `fc9d7db Add daily squad report skill and restore discovery skills`
- Adds 3 skill files to `.ai-team/skills/`
- Total: 569 insertions, 4 files changed
- **These skills already exist on disk in the current workspace** (visible in .ai-team/skills/)

**Problem:** This commit was made on `main` but the skills are operational work that belongs on `dev` with the rest of the .ai-team/ infrastructure.

### 3. Sync Strategy Assessment

**Option A: Merge dev → main** (bring planning work to main)
- ❌ Pollutes main with 45K+ lines of planning/logging work
- ❌ main should be clean release branch
- ❌ Not aligned with repo conventions

**Option B: Merge main → dev** (bring releases to dev)
- ⚠️ Brings release commits to dev
- ⚠️ May create merge conflicts in .ai-team/
- ⚠️ dev might not need release history

**Option C: Cherry-pick fc9d7db to dev, reset main** (surgical fix)
- ✅ Skills commit goes to dev where it belongs
- ✅ main stays clean with just releases
- ✅ Minimal disruption
- ⚠️ Requires force-push if fc9d7db was already pushed (it hasn't been)

**Recommendation: Option C** — This is the cleanest path forward.

## Recommended Cleanup Plan

### Phase 1: Fix the Unpushed Commit (URGENT)

```bash
# 1. Save the skills commit to dev
git checkout dev
git cherry-pick fc9d7db

# 2. Reset main to origin/main (remove unpushed commit)
git checkout main
git reset --hard origin/main

# 3. Verify
git status  # should show "up to date with origin/main"
git checkout dev
git log --oneline -1  # should show fc9d7db as latest
```

### Phase 2: Branch Cleanup

```bash
# Delete merged local branches (confirm first)
git branch --merged main | Select-String "squad/" | ForEach-Object { $_.Line.Trim() } | ForEach-Object { git branch -d $_ }

# Review unmerged branches manually
git branch --no-merged main | Select-String "squad/"
# Evaluate each: keep if active work, delete if stale

# Delete merged remote branches (after confirming with team)
git branch -r --merged main | Select-String "squad/" | ForEach-Object { 
    $branch = $_.Line.Trim() -replace "origin/", ""
    git push origin --delete $branch
}
```

### Phase 3: Stash Cleanup

```bash
# Review each stash
git stash list

# Drop stale stashes (keep only recent/relevant ones)
git stash drop stash@{N}  # for each stale stash
```

### Phase 4: Working on dev

```bash
# Brady can now work on dev cleanly
git checkout dev
git pull origin dev

# Create feature branches from dev
git checkout -b squad/issue-N-description
```

## Risks & Gotchas

1. **Force-push risk**: If fc9d7db was pushed to origin/main, we'd need `git push --force`. It hasn't been pushed, so safe.
2. **Lost work**: Stashes might contain valuable WIP. Review before dropping.
3. **Branch dependencies**: Unmerged squad/* branches might depend on each other. Check before deleting.
4. **Merge conflicts**: Cherry-picking fc9d7db to dev might conflict with existing .ai-team/skills/ work. Unlikely but possible.
5. **Release sync**: main and dev will remain diverged. This is acceptable if main is release-only and dev is development.

## Decision

**RECOMMENDED:** Execute Phase 1 immediately to fix the misplaced commit. Then proceed with Phases 2-3 for branch/stash cleanup.

**Alternative (if concerned about divergence):** After Phase 1, create a decision to align on main/dev branching strategy:
- Should dev periodically merge from main to get releases?
- Should main ever merge from dev?
- Or should they remain fully independent (main = releases, dev = features)?

## Next Steps

1. Brady confirms approval of Phase 1
2. Keaton executes Phase 1 (5 minutes)
3. Brady verifies skills on dev, main clean
4. Team decides on Phases 2-3 timing (can be deferred)
5. Consider branching strategy discussion if main/dev divergence becomes problematic

## Tags

`git`, `cleanup`, `branching`, `dev-workflow`
