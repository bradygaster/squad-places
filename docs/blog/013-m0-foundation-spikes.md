# M0: Foundation Spike Results

> **⚠️ INTERNAL ONLY — DO NOT PUBLISH**

**Milestone:** M0 · **Issue:** #46
**Date:** Sprint 0

---

## Why We Spiked First

Before writing a single line of production code, we ran three foundation spikes to validate the technical assumptions behind the SDK replatform. Each spike answered a question that would have been expensive to get wrong later.

## Spike 1: Adapter Pattern

The first question: can we wrap `@github/copilot-sdk` without leaking its internals? We built a thin `SquadClient` adapter that owns the connection lifecycle — connect, disconnect, reconnect — while exposing only typed Squad interfaces to the rest of the codebase. The spike proved that the SDK's event model maps cleanly to our `EventBus`, and that auto-reconnection can be handled transparently at the adapter boundary. The key insight: errors from the SDK are untyped, so the adapter must classify them into `SquadError` subtypes (`SDKConnectionError`, `ModelAPIError`, etc.) before they propagate.

## Spike 2: Reconnection Strategy

Copilot connections drop. The spike tested exponential backoff with jitter, capped at 30 seconds, with a `SquadConnectionState` machine tracking `disconnected → connecting → connected → reconnecting → error`. We validated that in-flight sessions survive brief disconnections when the adapter buffers pending operations. The `TelemetryCollector` was born here — we needed latency and error-rate data to tune reconnection thresholds.

## Spike 3: Event Bus Design

The third spike explored pub/sub for session lifecycle events. We needed `session.created`, `session.destroyed`, and `session.status_changed` events flowing from `SessionPool` through `EventBus` to any subscriber — coordinator, hooks, telemetry. The spike confirmed that a synchronous, in-process event bus with typed `SquadEvent` payloads is sufficient for v1. No need for an external message broker.

## What We Carried Forward

All three spikes shipped code that landed in the production adapter layer: `SquadClient`, `EventBus`, `SquadConnectionState`, and `ErrorFactory`. The spike-first approach meant M0 delivered tested foundations, not throwaway prototypes.
