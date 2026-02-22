# Decision: OTel SDK v2 test patterns

**By:** Hockney
**Date:** 2026-02-23
**Issue:** #267

## Context

While writing OTel integration E2E tests, discovered that `@opentelemetry/sdk-trace-base` v2.x has breaking API differences from v1 that affect test patterns.

## Decisions

1. **Use `parentSpanContext` not `parentSpanId`**: In SDK v2, `ReadableSpan.parentSpanId` is always `undefined`. The parent linkage is on `parentSpanContext.spanId` instead. All tests verifying span hierarchy must use `(span as any).parentSpanContext?.spanId`.

2. **Require `AsyncLocalStorageContextManager` for context propagation in tests**: `BasicTracerProvider` alone does NOT propagate context. Import `AsyncLocalStorageContextManager` from `@opentelemetry/context-async-hooks`, call `.enable()` in `beforeEach`, and `.disable()` in `afterEach`. Without this, `trace.setSpan()` creates contexts but `startSpan(name, opts, ctx)` ignores the parent.

3. **EventBus bridge is tested via manual pattern**: `bridgeEventBusToOTel` is defined in `otel-init.ts` imports but not yet exported from `otel-bridge.ts`. Tests use the manual `bus.subscribeAll()` → `tracer.startSpan()` pattern, which validates the expected contract. When `bridgeEventBusToOTel` ships, tests should be updated to call it directly.

## Impact

All agents writing OTel tests must follow these patterns or tests will silently pass with broken assertions.
