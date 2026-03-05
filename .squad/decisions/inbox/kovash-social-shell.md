# Social Shell Terminal UX Pattern

**By:** Kovash (REPL & Interactive Shell)  
**Date:** 2026-03-01  
**Context:** Brady requested PRD for `squad social` command — live terminal view of autonomous agent social activity

## Decision

The social shell uses a **live 3-panel dashboard** pattern for observing autonomous agent behavior:

1. **Agent Status Panel** — shows who's active, what they're doing RIGHT NOW (ACTIVE/IDLE/PAUSED/ERROR)
2. **Activity Stream** — scrolling feed of agent actions with timestamps and icons (📖📝💬❤️🔍⚠️)
3. **Stats Bar** — real-time counters (posts, replies, reactions, discoveries)
4. **Input Bar** — always-available command input (doesn't pause agents)

Key UX principles:

- **Non-blocking input:** Human can type commands while agents work (unlike regular REPL where processing locks input)
- **Focus modes:** Follow single agent, filter by action type (discoveries/errors/posts), return to full view
- **Ambient mode:** `--ambient` flag runs in background with minimal output (occasional notifications), vs. active mode (full dashboard)
- **Session persistence:** JSONL logs in `.squad/social/sessions/{timestamp}.jsonl`, replayable with `squad social replay`
- **End-of-session summary:** Full breakdown of what happened (per-agent contributions, knowledge brought home to `.squad/social/discoveries/`)

## Why

The social shell must show **live agent autonomy** — what agents are doing RIGHT NOW, not just a log of what they did. It's an observation surface, not just a command interface.

The 3-panel layout separates:
- **Status** (who's working) from **Activity** (what just happened) from **Stats** (aggregate metrics)
- Human can focus on any view (follow Keaton, show only discoveries) without losing context

Non-blocking input is critical — agents work autonomously, human can guide/redirect without stopping the flow.

## Impact

- **Implementation:** New `src/cli/social/` module with Ink-based `SocialDashboard` component
- **Dependencies:** EventEmitter for agent action events → UI updates, virtual scrolling for activity stream (only render visible lines)
- **Performance budget:** Batch updates every 100ms, max 500 events in memory, ~200KB total memory footprint
- **Accessibility:** NO_COLOR compliant (plain text fallback), screen reader friendly (linear layout)

## Related

- PRD section: `docs/prd/sections/27-social-shell.md`
- TUI concepts: `docs/prd/sections/15-tui-concepts.md` (Cheritto's social feed component design)
- REPL architecture: `packages/squad-cli/src/cli/shell/` (existing shell patterns)
