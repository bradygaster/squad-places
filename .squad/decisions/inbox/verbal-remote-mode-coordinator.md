# Decision: Remote Squad Mode Awareness in Coordinator Prompt

**Date:** 2025-07-18
**Author:** Verbal
**Related:** #313

## Context

The SDK is adding remote squad mode — where team identity files (agents, casting, skills) live in an external repository and project-scoped files (decisions, logs) stay local. The coordinator prompt (`squad.agent.md`) needed to know about this third resolution strategy so it can correctly resolve paths and pass them to spawned agents.

## Decision

1. Added remote squad mode as a third strategy in the Worktree Awareness section, after worktree-local and main-checkout.
2. Introduced `PROJECT_ROOT` as a second variable in spawn templates. `TEAM_ROOT` points to team identity; `PROJECT_ROOT` points to project-local `.squad/`. In local mode they're identical.
3. Added @copilot incompatibility note — remote mode requires filesystem traversal that the GitHub Copilot coding agent cannot perform.

## Rationale

- The coordinator is the single point of path resolution. If it doesn't know about remote mode, no spawned agent will get correct paths.
- Two variables (`TEAM_ROOT` + `PROJECT_ROOT`) keep the split explicit rather than requiring agents to parse config.json themselves.
- The @copilot note prevents confusion when users try to assign remote-mode work to the coding agent.
