# Decision: No content suppression based on terminal width

**By:** Cheritto (TUI Engineer)
**Date:** 2026-03-01
**Context:** Brady directive — "The app should scroll. Stop compacting text vertically."

## What
Terminal width tiers (compact ≤60, standard, wide ≥100) may adjust *layout* (e.g., wrapping, column arrangement) but must NOT suppress or truncate *content*. Every piece of information shown at 120 columns must also be shown at 40 columns.

## Why
Users can scroll. Hiding roster names, spacing, help text, or routing hints on narrow terminals removes information the user needs. Layout adapts to width; content does not.

## Convention
- `compact` variable may be used for layout decisions (flex direction, column vs. row)
- `compact` must NOT gate visibility of text, spacing, or UI sections
- `wide` may add supplementary content (e.g., focus line) but narrow must not remove it
