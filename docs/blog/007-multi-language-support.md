# Skills System: Domain-Specific Knowledge On Demand

> **⚠️ INTERNAL ONLY — DO NOT PUBLISH**

**Milestone:** M3 · **Issue:** #50 (M3-16)
**Date:** Sprint 3

---

## The Problem With Always-On Context

Early Squad agents carried everything in their system prompt — testing conventions, API patterns, deployment runbooks, style guides. It worked for small teams, but as agent charters grew, so did token pressure. An agent loaded with six domains of expertise spent half its context window on knowledge it rarely used.

We needed a way to load domain knowledge **on demand**: present when relevant, invisible when not.

## How Skills Work

The skills system introduces three concepts: **SkillRegistry**, **SKILL.md files**, and **keyword matching with role affinity**.

### SKILL.md Format

Each skill lives in its own directory as a `SKILL.md` file with YAML frontmatter:

```yaml
---
name: TypeScript Testing
domain: testing
triggers: [vitest, jest, test, spec]
roles: [tester, developer]
---
Markdown body with domain knowledge…
```

The `parseFrontmatter()` function extracts metadata; the body becomes injectable context.

### SkillRegistry

`SkillRegistry` holds all registered skills in memory. When the coordinator routes a task, it calls `matchSkills(task, agentRole)` to find relevant knowledge. Scoring is straightforward:

- **+0.5** per trigger keyword found in the task text (capped at 0.7)
- **+0.3** if the agent's role matches the skill's `agentRoles` list
- Scores clamped to [0, 1], sorted descending

Only skills with a score above zero are returned. The coordinator then calls `loadSkill(id)` to get the markdown content and injects it into the agent's context for that request alone.

### Pluggable Skill Sources

`SkillSourceRegistry` (M5-5) adds pluggable discovery. `LocalSkillSource` reads from `.squad/skills/` on disk; `GitHubSkillSource` fetches from remote repositories. Sources are priority-ranked — local skills shadow remote ones, so teams can override community skills without forking.

## Why It Matters

Skills decouple **what an agent knows** from **what an agent is**. A testing agent doesn't need deployment knowledge in its charter — it loads the deployment skill only when a task mentions "deploy" or "CI". Context pressure drops, response quality improves, and adding new domain knowledge is a file creation, not a prompt rewrite.
