# Decision: Agent Repository Architecture — Elevated to Core Foundation

**By:** Keaton (Lead)
**Date:** 2026-02-21
**Status:** Decided (Brady directive)
**Scope:** Cross-cutting — affects PRDs 1, 4, 5, 7, 11, 14

---

## Context

Brady elevated "Agent Repositories" from a Phase 3 directive to a **non-negotiable core architectural requirement**. The concept: agents/team members can be pulled from pluggable sources — local disk, other GitHub repos, cloud APIs, server endpoints. The architecture must support this from day one, not bolt it on later.

## Architectural Principle

**Agent resolution is pluggable from day one.**

```typescript
interface AgentSource {
  list(): Promise<AgentManifest[]>;
  load(name: string): Promise<AgentConfig>;
  type: 'local' | 'github' | 'api' | 'custom';
}
```

First implementation: `LocalAgentSource` reads from `.squad/agents/`. But `SquadClient`, the coordinator, and the session lifecycle MUST use the `AgentSource` interface, never hardcoded paths.

## PRD Impact Analysis

### PRD 1 — SDK Orchestration Runtime (Fenster)
**Impact: HIGH — defines the interface**

The adapter layer (`src/adapter/types.ts`) must define:
- `AgentSource` interface
- `AgentManifest` type (name, role, source URI, version)
- `AgentSourceRegistry` — manages multiple sources, priority ordering

`SquadClient` gains:
```typescript
class SquadClient {
  registerAgentSource(source: AgentSource): void;
  resolveAgent(name: string): Promise<AgentConfig>;
  listAvailableAgents(): Promise<AgentManifest[]>;
}
```

Currently missing: no abstraction for where agents come from. All code assumes `.squad/agents/{name}/charter.md` on disk.

### PRD 4 — Agent Session Lifecycle (Verbal)
**Impact: HIGH — charter compilation must abstract source**

`compileCharter()` currently hardcodes `readFileSync(charterPath)`. Must change to:
```typescript
// Before (hardcoded):
const markdown = readFileSync(charterPath, 'utf-8');

// After (abstracted):
const agentConfig = await client.resolveAgent(agentName);
const markdown = agentConfig.charter;
```

Session pool's `compiledAgents` map must load agents through `AgentSource`, not filesystem traversal. `historyPath` becomes source-dependent — local agents have local history, remote agents may not have writable history.

### PRD 5 — Coordinator Replatform (Keaton)
**Impact: HIGH — team loading resolves through sources**

`loadTeam(teamRoot)` in `coordinator.ts` currently scans `.squad/agents/` directory. Must change to:
```typescript
const team = await loadTeam(client.getAgentSources());
```

The router must handle agents from different sources — an agent from a GitHub repo is routable the same way as a local agent. Fan-out to remote agents may have different latency characteristics.

### PRD 7 — Skills Migration (Verbal)
**Impact: MEDIUM — skills should mirror the pattern**

If agents can come from repositories, skills should too. The `SkillManifest.author.source` field already hints at this (`'github:org/repo/skills/skill-name'`). A `SkillSource` interface mirroring `AgentSource` is the natural extension:
```typescript
interface SkillSource {
  list(): Promise<SkillManifest[]>;
  load(name: string): Promise<SkillContent>;
  type: 'local' | 'github' | 'api';
}
```

PRD 7 already has `squad skill import github:someorg/repo/skills/advanced-testing` — this should use `SkillSource`, not ad-hoc import logic.

### PRD 11 — Casting System v2 (Verbal)
**Impact: MEDIUM — cross-repo casting must resolve across sources**

Cross-repo casting awareness (`importWithCastingAwareness()`) needs to know what agents exist across all sources, not just local. The casting module must accept an `AgentSource[]` to resolve available agents when:
- Checking for name collisions across repos
- Casting imported agents with overflow strategies
- Generating `customAgents` array from multi-source teams

### PRD 14 — Clean-Slate Architecture (Keaton)
**Impact: HIGH — directory structure and config must accommodate**

`squad.config.ts` must define `agentSources`:
```typescript
export default defineSquadConfig({
  agentSources: [
    { type: 'local', path: '.squad/agents/' },
    { type: 'github', repo: 'org/shared-agents', path: 'agents/' },
    { type: 'api', url: 'https://registry.example.com/agents' },
  ],
  // ...
});
```

The `.squad/` directory structure needs:
- `.squad/agents/` — local agent configs (unchanged)
- `.squad/.cache/remote-agents/` — cached remote agent configs (git-ignored)
- Source resolution order defined in config

## New Dependency Relationships

```
PRD 1 → defines AgentSource interface
PRD 15 → implements LocalAgentSource + source registry (NEW PRD)
PRD 4 → uses AgentSource for charter compilation (depends on PRD 1, 15)
PRD 5 → uses AgentSource for team loading (depends on PRD 1-4, 15)
PRD 7 → mirrors pattern with SkillSource (depends on PRD 4, 15)
PRD 11 → uses AgentSource for cross-repo casting (depends on PRD 4, 15)
PRD 14 → defines agentSources[] in config (depends on PRD 1, 15)
```

## What Changed in the Plan

1. **PRD 15 (Agent Repository Architecture) created** — cross-cutting PRD, starts Phase 1, extends through Phase 3
2. **PRD index updated** — PRD 15 added to Phase 1 table, dependency graph updated, execution order adjusted
3. **GitHub issue #16 updated** — elevated from Phase 3 to Phase 1 milestone, body reflects non-negotiable status
4. **PRDs 4, 7, 11, 14 gain PRD 15 as dependency** — they must use AgentSource, not hardcoded paths

## Trade-offs

**Give up:**
- Slightly more complexity in Phase 1 — Fenster must define AgentSource interface alongside SDK adapter
- LocalAgentSource is the only implementation until Phase 2 — the interface exists but has one backend

**Get:**
- No rewrite when remote sources ship in Phase 2-3
- Clean separation of agent resolution from agent consumption
- Every PRD built on the abstraction from day one
- Skills naturally follow the same pattern (SkillSource)
- Cross-repo casting works without retrofit

**Risk:**
- Over-engineering for Phase 1 (only one source type). Mitigated by keeping LocalAgentSource simple — it's literally what we do today, wrapped in an interface.
