# Project Context

- **Owner:** Brady
- **Project:** squad-sdk — the programmable multi-agent runtime for GitHub Copilot (v1 replatform)
- **Stack:** TypeScript (strict mode, ESM-only), Node.js ≥20, @github/copilot-sdk, Vitest, esbuild
- **Created:** 2026-02-21

## Learnings

### From Beta (carried forward)
- VS Code runSubagent spawn patterns: different from CLI task tool — no agent_type, mode, model params
- Model selection gap: CLI has per-spawn model control, VS Code uses session model only
- Platform parity strategies: what works on CLI must work in VS Code
- SQL tool is CLI-only: never depend on it in cross-platform code paths
- Multiple subagents in one turn run concurrently on VS Code (equivalent to background mode)

### 2026-03-05: Cross-Platform Integration PRD (Section 16)
Completed **docs/prd/sections/16-cross-platform.md** — defines platform parity for social network across CLI, VS Code, and GitHub.

**Key findings:**
- **Parity = data consistency + platform-native UX.** A post from CLI is readable in VS Code sidebar with identical data but different UI.
- **Shared API backbone** (REST/GraphQL) ensures all platforms see same results within 1 second.
- **VS Code constraints:** Session model fixed (no per-spawn models), no SQL tool. Mitigate with pre-computed API views + template system.
- **Progressive enhancement:** Core (post/reply/feed/discover) works everywhere. Rich features (WebSocket stream, sidebar panel, code linking) vary by platform.
- **GitHub is observer mode:** Can publish (via PR comments), receive notifications, but not real-time streaming.

**Design decisions locked:**
- Single shared REST/GraphQL API (not three separate backends)
- Agent identity = Squad token everywhere (platform-agnostic)
- CLI = full-featured; VS Code = enhanced UX; GitHub = read-mostly
- Fallback strategies: CLI queues posts offline, VS Code shows cached feed, GitHub receives digests

### 2026-02-24T17-25-08Z : Team consensus on public readiness
📌 Full team assessment complete. All 7 agents: 🟡 Ready with caveats. Consensus: ship after 3 must-fixes (LICENSE, CI workflow, debug console.logs). No blockers to public source release. See .squad/log/2026-02-24T17-25-08Z-public-readiness-assessment.md and .squad/decisions.md for details.

---

### History Audit — 2026-03-03

**Audit Result:** CLEAN

**Findings:**
- ✓ No conflicting entries detected.
- ✓ No stale or reversed decisions left unresolved.
- ✓ No v0.6.0 references (correct target: v0.8.17).
- ✓ No intermediate states recorded as final outcomes.
- ✓ All learnings properly attributed (Beta-carried vs. session-specific).
- ✓ Clear traceability for future spawns reading cold.

**Notes:**
History.md is concise and accurate. Project context and learnings are properly separated. Team consensus entry correctly references decision file location. No corrections needed.

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
