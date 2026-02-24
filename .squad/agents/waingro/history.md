# Waingro — History

## Project Context
- **Project:** Squad — programmable multi-agent runtime for GitHub Copilot
- **Owner:** Brady
- **Stack:** TypeScript (strict, ESM), Node.js ≥20, Ink 6 (React for CLI), Vitest
- **CLI entry:** packages/squad-cli/src/cli-entry.ts
- **Key concern:** Cross-platform (Windows + macOS + Linux), TTY and non-TTY modes

## Learnings

### 2026-02-23: Hostile QA — Issue #327
**Tested 32 adversarial scenarios across 7 hostile condition categories:**
- Tiny terminal (40x10): All 5 pass. CLI degrades gracefully at small sizes.
- Missing config: All 5 pass. CLI works without .squad/ directory for non-interactive commands.
- Invalid input: All 5 pass. Control chars, 10KB+ args, empty/whitespace args handled.
- Corrupt config: All 5 pass. Empty .squad/, empty team.md, invalid content, .squad-as-file all survive.
- Non-TTY pipe mode: All 4 pass. Version/help/status/error all work piped.
- UTF-8 edge cases: All 5 pass. Emoji, CJK, RTL, zero-width, mixed scripts all handled.
- Rapid input: All 3 pass. 5 concurrent, alternating, and parallel commands all stable.

**Bugs found:**
1. **BUG: `--version` output omits "squad" prefix.** `cli-entry.ts:48` says `console.log(\`squad ${VERSION}\`)` but actual output is bare `0.8.5.1`. The existing `version.feature` test also fails on this. Likely the VERSION import returns the number directly and `console.log` produces different output than expected, OR the build artifact differs.
2. **BUG: Empty/whitespace CLI args trigger interactive shell launch in non-TTY.** When `args[0]` is `""` or `"   "`, `cmd` is falsy, so `runShell()` is called. In non-TTY mode, Ink renders and exits with code 1. Should detect non-TTY and show help or error instead.
3. **Observation: Node.js rejects null bytes in spawn args** (`ERR_INVALID_ARG_VALUE`). This is Node-level, not a Squad bug, but the CLI should sanitize or reject args containing null bytes before they reach spawn.

**Key patterns:**
- Acceptance test step registration order matters — greedy regex `I run "(.+)"` in cli-steps matches before more-specific hostile patterns. Register specific patterns first.
- The nasty-inputs corpus at `test/acceptance/fixtures/nasty-inputs.ts` has 80+ adversarial strings for fuzz testing.
- Corrupt .squad/ configurations are handled gracefully — no crashes or unhandled exceptions observed.

### 2026-02-23: Hostile QA — End-to-End Quality Assessment (Brady's request)
**Scope:** Honest, candid quality review of CLI entry point, shell, commands, error handling, streaming, and test coverage.

#### VERDICT: The happy path is solid. Everything else is held together with string and hope.

---

#### 1. DEAD TEST CORPUS — SEVERITY: HIGH

The nasty-inputs corpus at `test/acceptance/fixtures/nasty-inputs.ts` (95 adversarial strings) is **never imported by any test file**. Zero consumers. The hostile acceptance tests in `test/acceptance/hostile.test.ts` run Gherkin `.feature` files via a step registry—they use `hostile-steps.js`, not the corpus. The 80+ fuzz strings I wrote are sitting in a drawer, collecting dust. Nobody runs them against the shell input path.

**Impact:** We have zero automated fuzz coverage against the interactive shell's `parseInput()` or `InputPrompt.onSubmit()`. We claim adversarial testing. We don't have it where it counts.

---

#### 2. NO REACT ERROR BOUNDARY — SEVERITY: HIGH

There is **no ErrorBoundary component** anywhere in the Ink shell. Zero hits for `componentDidCatch`, `getDerivedStateFromError`, or `ErrorBoundary` in the entire `packages/squad-cli/src/` tree.

If any React component throws during render (bad agent name in `AgentPanel`, malformed streaming content in `MessageStream`, a null deref from a race condition), **the entire Ink process crashes with an unhandled React error**. No recovery. No friendly message. Just a stack trace.

In a 20-minute session, the probability of hitting a render error approaches 1 if the user is doing anything interesting (multi-agent, concurrent routing, rapid input).

---

#### 3. SDK CONNECTION DROP MID-STREAM — SEVERITY: HIGH

Here's the scenario: User sends a message → `dispatchToAgent` fires → `awaitStreamedResponse` calls `session.sendAndWait()` with a **10-minute timeout** (`TIMEOUTS.SESSION_RESPONSE_MS = 600000`).

If the SDK connection drops mid-stream:
- `sendAndWait` will throw. Good — it's caught by `handleDispatch`.
- But the `accumulated` buffer and `onDelta` listener are left in a half-state.
- The `finally` block in `dispatchToAgent` (line 291) cleans up the listener, but `streamBuffers` in the outer scope is never flushed.
- The `session` object in `agentSessions` Map is now dead. **The next message to that agent will reuse the dead session** (line 228: `let session = agentSessions.get(agentName)`). It will fail again. And again. Forever.

