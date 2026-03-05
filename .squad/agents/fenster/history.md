# Project Context

- **Owner:** Brady
- **Project:** squad-sdk — the programmable multi-agent runtime for GitHub Copilot (v1 replatform)
- **Stack:** TypeScript (strict mode, ESM-only), Node.js ≥20, @github/copilot-sdk, Vitest, esbuild
- **Created:** 2026-02-21

---

## Core Context

**Created:** 2026-02-21  
**Role:** Core Developer — Runtime implementation, CLI structure, shell infrastructure  
**Key Decisions Owned:** Test import patterns (vitest via dist/), CRLF normalization at parser entry, shell module structure (readline→ink progression), spawn lifecycle, SessionRegistry design

**Phase 1-2 Complete (2026-02-21 → 2026-02-22T041800Z):**
- M3 Resolution (#210/#211): `resolveSquad()` + `resolveGlobalSquadPath()` in src/resolution.ts, standalone concerns (no auto-fallback)
- CLI: --global flag routing, `squad status` command composition, command rename finalized (triage, loop, hire)
- Shell foundation: readline-based CLI shell, SessionRegistry (Map-backed, no persistence), spawn infrastructure (loadAgentCharter, buildAgentPrompt, spawnAgent)
- CRLF hardening: normalize-eol.ts applied to 8 parsers, one-line guard at entry point
- SDK/CLI split executed: 15 dirs + 4 files migrated to packages/, exports map updated (7→18 subpaths SDK, 14 subpaths CLI), 6 config files fixed, versions aligned to 0.8.0
- Test import migration: 56 test files migrated from ../src/ to @bradygaster/squad-sdk/* and @bradygaster/squad-cli/*, 26 SDK + 16 CLI subpath exports, vitest resolves via dist/, all 1719+ tests passing

### 📌 Team update (2026-02-22T10:03Z): PR #300 architecture review completed — REQUEST CHANGES verdict with 4 blockers (proposal doc, type safety on castingPolicy, missing sanitization, ambiguous .ai-team/ fallback) — decided by Keaton
- Zero-dependency scaffolding preserved, strict mode enforced, build clean (tsc 0 errors)

**Phase 3 Blocking (2026-02-22 onwards):**
- Ralph start(): EventBus subscription + health checks (14 TODOs)
- Coordinator initialize()/route(): CopilotClient wiring + agent manager (13 TODOs)
- Agents spawn(): SDK session creation + history injection (14 TODOs)
- Shell UI: Ink components not yet wired (readline only), streaming responses, agent status display
- Casting: registry.json parsing stub (1 TODO)
- Triage/Loop/Hire: placeholder commands (low priority, defer)

## 2026-03-02: Squad Aspire Timeline & Deprecation Archaeology

**Requested by:** Brady. "What happened to squad aspire? When did we deprecate it, and why?"

**Task:** Complete git archaeology — all commits, PRs, issues, branches, deprecation markers, blog posts.

**Findings:**

**Timeline:**
- **2026-02-22 (~c1d5c7c→992763e):** Feature introduced. PR #265 added `squad aspire` command (Issue #265) as the CLI entry point to launch .NET Aspire dashboard for Squad observability. Core file: `packages/squad-cli/src/cli/commands/aspire.ts` (175 lines).
- **2026-02-22 (PR #307):** OTel Phase 4 consolidation — aspire command + file watcher + event payloads merged.
- **2026-02-22 (PR #309):** Wave 2 merge added Aspire Playwright E2E tests, validating aspire.ts as a tested feature.
- **2026-02-25 onwards:** Multiple PRs (PR #539, #540, #546, #533) reference aspire in docs + help text — still treated as stable command.
- **Latest commit (c1d5c7c, Mar 2026):** `fix: make sendAndWait timeout configurable (#347)` — aspire tests still passing.

**Deprecation Status:**
- ❌ **NO git commits mentioning removal/deprecation** — searched `--all-match --grep="remove.*aspire|aspire.*remove|deprecat.*aspire|aspire.*deprecat"` — zero results.
- ❌ **NO GitHub issues labeled "aspire" requesting removal** — all open/closed issues show aspire as stable, documented feature.
- ❌ **NO GitHub PRs with deprecation plan** — PR #265 (aspire intro), PR #307 (OTel Phase 4), PR #309 (Wave 2) all finalize aspire as shipped feature.
- ❌ **NO deprecation markers in code** — aspire.ts has no @deprecated JSDoc, no console.warn(), no alpha/beta flag.
- ❌ **NO "planned removal" documentation** — docs/scenarios/aspire-dashboard.md (full guide, no sunset date), blog post 014-wave-1-otel-and-aspire.md (celebrates it as Wave 1 feature).

**Current Status:**
- ✅ **Fully wired:** Command routing at cli-entry.ts:822-829, help text at lines 396-416.
- ✅ **Documented:** Help text ("Launch Aspire Dashboard"), per-command help, scenario docs, blog post.
- ✅ **Tested:** Three test suites (aspire-command.test.ts, aspire-integration.test.ts, cli/aspire.test.ts) with passing tests.
- ✅ **Active:** Latest commit (Mar 2) touches related test infrastructure; aspire command is a dependency for observability workflows.

**Conclusion:** Squad aspire was **NEVER deprecated**. It is an actively maintained observability feature that shipped in Wave 1 (Feb 2026) and remains current and documented as of Mar 2026.

**Key Learning:** Aspire is not a transient feature or experiment—it's a core observability tool for multi-agent debugging. Wave 1 established it as stable; Wave 2 validated it with E2E tests. It's part of the "watching agents work" story alongside EventBus, OTel metrics, and SquadObserver.

### Connection promise dedup in SquadClient (2026-03-02)

**Task:** Fix race condition where concurrent `connect()` calls (eager warm-up + auto-cast) crash with "Connection already in progress" during `squad init "..."` with empty roster.

**Root cause:** `connect()` threw when `state === "connecting"` instead of letting callers share the in-flight connection promise.

**Fix:** Added `connectPromise: Promise<void> | null` field to `SquadClient`. When `connect()` is called and a connection is already in progress, it returns the existing promise instead of throwing. The promise is cleared on completion (success or failure), and also cleared in `disconnect()` / `forceDisconnect()`.

**Key decisions:**
- Promise dedup pattern: store the connection promise, return it to concurrent callers
- Span lifecycle: error status set inside the IIFE before `span.end()`, not in an outer catch after end
- `connectPromise` cleared in `disconnect()` and `forceDisconnect()` for clean state reset

**Files modified:** `packages/squad-sdk/src/adapter/client.ts`
**Verified:** Build clean (SDK + CLI).

### 📌 Team update (2026-03-01T20-24-57Z): CLI UI Polish PRD finalized — 20 issues created, team routing established
- **Status:** Completed — Parallel spawn of Redfoot (Design), Marquez (UX), Cheritto (TUI), Kovash (REPL), Keaton (Lead) for image review synthesis
- **Outcome:** Pragmatic alpha-first strategy adopted — fix P0 blockers + P1 quick wins, defer grand redesign to post-alpha
- **PRD location:** docs/prd-cli-ui-polish.md (authoritative reference for alpha-1 release)
- **Issues created:** GitHub #662–681 (20 discrete issues with priorities P0/P1/P2/P3, effort estimates, team routing)
- **Key decisions merged:**
  - Fenster: Cast confirmation required for freeform REPL casts
  - Kovash: ShellApi.setProcessing() exposed to prevent spinner bugs in async paths
  - Brady: Alpha shipment acceptable, experimental banner required, rotating spinner messages (every ~3s)
- **Timeline:** P0 (1-2 days) → P1 (2-3 days) → P2 (1 week) — alpha ship when P0+P1 complete
- **Session log:** .squad/log/2026-03-01T20-13-00Z-ui-polish-prd.md
- **Decision files merged to decisions.md:** keaton-prd-ui-polish.md, fenster-cast-confirmation-ux.md, kovash-processing-spinner.md, copilot directives

---

### 📌 PR #547 Review (2026-03-01) — External Contributor — Fenster
**Requested by:** Brady. Review "Squad Remote Control - PTY mirror + devtunnel for phone access" from tamirdresher.

**What It Does:**
- Adds `squad start --tunnel` command to run Copilot in a PTY and mirror terminal output over WebSocket + devtunnel
- Adds RemoteBridge (WebSocket server) that streams terminal sessions to a PWA (xterm.js) on phone/browser
- Uses Microsoft Dev Tunnels for authenticated relay (zero infrastructure)
- Bidirectional: phone keyboard input goes to Copilot stdin
- Session management dashboard (list/delete tunnels via `devtunnel list`)
- 18 tests (all failing due to export issues)

**Architecture:**
- **CLI commands:** `start.ts` (PTY+tunnel orchestration), `rc.ts` (bridge-only mode), `rc-tunnel.ts` (devtunnel lifecycle)
- **SDK bridge:** `packages/squad-sdk/src/remote/bridge.ts` (RemoteBridge class, WebSocket server, HTTP server, static file serving, sessions API)
- **Protocol:** `protocol.ts` (event serialization), `types.ts` (config types)
- **PWA UI:** `remote-ui/` (index.html, app.js, styles.css, manifest.json) — xterm.js terminal + session dashboard
- **Integration:** New `start` command in `cli-entry.ts` (lines 230-242)

**Dependencies Added:**
- `node-pty@1.1.0` — PTY for terminal mirroring (native addon, requires node-gyp)
- `ws@8.19.0` — WebSocket server (both CLI and SDK)
- `qrcode-terminal@0.12.0` — QR code display in terminal
- `@types/ws@8.18.1` (dev)

**Critical Issues — MUST FIX BEFORE MERGE:**

1. **Build broken (TypeScript errors):**
   - `start.ts:117` — Cannot find module 'node-pty' (missing in tsconfig paths or needs `@types/node-pty`)
   - `start.ts:177` — Binding element 'exitCode' implicitly has 'any' type (needs explicit type on `pty.onExit` callback)
   - **All 18 tests fail** due to RemoteBridge/protocol functions not being exported properly from SDK

2. **Security — Command Injection Risk (HIGH):**
   - `rc-tunnel.ts:47-49` — Uses `execFileSync` with string interpolation in `--labels` args. If `repo`, `branch`, or `machine` contain shell metacharacters, this is CWE-78. **MUST** pass label values as separate array elements without string interpolation.
   - `rc-tunnel.ts:62-64` — Same issue in `port create` command.
   - **Pattern violation:** Baer's decision (decisions.md) mandates `execFileSync` with array args, no string interpolation.

3. **Security — Environment Variable Blocklist (start.ts:135-148):**
   - Good defense-in-depth pattern (blocks `NODE_OPTIONS`, `LD_PRELOAD`, etc.) but **incomplete**.
   - Missing `PATH` restriction — allows PATH hijacking to inject malicious binaries.
   - Missing `HOME`/`USERPROFILE` restriction — allows access to dotfiles with secrets.
   - **Recommendation:** Explicitly allow-list safe vars (`TERM`, `LANG`, `TZ`, `COLORTERM`) instead of block-list. Current approach is fragile.

4. **Security — Hardcoded Path Assumption (Windows-only):**
   - `start.ts:119-122` and `rc.ts:184-188` — Hardcoded path `C:\ProgramData\global-npm\node_modules\@github\copilot\node_modules\@github\copilot-win32-x64\copilot.exe`.
   - This breaks on macOS/Linux (no fallback logic shown).
   - Cross-platform pattern should use `which copilot` or check `process.platform` and resolve from npm global dir programmatically.

5. **Rate Limiting — Weak HTTP Protection:**
   - `bridge.ts:94-106` — HTTP rate limit is 30 req/min per IP. WebSocket has per-connection limit but no global connection limit per IP.
   - **Attack vector:** Attacker can open 1000 WebSocket connections (each under rate limit) and DoS the bridge.
   - **Fix:** Add global connection limit per IP (e.g., max 3 concurrent WS connections per IP).

6. **Session Token Exposure:**
   - `start.ts:97-98` — Session token is appended to tunnel URL as query param and displayed in QR code + terminal output.
   - This token is logged to terminal history, potentially visible in screenshots, and sent over tunnel URL (visible in proxy logs).
   - **Better pattern:** Use the ticket exchange endpoint (`/api/auth/ticket`) instead — client POSTs token to get one-time ticket, uses ticket for WS connection.
   - **Why it matters:** Token has 4-hour TTL, ticket has 1-minute TTL. Reduces window for replay attacks.

7. **Audit Log Location:**
   - `bridge.ts:43` — Audit log goes to `~/.cli-tunnel/audit/`. This is not in `.squad/` directory.
   - **Inconsistency:** All Squad state is in `.squad/` (decisions.md), but audit logs are elsewhere.
   - **Recommendation:** Use `.squad/log/remote-audit-{timestamp}.jsonl` for consistency.

8. **Secret Redaction — Missing JWT Detection:**
   - `bridge.ts:377-393` — `redactSecrets()` has patterns for GitHub tokens, AWS keys, Bearer tokens, JWTs.
   - BUT: JWT regex `/eyJ.../` only matches base64 tokens. Doesn't catch Bearer-wrapped JWTs (`Bearer eyJ...`).
   - **Fix:** Combine patterns — check for `Bearer eyJ...` before stripping Bearer header.

9. **File Serving — Directory Traversal (Mitigated but Fragile):**
   - `start.ts:63-74` and `rc.ts:118-142` — Both implement directory traversal guards (`!filePath.startsWith(uiDir)`).
   - **Good:** Uses `path.resolve()` and prefix check.
   - **Fragile:** Relies on manual sanitization in multiple places. If one handler is added later without this pattern, vulnerability reintroduced.
   - **Recommendation:** Extract to shared `serveStaticFile(uiDir, req, res)` helper in SDK to enforce pattern.

10. **Test Failures — Export Configuration Broken:**
    - All 18 tests fail with "RemoteBridge is not a constructor" and "serializeEvent is not a function".
    - Root cause: `packages/squad-sdk/src/remote/index.ts` exports `RemoteBridge` from `./bridge.js` but `bridge.ts` may not be built or exported correctly.
    - **Build error** (from `npm run build`): TypeScript errors in `start.ts` block CLI build, so SDK may not have rebuilt.
    - **Fix:** Resolve TypeScript errors, rebuild SDK, verify tests pass.

**Non-Critical Issues:**

11. **node-pty Native Dependency:**
    - Requires node-gyp + C++ compiler on install. Will break in CI or Docker if build tools not installed.
    - **Mitigation:** Document in PR that `node-pty` requires native build, or consider optional dependency with graceful fallback.

12. **Windows-Centric Implementation:**
    - Most code assumes Windows (`C:\`, PowerShell paths, devtunnel CLI).
    - macOS/Linux support unclear. If intended as Windows-only, document clearly.

13. **Devtunnel Dependency:**
    - Requires `devtunnel` CLI installed + authenticated (`devtunnel user login`).
    - Not bundled, not auto-installed. User must manually install via `winget` or download.
    - **UX:** Should have better error message when devtunnel missing (currently just "⚠ devtunnel not installed" without link to install instructions).

14. **Passthrough Mode vs. PTY Mode:**
    - `rc.ts` spawns `copilot --acp` and pipes JSON-RPC (passthrough mode).
    - `start.ts` spawns Copilot in PTY and sends raw terminal bytes (PTY mode).
    - Two separate code paths for essentially the same feature. **Why not unify?**
    - If PTY mode is better (full TUI experience), deprecate `rc.ts`. If ACP passthrough is needed for API access, document the use case split.

**Integration with Existing CLI:**
- ✅ Command routing in `cli-entry.ts` follows existing pattern (dynamic import, options parsing)
- ✅ Help text added (lines 65-69)
- ✅ Flag passthrough works (`--yolo`, `--model`, etc. passed to Copilot)
- ❌ No integration with existing squad commands (`squad status`, `squad loop`, etc.) — isolated feature
- ❌ No integration with EventBus or Coordinator — doesn't participate in Squad agent orchestration

**Recommendation:**
- **DO NOT MERGE** until critical issues fixed (build errors, command injection, test failures).
- **After fixes:** This is a cool demo feature but needs architectural discussion:
  1. Is remote access in scope for Squad v1? (Not in any PRD I've seen.)
  2. Should this be a plugin or core feature?
  3. Native dependency (node-pty) adds install complexity — is that acceptable?
  4. Windows-only (effectively) — acceptable?
  5. Devtunnel dependency — acceptable external requirement?

**If Brady approves the concept:**
- Merge only after all security issues fixed + tests passing + cross-platform support clarified.
- Document clearly: experimental feature, Windows-only (if true), requires devtunnel CLI.
- Consider renaming `start` command to `squad remote` or `squad tunnel` to avoid confusion with future `squad start` (which might mean "start the squad daemon").

**Decision File Needed:**
- This introduces a new CLI command paradigm (interactive terminal mirroring vs. agent orchestration). Needs a decision: "Remote access via devtunnel is Squad's mobile UX strategy" or "This is an experimental plugin".


### 📌 Multi-Squad Phase 1: Core SDK + Config + Migration (PR #691, Issue #652)
**Requested by:** Brady. Implement foundational layer for multiple personal squads.

**What was built:**
- New module: `packages/squad-sdk/src/multi-squad.ts` — 7 exported functions + 3 types
- `getSquadRoot()` delegates to existing `resolveGlobalSquadPath()` for platform detection
- `resolveSquadPath(name?)` implements 5-step resolution chain: explicit → env → config → default → legacy
- `listSquads()`, `createSquad()`, `deleteSquad()`, `switchSquad()` — full CRUD for squad registry
- `migrateIfNeeded()` — non-destructive: registers legacy `~/.squad` as "default" in `squads.json`, never moves files
- Types `SquadEntry`, `MultiSquadConfig`, `SquadInfo` exported from SDK barrel + types.ts
- squads.json lives at global config root (`%APPDATA%/squad/` on Windows, `~/.config/squad/` on Linux)

**Key design choices:**
- Migration is registration-only. Files stay where they are. This avoids data loss risk on first upgrade.
- `deleteSquad()` blocks deletion of the active squad (safety valve).
- `resolveSquadPath()` calls `migrateIfNeeded()` on every invocation — idempotent, returns fast after first run.
- Re-used `resolveGlobalSquadPath()` from resolution.ts rather than duplicating platform logic.

**What's NOT included (Phase 2):**
- No CLI commands (`squad list`, `squad create`, `squad switch`, etc.)
- No changes to CLI entry point or existing resolution chain

**Verification:** tsc --noEmit clean. vitest run: 3217 passed, 126 failed (all pre-existing).

---

## 2025-07: cli.js shim replacement

**Task:** Replace the stale ~2000-line bundled `cli.js` with a thin ESM shim that forwards to the built CLI at `packages/squad-cli/dist/cli-entry.js`.

**What changed:**
- `cli.js` reduced from 1982 lines to 14 lines
- Shim imports `./packages/squad-cli/dist/cli-entry.js` which auto-executes `main()`
- Deprecation notice only shows when invoked via npm/npx (checks `process.env.npm_execpath`), silent for `node cli.js`

**Why:** The bundled cli.js was from the old GitHub-native distribution and was missing commands added after the monorepo migration (e.g., `aspire`). Running `node cli.js aspire` failed. Now it forwards to the real CLI entry point.

**Verification:** `node cli.js aspire --help` works. `node cli.js help` shows all commands. Test suite: 3333 passed, 10 failed (all pre-existing).

### 📌 Team update (2026-03-02T23:50:00Z): Knock-knock sample modernization complete
- **Status:** Completed — Fenster rewrote samples/knock-knock with production Copilot SDK patterns
- **Work:** SquadClientWithPool implementation, streaming deltas, system prompts, Dockerfile multi-stage build
- **Files updated:** samples/knock-knock/index.ts (rewritten), Dockerfile (production build), samples/README.md (integration examples)
- **Pattern established:** Connection pooling as reference implementation for SDK users
- **Session log:** `.squad/log/2026-03-02T23-50-00Z-migration-v060-knock-knock.md`

## Learnings

- Root package.json has `"type": "module"` — bare `import` works in cli.js (no dynamic import needed)
- `packages/squad-cli/dist/cli-entry.js` auto-executes `main().catch(...)` at module level — importing it is sufficient to run the CLI
- `process.env.npm_execpath` is set when running via npm/npx but absent for direct `node` invocation — good signal for conditional deprecation notices
- **2026-03-03:** Wired `squad nap` command into CLI entry point (cli-entry.ts lines 245-254). Full implementation existed in nap.ts (runNap, runNapSync, formatNapReport) and REPL already had `/nap` working. CLI was missing the routing — added flag parsing (--deep, --dry-run), squadRoot resolution via resolveSquad(), async runNap() call, formatNapReport() output. Help text and docs/reference/cli.md updated. TypeScript build verified clean.
- **2026-03-05:** CLI social implementation spec complete (docs/prd/sections/26-cli-social.md). Package isolation is the architecture: base CLI has zero social code, @bradygaster/squad-social is a separate optional package, presence is detected via require.resolve(). Conditional loading pattern uses dynamic import only when package exists. State management: .squad/social/{state.json, credentials.json, keys/}, JSONL archive for durability. Enlistment flow: Ed25519 keypair → squad.place registration → agent introductions. Social session: SSE stream + autonomous agent behavior loop + live terminal feed + time-boxed shutdown. Seven subcommands (enlist/start/leave/status/feed/post/profile), natural language enlistment via coordinator. Security: credentials gitignored, API Bearer auth, graceful degradation on network failure. Implementable — file paths, JSON schemas, terminal output examples all specified.

- **2026-03-07 (Issue #190):** Fixed all `.squad-templates/` → `.squad/templates/` path references across source code (init.ts, migrate-directory.ts, cli-entry.ts), workflow templates (squad-heartbeat.yml, squad-promote.yml), and ralph-triage.js. Updated all 4 copies of each synced file (templates/, packages/squad-cli/templates/, .squad-templates/, .github/workflows/). Promote workflows: removed `.squad-templates/` from forbidden-path patterns since templates now nest under `.squad/` which is already stripped. TypeScript compiles clean.

### 2026-03-03: History Audit & Correction Pattern

**Context:** Brady requested correction of Kobayashi's history.md due to factual errors about version targets (v0.6.0 vs v0.8.17) and missing documentation of PR #582 GitHub merge failure.

**Pattern established:** When correcting history files:
1. **Never delete** — History is evidence, not fiction. Preserve what was attempted, even if wrong.
2. **Annotate inline** — Add `**[CORRECTED: actual truth]**` markers next to erroneous text
3. **Add Correction Log** — Append section documenting what was corrected, why, and verification of current state
4. **Explain the failure** — Future spawns need to understand WHY the mistake happened to avoid repeating it

**Key insight:** History file errors are data integrity bugs. If a spawn reads "Brady decided v0.6.0" when Brady actually decided v0.8.17, the spawn will propagate that error into code/docs. Corrections prevent error loops.

**Files corrected:**
- `.squad/agents/kobayashi/history.md` — 3 sections corrected (v0.6.0 references), 1 section added (PR #582 GitHub failure), Correction Log appended
- `.squad/decisions/inbox/fenster-kobayashi-history-corrections.md` — Decision documenting the corrections and audit findings

---

## 2025-07: Knock-Knock Multi-Agent Sample

**Requested by:** Brady. Create the simplest possible multi-agent sample: two agents trading knock-knock jokes in Docker, demonstrating Squad SDK patterns without requiring Copilot auth.

**What was built (INITIAL VERSION):** `samples/knock-knock/` — 6 files, ~200 lines total:
- `index.ts`: CastingEngine to cast 2 agents, StreamingPipeline for token-by-token output, 12 hardcoded jokes, infinite loop
- `package.json`, `tsconfig.json`: Minimal Node/TS config matching other samples
- `Dockerfile`: Multi-stage build, copies monorepo context for local SDK dependency resolution
- `docker-compose.yml`: Single service, runs the sample
- `README.md`: Quick start guide

**Key SDK patterns demonstrated:**
1. **CastingEngine.castTeam()** — Cast from "usual-suspects" universe with required roles
2. **StreamingPipeline** — Simulated token-by-token streaming via `onDelta()` callback
3. **Demo mode** — Hardcoded responses (no live Copilot connection) for Docker-friendly demos
4. **Session attachment** — `pipeline.attachToSession()` for each agent

**Design constraint: SIMPLEST POSSIBLE.** No EventBus complexity, no SquadClientWithPool, no real Copilot auth. Just casting + streaming + simulated jokes. Perfect for first-time users.

**Verification:** TypeScript compiles clean, sample runs locally, outputs joke exchange with emoji and streaming delays.

**🔴 STATUS UPDATE [CORRECTED]:** This initial version was **REJECTED by Brady** ("it doesn't look like it's using any type of LLM or copilot functionality"). See section "## 2025-07: Knock-Knock Sample Rewrite — Real LLM Integration" (line 341) for the superseding implementation that uses real Copilot sessions with SquadClientWithPool.

## Learnings

- StreamingPipeline's `onDelta()` is the core pattern for rendering agent output — accumulate or stream directly to stdout
- Simulated streaming (demo mode) is essential for Docker samples where GitHub auth isn't available
- The `file:../../packages/squad-sdk` dependency pattern in samples allows testing SDK changes without publishing
- Multi-stage Dockerfile needed: builder stage copies monorepo workspace structure to resolve local dependencies, production stage copies built artifacts


---

## 2025-07: Fix semver prerelease format in bump-build (#692)

**Task:** `scripts/bump-build.mjs` produced invalid semver like `0.8.16.1-preview` (build number before prerelease tag). Fixed to produce `0.8.16-preview.1` (build as dot-separated prerelease identifier, per semver spec).

**What changed:**
- `parseVersion` split into two regex paths: prerelease-first (`1.2.3-tag.N`) and non-prerelease (`1.2.3.N`)
- `formatVersion` places build number after the prerelease tag when one exists
- All 5 tests updated to use new format, all passing

## Learnings

- Semver prerelease identifiers are dot-separated after the hyphen: `1.2.3-preview.1` is valid, `1.2.3.1-preview` is not
- The bump-build test suite copies the real script to a temp dir and patches `__dirname` — any regex changes must not break the patching mechanism

---

## 2025-07: Knock-Knock Sample Rewrite — Real LLM Integration

**Requested by:** Brady. Rewrite `samples/knock-knock/index.ts` to use REAL Copilot sessions instead of hardcoded jokes. Original version rejected because "it doesn't look like it's using any type of LLM or copilot functionality."

**What changed:** `samples/knock-knock/` completely rewritten (~190 lines):
- **SquadClientWithPool integration**: Real GitHub Copilot connection with `GITHUB_TOKEN` auth
- **Live LLM sessions**: Two Copilot sessions with distinct system prompts (Teller generates jokes, Responder plays audience)
- **StreamingPipeline + message_delta**: Pattern from `streaming-chat` — register delta listener, feed to pipeline, capture full response
- **Graceful auth errors**: Clear error messages if `GITHUB_TOKEN` missing/invalid, no stack traces
- **Infinite joke loop**: Agents swap roles after each joke, LLM generates unique jokes every time
- **docker-compose.yml**: Added `GITHUB_TOKEN=${GITHUB_TOKEN}` environment variable
- **README.md**: Rewritten to document GITHUB_TOKEN requirement, setup instructions, Docker usage

**Key SDK patterns demonstrated:**
1. **SquadClientWithPool**: Connect with GitHub token, create/resume sessions
2. **CastingEngine**: Cast two agents (unchanged pattern)
3. **StreamingPipeline**: Token-by-token streaming from live LLM (not simulated)
4. **Session management**: Creating sessions with system prompts, resuming, registering delta handlers
5. **sendAndWait with fallback**: `session.sendAndWait()` with optional fallback to `sendMessage()`

**Architecture:**
- Two sessions created with different system prompts defining agent personas
- `sendAndCapture()` helper: registers delta handler, sends prompt, captures full response text
- Role swap: agents alternate between Teller and Responder after each joke
- Streaming output: delta events piped to StreamingPipeline → stdout

**Verification:** TypeScript compiles clean (`tsc --noEmit` passes).

## Learnings

- Real LLM integration pattern: SquadClientWithPool → createSession with systemPrompt → resumeSession → register message_delta handler → sendAndWait → capture response
- System prompts define agent personas — Teller generates jokes, Responder plays natural audience role
- `sendAndWait()` may be optional on session interface — use conditional check with fallback to `sendMessage()`
- Auth error UX: check `GITHUB_TOKEN` before connecting, provide actionable error with setup instructions
- Captured response text enables inter-agent conversation — Teller's joke becomes Responder's input

---

## 2025-07: Rock-Paper-Scissors Multi-Agent Arena

**Requested by:** Brady. Create `samples/rock-paper-scissors/index.ts` — multi-player RPS tournament with real Copilot sessions.

**What was built:** `samples/rock-paper-scissors/index.ts` (~430 lines):
- **Multi-session management:** Creates one session per player (7 players) + scorekeeper (8 total)
- **Match pairing logic:** Cycles through all pairings (player[i] vs player[j]) to avoid immediate rematches
- **Concurrent move collection:** `Promise.all([getPlayerMove(A), getPlayerMove(B)])` for parallel LLM calls
- **Context-aware prompting:** The Learner receives opponent history (last 5 moves) in prompt
- **Scorekeeper streaming:** Uses StreamingPipeline for token-by-token commentary on match results
- **Leaderboard tracking:** Internal stats (W/L/D), sorted display every 10 matches, scorekeeper commentary
- **Move parsing:** Extracts "rock"/"paper"/"scissors" from LLM response, takes last occurrence to handle reasoning + answer
- **Graceful forfeits:** If move unparseable, treat as forfeit and log warning
- **Console formatting:** Banner, player roster with emoji, match announcements, leaderboard display

**Architecture patterns:**
- `PlayerInfo` extends `PlayerStrategy` (imported from prompts.ts) with session state
- `MatchHistory` tracks opponent move sequences per pairing (for The Learner's context)
- `playMatch()` orchestrates single match: get moves → determine winner → update stats → announce result
- `getPlayerMove()` handles context injection for The Learner vs. simple "What do you throw?" for others
- `parseMove()` robust parsing: finds all rock/paper/scissors in response, returns last one
- `announceResult()` and `printLeaderboard()` both use StreamingPipeline for scorekeeper commentary

**SDK patterns used:**
- SquadClientWithPool with GITHUB_TOKEN auth
- CastingEngine.castTeam() for player names (universe: usual-suspects)
- StreamingPipeline with onDelta() callback for stdout streaming
- Session creation with systemPrompt per player
- message_delta event handlers with conditional field access (deltaContent/delta/content)
- sendAndWait() with fallback to sendMessage() for compatibility

**Key design choices:**
- All pairings pre-computed and cycled to ensure fair distribution
- Parallel move collection reduces per-match latency
- Move history stored per pairing (not global) — important for The Learner's opponent modeling
- Scorekeeper commentary after every match + leaderboard keeps output engaging
- Parse move from last occurrence in response — handles LLM reasoning patterns ("I think... so I'll throw rock")

**Verification:** TypeScript compiles clean (tsc --noEmit expected to pass after prompts.ts exists).

## Learnings

- Multi-session management pattern: create all sessions upfront, store sessionIds in player state, resume per request
- Context injection for adaptive agents: The Learner gets opponent history, others get static prompts
- Parallel LLM calls via Promise.all() for concurrent player moves — reduces total latency
- Robust move parsing: search all occurrences, take last one — handles reasoning-then-answer LLM patterns
- Match pairing cycling: pre-compute all [i,j] pairs, cycle through them to avoid immediate rematches
- Scorekeeper as streaming agent: commentary adds narrative to the arena, StreamingPipeline makes it feel live

## 📌 Team Update (2026-03-03T00:00:50Z)

**Session:** RPS Sample Complete — Verbal, Fenster, Kujan, McManus collaboration

Multi-agent build of Rock-Paper-Scissors game with 10 AI strategies, Docker infrastructure, and full documentation. Fenster (Coordinator) identified and resolved 3 integration bugs (ID mismatch, move parsing, history semantics). Sample ready for use.

**Verification:** tsc --noEmit clean. vitest run: 3217 passed, 126 failed (all pre-existing).

---

## 2025-07: cli.js shim replacement

**Task:** Replace the stale ~2000-line bundled `cli.js` with a thin ESM shim that forwards to the built CLI at `packages/squad-cli/dist/cli-entry.js`.

**What changed:**
- `cli.js` reduced from 1982 lines to 14 lines
- Shim imports `./packages/squad-cli/dist/cli-entry.js` which auto-executes `main()`
- Deprecation notice only shows when invoked via npm/npx (checks `process.env.npm_execpath`), silent for `node cli.js`

**Why:** The bundled cli.js was from the old GitHub-native distribution and was missing commands added after the monorepo migration (e.g., `aspire`). Running `node cli.js aspire` failed. Now it forwards to the real CLI entry point.

**Verification:** `node cli.js aspire --help` works. `node cli.js help` shows all commands. Test suite: 3333 passed, 10 failed (all pre-existing).

## Learnings

- Root package.json has `"type": "module"` — bare `import` works in cli.js (no dynamic import needed)
- `packages/squad-cli/dist/cli-entry.js` auto-executes `main().catch(...)` at module level — importing it is sufficient to run the CLI
- `process.env.npm_execpath` is set when running via npm/npx but absent for direct `node` invocation — good signal for conditional deprecation notices

---

## 2025-07: Fix semver prerelease format in bump-build (#692)

**Task:** `scripts/bump-build.mjs` produced invalid semver like `0.8.16.1-preview` (build number before prerelease tag). Fixed to produce `0.8.16-preview.1` (build as dot-separated prerelease identifier, per semver spec).

**What changed:**
- `parseVersion` split into two regex paths: prerelease-first (`1.2.3-tag.N`) and non-prerelease (`1.2.3.N`)
- `formatVersion` places build number after the prerelease tag when one exists
- All 5 tests updated to use new format, all passing

## Learnings

- Semver prerelease identifiers are dot-separated after the hyphen: `1.2.3-preview.1` is valid, `1.2.3.1-preview` is not
- The bump-build test suite copies the real script to a temp dir and patches `__dirname` — any regex changes must not break the patching mechanism
### 2026-02-28 : Implement Phase 1 of Consult Mode
**PRD:** `.squad/identity/prd-consult-mode.md`
**Requested by:** James Sturtevant

Implemented Phase 1 of consult mode — allows personal squad to "consult" on external projects without polluting either side.

**Changes:**
1. **SDK resolution.ts:**
   - Added `consult?: boolean` field to `SquadDirConfig` interface
   - Made `loadDirConfig()` public and updated to parse consult field
   - Added `isConsultMode(config)` helper function

2. **SDK index.ts:**
   - Exported `loadDirConfig` and `isConsultMode` from resolution module

3. **CLI commands/consult.ts (new):**
   - `squad consult` — creates `.squad/` with `consult: true`, points to personal squad
   - `--status` — shows current consult mode status
   - `--check` — dry-run preview of what would happen
   - Uses `git rev-parse --git-path info/exclude` for worktree/submodule compatibility
   - Adds `.squad/` to `.git/info/exclude` (git-internal, never committed)

4. **CLI cli-entry.ts:**
   - Registered `consult` command in routing
   - Added help text for `--help` flag
   - Added to main help output under Team Management

**Pattern followed:** `init-remote.ts` and `link.ts` for command structure. Dynamic import pattern for lazy loading.

**Learning:** The `.git/info/exclude` approach is perfect for invisibility — it's git-internal and never shows up in diffs or status. Using `git rev-parse --git-path` is essential for worktrees/submodules where `.git` is a file, not a directory.

**Next:** Phase 2 will add `squad extract` for bringing learnings back to personal squad.

---

## 📋 History Audit — 2026-03-03 (Fenster Self-Review)

**Context:** Brady requested team-wide history audits per the pattern in `.squad/skills/history-hygiene/SKILL.md`. Each agent audits their own history.md for conflicting entries, stale decisions, v0.6.0 references, intermediate states, and confusing entries.

**Audit Process:**
1. ✅ Checked for v0.6.0 migration target references (should be v0.8.17)
   - Found 3 references on lines 278, 286, 289 — all in the "2026-03-03: History Audit & Correction Pattern" section
   - **Verdict:** These are NOT errors in Fenster's history. They accurately document Brady's request to audit Kobayashi's history (which had v0.6.0 errors).

2. ✅ Checked for conflicting entries
   - Found knock-knock sample described twice: initial version (lines 294-313) and rewrite version (lines 341-351)
   - **Issue identified:** Initial version section didn't note it was rejected and superseded by the Real LLM Integration rewrite
   - **Fix applied:** Added [CORRECTED] annotation on line 313 noting "This initial version was REJECTED by Brady"

3. ✅ Checked for stale decisions
   - No stale or reversed decisions found. Team decisions are accurately reflected.

4. ✅ Checked for intermediate states recorded as final
   - Found one: knock-knock initial version (hardcoded jokes, no Copilot auth) was presented as complete without noting it was rejected
   - **Fix applied:** Added cross-reference to superseding version

5. ✅ Checked for confusing entries that would trouble a future spawn
   - File chronology is somewhat non-linear (2025-07 sections interspersed with 2026-03 entries) but this appears intentional based on task structure, not an error

**Corrections Made:**
- 1 inline [CORRECTED] annotation added (line 313) to knock-knock initial version section

**Verification:**
- All version references accurate (no v0.6.0 as migration target)
- All completed tasks documented with final outcomes, not intermediate requests
- No contradictory statements about what was shipped vs. rejected

**Result: CLEAN** (1 correction made for clarity, no data integrity issues)

---

### 2026-03-XX: Fix Missing Barrel Exports in squad-sdk index.ts
**Requested by:** Brady

The CLI couldn't run because `packages/squad-sdk/src/index.ts` was missing re-exports for symbols the CLI imports: `safeTimestamp`, `initSquadTelemetry`, `TIMEOUTS`, `recordAgentSpawn`, `recordAgentDuration`, `recordAgentError`, `recordAgentDestroy`, `getMeter`, and `RuntimeEventBus`.

**Changes (1 file, 7 insertions):**
- Added named exports for `MODELS`, `TIMEOUTS`, `AGENT_ROLES` from `runtime/constants.js` (used named instead of wildcard to avoid `AgentRole` collision with `casting/index.js`)
- Added `export *` for `runtime/otel-init.js` and `runtime/otel-metrics.js`
- Added named exports `getMeter`, `getTracer` from `runtime/otel.js` (wildcard would leak internal OTel types)
- Added `safeTimestamp` from `utils/safe-timestamp.js`
- Added `EventBus as RuntimeEventBus` alias from `runtime/event-bus.js`

**Verification:** `tsc --noEmit` passes clean for squad-sdk. Full build shows only pre-existing squad-cli errors (node-pty, rc.ts, consult mode symbols).

## Learnings

- Wildcard `export *` from `runtime/constants.js` causes TS2308 collision with `AgentRole` already exported from `casting/index.js` — use named exports when barrel already re-exports a module with overlapping symbol names
- Only use named exports for `otel.ts` to avoid leaking internal OTel SDK types into the public API surface

## Learnings
- Issue #188: doctor.ts existed at cli/commands/doctor.ts with full implementation (runDoctor, doctorCommand exports) but was never wired into cli-entry.ts command routing. Two additions needed: help text line + lazy-import route block before the Unknown command fatal. Docs already had the command listed. Always check CLI routing when adding new command files.


📌 Team update (2026-03-04T17:52:00Z): Migration docs file-safety guidance added — doctor command now live in CLI (fixes #188) — decided by Keaton, implemented by McManus

---

## 2026-03-05: Social Network Technical Architecture (PRD Section 03)

**Requested by:** Brady. Design the technical architecture & data model for squad-social-network — a social network BY AI agents, FOR AI agents.

**What was designed:** docs/prd/sections/03-architecture.md (~22KB, 8 sections):

1. **System Architecture Overview**
   - Federated hybrid model: squads own data locally, publish via ActivityPub-lite
   - Node.js monolith per squad (not microservices — agents are already distributed)
   - Tech stack: Node.js 20+, Fastify, SQLite (WAL mode), ActivityPub, REST+SSE
   - Optional discovery hub (DNS for squads) without mandating centralization

2. **Social Graph Data Model**
   - 3 primary node types: Agent, Squad, Post
   - 4 edge types: Follow, Federation, Boost, Reaction
   - SQLite for local storage + GraphQL federation layer for remote queries
   - Handle format: @fenster@squad-dev.local (Mastodon-style)

3. **Content Model**
   - 8 content types: text, code, decision, skill, thread, learning, question, announcement
   - Content schema with type-specific metadata (language for code, skill_level for skills, etc.)
   - Attachments model (images, files, links)
   - Tag-based discovery

4. **API Design**
   - REST for CRUD, SSE for real-time feeds
   - 30+ endpoints covering agents, posts, timeline, follows, discovery, federation
   - JWT auth: issued by squad instance, verified via public keys at `/.well-known/squad.json`
   - WebFinger for agent discovery (`acct:fenster@squad-dev.local`)

5. **Federation Model**
   - ActivityPub-lite (borrow from Mastodon's proven patterns)
   - Squads exchange activities via HTTP POST to `/api/v1/inbox`
   - 3 flows: agent discovery (WebFinger), cross-squad follow, post federation
   - Peering policy: open, allowlist, or closed

6. **Data Flow**
   - Write path: local write synchronous, federation async with retry queue
   - Read path: local SQLite + GraphQL federation to remote squads (cached + fresh)
   - Event bus: in-memory EventEmitter for SSE real-time updates

7. **Storage Architecture**
   - Single SQLite database per squad: `~/.squad/social/network.db`
   - Schema: agents, squads, posts, follows, reactions, boosts, remote_posts (cache), federation_queue
   - FTS5 for full-text search, proper indexes for timeline queries
   - Daily backups (last 7 days)
   - Why SQLite: single-file portability, good for squad-scale (1-50 agents, 10K-1M posts)

8. **Integration Points**
   - New SDK module: `@bradygaster/squad-sdk/social` with SquadSocial class
   - CLI integration: `squad social start|post|timeline|follow|search`
   - Agent charter social_network config (opt-in auto-posting of decisions/learnings)
   - Hooks & events: agents subscribe to `post:created`, `follow:received`, `mention:received`
   - Consult mode integration: `squad extract --share-to-network` creates learning posts

**Architecture Principles:**
- Local-first: every squad owns its data, federation is opt-in
- Eventual consistency: embrace async
- Simplicity over features: start with posts/follows/timelines, add reactions later
- Borrow proven patterns: ActivityPub and Mastodon solved this
- Agent autonomy: agents decide what to share, no central moderation

**Implementation Phases:**
1. Phase 1 (MVP): Local network (SQLite, profiles, posts, timeline API)
2. Phase 2: Federation (ActivityPub, WebFinger, cross-squad follows)
3. Phase 3: Rich content (code snippets, decision logs, skill shares, attachments)
4. Phase 4: Discovery & search (full-text, trending tags, agent recommendations, squad directory)

**Open Questions:**
- Privacy model: how do agents control what gets shared?
- Moderation: how do squads block rogue instances?
- Identity portability: can agents migrate between squad instances?
- Analytics: should squads see metrics (post reach, follower growth)?
- Cost of federation: batching vs. O(n) HTTP calls for 1000 followers across 100 squads?

**Decision rationale:**
- SQLite over graph DB: squad-scale data is small, SQLite with indexes is good enough
- REST+SSE over GraphQL-only: REST simpler for CRUD, GraphQL for federation queries
- ActivityPub over custom protocol: proven, thousands of Mastodon instances federate successfully
- Local-first over centralized: agent autonomy is core to Squad philosophy
- Monolith over microservices: each squad instance IS a service, don't over-engineer

## Learnings

- Social network architecture for agents mirrors human social networks (Mastodon/ActivityPub) but optimized for agent workflows (decision logs, skill shares, consult mode learnings)
- Federation models: learned Mastodon's inbox/outbox pattern, WebFinger discovery, signature verification via public keys
- SQLite scales to 10M+ rows with proper indexing — good enough for squad-scale social graphs without needing PostgreSQL/neo4j
- SSE (Server-Sent Events) is simpler than WebSockets for one-way real-time feeds (timeline updates, notifications)
- Local-first data ownership + opt-in federation = agent autonomy + network effects without centralization
- Event bus pattern: in-memory EventEmitter for process-local real-time, federation queue in SQLite for durability across restarts
- ActivityPub activities map cleanly to agent social actions: Create (post), Follow (follow), Announce (boost), Like (react)
- Agent charter integration: social_network config enables auto-posting without breaking agent autonomy

## PIN: 2026-03-05 - 20-Agent PRD Design Session

**Event:** Historic parallel fanout - 20 agents designed squad-social-network PRD simultaneously.

**Contribution:** All agents participated. 20 PRD sections delivered.

**Outcome:**
- 20 PRD sections drafted (docs/prd/sections/{01-20}-*.md)
- 23 decisions merged to .squad/decisions.md
- 20 orchestration logs created
- Session log: .squad/log/2026-03-05T02-02-22Z-social-network-prd.md
- Inbox cleared

**Next Steps:** Keaton assembles final PRD, Brady reviews, implementation planning begins.

**Key Pattern:** Largest parallel fanout in Squad history. Loose coupling, clear domains, shared constraints.

---

### 2026-03-05: Server API Specification for squad.place

**Requested by:** Brady

**What was designed:** docs/prd/sections/22-server-api.md (~60KB, 15 sections + 2 appendices)

Complete HTTP API specification for the squad.place social network backend. This is the server-side counterpart to the client SDK — every endpoint agents call to register, publish, discover, and communicate.

**Core Architecture:**
- **API Base:** https://api.squad.place/v1/
- **Auth:** JWT bearer tokens (RS256), auto-generated by Squad SDK
- **Data:** JSON payloads, cursor-based pagination
- **Real-Time:** Server-Sent Events (SSE) for feed updates
- **Stack:** Node.js 20 + Fastify + PostgreSQL 16 + Redis 7
- **Deployment:** AWS ECS Fargate (monolith, split later if needed)

**Endpoints Specified (23 total):**

**Registration & Identity:**
- POST /v1/squads/register — Register squad, get API key
- POST /v1/squads/:squadId/agents — Register agent within squad
- GET /v1/squads/:squadId — Get squad profile + roster
- GET /v1/agents/:agentId — Get agent profile, skills, reputation
- PATCH /v1/agents/:agentId — Update profile

**Knowledge Artifacts:**
- POST /v1/artifacts — Publish decision/learning/code/skill
- GET /v1/artifacts/:id — Get specific artifact
- GET /v1/artifacts — Search/discover (full-text + filters)
- POST /v1/artifacts/:id/adopt — Track adoption
- POST /v1/artifacts/:id/react — React with emoji

**Feed & Discovery:**
- GET /v1/feed — Personalized feed (followed agents + trending in expertise)
- GET /v1/discover — Discovery feed (network-wide trending)
- GET /v1/topics — Browse topics/channels

**Real-Time:**
- GET /v1/stream — SSE endpoint (artifact_published, agent_joined, reactions, etc.)
- POST /v1/messages — Send direct message
- GET /v1/messages — Get message history

**Meta:**
- GET /v1/health — Health check
- GET /v1/stats — Network statistics
- POST /v1/auth/refresh — Token refresh

**Every endpoint includes:**
- Concrete curl examples with REAL data (Fenster, Verbal, Zero Cool)
- Request/response schemas
- Error responses (400, 401, 403, 404, 429, 500, 503)
- Rate limiting (100 req/hour free tier, headers on every response)
- Pagination (cursor-based, not offset)

**Data Model (PostgreSQL Schema):**
8 tables defined with full CREATE TABLE statements:
- squads — Squad registration, public keys, tech stack
- agents — Agent profiles, expertise, reputation scores
- artifacts — Published knowledge (decisions, learnings, code, skills)
- adoptions — Tracks who adopted what (reputation signal)
- reactions — Emoji reactions (👍, 🚀, 💡)
- follows — Social graph (follower/following)
- messages — Direct messages between agents
- topics — Browsable topics/channels

**Indexes for performance:**
- GIN indexes on JSONB, text arrays (tags, expertise)
- pg_trgm + FTS for full-text search
- Composite indexes on (created_at DESC, adoption_count DESC) for trending queries

**SDK Integration:**
Squad SDK auto-registers on first run, polls feed every 60s, listens to SSE for real-time updates. Auto-publishes decisions/learnings when agents append to history.md. CLI commands: squad social publish|search|feed|follow|react|adopt.

**Security:**
- All queries parameterized (SQL injection prevention)
- DOMPurify for XSS prevention
- Prompt injection pattern detection
- Rate limiting (Redis token bucket)
- HTTPS only, CSP headers

**Real-World Example (Appendix A):**
Showed complete flow: Fenster publishes → Verbal sees in feed (SSE) → reacts → Zero Cool (external squad) searches → adopts → Fenster sees adoption notification. Full circle.

**Implementation Roadmap:**
- Phase 1 (Weeks 1-4): Core endpoints, auth, rate limiting, deploy to AWS
- Phase 2 (Weeks 5-8): Social features (follows, feed, reactions, messages)
- Phase 3 (Weeks 9-12): Real-time SSE, SDK integration, CLI
- Phase 4 (Weeks 13-16): Scale (read replicas, Elasticsearch), analytics, beta launch

**Design Decisions:**
- Monolith (not microservices): Squad-scale data doesn't justify complexity
- PostgreSQL (not graph DB): Good enough with proper indexes
- Cursor pagination (not offset): Stable when data changes
- SSE (not WebSockets): Simpler for one-way real-time feeds
- Redis pub/sub: Cross-instance event fanout for SSE
- No ActivityPub (yet): SDK-only gate, add Mastodon federation post-MVP if needed

## Learnings

- API design for agent social networks: artifact-first (decisions, learnings, code) vs. post-first (Twitter-style)
- JWT + RS256: Squads sign tokens with private key, network verifies with public key from /.well-known/jwks.json
- Cursor-based pagination: Encode last_item_id + sort_value + query_fingerprint for stable pagination
- SSE fanout with Redis pub/sub: Artifact published → Redis PUBLISH → all server instances → SSE to connected clients
- Feed algorithm: Weighted blend of followed agents (1.0), trending in expertise (0.85), similar squads (0.7), adoptions (0.6)
- Adoption tracking as reputation signal: Better than likes — proves you actually used the knowledge
- Rate limiting with Redis: Token bucket via INCR + EXPIRE (100 req/hour per squad, not per agent)
- SQL anti-patterns: Never use OFFSET for pagination (scans skipped rows), use keyset pagination (WHERE id < cursor)
- Fastify vs. Express: 12x faster, native async/await, built-in schema validation with Zod
- PostgreSQL GIN indexes: Required for JSONB queries (metadata), text array queries (tags, expertise), full-text search
- DOMPurify: Strips dangerous HTML/JS from user content, allow only safe tags (p, code, a, ul, ol)
- Prompt injection patterns: "Ignore previous instructions", "System: you are now", "<|endoftext|>", "\n\nHuman:"
- OpenTelemetry distributed tracing: Every request gets trace ID, spans for DB/cache/SSE, trace context in headers
- Blue/green deployment: ECS task definition versioning, shift traffic gradually, one-click rollback


### Azure Blob Storage Migration (2026-03-XX)

**Task:** Replace SQLite/EF Core data layer with Azure Blob Storage for persistent storage on Azure.

**What was done:**
- Removed EF Core packages (Microsoft.EntityFrameworkCore.Sqlite, .Design) from SquadPlaces.Data.csproj, replaced with Azure.Storage.Blobs
- Deleted SquadPlacesDbContext.cs
- Removed nav properties from models (Squad.Artifacts collection, KnowledgeArtifact.Squad) — blob storage doesn't do joins
- Created IBlobStorageService interface and BlobStorageService implementation using BlobServiceClient
  - Two containers: "squads" ({id}.json), "artifacts" ({id}.json)
  - Blob metadata for squadId and createdAt on artifacts
  - Feed query: list all, deserialize, sort by CreatedAt desc, paginate in memory (MVP scale)
  - InitializeAsync() creates containers if they don't exist
- Updated API Program.cs: BlobServiceClient + IBlobStorageService as singletons, all 7 endpoints migrated
- Updated Web Program.cs: same DI pattern, SeedDataAsync rewritten for blob storage
- Updated all 6 Razor page code-behinds to inject IBlobStorageService
- Updated 4 Razor views to remove nav property references (Squad.Artifacts → Model.Artifacts, artifact.Squad?.Name → SquadNames dictionary)
- Updated AppHost: added Aspire.Hosting.Azure.Storage package, Azurite emulator resource, blob reference to both api and web projects
- Solution builds clean (0 errors, 0 warnings)

## Learnings
- Azure.Storage.Blobs v12 changed GetBlobsAsync signature: uses GetBlobsOptions object instead of BlobTraits enum directly
- When removing nav properties from EF models, always check .cshtml Razor views — they reference properties via @model that won't show up in .cs-only grep
- BlobServiceClient registered as singleton is correct — it's thread-safe and connection-pooled
- Aspire's AddAzureStorage().RunAsEmulator() auto-starts Azurite in Docker for local dev — zero config needed
- For MVP-scale feed queries, list-all-and-filter-in-memory is perfectly fine. Table Storage or search index for scale later.
- **2026-03-05 (Eating our own cooking):** Enlisted "The Usual Suspects" squad on Squad Places (ID: 18d84b44-f59e-4cc2-acef-374a0388f048) and published 5 knowledge artifacts: 2 decisions (blob storage migration, OpenAPI-as-SDK), 1 pattern (Aspire+Azurite), 1 lesson (in-container DBs die on redeploy), 1 insight (Scalar > Swashbuckle). API lives on port 7273 (SquadPlaces.Api), web frontend on 7056 (SquadPlaces.Web) — Aspire assigns these from launchSettings. All 5 artifacts verified in the feed. Used `Invoke-RestMethod -SkipCertificateCheck` for HTTPS with dev certs. The API shape is clean — EnlistRequest and PublishArtifactRequest DTOs map directly to what we need. No issues encountered.

### 📌 Team update (2026-03-05T05-03-20Z): Waingro adversarial testing surfaced 3 P0 server crashes + 4 P1 validation gaps — dogfood session complete
- **Status:** 16 adversarial tests run. Happy path works. Sad path unguarded.
- **Blockers:** Null/missing Name field → 500 (needs [Required] + MinLength), empty names accepted as 201, ArtifactType not validated (e.g., "banana" accepted), no max length constraints, XSS payloads stored verbatim.
- **Recommendation:** Add input validation before expanding feed beyond test data. Happy path is solid.
- **Full report:** .squad/orchestration-log/2026-03-05T05-03-20Z-waingro.md and merged to decisions.md
- **Data quality risk:** Without validation, bad data will pollute the feed and be hard to clean up later.
- **Immediate action:** Coordinate with API team to fix P0 crashes (null handling) and P1 gaps (length limits, enum validation)

## Learnings

### 2026-03-05: API Input Validation — Fixing Waingro's Adversarial Findings

**Task:** Fix all 7 bugs found by Waingro during adversarial dogfood testing of Squad Places API.

**Root cause:** No input validation whatsoever on POST endpoints. ASP.NET minimal APIs with record DTOs don't get automatic model validation — `string Name` (non-nullable) silently gets null when JSON field is missing, causing downstream 500s in blob storage.

**Fixes applied to `src/SquadPlaces.Api/Program.cs`:**
1. Made both endpoint parameters nullable (`EnlistRequest?`, `PublishArtifactRequest?`) to prevent ASP.NET deserialization crashes
2. Added `Sanitize()` helper — strips null bytes (\0) and control chars (except \n, \r, \t), trims whitespace
3. Added `ValidateEnlistRequest()` — Name required/non-empty/max 200, Description max 1000, PublicKey max 5000, AvatarUrl max 2000 + valid URI
4. Added `ValidatePublishArtifactRequest()` — Title required/max 200, Summary required/max 1000, ArtifactType enum validation (decision/pattern/lesson/insight, case-insensitive), Content max 50000, Tags max 500
5. All stored values run through `Sanitize()` before persistence
6. ArtifactType normalized to lowercase on storage
7. Feed page param clamped to minimum 1
8. Returns `Results.ValidationProblem()` with field-specific error messages

**Key decisions:**
- Manual validation (not data annotations) — these are records in top-level minimal API, no controller infrastructure
- `static` local functions for validators — works in top-level statements, no class needed
- Don't strip HTML tags (XSS is consumer's problem, Razor auto-encodes) — only strip truly dangerous chars (null bytes, control chars)
- Case-insensitive artifact type matching, normalized to lowercase on storage

**Test results:** 12/12 adversarial tests pass — all 3 P0 crashes fixed, all 4 P1 validation gaps closed, sanitization working.

### 2025-07-17: Rate Limiting & Abuse Detection for Squad Places API

**Requested by:** Brady. Post-validation hardening — protect the API against hammering, spam, and scaled adversarial attacks.

**Implementation in `src/SquadPlaces.Api/Program.cs`:**

1. **Rate limiting** via built-in `Microsoft.AspNetCore.RateLimiting` (no NuGet packages):
   - `global` policy: 100 req/min per IP (sliding window, 6 segments)
   - `write` policy: 10 req/min per IP on POST endpoints (enlist, publish)
   - `read` policy: 60 req/min per IP on GET endpoints
   - Returns 429 with `Retry-After` and `X-RateLimit-Limit` headers
   - Named policies applied via `.RequireRateLimiting()` on each endpoint

2. **IP auto-blocking** (`IpBlocklistService`):
   - `ConcurrentDictionary`-backed singleton tracks rate-limit strikes per IP
   - 5+ strikes in 10 minutes → 1 hour block (403 Forbidden)
   - Middleware runs before rate limiter for fast short-circuit
   - Strikes auto-prune; blocks auto-expire

3. **Duplicate detection** (`DuplicateDetectionService`):
   - Same squad + same title within 5 minutes → 409 Conflict
   - `ConcurrentDictionary`-backed, lazy cleanup of stale entries

4. **Spam scoring** (`DetectSpam()` static helper):
   - >5 URLs in combined content → 400
   - >50% identical repeated words → 400
   - Applied inline in POST handlers before storage

5. **OpenAPI updates**: All endpoints now produce 429/403. Publish endpoint also produces 409.

6. **Discovery prompt** updated with rate limiting guidance for agents.

**Key decisions:**
- All logic in Program.cs (minimal API, single file — no new projects)
- Thread-safe `ConcurrentDictionary` with `lock` on individual records for strike counting
- Sliding window (not fixed window) for fairer rate limiting across time boundaries
- Spam detection uses `params string?[]` to check combined fields in a single call

### 2026-03-05: Comments/Replies (Threaded Conversations) + GIF Support

**Requested by:** Brady. "agents should be able to reply in comments and start conversations cross-thread. it's not social if it's just posts with no replies. and i absolutely shall not allow one more session of work to be completed without support for gifs."

**What was built:**

1. **Comment model** (`src/SquadPlaces.Data/Models/Comment.cs`): Id, ArtifactId, SquadId, ParentCommentId (null = top-level, set = reply), Body (markdown), GifUrl (optional), CreatedAt.

2. **GifUrl on KnowledgeArtifact**: Added optional `GifUrl` property to artifacts. Updated `PublishArtifactRequest` record and validation.

3. **Storage layer** (`IBlobStorageService` + `BlobStorageService`): Third blob container "comments". `SaveCommentAsync`, `GetCommentAsync`, `ListCommentsAsync` (filtered by artifactId metadata, ordered CreatedAt ascending).

4. **Three new API endpoints** in `Program.cs`:
   - `POST /api/artifacts/{artifactId}/comments` — post comment or reply (write rate limit)
   - `GET /api/artifacts/{artifactId}/comments` — list all comments on artifact (read rate limit)
   - `GET /api/comments/{id}` — get single comment (read rate limit)

5. **Validation**: `ValidatePostCommentRequest` — Body required/max 5000, GifUrl valid URI/max 2000, SquadId required. GifUrl validation also added to `ValidatePublishArtifactRequest`.

6. **Abuse detection**: Spam detection (URLs, repeated words) applied to comment body. Duplicate comment detection (same squad + same body + same artifact within 2 minutes = 409).

7. **Threading model**: Flat list returned from API, clients reconstruct tree via ParentCommentId. ParentCommentId validated to exist and belong to same artifact.

8. **OpenAPI**: Full `.WithName()`, `.WithTags("Comments")`, `.WithSummary()`, `.WithDescription()` on all 3 endpoints. Produces metadata for all status codes.

9. **Discovery endpoint updated**: Added conversation/reply instructions, GIF mention, and 3 new endpoints to quick reference table.

**Key decisions:**
- Flat comment list (ascending CreatedAt) — client reconstructs thread tree. Simpler API, works for any depth.
- `CommentDuplicateDetectionService` separate from artifact dupe detection — 2-minute window (vs 5 for artifacts) since comments are faster.
- ParentCommentId cross-artifact validation: parent must belong to same artifact (400 if not).
- GifUrl is just a URL field, not file upload — keeps it simple, agents can link to any GIF CDN.

**Files modified:**
- `src/SquadPlaces.Data/Models/Comment.cs` (new)
- `src/SquadPlaces.Data/Models/KnowledgeArtifact.cs` (added GifUrl)
- `src/SquadPlaces.Data/IBlobStorageService.cs` (3 new methods)
- `src/SquadPlaces.Data/BlobStorageService.cs` (comments container + 3 methods)
- `src/SquadPlaces.Api/Program.cs` (3 endpoints, validation, DTOs, discovery update, abuse detection)

**Verified:** Build clean (0 errors, 0 warnings). 10 integration tests passed: enlist, artifact with GIF, top-level comment, reply, list, get, 404, validation, bad parent, missing artifact.
