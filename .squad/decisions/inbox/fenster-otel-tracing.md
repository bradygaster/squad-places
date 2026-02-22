# Decision: OpenTelemetry Tracing for Agent Lifecycle & Coordinator Routing

**Date:** 2026-02-22  
**By:** Fenster  
**Issues:** #257, #258  
**Status:** Implemented

## What

Added OpenTelemetry trace instrumentation to four files in `packages/squad-sdk/src/`:

1. **`agents/index.ts`** — AgentSessionManager: `spawn()`, `resume()`, `destroy()` wrapped with spans (`squad.agent.spawn`, `squad.agent.resume`, `squad.agent.destroy`).
2. **`agents/lifecycle.ts`** — AgentLifecycleManager: `spawnAgent()`, `destroyAgent()` wrapped with spans (`squad.lifecycle.spawnAgent`, `squad.lifecycle.destroyAgent`).
3. **`coordinator/index.ts`** — Coordinator: `initialize()`, `route()`, `execute()`, `shutdown()` wrapped with spans (`squad.coordinator.initialize`, `squad.coordinator.route`, `squad.coordinator.execute`, `squad.coordinator.shutdown`).
4. **`coordinator/coordinator.ts`** — SquadCoordinator: `handleMessage()` wrapped with span (`squad.coordinator.handleMessage`).

## Why

- Observability is foundational for debugging multi-agent orchestration at runtime.
- Using `@opentelemetry/api` only — no-ops without a registered provider, so zero cost in production unless OTel is configured.
- Trace hierarchy: `coordinator.handleMessage → coordinator.route → coordinator.execute → lifecycle.spawnAgent → agent.spawn`.

## Convention Established

- **Tracer name:** `trace.getTracer('squad-sdk')` — one tracer per package.
- **Span naming:** `squad.{module}.{method}` (e.g., `squad.agent.spawn`).
- **Attributes:** Use descriptive keys like `agent.name`, `routing.tier`, `target.agents`, `spawn.mode`.
- **Error handling:** `span.setStatus({ code: SpanStatusCode.ERROR })` + `span.recordException(err)` in catch blocks. Always `span.end()` in `finally`.
- **Import only from `@opentelemetry/api`** — never from SDK packages directly.
