# Decision: Animation Architecture — setState-during-render for synchronous transitions

**Author:** Cheritto (TUI Engineer)
**Date:** 2026-02-26
**Issue:** #337 — Animations and transitions
**Status:** Implemented

## Context

Adding tasteful animations to the CLI required detecting agent state transitions (working → idle) and rendering completion flash badges synchronously so they appear on the same render frame.

## Decision

Used React's setState-during-render pattern in `useCompletionFlash` instead of `useEffect`-based detection. This ensures the "✓ Done" flash is visible immediately when the agent status changes, without requiring an extra render cycle.

**Why not `useEffect`?** Effects run asynchronously after render. In ink-testing-library, `rerender()` + `lastFrame()` wouldn't capture the flash because the state update from the effect hadn't committed yet. More importantly, users would see one frame without the flash before it appears — a visible flicker.

**Trade-offs:**
- (+) Synchronous: flash visible on same render as status change
- (+) Testable: ink-testing-library `rerender()` captures flash immediately
- (-) Uses a less common React pattern (setState during render)
- (-) Requires careful loop termination (`!flashing.has(name)` guard prevents infinite re-render)

## Alternatives Considered

1. **useEffect + timer**: Simpler but async — flash appears one frame late
2. **useSyncExternalStore**: Overkill for this use case
3. **External state (zustand/jotai)**: Adds dependency, unnecessary complexity

## Animation Budget

| Animation | Duration | Frame rate |
|-----------|----------|------------|
| Welcome typewriter | 500ms | ~15fps |
| Banner fade-in | 300ms | single transition |
| Message fade-in | 200ms | single transition |
| Completion flash | 1500ms | single transition |

All animations use `dimColor` toggle (single re-render) or `useTypewriter` (~15fps interval). No continuous high-frequency animations.
