# Project Context

- **Owner:** Brady
- **Project:** squad-sdk — the programmable multi-agent runtime for GitHub Copilot (v1 replatform)
- **Stack:** TypeScript (strict mode, ESM-only), Node.js ≥20, @github/copilot-sdk, Vitest, esbuild
- **Created:** 2026-02-21

## Learnings

### From Beta (carried forward)
- Event-driven over polling: always prefer event-based patterns
- Streaming-first: async iterators over buffers — this is a core design principle
- Graceful degradation: if one session dies, others survive
- Node.js ≥20: use modern APIs (structuredClone, crypto.randomUUID, fetch, etc.)
- ESM-only: no CJS shims, no dual-package hazards
- Cost tracking and telemetry: runtime performance is a feature, not an afterthought

### Issue #239: StreamingPipeline Bridge + Console Renderer
- **StreamBridge** (`src/cli/shell/stream-bridge.ts`): Callback-based bridge connecting `StreamingPipeline` events to shell rendering. Accumulates `message_delta` chunks in per-session buffers, dispatches `usage` and `reasoning_delta` events to optional callbacks, and `flush()` finalizes buffered content into `ShellMessage` objects while updating `SessionRegistry` status.
- **ShellRenderer** (`src/cli/shell/render.ts`): Pre-ink console renderer using `process.stdout.write()` for streaming deltas and `console.log()` for complete messages. Tracks current agent to avoid redundant headers during contiguous streaming from the same agent.
- **Design pattern**: The bridge is event-sink only — it receives `StreamingEvent` from the pipeline but does not subscribe itself. The caller (shell entry point) is responsible for wiring `pipeline.onDelta()` → `bridge.handleEvent()`. This keeps the bridge testable without a live pipeline.
- **Key type alignment**: `StreamingEvent` is a union of `StreamDelta | UsageEvent | ReasoningDelta` — no `stream_end` or `stream_error` variants exist. Stream completion is signaled externally via `flush()`.
