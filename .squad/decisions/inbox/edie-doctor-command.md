# Decision: squad doctor command — diagnostic conventions

**Date:** 2026-02-22
**By:** Edie
**Issue:** #312
**Status:** Implemented

## What

`squad doctor` is a diagnostic CLI command that validates .squad/ setup integrity. It always exits 0 — it reports problems, never gates on them.

## Key conventions

1. **DoctorCheck interface** — `{ name, status: 'pass' | 'fail' | 'warn', message }` — the typed contract for every check result.
2. **Mode detection order** — config.json `teamRoot` → remote; `squad-hub.json` → hub; else → local.
3. **Exit code always 0** — doctor is informational. `fatal()` is never used for check failures.
4. **Lazy import in cli-entry.ts** — follows the established pattern (`await import('./cli/commands/doctor.js')`).
5. **Subpath export** — `./commands/doctor` in CLI package.json with types-first condition ordering.

## Why

Inspired by Shayne Boyer's PR #131. Teams need a quick way to validate their .squad/ directory without running a full session. The diagnostic-not-gate pattern means CI can call it without risk.
