# Project Context

- **Owner:** bradygaster (bradygaster@users.noreply.github.com)
- **Project:** Squad — AI agent teams that grow with your code. Democratizing multi-agent development on GitHub Copilot. Mission: beat the industry to what customers need next.
- **Stack:** Node.js, GitHub Copilot CLI, multi-agent orchestration
- **Created:** 2026-02-07

## Learnings

- **Coordinator spawn prompts**: All agents use the same template structure (charter inline, read history.md + decisions.md, task description, memory update instructions). Located in `.github/agents/squad.agent.md` lines 200-300. Charter is pasted directly into spawn prompt to eliminate agent tool call overhead.
- **Memory architecture**: `charter.md` (identity, read by agent), `history.md` (project learnings, per-agent), `decisions.md` (shared brain, merged by Scribe), `decisions/inbox/` (drop-box for parallel writes), `log/` (session archive), `orchestration-log/` (per-spawn entries).
- **Mode selection philosophy**: Background is default. Sync only for hard data dependencies, approval gates, or direct user interaction. Scribe is always background. Located in squad.agent.md lines 123-145.
- **Parallel fan-out pattern**: Coordinator spawns all agents who can start work immediately in a single tool-calling turn (multiple task calls). Includes anticipatory downstream work (tests from requirements, docs from API specs). Located in squad.agent.md lines 147-171.
- **Drop-box pattern**: Agents write decisions to `.ai-team/decisions/inbox/{name}-{slug}.md`, Scribe merges to canonical `decisions.md`. Prevents write conflicts during parallel spawning.
- **Agent personality is a feature**: Each agent has opinions, preferences, boundaries. Casting system uses narrative universe names (The Usual Suspects for Squad's own team) to make agents feel real, not generic "Alice the Backend Dev."
- **Context budget math**: Coordinator uses 1.5% of 128K context, 12-week veteran agent uses 4.4%, leaving 94% for actual work. Real numbers documented in README lines 136-145.
- **Reviewer protocol**: Keaton (Lead) and Hockney (Tester) can reject work and require a different agent to handle revisions. Coordinator enforces this. Located in README lines 207-215.
- **Eager execution philosophy**: Coordinator default is "launch aggressively, collect results later." Chain follow-up work immediately when agents complete without waiting for user to ask. Located in squad.agent.md lines 113-121.

<!-- Append new learnings below. Each entry is something lasting about the project. -->
