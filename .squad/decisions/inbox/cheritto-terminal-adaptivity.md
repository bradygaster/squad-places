# Decision: Terminal Adaptivity Strategy (3-tier responsive layout)

**Issue:** #336 — Terminal adaptivity 40→120 col range
**Author:** Cheritto (TUI Engineer)
**Date:** 2026-02-23
**Status:** Proposed (PR #360)

## Context

The CLI needed to render cleanly across terminal widths from 40 to 120+ columns. Some components overflowed or looked awkward at narrow widths, and the layout was rigid.

## Decision

Implemented a 3-tier responsive system based on terminal width:

| Tier | Width | Behavior |
|------|-------|----------|
| Compact | ≤60 cols | Minimal UI — single-line agents, short prompt, no banner detail |
| Standard | 61–99 cols | Current layout with hint truncation |
| Wide | ≥100 cols | Full detail — all hints, focus line, full descriptions |

### Key choices:

1. **`useTerminalWidth()` hook in terminal.ts** — single canonical source for reactive width. Listens for `process.stdout` `resize` events. Pure `getTerminalWidth()` for non-React contexts (e.g., `/help` command handler).

2. **Width floor of 40** — `getTerminalWidth()` clamps to minimum 40. Below that, things will clip but won't crash.

3. **Breakpoints at 60 and 100** — chosen because 60 is roughly the minimum for readable agent names + status, and 100 is where activity hints and descriptions add value without cramming.

4. **No horizontal scrolling** — Ink's `wrap="wrap"` already handles message text. The separator, prompt, and banner are the only fixed-width elements, and they now all use the reactive width.

## Alternatives Considered

- **CSS-like media queries via Ink's `useStdout`** — Ink doesn't provide this natively. The `process.stdout.on('resize')` approach is simpler and more portable.
- **Single breakpoint (narrow vs. wide)** — 3 tiers give a smoother experience across the real range of terminal sizes.

## Consequences

- All width-responsive components must use `useTerminalWidth()` (not raw `process.stdout.columns`)
- New components should follow the 60/100 breakpoint convention
- Tests run at width=80 (default when `process.stdout.columns` is undefined), which is in the "standard" tier
