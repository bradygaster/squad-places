# Saul — History

## Project Context

- **Project:** Squad SDK — the programmable multi-agent runtime for GitHub Copilot
- **Owner:** Brady
- **Stack:** TypeScript (strict mode, ESM-only), Node.js ≥20, @github/copilot-sdk, Vitest, esbuild
- **Joined:** 2026-02-22

## Key Context

- OTel Phases 1-3 complete: provider init, telemetry bridge, agent lifecycle traces, session traces, tool enhancements, metric wiring
- OTel Phase 4 complete: Aspire command (`squad aspire`), file watcher (squad-observer), event payloads, WS bridge
- Aspire dashboard: `mcr.microsoft.com/dotnet/aspire-dashboard:latest`
  - UI: port 18888, OTLP/gRPC: port 18889 (mapped to host 4317)
  - Anonymous mode: `ASPIRE_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true`
- The `squad aspire` CLI command handles Docker lifecycle (start/stop container)
- OTel exports from SDK: initializeOTel, shutdownOTel, getTracer, getMeter, bridgeEventBusToOTel, initSquadTelemetry
- 2022 tests passing across 74 files (before Wave 2)

## Learnings

- Aspire dashboard Playwright integration tests: `test/aspire-integration.test.ts` (5 tests)
  - Uses `@opentelemetry/sdk-node` (0.57.2) with gRPC exporters for version-aligned OTel setup
  - Direct URL navigation (`/traces`, `/metrics`) is more reliable than sidebar click selectors — Aspire uses Fluent UI web components
  - NodeSDK registers global providers automatically; `trace.getTracer()` / `metrics.getMeter()` work without separate registration
  - `forceFlush` on global providers works via `trace.getTracerProvider()` cast — the SDK's NodeSDK doesn't expose flush directly
  - Dashboard needs ~3s after flush to index traces, ~5s for metrics
  - Test auto-skips when `SKIP_DOCKER_TESTS=1` or Docker is unavailable
  - 2141 tests passing across 79 files (post Aspire integration test)
