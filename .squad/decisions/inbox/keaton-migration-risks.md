# Risk Assessment: Migration from bradygaster/squad-pr → bradygaster/squad

**Date:** 2026-03-03  
**Assessor:** Keaton (Lead)  
**Requestor:** Brady  
**Context:** Migration checklist at `docs/migration-checklist.md`, detailed guide at `docs/migration-guide-private-to-public.md`

---

## Executive Summary

Migration plan has **5 high-risk issues** and **3 medium-risk gaps** that must be addressed before execution. Most critical: PR #582 merge conflicts (CONFLICTING status), version number mismatch between checklist and package.json, and unaddressed .squad/ state cleanup.

**Recommendation:** Do NOT execute migration until all 🔴 HIGH risks are resolved.

---

## Risk Register

### 🔴 HIGH — PR #582 Merge Conflicts (11 commits, 57 files, CONFLICTING)

**Description:**  
PR #582 ("Consult mode implementation" by James Sturtevant) is marked CONFLICTING by GitHub merge status. The checklist assumes a clean merge, but:
- 57 files changed (package.json, SDK exports, CLI entry, templates, tests)
- Migration branch is 9 commits ahead of main
- Merge base is commit 87e4f1c (main), but migration branch has diverged significantly
- Key conflict zones: `packages/squad-sdk/src/index.ts` (exports), `package.json` (dependencies), `package-lock.json` (lockfile regeneration)

**Impact:**  
- Merge failure blocks entire migration (cannot proceed to Phase 3)
- Version corruption risk: PR may have different version strings than migration branch's 0.8.18-preview
- Build breakage: SDK export conflicts could cause `npm run build` to fail post-merge
- Test failures: 57 files include new tests — conflicts could break test suite

**Mitigation:**  
1. **Pre-merge simulation:** Execute merge locally on a test branch FIRST to discover all conflicts
2. **Conflict resolution strategy:** Use Kobayashi's decision file (.squad/decisions/inbox/kobayashi-pr582-merge.md) as baseline, but UPDATE it with actual conflict resolution steps from simulation
3. **Version validation gate:** Add explicit checklist step: `grep '"version"' package.json packages/*/package.json | grep -v 0.8.18-preview` must return empty
4. **Build/test validation:** Add Phase 2.5 validation: `npm install && npm run build && npm test` all must pass before proceeding to Phase 3
5. **Rollback plan:** Document clear rollback: `git merge --abort` if any validation fails
6. **Escalation path:** If conflicts are non-trivial, escalate to James Sturtevant (PR author) + Brady before forcing resolution

**Phase Impact:** Blocks Phase 3 (push to beta/migration). Phase 2.5 must succeed first.

---

### 🔴 HIGH — Version Number Mismatch (0.8.17 vs 0.8.18)

**Description:**  
Critical inconsistency between checklist, package.json files, and migration plan:
- **Current package.json versions:** 0.8.18-preview (all 3 packages)
- **npm published version:** 0.8.17 (confirmed via `npm view @bradygaster/squad-cli`)
- **Checklist Phase 8 says:** "publishes 0.8.18 from current package.json"
- **Checklist Phase 5 says:** "npm packages and public repo are both 0.8.17"
- **Migration guide says:** Target version is v0.8.17 for public release

**What's happening:**  
The packages are already at 0.8.18-preview (next dev version), but migration intends to publish 0.8.17 to match already-released npm packages. This is **backwards** — you cannot publish an older version over a newer prerelease tag.

**Impact:**  
- **npm publish will fail:** Cannot publish 0.8.17 when packages say 0.8.18-preview
- **Tag mismatch:** GitHub tag will be v0.8.17, but packages will be v0.8.18
- **User confusion:** Version number on public repo won't match installed CLI version
- **Breaking the release sequence:** 0.8.17 is already on npm. The migration should either publish 0.8.18 (next version) or skip npm publish entirely if 0.8.17 is the target public release.

**Correct Resolution:**  
Determine the TRUE target version. Two valid scenarios:

