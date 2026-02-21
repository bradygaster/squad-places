# M3: Routing Redone — SDK-Native Policy Engine

> **⚠️ INTERNAL ONLY — DO NOT PUBLISH**

**Milestone:** M3 · **Issue:** #49
**Date:** Sprint 3

---

## From Markdown Tables to Compiled Rules

Beta routing was a markdown table: columns for pattern, target agent, and priority. The coordinator parsed it on every message. Regex errors surfaced as silent misroutes. There was no concept of response tiers, no parallel fan-out, and no way to short-circuit trivial requests.

M3 rebuilt routing as a compiled, policy-driven dispatch engine inside the `Coordinator`.

## Coordinator Dispatch

The `Coordinator` class is the central orchestrator. When a message arrives, `compileRoutingRules()` has already transformed `RoutingConfig` into a precompiled router with regex patterns and target mappings. `matchRoute()` evaluates the message against compiled rules. `matchIssueLabels()` handles label-based routing for GitHub issue workflows. The result is a `RoutingDecision` — a typed object specifying which agents handle the work, whether they run in parallel, and what response tier applies.

## Response Tiers

Not every request needs a full agent session. M3 introduced four `ResponseTier` levels:

- **direct** — Coordinator answers immediately (factual lookups, status queries)
- **lightweight** — Single agent, fast model, minimal context
- **standard** — Single agent, full model, complete context
- **full** — Multiple agents via `spawnParallel()`, results aggregated

`selectResponseTier()` classifies incoming messages by complexity. The coordinator checks if a direct response is possible before spawning any agents — saving latency and compute for simple questions.

## Policy Enforcement

Routing decisions flow through `HookPipeline` before execution. `ReviewerLockoutHook` prevents conflicts of interest. File guards restrict which agents can write to sensitive paths. Shell restrictions limit command execution. Rate limits prevent runaway agent spawning. PII filters redact sensitive data before it reaches models.

## Result

M3 turned routing from a fragile text parse into a compiled, tiered, policy-enforced dispatch system — faster, safer, and observable.
