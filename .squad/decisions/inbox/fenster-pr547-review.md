# Decision: PR #547 Remote Control Feature — Architectural Review

**By:** Fenster  
**Date:** 2026-03-01  
**PR:** #547 "Squad Remote Control - PTY mirror + devtunnel for phone access" by tamirdresher (external)

## Context

External contributor Tamir Dresher submitted a PR adding `squad start --tunnel` command to run Copilot in a PTY and mirror terminal output to phone/browser via WebSocket + Microsoft Dev Tunnels.

## Architectural Question

Is remote terminal access via devtunnel + PTY mirroring in scope for Squad v1 core?

## Technical Assessment

**What works:**
- RemoteBridge WebSocket server architecture is sound
- PTY mirroring approach is technically correct
- Session management dashboard is useful
- Security headers and CSP are present
- Test coverage exists (18 tests, though failing due to build issues)

**Critical blockers:**
1. **Build broken** — TypeScript errors in `start.ts`, all tests failing
2. **Command injection vulnerability** — `execFileSync` with string interpolation in `rc-tunnel.ts`
3. **Native dependency** — `node-pty` requires C++ compiler (install friction)
4. **Windows-only effectively** — hardcoded paths, devtunnel CLI Windows-centric
5. **No cross-platform strategy** — macOS/Linux support unclear

**Architectural concerns:**
1. **Not integrated with Squad runtime** — doesn't use EventBus, Coordinator, or agent orchestration. Isolated feature.
2. **Two separate modes** — PTY mode (`start.ts`) vs. ACP passthrough mode (`rc.ts`). Why both?
3. **New CLI paradigm** — "start" implies daemon/server, not interactive mirroring. Command naming collision risk.
4. **External dependency** — requires `devtunnel` CLI installed + authenticated. Not bundled, not auto-installed.
5. **Audit logs** — go to `~/.cli-tunnel/audit/` instead of `.squad/log/` (inconsistent with Squad state location).

## Recommendation

**Request Changes** — Do not merge until:
1. TypeScript build errors fixed
2. Command injection vulnerability patched (use array args, no interpolation)
3. Tests passing (currently 18/18 failing)
4. Cross-platform support documented or Windows-only label added
5. Architectural decision on scope: Is this core or plugin?

**If approved as core feature:**
- Extract to plugin first, prove value, then consider core integration
- Unify PTY vs. ACP modes (pick one)
- Integrate with EventBus/Coordinator (or explain why isolated is correct)
- Rename command to `squad remote` or `squad tunnel` (avoid `start` collision)
- Move audit logs to `.squad/log/`

**If approved as plugin:**
- This is the right path — keeps core small, proves value independently
- Still fix security issues before merge to plugin repo

## For Brady

You requested a runtime review. Here's the verdict:

- **Concept is cool** — phone access to Copilot is a real use case.
- **Implementation needs work** — build broken, security issues, Windows-only.
- **Architectural fit unclear** — not in any Squad v1 PRD. No integration with agent orchestration.
- **Native dependency risk** — `node-pty` adds install friction (C++ compiler required).

**My take:** This belongs in a plugin, not core. External contributor did solid work on the WebSocket bridge, but Squad v1 needs to ship agent orchestration first. Remote access is a nice-to-have, not a v1 must-have.

If you want this in v1, we need a proposal (docs/proposals/) first.