**Scenario A: Public release IS 0.8.17 (match npm)**
- Migration tags public repo at v0.8.17 (matches npm packages already published)
- Do NOT re-publish 0.8.17 to npm (already there)
- Migration branch stays at 0.8.18-preview for continued dev (correct)
- Phase 8 becomes: "Skip npm publish — 0.8.17 already on npm registry"

**Scenario B: Public release IS 0.8.18 (new version)**
- Downgrade package.json versions from 0.8.18-preview → 0.8.18 (remove preview tag)
- Publish 0.8.18 to npm (new stable release)
- Tag public repo at v0.8.18
- Post-release: bump to 0.8.19-preview.1 for next dev cycle

**Mitigation:**  
1. **Brady decision required:** Which scenario is correct? Is 0.8.17 the migration target or 0.8.18?
2. **Update checklist:** Align Phase 5, Phase 8, and Phase 11 version numbers consistently
3. **Update migration guide:** Fix all references to target version (currently inconsistent between 0.6.0, 0.8.17, and 0.8.18)
4. **Add version verification step:** Before Phase 8, validate: `cat package.json | jq -r .version` matches intended publish version

---

### 🔴 HIGH — Prebuild Script Auto-Increment Risk

**Description:**  
The `scripts/bump-build.mjs` script auto-increments the build number on every `npm run build` (via `prebuild` hook):
- Current versions: 0.8.18-preview (no build number yet)
- Script will convert to: 0.8.18-preview.1 on first build
- Every subsequent build increments: .1 → .2 → .3 → ...

**Migration Risk:**  
If `npm run build` is executed during migration (Phase 8 or during PR merge validation):
- Version changes from 0.8.18-preview → 0.8.18-preview.1
- Commits the version bump to migration branch
- Creates a moving target for Phase 8 npm publish
- Forces an extra commit during migration execution

**Phase 8 Correctness:**  
Checklist Phase 8 says "publishes 0.8.18 from current package.json" but doesn't account for:
- Prebuild hook modifying versions mid-execution
- Lock file regeneration after version bump
- Whether the bump should be committed to migration branch or not

**Impact:**  
- Unexpected version drift during migration
- Version mismatch between migration branch HEAD and published packages
- Unclear which version gets tagged: 0.8.18-preview vs 0.8.18-preview.1 vs 0.8.18

**Mitigation:**  
1. **Disable prebuild during migration:** Add step before Phase 8: `npm config set ignore-scripts true` or manually run esbuild without npm scripts
2. **Explicit version control:** Add Phase 7.5: Manually set versions to exact publish target (e.g., 0.8.18 without -preview or .N suffix)
3. **Lock the versions:** Commit the exact versions to be published BEFORE running build/test/publish
4. **Post-publish bump:** After Phase 8 succeeds, bump to next dev version (e.g., 0.8.19-preview.1) and commit separately

---

### 🔴 HIGH — .squad/ Directory Cleanup Missing

**Description:**  
The `.squad/` directory contains session-specific and private development state that should NOT be pushed to the public repo:
- `.squad/.first-run` — session marker file
- `.squad/orchestration-log.md` — contains internal agent coordination logs
- Multiple `history.md` files with 21+ agent session logs (47KB+ for keaton/history.md alone)
- `.squad/identity/prd-consult-mode.md` — draft PRD (118 lines)
- `.squad/identity/prd-next-waves.md` — internal planning
- `.squad/decisions.md` — 67KB of team decisions (includes Copilot session logs, internal discussions)
- Various inbox files, catalogs, assessments from squad members

**Why This Matters:**  
These files expose:
- Internal squad development process and coordination
- Session-specific context that doesn't apply to public users
- Unfinished drafts and planning docs
- Agent personality and decision-making process (not relevant for public users who just want to USE Squad)

**What Should Stay vs Go:**  
**KEEP (public-facing squad config):**
- `.squad/team.md` (roster)
- `.squad/routing.md` (routing rules)
- `.squad/charter.md` (team charter)
- `.squad/agents/*/charter.md` (agent charters only)
- `.squad/templates/` (scaffolding templates)
- `.squad/copilot-instructions.md` (how to use squad)

