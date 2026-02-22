# Decision: OTel metrics test pattern — spy meter mock

**By:** Hockney (Tester)
**Date:** 2026-02-23
**Status:** Implemented

## What
OTel metrics tests use a spy-meter pattern: mock `getMeter()` to return a fake meter where every `createCounter`/`createHistogram`/`createUpDownCounter`/`createGauge` returns a spy instrument with `.add()` and `.record()` mocks. This allows verifying exact metric names, values, and attributes without a real OTel SDK or collector.

## Why
- The otel-metrics module is a thin instrumentation layer — tests need to verify *what* gets recorded, not *how* OTel processes it.
- Spy meter pattern avoids needing `InMemoryMetricExporter` (which has complex async flush semantics) and keeps tests synchronous and fast.
- Pattern is consistent with existing otel-bridge tests (spy spans via InMemorySpanExporter) but adapted for the metrics API surface.

## Applies to
- `test/otel-metrics.test.ts` (34 tests)
- `test/otel-metric-wiring.test.ts` (5 tests)
- Future OTel metric tests should follow this same pattern.