**There is no session invalidation on error.** The dead session stays in the Map. The user has to restart the CLI.

---

#### 4. GHOST RESPONSE RETRY MASKS REAL FAILURES — SEVERITY: MEDIUM

`withGhostRetry` retries 3 times with 1s/2s/4s backoff when `sendFn` returns empty. Problem: if the SDK connection is dead, each retry creates a new `awaitStreamedResponse` call on the same dead session, waits up to 10 minutes, gets nothing, retries. That's potentially **30+ minutes of silent hanging** before the user sees "Agent did not respond."

The retry mechanism conflates "ghost response" (SDK race condition) with "connection dead" (infrastructure failure). They need different handling.

---

#### 5. CONCURRENT AGENT DISPATCH — SEVERITY: MEDIUM

When the coordinator returns `MULTI:` routing, `dispatchToCoordinator` fires `Promise.allSettled` on multiple `dispatchToAgent` calls (line 370). Each agent dispatch:
- Sets `shellApi?.setStreamingContent()` — **there's only one streaming content slot**.
- Sets `shellApi?.setActivityHint()` — **there's only one hint slot**.

Two agents streaming simultaneously will clobber each other's display content. The `streamBuffers` Map in `runShell` handles per-agent accumulation correctly, but the React state in `App.tsx` has a single `streamingContent` useState. Last writer wins. The user sees a jumbled mess of interleaved partial responses.

---

#### 6. `processing` LOCK IS A LIE — SEVERITY: MEDIUM

`InputPrompt` is disabled when `processing=true` (App.tsx:139). But `useInput` in Ink **still receives keystrokes**. The guard `if (disabled) return` at InputPrompt.tsx:36 means keystrokes are **silently dropped**. If the user types during processing, their input vanishes. No buffer, no queue, no "I'll handle that next." Just gone.

This is a UX time bomb. Users WILL type ahead. Their messages WILL disappear.

---

#### 7. NO SIGINT/SIGTERM HANDLER IN SHELL — SEVERITY: MEDIUM

The shell uses Ink's `exitOnCtrlC: false` and manually handles Ctrl+C via `useInput` (App.tsx:91-94). But:
- There is **no `process.on('SIGTERM')` handler** in the shell. Only `aspire.ts` and `watch.ts` register signal handlers.
- If the process receives SIGTERM (e.g., Docker stop, CI timeout), the cleanup block at index.ts:420-432 never runs. Sessions leak. The SDK client is abandoned mid-stream.
- The Ink `exit()` call on Ctrl+C triggers `waitUntilExit()` to resolve, which then runs cleanup. But SIGTERM bypasses Ink entirely.

---

#### 8. MemoryManager IS DEAD CODE — SEVERITY: LOW

`MemoryManager` is exported from `index.ts` (line 38) but **never instantiated or used** in `runShell()` or any component. The `messages` array in `App.tsx` grows unbounded. The `streamBuffers` Map in `runShell` grows unbounded. In a 20-minute multi-agent session, memory grows linearly with no ceiling.

`DEFAULT_LIMITS.maxMessages = 1000` and `maxStreamBuffer = 1MB` are defined but completely dead. Nobody calls `trimMessages()` or `trackBuffer()`.

---

#### 9. COORDINATOR RESPONSE PARSING IS FRAGILE — SEVERITY: LOW