**REMOVE (private development state):**
- `.squad/agents/*/history.md` (all 21 agent histories — contains session logs)
- `.squad/identity/prd-*.md` (internal PRDs)
- `.squad/decisions.md` (too large, too internal — extract only "final decisions" to a clean file)
- `.squad/orchestration-log.md` (session-specific)
- `.squad/.first-run` (session marker)
- `.squad/agents/*/fragility-catalog.md`, `quality-assessment.md`, `ux-gap-catalog.md` (internal audits)
- `.squad/decisions-archive.md` (67KB archive)
- `.squad/triage/` (internal issue triage)

**Impact:**  
- Public repo exposes internal development process (not professional)
- Confuses public users ("why are there 21 agent histories in my .squad/ folder?")
- Large files (67KB decisions.md, 47KB histories) bloat public repo
- Potential exposure of private session context or internal discussions

**Mitigation:**  
1. **Add Phase 2.5: Clean .squad/ directory**
   - Create a `scripts/clean-squad-for-public.sh` script
   - Removes all `history.md`, `prd-*.md`, `orchestration-log.md`, catalogs, assessments
   - Trims `decisions.md` to only "team decisions" section (remove session logs)
   - Deletes `.first-run`, `triage/`, `decisions-archive.md`
2. **Whitelist approach:** Only include files explicitly needed for public squad usage
3. **Add .gitignore rules:** Prevent future session state from being committed
4. **Validation step:** `git diff --stat origin/main...migration -- .squad/` must show only expected public files

---

### 🔴 HIGH — Phase Ordering: PR #582 Merge Happens Too Late

**Description:**  
PR #582 merge is documented in Kobayashi's decision file but NOT integrated into the migration checklist as a numbered phase. The checklist jumps from Phase 2 (tagging) to Phase 3 (push to beta), with no mention of PR #582.

**Current Flow:**
1. Phase 2: Tag v0.8.17 on origin (SKIPPED per checklist note)
2. Phase 3: Push origin/migration to beta/migration
3. (MISSING) PR #582 merge

**Correct Flow:**
1. Phase 2: Prerequisites complete
2. **Phase 2.5: Merge PR #582 into origin/migration** (NEW)
3. Phase 3: Push origin/migration to beta/migration

**Impact:**  
- If PR #582 is not merged before Phase 3, consult mode feature ships incomplete
- If PR #582 is merged AFTER pushing to beta, requires a second push (messy history)
- Checklist execution will fail because executor won't know when to merge PR #582

**Mitigation:**  
1. **Add Phase 2.5:** Insert explicit "Merge PR #582" phase between Phase 2 and Phase 3
2. **Phase 2.5 content:** Reference Kobayashi's decision file, add conflict resolution steps, add validation checklist (build, test, version verification)
3. **Update Phase 1 prerequisites:** "PR #582 is approved and ready to merge"
4. **Update rollback plan:** If Phase 2.5 fails, document rollback procedure

---

### 🟡 MEDIUM — No .gitignore Update for .squad/ Session State

**Description:**  
Even after cleaning .squad/ for the public release, future development will regenerate session state files (orchestration-log.md, .first-run, etc.). Without .gitignore rules, these files will be accidentally committed again.

**Impact:**  
- Session state leaks into public repo on future commits
- Requires manual vigilance to exclude session files
- Risk of accidental commit of sensitive internal context

**Mitigation:**  
1. Add `.squad/.first-run`, `.squad/orchestration-log.md`, `.squad/agents/*/history.md`, `.squad/identity/prd-*.md` to .gitignore
2. Add Phase 2.6: Commit .gitignore updates to migration branch
3. Pattern-based ignore: `.squad/**/*-log.md`, `.squad/triage/`, `.squad/decisions-archive.md`

---

