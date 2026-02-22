# Decision: Ink Shell Wiring — ShellApi callback pattern

**Author:** Fenster  
**Date:** 2026-02-22  
**Status:** Implemented  
**Scope:** CLI shell UI (`packages/squad-cli/src/cli/shell/`)

## Context

The shell needed to move from a readline echo loop to an Ink-based UI using the three existing components (AgentPanel, MessageStream, InputPrompt). The key challenge was connecting the StreamBridge (which pushes events from the streaming pipeline) into React component state.

## Decision

**ShellApi callback pattern:** The `App` component accepts an `onReady` prop that fires once on mount, delivering a `ShellApi` object with three methods: `addMessage`, `setStreamingContent`, `refreshAgents`. The host (`runShell()`) captures this API and wires it to StreamBridge callbacks.

This keeps the Ink component decoupled from StreamBridge internals — the component doesn't import or know about the bridge. The host is the only place where both meet.

**`React.createElement` in index.ts:** Rather than renaming `index.ts` to `index.tsx` (which would ripple through exports maps and imports), `runShell()` uses `React.createElement(App, props)` directly. This keeps the file extension stable.

**Streaming content accumulation:** StreamBridge's `onContent` callback delivers deltas. The host maintains a `streamBuffers` Map to accumulate content per agent and pushes the full accumulated string to `setStreamingContent`. On `onComplete`, the buffer is cleared and the final message is added.

## Consequences

- StreamBridge is ready for coordinator wiring — call `_bridge.handleEvent(event)` when the coordinator emits streaming events.
- Direct agent messages and coordinator routing show placeholders until coordinator integration (Phase 3).
- All existing exports from `shell/index.ts` are preserved. New exports: `App`, `ShellApi`, `AppProps`.
