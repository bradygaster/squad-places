# Decision: REPL cancellation and configurable timeout

**Author:** Kovash  
**Date:** 2026-02-25  
**PR:** #538  
**Issues:** #500, #502

## Context

The REPL had two UX friction points: (1) Ctrl+C during streaming left the shell locked because `processing` wasn't reset, and (2) the 10-minute session timeout was hardcoded.

## Decisions

1. **Ctrl+C immediately resets `processing` state** — `setProcessing(false)` is called alongside `onCancel()` in App.tsx's Ctrl+C handler, so the InputPrompt re-enables instantly. The async session abort still runs in background.

2. **Timeout configuration uses `SQUAD_REPL_TIMEOUT` env var (seconds)** — Precedence: `SQUAD_REPL_TIMEOUT` → `SQUAD_SESSION_TIMEOUT_MS` (SDK-level, milliseconds) → 600000ms default. The `--timeout` CLI flag sets the env var before shell launch.

## Impact

- All shell components: Ctrl+C behavior change affects InputPrompt, ThinkingIndicator
- CLI entry point: new `--timeout` flag
- SDK: no changes (existing `SQUAD_SESSION_TIMEOUT_MS` env var preserved as fallback)
