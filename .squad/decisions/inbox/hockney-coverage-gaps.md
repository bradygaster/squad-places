# Decision: Coverage Gap Audit — 114 New Tests

**By:** Hockney (Tester)
**Date:** 2026-02-23
**Status:** Implemented

## What

Created 4 new test files covering critical test gaps identified in audit:

1. **`test/shell-integration.test.ts`** (32 tests) — Shell startup lifecycle, input routing (parseInput), coordinator response parsing (ROUTE/DIRECT/MULTI), session cleanup, graceful degradation when SDK disconnected.
2. **`test/health.test.ts`** (17 tests) — HealthMonitor.check() success/timeout/degraded cases, getStatus() passive checks, diagnostics logging toggle.
3. **`test/model-fallback.test.ts`** (25 tests) — Cross-tier fallback chain exhaustion, tier ceiling enforcement (fast never escalates to premium), provider preference reordering ("use Claude" stays in Claude family), nuclear fallback (all models exhausted → null).
4. **`test/cli/upstream-clone.test.ts`** (40 tests) — Git ref validation (14 injection vectors rejected), upstream name validation, source type detection, git clone arg construction, failure recovery messages, file I/O round-trips, gitignore idempotency.

## Why

Audit identified these as the highest-risk untested paths. Shell integration had zero tests for the end-to-end lifecycle. HealthMonitor had no tests at all. Model fallback had catalog tests but no chain-walk or tier-ceiling tests. Upstream git clone had resolver tests but no validation or error recovery tests.

## Impact

- Test count: 2022 → 2136 (114 new tests, +5.6%)
- Test files: 74 → 78 (4 new files)
- Zero regressions — all 2136 tests pass
- No existing test files modified

## Constraints Respected

- Vitest patterns throughout (describe/it/expect/vi.mock/vi.fn)
- Mock SDK client for health tests — no real Copilot connection
- New test files only — no modifications to existing 74 test files
- Exceeds minimum target of 50 new tests (delivered 114)
