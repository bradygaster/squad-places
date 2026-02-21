# Technical Deep Dive — SDK Replatform

> **⚠️ INTERNAL ONLY — DO NOT PUBLISH**

**Issue:** #40 (M6-1)

---

## Overview

This document provides an in-depth look at the Squad SDK architecture: the layer model, key abstractions, design decisions, and the tradeoffs we accepted.

## Architecture Layers

The SDK is organized into seven layers, each with a clear responsibility boundary:

```
squad.config.ts
    ↓ validates & compiles
Config Layer (SquadConfig, ModelRegistry, RoutingRules, AgentSources, Migration)
    ↓ feeds
Coordinator Layer (Coordinator, FanOut, RoutingDecision, ResponseTier)
    ↓ manages          ↓ enforces
Agents Layer           Tools & Hooks Layer
(Charter, Lifecycle,   (ToolRegistry, HookPipeline,
 Spawn, Model)          PolicyConfig)
    ↓ uses              ↓ extends
Adapter Layer (SquadClient, EventBus, ErrorFactory, Telemetry)
    ↓ wraps
@github/copilot-sdk
```

Each layer depends only on layers below it. No upward or circular dependencies.

## Key Abstractions

### SquadConfig
The root configuration type. Every runtime behavior traces back to a config field. `defineConfig()` provides the authoring surface; `loadConfig()` provides the runtime surface.

### Coordinator
The dispatch hub. Receives messages, evaluates compiled routing rules, selects a response tier, runs hooks, and either responds directly or spawns agents. Owns the `RoutingDecision` lifecycle.

### AgentLifecycleManager
State machine for agent sessions. Enforces `pending → spawning → active → idle → destroyed` transitions. Manages `SessionPool` allocation, idle reaping, and health monitoring.

### HookPipeline
Interception layer between routing decisions and tool execution. Pre-hooks can block or modify; post-hooks can audit or trigger side effects. Five built-in policies; custom hooks via typed interfaces.

### SquadClient
Adapter wrapping `@github/copilot-sdk`. Owns connection lifecycle, auto-reconnection, and error classification. The SDK never leaks past this boundary.

## Design Decisions

### Why TypeScript Config Over JSON/YAML?
TypeScript gives us type inference, editor support, and the ability to use expressions (regex patterns in routing, computed values). JSON schema validation is a subset of what TypeScript provides at edit time.

### Why a Coordinator Instead of Peer-to-Peer?
Centralized routing is easier to observe, debug, and enforce policies on. Peer-to-peer would require distributed consensus for policy enforcement. The coordinator is the single point of observability.

### Why Response Tiers?
Not every request justifies an agent session. Direct responses save 2-5 seconds of latency and avoid unnecessary model calls. The tier system makes this optimization explicit and configurable.

### Why Compiled Routing Rules?
Compiling regex patterns once at config load (via `compileRoutingRules()`) avoids re-parsing on every message. The compiled router is a pure function: message in, `RoutingDecision` out.

## Tradeoffs

| Decision | Benefit | Cost |
|----------|---------|------|
| Single coordinator | Observability, policy enforcement | Single point of failure |
| TypeScript config | Type safety, editor support | Requires TS toolchain |
| Adapter pattern | SDK isolation, testability | Extra abstraction layer |
| Eager validation | Fail-fast on bad config | Slower startup |
| In-process event bus | Simplicity, no external deps | No cross-process events |

## What We Chose Not to Build (Yet)

- **Distributed coordinator** — Not needed until multi-process deployments
- **External event broker** — In-process `EventBus` sufficient for v1
- **Plugin system for config loaders** — Only TS/JSON needed today
- **Agent-to-agent direct communication** — All routing through coordinator for policy enforcement