### 🟡 MEDIUM — Migration Branch Is 9 Commits Ahead of Main (Risky Drift)

**Description:**  
Migration branch has diverged from main by 9 commits. This is normal for a long-running migration branch, but creates risk:
- More drift = higher merge conflict probability
- Harder to reason about what changed
- Rollback is harder if migration fails

**Commits ahead:**
```
a276bf7 fix: migration target is v0.8.17 everywhere, not v0.6.0
8b4fdb7 docs(ai-team): Versioning model fix + decision merge
a6fe361 fix: correct versioning model — npm 0.8.18 vs public repo v0.6.0
2226a8c chore(squad): update now.md for migration + samples phase
45921fd docs(ai-team): RPS sample orchestration log + team history updates
376ac4e feat: rock-paper-scissors multi-agent sample with LLM strategies
e4d6ecc docs(ai-team): Migration v0.6.0 alignment and knock-knock sample modernization
a5564e6 feat: knock-knock sample with real Copilot sessions + v0.6.0 migration docs
ded5f35 chore(squad): commit state from crashed session
```

**Observations:**  
- Multiple version number fix commits (a276bf7, a6fe361) — suggests confusion about target version
- Sample implementations (knock-knock, rock-paper-scissors) — good, should be in migration
- Migration docs updates — necessary
- "crashed session" commit (ded5f35) — might contain unwanted session state

