# What's Next for Squad

> **⚠️ INTERNAL ONLY — DO NOT PUBLISH**

**Milestone:** M6 · **Issue:** #55 (M6-17)
**Date:** Sprint 6

---

## The Foundation Is Set

v0.6.0 gives Squad a typed, tested, extensible runtime. Six milestones, 80+ work items, 1,400+ tests, and full feature parity with beta. But the replatform was never the goal — it was the prerequisite. Here's where we go next.

## Marketplace GA

The marketplace module ships with export, import, browse, install, and seven security rules. For GA, we're adding: **verified publisher badges**, **usage analytics** surfaced to authors, **dependency tracking** between agents, and **automatic update notifications**. The `MarketplaceBackend` will move from local-first to a hosted registry with CDN-backed distribution.

## More Agent Sources

`AgentRegistry` already supports local, GitHub, and marketplace sources. Next: **GitLab and Azure DevOps** sources for enterprise teams, **URL-based sources** for quick sharing, and **organization-scoped registries** that let companies maintain internal agent catalogs without exposing them to the public marketplace.

## Community Plugins

The `HookPipeline` and `ToolRegistry` are designed for extension. We'll publish a plugin API that lets the community add custom hooks (compliance checks, audit logging, cost gates) and tools (database queries, API integrations, custom scaffolding) without forking the runtime.

## Multi-Model Intelligence

`ModelRegistry` catalogs 17 models today. The next step is **task-aware model selection**: the coordinator analyzes task complexity and automatically picks the right tier. Simple formatting tasks use fast models; architecture decisions use premium. `CostTracker` data feeds back into selection, so teams can set budget constraints and let the system optimize within them.

## Cost Optimization

Speaking of cost — streaming cost events are already in the pipeline. We'll add **budget alerts**, **per-agent cost caps**, and **session cost forecasting** based on historical patterns. Teams will see projected spend before approving a multi-agent task.

## Enterprise Features

Larger organizations need: **SSO-integrated authentication**, **role-based access control** for agent management, **audit trails** for every agent action, and **compliance policies** that enforce tool restrictions at the organization level. The `PolicyConfig` system in `HookPipeline` is the foundation — enterprise features extend it with organization-scoped rules.

## Get Involved

Squad is built in the open. The best way to shape what comes next is to use v0.6.0, break things, and tell us what matters. File issues, propose agents, contribute skills. The replatform gave us the foundation — the community decides what we build on it.
