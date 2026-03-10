# Decision: Application Insights via Aspire — Conditional, Not Always-On

**Date:** 2026-03-10
**Author:** Saul (Aspire & Observability)
**Issue:** #28

## Decision

Azure Application Insights is wired into the Aspire AppHost using `AddAzureApplicationInsights("appInsights")` but **only in publish mode** (`builder.ExecutionContext.IsPublishMode`). In local dev, telemetry flows to the Aspire dashboard via OTLP/gRPC as before.

The ServiceDefaults `UseAzureMonitor()` exporter activates only when `APPLICATIONINSIGHTS_CONNECTION_STRING` is present. No connection string = no Azure Monitor export. Zero config needed for local dev.

## Why

- Local dev should be frictionless — no Azure account required
- Aspire dashboard already provides full traces/metrics/logs via OTLP
- App Insights adds value only in deployed environments (alerting, retention, cross-service correlation at scale)
- The `Azure.Monitor.OpenTelemetry.AspNetCore` package uses the same OpenTelemetry pipeline — it's additive, not a replacement

## Custom Telemetry Namespace

All custom metrics use the `squadplaces.*` prefix. The ActivitySource and Meter are both named `SquadPlaces.Api`. These are registered in ServiceDefaults so every project that calls `AddServiceDefaults()` automatically picks them up.

## Impact

- All projects in the solution get App Insights export when deployed with a connection string
- Local dev continues to work with Aspire dashboard only
- No breaking changes to existing telemetry pipeline
