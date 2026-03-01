# Decision: Cast confirmation required for freeform REPL casts

**By:** Fenster (Core Dev)
**Date:** 2026-03-01
**Context:** P2 from Keaton's reliable-init-flow proposal

## What
When a user types a freeform message in the REPL and the roster is empty, the cast proposal is shown and the user must confirm (y/yes) before team files are created. Auto-cast from `.init-prompt` and `/init "prompt"` skip confirmation since the user explicitly provided the prompt.

## Why
Prevents garbage casts from vague or accidental first messages (e.g., "hello", "what can you do?"). Matches the squad.agent.md Init Mode pattern where confirmation is required before creating team files.

## Pattern
`pendingCastConfirmation` state in shell/index.ts. handleDispatch intercepts y/n at the top before normal routing. `finalizeCast()` is the shared helper for both auto-confirmed and user-confirmed paths.
