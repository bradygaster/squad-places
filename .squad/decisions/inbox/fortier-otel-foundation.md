# Decision: OTel Foundation — NodeSDK over individual providers

**Author:** Fortier (Node.js Runtime)
**Date:** 2026-02-22
**Issues:** #255, #256
**Status:** Implemented

## Context

Issues #255 and #256 require OTel provider initialization and a bridge from the existing TelemetryCollector to OTel spans. The OpenTelemetry JS ecosystem has multiple packages (`sdk-trace-base`, `sdk-trace-node`, `sdk-metrics`, `resources`, `exporter-*`) that frequently ship with incompatible transitive versions, causing TypeScript type errors even when runtime behavior is correct.

## Decision

Use `@opentelemetry/sdk-node`'s `NodeSDK` class as the single provider manager, and import `Resource` and `PeriodicExportingMetricReader` from its re-exports (`resources`, `metrics` sub-modules) rather than installing them as direct dependencies.

Direct deps added to `packages/squad-sdk`:
- `@opentelemetry/api`
- `@opentelemetry/sdk-node`
- `@opentelemetry/exporter-trace-otlp-http`
- `@opentelemetry/exporter-metrics-otlp-http`
- `@opentelemetry/semantic-conventions`

NOT added (bundled via `sdk-node`):
- `@opentelemetry/sdk-trace-base`
- `@opentelemetry/sdk-trace-node`
- `@opentelemetry/sdk-metrics`
- `@opentelemetry/resources`

## Consequences

- Single `_sdk.shutdown()` handles both tracing and metrics cleanup.
- No version skew between trace and metric providers.
- `getTracer()` / `getMeter()` return no-op instances when OTel is not initialized — zero overhead in the default case.
- The bridge (`otel-bridge.ts`) is additive — it produces a `TelemetryTransport` that can be registered alongside any existing transport.

## Files Changed

- `packages/squad-sdk/src/runtime/otel.ts` — Provider initialization module
- `packages/squad-sdk/src/runtime/otel-bridge.ts` — TelemetryCollector → OTel span bridge
- `packages/squad-sdk/src/index.ts` — Barrel exports for both modules
- `packages/squad-sdk/package.json` — Dependencies + subpath exports