**Impact:**  
- Higher conflict probability when merging PR #582
- Unclear migration payload (what's in these 9 commits vs what's in PR #582?)
- Version number confusion (commits reference 0.6.0, 0.8.17, 0.8.18)

**Mitigation:**  
1. **Audit migration branch commits:** Verify all 9 commits should be in public release
2. **Squash if needed:** Consider squashing the 9 commits into a single "Migration preparation" commit to clean history
3. **Validate ded5f35:** Check what "crashed session" committed — might need to revert session state changes
4. **Sync with main:** Consider rebasing migration onto main before merging PR #582 (reduces conflict surface)

---

### 🟡 MEDIUM — No Validation of npm Publish Permissions

**Description:**  
Phase 8 assumes `npm whoami` succeeds and user has publish rights to `@bradygaster/squad-cli` and `@bradygaster/squad-sdk`, but doesn't validate:
- User is authenticated to correct npm account
- User has publish permissions for both packages
- npm registry is reachable
- Two-factor auth is configured (if required)

**Impact:**  
- Phase 8 npm publish fails mid-execution
- Migration is partially complete (GitHub side done, npm side failed)
- Requires manual recovery and re-publish

**Mitigation:**  
1. **Add Phase 1.5: npm publish dry-run**
   - `npm publish -w packages/squad-sdk --dry-run --access public`
   - `npm publish -w packages/squad-cli --dry-run --access public`
   - Verifies permissions, auth, and package validity WITHOUT actually publishing
2. **Add Phase 8 prerequisite:** "npm credentials verified via dry-run"
3. **Document 2FA:** If npm account has 2FA enabled, Phase 8 will require OTP code — document this

---

## Missing Phases & Gaps

### Gap 1: No Communication Plan
- Who announces the migration? (Brady? Team?)
- Where? (GitHub Discussions, Twitter, blog post?)
- When? (Same day as migration? After validation?)
- What's the message? (Breaking changes, upgrade path, migration guide link)

**Recommendation:** Add Phase 14: Communication & Announcement

---

### Gap 2: No Post-Migration Smoke Test
- After Phase 9 (GitHub Release), should validate:
  - `npm install -g @bradygaster/squad-cli@0.8.17`
  - `squad --version` → 0.8.17
  - `squad init` (fresh project) → works
  - `squad doctor` → no errors
- Phase 13 has "Test install" but it's not structured as a validation gate

**Recommendation:** Expand Phase 13 with explicit smoke test steps and failure criteria

---

### Gap 3: No Monitoring Plan
- After migration, how do we detect issues?
- npm download metrics? GitHub issue velocity? User reports?
- Who monitors? For how long?

**Recommendation:** Add Phase 15: Post-Migration Monitoring (first 48 hours)

---

## Phase Ordering Issues

### Issue 1: Phase 2 (Tag v0.8.17 on Origin) is marked "Note: skip this"
Why? The note says "v0.8.17 tag will be created at the migration merge commit on the public repo (not retroactively on origin)". This is correct — but Phase 2's **title** is misleading. Should be renamed to "Phase 2: Version Alignment Check" instead of "Tag v0.8.17 on Origin".

### Issue 2: Phase 11 (Post-Release Bump) Already Done
Checklist says "Already done: commit 87e4f1c bumped to 0.8.18-preview". If this is already done, Phase 11 should be marked COMPLETE, not left as a TODO checkbox.

### Issue 3: Phase 6 (Package Name Reconciliation) is speculative
Option A vs Option B — decision hasn't been made. Phase 6 should either:
- Be decided before execution (Brady chooses A or B)
- Be removed if `@bradygaster/create-squad` doesn't exist on npm
- Be marked as "Post-migration cleanup" instead of blocking phase

---

## Recommended Execution Order (Fixed)

1. **Phase 1:** Prerequisites (all green)
2. **🆕 Phase 1.5:** npm publish dry-run validation
3. **Phase 2:** Version alignment check (determine 0.8.17 vs 0.8.18 target)
4. **🆕 Phase 2.5:** Clean .squad/ directory for public release
5. **🆕 Phase 2.6:** Merge PR #582 into origin/migration (with conflict resolution + validation)
6. **Phase 3:** Push origin/migration to beta/migration
7. **Phase 4:** Merge beta/migration → beta/main (PR on public repo)
8. **Phase 5:** Version alignment on beta (already aligned from Phase 2)
9. **Phase 6:** Package name reconciliation (decide A vs B, or skip)
10. **Phase 7:** Beta user upgrade path (documentation only)
11. **Phase 8:** npm publish (or skip if 0.8.17 already on npm)
12. **Phase 9:** GitHub Release (v0.8.17 or v0.8.18 per Phase 2 decision)
13. **Phase 10:** Deprecate old package (if Option A chosen in Phase 6)
14. **Phase 11:** Post-release version bump (if needed, or mark COMPLETE)
15. **Phase 12:** Update migration docs
16. **Phase 13:** Verification & smoke tests
17. **🆕 Phase 14:** Communication & announcement
18. **🆕 Phase 15:** Post-migration monitoring (48 hours)

---

## Blockers Summary

**Must resolve before "banana":**
1. 🔴 Clarify target version: 0.8.17 or 0.8.18? (Brady decision)
2. 🔴 Add Phase 2.5: Clean .squad/ directory (script + validation)
3. 🔴 Add Phase 2.6: Merge PR #582 (with conflict resolution plan)
4. 🔴 Fix prebuild script behavior during migration (disable or control version bumps)
5. 🔴 Add .gitignore rules for .squad/ session state

**Should resolve before "banana":**
6. 🟡 npm publish dry-run validation (Phase 1.5)
7. 🟡 Audit migration branch commits (especially ded5f35)
8. 🟡 Expand Phase 13 with structured smoke tests

---

## Final Recommendation

**DO NOT EXECUTE MIGRATION** until all 🔴 HIGH risks are resolved. The version number confusion alone (0.8.17 vs 0.8.18) is a showstopper — publishing will fail or create user confusion.

**Estimated time to resolve:** 4-6 hours of focused work:
- 2 hours: PR #582 merge simulation + conflict resolution
- 1 hour: .squad/ cleanup script + .gitignore
- 1 hour: Version number alignment (Phase 2, 5, 8, 9, 11 consistency)
- 30 min: Prebuild script handling
- 30 min: Checklist updates (new phases, reordering)

**Next step:** Brady reviews this assessment, makes version target decision, then Kobayashi executes risk mitigations.

---

**Assessor:** Keaton (Lead)  
**Date:** 2026-03-03  
**Status:** Ready for Brady review
