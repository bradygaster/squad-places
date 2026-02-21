# Project Context

- **Owner:** Brady
- **Project:** squad-sdk — the programmable multi-agent runtime for GitHub Copilot (v1 replatform)
- **Stack:** TypeScript (strict mode, ESM-only), Node.js ≥20, @github/copilot-sdk, Vitest, esbuild
- **Created:** 2026-02-21

## Learnings

### From Beta (carried forward)
- Multi-agent concurrency tests: spawning is the heart of the system, test it thoroughly
- Casting overflow edge cases: universe exhaustion, diegetic expansion, thematic promotion — all need test coverage
- GitHub Actions CI/CD pipeline: tests must pass before merge
- 80% coverage floor, 100% on critical paths (casting, spawning, coordinator routing)
- 1551 tests across 45 test files — this is the baseline to maintain or exceed
- Vitest is the test runner — fast, ESM-native, good TypeScript support

### Issue #214: Resolution & CLI global/status tests (2026-02-21)
- Added 14 new tests to resolution.test.ts: deeply nested dirs, nearest .squad/ wins, symlink support
- Created cli-global.test.ts with 10 tests: status routing (repo/personal/none), --global flag for init/upgrade
- Test count grew from ~1592 to 1616 across 51 files — all passing
- Symlink test skipped on Windows (requires elevated privileges) — pattern: `if (process.platform === 'win32') return;`
- CLI routing testable without spawning processes by replicating the conditional logic from src/index.ts main()
- resolveGlobalSquadPath() always creates the directory — tests that check global .squad/ must clean up after themselves

### Issue #248: Shell module integration tests (2026-02-21)
- Created test/shell.test.ts with 47 tests covering all shell module components
- **SessionRegistry** (9 tests): register, get, getAll, getActive filter, updateStatus, remove (true/false), clear
- **Spawn infrastructure** (6 tests): loadAgentCharter (load, case-insensitive, missing), buildAgentPrompt (charter, systemContext, omit)
- **Coordinator** (11 tests): buildCoordinatorPrompt (team.md, routing.md, fallbacks), parseCoordinatorResponse (DIRECT, ROUTE, ROUTE no-context, MULTI, fallback), formatConversationContext (all, maxMessages, agentName prefix)
- **ShellLifecycle** (10 tests): init state, ready transition, agent discovery, registry population, addUserMessage, addAgentMessage, addSystemMessage, getHistory (all/filtered), shutdown
- **StreamBridge** (11 tests): message_delta, buffer accumulation, usage, reasoning_delta, flush, flush empty, getBuffer unknown, clear, streaming/idle status transitions
- Created test-fixtures/.squad/ with team.md, routing.md, and agent charters for hockney/fenster
- Test count grew from 1621 to 1668 across 52 files — all passing
- Shell modules are well-structured for testing: pure functions (coordinator parsing), simple classes (SessionRegistry), callback-based bridges (StreamBridge)
- loadAgentCharter accepts optional teamRoot param — critical for test isolation (avoids resolveSquad() cwd dependency)
- Ink components (render.ts replacement) left untested — separate issue per task brief
