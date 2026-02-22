# Decision: Runtime Module Test Patterns

**Author:** Hockney (Tester)
**Date:** 2026-02-22
**Status:** Adopted

## Context

Writing proactive tests for runtime modules (CharterCompiler, AgentSessionManager, Coordinator, RalphMonitor) being built in parallel by multiple team members.

## Decisions

1. **Two EventBus APIs require different mocks.** The client EventBus uses `on()`/`emit()` while the runtime EventBus uses `subscribe()`/`emit()`. Tests must use the correct mock depending on which bus the module under test consumes. AgentSessionManager uses the client bus (`on`); Coordinator uses the runtime bus (`subscribe`).

2. **CharterCompiler tests use real test-fixtures.** Instead of mocking the filesystem, we read from `test-fixtures/.squad/agents/` for `compile()` and `compileAll()` tests. This gives integration-level confidence. Only `parseCharterMarkdown` uses inline string fixtures for unit isolation.

3. **Coordinator routing priority is: direct > @mention > team keyword > default.** Tests explicitly verify this ordering. Any change to routing logic must preserve this priority chain.

4. **RalphMonitor tests are future-proof stubs.** Since Ralph is mostly TODO stubs, tests validate current behavior (empty arrays, no-throw lifecycle) and will automatically exercise real logic once implemented — no test changes needed.

## Impact

- 105 new tests across 4 files, all passing
- Test count: 1727 → 1832 across 61 files
