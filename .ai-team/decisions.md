# Squad Decisions Log

## v0.5.0 Investigation Findings — February 20, 2026

### Issue #125: Bug — upgrade --migrate-directory exits early

**Status:** Confirmed  
**Severity:** High  
**Finding:** `process.exit(0)` at index.js:1181 exits before normal upgrade flow runs.

**Current Behavior:**
```
npx squad upgrade --migrate-directory
→ Renames .ai-team/ → .squad/
→ Updates .gitattributes
→ Scrubs emails
→ EXITS (early exit prevents remaining upgrade steps)
```

**User Impact:**
- Directory migrated ✅
- squad.agent.md NOT updated ❌
- Workflows NOT updated ❌
- Templates NOT refreshed ❌
- Migrations NOT applied (except early email scrubbing) ❌

**Required Fix:** Either remove process.exit(0) to continue with normal upgrade, or document that users must run two commands:
1. `npx squad upgrade --migrate-directory`
2. `npx squad upgrade`

---

### Issue #124: Bug — detectProjectType() misses .slnx

**Status:** Confirmed  
**Severity:** Medium  
**Finding:** Line 395 only checks .csproj/.sln, not modern .slnx format.

**Current Detection Logic:**
```javascript
if (entries.some(e => e.endsWith('.csproj') || e.endsWith('.sln'))) return 'dotnet';
```

**Missing Patterns:**
- `.slnx` (modern Visual Studio solution format)
- `Directory.Build.props` (MSBuild shared properties convention)
- `*.fsproj` (F# projects)
- `*.vbproj` (VB.NET projects)

**User Impact:** SqncR (with .slnx but no .sln/.csproj in root) gets classified as 'unknown', receives generic workflow stubs instead of .NET-specific build hints.

**Required Fix:** Add `.slnx` to line 395 check. Consider Directory.Build.props fallback.

---

### Issue #126: UX — upgrade --migrate-directory should warn about git history loss

**Status:** Confirmed  
**Severity:** Medium  
**Finding:** Filesystem rename breaks git history tracking and leaves uncommitted changes orphaned.

**Git-Side Risks:**
1. **Rename tracking broken:** `fs.renameSync()` shows as deleted + added, not moved. Blame/history continuity lost.
2. **Unstaged changes orphaned:** If user has uncommitted changes in .ai-team/, they move to .squad/ but git is unaware.
3. **Guard blocks merging:** .squad/ blocked from main by design (intentional), but users should be warned upfront.
4. **.gitattributes not staged:** Users must manually `git add .gitattributes` for new merge rules to take effect.

**User Remediation:**
```bash
git add -A
git commit -m "chore: migrate .ai-team/ → .squad/"
```

**Required Fix:** Add explicit warning in migration output explaining git safety steps. Consider adding pre-flight check for uncommitted changes.

---

### Test Coverage Gaps

**13 critical test cases missing:**
- Migration blocks upgrade (not tested)
- .NET project detection (.slnx, Directory.Build.props)
- Version-specific migrations (0.3.0 → 0.5.0 path)
- Upgrade/migrate interaction
- Project type detection edge cases
- Directory naming (.squad/ vs .ai-team/)

**Test File Status:**
- `email-scrub.test.js` — 1 test covers migration (insufficient)
- `workflows.test.js` — Tests project type but not migration scenarios
- `init-flow.test.js` — Partial coverage of upgrade flow
- Others — No migration or .NET detection tests

---

### Workflow Generation for .NET Projects

**Status:** Working as designed  
**Behavior:** SqncR (detected as .NET) receives stub workflows with TODO comments.

**Stub Example (squad-ci.yml):**
```yaml
- name: Build and test
  run: |
    # TODO: Add your dotnet build/test commands here
    # .NET: dotnet test
    echo "No build commands configured"
```

**Design Rationale:** Users must customize workflows for their specific build system.

**Is This Documented?** Yes, in comment hints. No separate guide.

---

### Decision: Early Exit in --migrate-directory

**Question:** Should `--migrate-directory` continue with normal upgrade or exit after migration?

**Option A:** Remove process.exit(0), continue with upgrade
- **Pros:** Single command, clear semantics, meets user expectations
- **Cons:** Changes flag semantics slightly

**Option B:** Document two-step process
- **Pros:** Clear, no code changes needed
- **Cons:** Poor UX, confusing for users

**Option C:** Create separate --migrate-and-upgrade flag
- **Pros:** Maintains backward compatibility
- **Cons:** More flag combinations, complexity

**Recommendation:** Implement Option A (remove early exit) as lowest-friction fix. Users expect `upgrade --migrate-directory` to be atomic.

---

### Decision: 0.5.0 Migration Directory Detection

**Issue:** 0.5.0 migration hardcodes .ai-team/ check, fails silently if directory renamed to .squad/.

**Code (index.js:1243):**
```javascript
const aiTeamDir = path.join(dest, '.ai-team');
if (fs.existsSync(aiTeamDir)) {
  scrubEmailsFromDirectory(aiTeamDir);
}
```

**Why It Works Currently:** Early email scrubbing in --migrate-directory handler (line 1163) scrubs the renamed .squad/ directory before migration registry runs. But this is fragile—future migrations won't have this fallback.

**Fix:** Update migration to detect both directories:
```javascript
const aiTeamDir = path.join(dest, '.ai-team');
const squadDir = path.join(dest, '.squad');
const targetDir = fs.existsSync(squadDir) ? squadDir : aiTeamDir;
if (fs.existsSync(targetDir)) {
  scrubEmailsFromDirectory(targetDir);
}
```

---

### PR #123 Status

**Date:** 2026-02-20  
**Status:** Opened and awaiting review  
**Content:** Project type detection + git safety fixes  
**Branch:** squad/87-project-type-detection  
**Target:** dev

**Addresses:** Issues #86 (git safety), #87 (project type detection)

---

### Release Status: v0.5.x Rollback

**Date:** 2026-02-20  
**Action:** Marked v0.5.0 and v0.5.1 as pre-releases  
**New Latest:** v0.4.2 (stable)  
**Reason:** User reported issues with v0.5.0; rolling back until investigation complete

---

**Log maintained by:** Squad decision system  
**Last updated:** 2026-02-20T11:24:16Z