`parseCoordinatorResponse` in `coordinator.ts` does literal `startsWith('ROUTE:')` / `startsWith('MULTI:')` / `startsWith('DIRECT:')` matching. If the LLM response has a single leading newline, space, or markdown fence (```), all parsing fails and falls through to the "treat as direct answer" fallback. The entire routing system is one whitespace character away from never routing anything.

The `trimmed` variable handles leading/trailing whitespace, but not markdown code fences, preamble text ("Sure, I'll route that..."), or any non-exact format match.

---

#### 10. `.env` PARSING IN CLI-ENTRY IS NAIVE — SEVERITY: LOW

The `.env` parser at cli-entry.ts:17-27 doesn't handle:
- Quoted values: `KEY="value with spaces"` → value includes the quotes
- Multi-line values
- Export prefix: `export KEY=value` → key becomes `export KEY`

It's a 10-line hand-rolled parser instead of `dotenv`. Won't crash, but will silently produce wrong values.

---

#### SUMMARY TABLE

| # | Issue | Severity | Will Break In Production? |
|---|-------|----------|--------------------------|
| 1 | Nasty-inputs corpus never imported | HIGH | No coverage when it matters |
| 2 | No React ErrorBoundary | HIGH | Yes — any render throw kills the shell |
| 3 | Dead sessions never evicted from Map | HIGH | Yes — connection drop permanently bricks an agent |
| 4 | Ghost retry masks connection failures | MEDIUM | Yes — 30-min silent hang |
| 5 | Single streaming content slot for multi-agent | MEDIUM | Yes — garbled concurrent output |
| 6 | Input silently dropped during processing | MEDIUM | Yes — user input lost |
| 7 | No SIGTERM handler in shell | MEDIUM | Yes — dirty exit in containers |
| 8 | MemoryManager is dead code | LOW | Slow leak over long sessions |
| 9 | Coordinator parsing is fragile | LOW | Routing silently fails to direct answer |
| 10 | .env parser is naive | LOW | Wrong env values with quoted strings |

#### WHAT WILL ACTUALLY BREAK IN A 20-MINUTE SESSION

1. User talks to 2+ agents → one SDK hiccup → that agent is permanently dead until restart.
2. User types while agent is responding → keystrokes vanish.
3. Multi-agent routing → garbled interleaved output.
4. Any unexpected null/undefined in agent data → React crash, no recovery.

#### WHAT'S ACTUALLY GOOD

- `main().catch()` in cli-entry.ts is a proper global error boundary for the non-interactive path.
- `handleDispatch` try/catch with friendly error messages is well done.
- Ghost retry concept is sound — just needs connection-aware short-circuiting.
- Cleanup block in `runShell` is comprehensive (sessions, coordinator, client, lifecycle, telemetry).
- The hostile acceptance tests that DO run (Gherkin features) cover real scenarios well.

#### RECOMMENDATIONS (ordered by impact)

1. **Add `agentSessions.delete(agentName)` in `dispatchToAgent`'s catch path.** One line fix. Prevents permanent agent death.
2. **Add a React ErrorBoundary component** wrapping `<App>`. Shows "Something went wrong, press Enter to continue" instead of crashing.
3. **Wire up the nasty-inputs corpus** to actual test cases against `parseInput()` and `handleSubmit()`.
4. **Add SIGTERM handler** in `runShell()` that calls `exit()` on the Ink instance.
5. **Buffer user input during processing** instead of dropping it.
6. **Support per-agent streaming content** in App state (Map instead of single slot).

### 2026-02-23: Hostile Test Coverage Sprint — Issues #376, #377, #378 → PR #380

**Filed 3 GitHub issues and built all the tests. Tonight was the night.**

#### Issues Filed
- **#376** — Wire nasty-inputs corpus into actual test execution
- **#377** — SDK failure scenario tests
- **#378** — Stress and boundary tests

#### Tests Built (62 tests, 3 files, all passing)

**test/hostile-integration.test.ts** (16 tests)
- Wired the 67-string nasty-inputs corpus (previously ZERO consumers) into parseInput(), executeCommand(), and MessageStream rendering
- Every hostile string — control chars, ANSI escapes, injection attempts, 100KB strings, unicode ZWJ, RTL, CJK — run through all three paths
- Full pipeline chain: parse → execute for slash-command-shaped hostile input
- Corpus count sanity check to catch silent truncation

**test/sdk-failure-scenarios.test.ts** (21 tests)
- Ghost response: sendAndWait returns undefined/null → empty content, no crash
- SDK throws: Error, TypeError, string, partial-stream-then-throw → error captured, partial content preserved
- Timeout: sendAndWait hangs → times out cleanly with partial content preserved
- Error events: session emits error mid-stream → no unhandled exception, content preserved
- Malformed data: number/object/null/undefined deltas → filtered out, no crash
- Coordinator parsing: garbage routing input → no throw, falls through gracefully
- Session recovery: error → remove → re-register cycle works

**test/stress.test.ts** (25 tests)
- MessageStream: 500 and 1000 messages rendered without crash; maxVisible prop enforced
- Rapid input: 1000 parseInput calls, alternating command types
- Long strings: 10KB, 100KB, 1MB through parseInput; 10KB through executeCommand and MessageStream; 10000-line input
- Concurrent: 5 parallel dispatches with content isolation verification (no cross-contamination)
- Sequential: 10 rapid dispatches, each producing correct unique output
- MemoryManager: trimMessages caps, trackBuffer enforces, clearBuffer resets, canCreateSession limits
- SessionRegistry: 100 agents, 1000 status transitions, rapid activity hint updates

#### Key Findings During Testing
- Corpus has 67 strings, not 95 as previously stated (some entries were consolidated)
- All parseInput/executeCommand paths handle hostile input without throwing — the parser is actually robust
- MessageStream rendering of 67 hostile strings takes ~2.5s (Ink render overhead per unmount)
- Concurrent dispatch isolation is clean — per-session listener maps prevent cross-contamination
- MemoryManager works correctly but remains dead code in production (nobody calls it)

#### Branch & PR
- Branch: `squad/hostile-test-coverage`
- PR: #380
- Commit: `test: hostile input, SDK failure, and stress tests (closes #376, closes #377, closes #378)`
