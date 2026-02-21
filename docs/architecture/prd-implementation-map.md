# PRD & Squad Places — Implementation Map

> ⚠️ **INTERNAL ONLY** — This document is for maintainer reference. Do not publish externally.

This map connects each PRD and Squad Places architecture issue to the code that implements it. All items were delivered across milestones M0–M6.

| # | Issue | Title | Implementation File(s) | Milestone | Status |
|---|-------|-------|------------------------|-----------|--------|
| 1 | #20 | PRD 1: Define AgentSource interface | `src/config/agent-source.ts` | M2-5 | Implemented — `AgentSource` interface defines the provider contract |
| 2 | #21 | PRD 15: Implement LocalAgentSource | `src/config/agent-source.ts` | M2-8 | Implemented — `LocalAgentSource` reads agents from the local repo |
| 3 | #22 | PRD 4: Abstract charter compilation | `src/agents/charter-compiler.ts` | M2-9 | Implemented — `compileCharterFull` assembles charters from fragments |
| 4 | #23 | PRD 5: Coordinator loadTeam() | `src/coordinator/coordinator.ts` | M3-1 | Implemented — `SquadCoordinator` loads and manages the full team |
| 5 | #24 | PRD 7: SkillSource interface | `src/skills/skill-source.ts` | M5-5 | Implemented — `SkillSource` interface for pluggable skill providers |
| 6 | #25 | PRD 11: Cross-repo casting | `src/casting/casting-engine.ts`, `src/config/agent-source.ts` | M3-2, M5-4 | Implemented — casting engine resolves agents across sources |
| 7 | #26 | PRD 14: agentSources[] in config | `src/config/schema.ts` | M2-1 | Implemented — `SquadConfig.agents` array in config schema |
| 8 | #27 | Squad Places: Private repo config | `src/sharing/agent-repo.ts` | M5-7 | Implemented — `AgentRepoConfig` for private repository settings |
| 9 | #28 | Squad Places: CLI commands | `src/sharing/export.ts`, `src/sharing/import.ts` | M5-1, M5-2 | Implemented — export and import CLI commands for agent portability |
| 10 | #29 | Squad Places: GitAgentSource | `src/config/agent-source.ts` | M5-4 | Implemented — `GitHubAgentSource` fetches agents from remote repos |
| 11 | #30 | Squad Places: Agent manifest schema | `src/marketplace/index.ts`, `src/marketplace/schema.ts` | M4-8, M5-10 | Implemented — manifest schema for marketplace agent listings |
| 12 | #31 | PRD 17: @copilot agent management | `src/agents/lifecycle.ts`, `src/agents/onboarding.ts` | M1-7, M2-10 | Implemented — lifecycle and onboarding flows for managed agents |
| 13 | #32 | PRD 16: Export/Import & portability | `src/sharing/` module | M5-1 – M5-9 | Implemented — full sharing module covers export, import, and sync |
| 14 | #33 | PRD 15: Agent Repository Architecture | `src/sharing/agent-repo.ts`, `src/config/agent-source.ts` | M5-7, M2-5 | Implemented — agent repo architecture spans sharing and config layers |
