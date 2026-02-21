# M1: Agent Lifecycle Redesigned

> **⚠️ INTERNAL ONLY — DO NOT PUBLISH**

**Milestone:** M1 · **Issue:** #47
**Date:** Sprint 1

---

## The Problem with Ad-Hoc Spawning

In beta Squad, agents were spawned by writing a markdown file and hoping the coordinator noticed. There was no state machine, no idle cleanup, no concurrency limit. If an agent crashed, it stayed in limbo. If ten agents spawned simultaneously, the system slowed to a crawl with no backpressure.

M1 replaced all of that with `AgentLifecycleManager` — a proper state machine with deterministic transitions.

## Charter Compilation

Every agent starts with a charter: a markdown document describing its identity, expertise, tools, and model preferences. `compileCharter()` transforms this prose into a `CompiledCharter` — a typed object with resolved model references, validated tool lists, and structured metadata. `parseCharterMarkdown()` extracts sections; `compileCharterFull()` layers on model resolution and tool validation. The charter is the agent's contract with the system.

## Model Selection

`resolveModel()` implements 4-layer priority resolution: user override → charter preference → task-based auto-selection → system default. Each layer can specify a `ModelTier` (`premium`, `standard`, `fast`), and the `ModelRegistry` provides fallback chains so degradation is graceful. A docs task defaults to `fast`; a complex architecture task escalates to `premium`.

## Spawn Pipeline

The lifecycle state machine enforces: `pending → spawning → active → idle → destroyed`. `AgentLifecycleManager` tracks every agent's state, manages `SessionPool` allocation, and runs idle reaping on a configurable interval. `SpawnAgentOptions` lets callers set priority, model overrides, and timeout. `AgentHandle` provides `sendMessage()` and `destroy()` for direct interaction.

## Hooks Integration

The spawn pipeline fires events at every transition. `HookPipeline` intercepts pre-tool and post-tool calls, enforcing policies like `ReviewerLockoutHook` (agents can't edit files they review). Five built-in policies ship with M1, and custom hooks plug in via `PreToolUseHook` and `PostToolUseHook` types.

## Result

M1 gave agents a predictable lifecycle with observable state, bounded concurrency, and policy enforcement at every step.
