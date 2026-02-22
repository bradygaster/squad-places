# Decision: OTel 3-Layer Public API Export

**By:** Kujan (SDK Expert)
**Date:** 2025-07-19
**Issue:** #266

## Context

SDK consumers need instrumented Squad telemetry that flows through their own OTel providers. The OTel internals existed but weren't fully surfaced as a coherent public API.

## Decision

Export a **3-layer OTel API** from `src/index.ts`:

| Layer | Function | Module | Purpose |
|-------|----------|--------|---------|
| Low | `initializeOTel()`, `shutdownOTel()`, `getTracer()`, `getMeter()` | `otel.ts` | Direct OTel control |
| Mid | `bridgeEventBusToOTel(bus)` | `otel-bridge.ts` | Wire EventBus → OTel spans |
| High | `initSquadTelemetry(options)` | `otel-init.ts` | One-call setup with lifecycle handle |

### Key choices:
- `initSquadTelemetry` lives in its own module (`otel-init.ts`) to avoid circular imports between `otel.ts` ↔ `otel-bridge.ts`
- `SquadTelemetryOptions` extends `OTelConfig` — backward compatible, additive only
- `installTransport` defaults to `true` so high-level consumers get TelemetryCollector → OTel bridging automatically
- Named exports for bridge/init (not `export *`) to keep the public surface explicit and tree-shakeable

## Zero-overhead guarantee

If no `TracerProvider` / `MeterProvider` is registered, `@opentelemetry/api` returns no-op implementations. All Squad instrumentation becomes zero-cost function calls that get optimized away.
