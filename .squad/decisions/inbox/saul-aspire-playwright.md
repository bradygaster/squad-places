# Decision: Aspire Dashboard Playwright Integration Tests

**By:** Saul (Aspire & Observability)
**Date:** 2026-02-22
**Requested by:** Brady

## What

Created `test/aspire-integration.test.ts` — a Playwright + Vitest integration test suite that:

1. Launches the Aspire dashboard container (`mcr.microsoft.com/dotnet/aspire-dashboard:latest`)
2. Configures OTel gRPC export via `@opentelemetry/sdk-node` + `@opentelemetry/exporter-trace-otlp-grpc`
3. Creates Squad-style spans and metrics (session traces, agent traces, counters, histograms)
4. Opens Playwright Chromium browser to the dashboard and validates telemetry appears
5. Tests `squad aspire` command lifecycle (runAspire export, AspireOptions interface, Docker container state)

## 5 Tests

| Test | What it validates |
|------|-------------------|
| traces appear in Aspire dashboard | Span creation → gRPC export → dashboard /traces page renders trace data |
| metrics appear in Aspire dashboard | Counter/histogram recording → gRPC export → dashboard /metrics page renders metric data |
| squad aspire command exists | `runAspire` is exported from `@bradygaster/squad-cli/commands/aspire` |
| AspireOptions with docker flag | Type-level: `{ docker: true, port: 18888 }` compiles |
| Docker lifecycle | Container running, dashboard port 18888 responds, OTLP gRPC port 4317 accepts TCP connections |

## Technical Choices

- **NodeSDK (0.57.2)** over BasicTracerProvider (2.x): Version alignment with gRPC exporters (also 0.57.2) avoids type mismatch
- **Direct URL navigation** over sidebar clicks: Aspire's Fluent UI web components don't match standard selectors reliably
- **Skip guard**: `SKIP_DOCKER_TESTS=1` env var or Docker absence auto-skips the suite — safe for CI without Docker

## Verification

- All 5 tests pass (traces 4.8s, metrics 6.8s, command tests <100ms each)
- Full suite: 2141 tests passing across 79 files
- Build clean, type-check clean
