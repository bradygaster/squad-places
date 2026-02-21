### 2026-02-21: Shell module test patterns — fixtures over mocks
**By:** Hockney (Tester)
**Date:** 2026-02-21
**Re:** #248

**What:** Shell module tests use `test-fixtures/.squad/` with real files (team.md, routing.md, agent charters) instead of mocking fs calls. The `loadAgentCharter` and `buildCoordinatorPrompt` functions accept a `teamRoot` parameter that enables this pattern.

**Why:**
1. Real file reads catch encoding issues, path resolution bugs, and parsing edge cases that mocks hide.
2. The `teamRoot` parameter was already designed into the API — tests should use it.
3. StreamBridge tests use callback spies (arrays capturing calls) rather than vi.fn() — simpler to assert on and read.

**Impact:** Low. Establishes fixture patterns for future shell module tests. test-fixtures/.squad/ is now a shared test resource.
