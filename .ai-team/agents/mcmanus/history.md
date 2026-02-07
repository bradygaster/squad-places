# Project Context

- **Owner:** bradygaster (bradygaster@users.noreply.github.com)
- **Project:** Squad — AI agent teams that grow with your code. Democratizing multi-agent development on GitHub Copilot. Mission: beat the industry to what customers need next.
- **Stack:** Node.js, GitHub Copilot CLI, multi-agent orchestration
- **Created:** 2026-02-07

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### README structure and messaging
- **README.md** is the primary developer-facing doc — Quick Start (3 steps), parallel work explanation, context budget table (real numbers), mermaid diagram for architecture visualization
- Strong hook: "It's not a chatbot wearing hats" — personality-driven messaging that differentiates Squad
- File: `README.md` (root)

### Documentation assets
- **docs/sample-prompts.md** contains 16 production-ready project demos (pomodoro timer → .NET migration) — not linked from README yet, high value for onboarding
- File: `docs/sample-prompts.md`

### Install flow and templates
- **index.js** is the npx installer — copies `.github/agents/squad.agent.md` (coordinator) and `templates/` to `.ai-team-templates/`
- Creates placeholder dirs: `.ai-team/decisions/inbox/`, `.ai-team/orchestration-log/`, `.ai-team/casting/`
- Install output is minimal (just checkmarks) — no explanation of what was created or why
- Files: `index.js`, `templates/` (charter.md, history.md, roster.md, routing.md)

### Casting system (under-documented)
- Squad uses thematic casting (The Usual Suspects, Alien, Ocean's Eleven) for persistent agent names — stored in `.ai-team/casting/registry.json`
- This is a signature feature but only mentioned briefly in README ("persistent thematic cast")
- Casting state: `policy.json` (config), `registry.json` (name mappings), `history.json` (usage history)

### Coordinator agent
- **squad.agent.md** is the coordinator — lives in `.github/agents/`, orchestrates all spawns
- Init Mode (no team yet) vs Team Mode (team exists in `.ai-team/team.md`)
- Always spawns via `task` tool, never simulates agent work inline
- File: `.github/agents/squad.agent.md` (large file, 32KB+)

### Key gaps identified
- No "Why Squad?" value prop section in README — explains *what* and *how* but not *why*
- Quick Start assumes familiarity with GitHub Copilot `/agents` command
- "What Gets Created" section is file-tree-focused, not workflow-focused
- No troubleshooting section (what if squad.agent.md doesn't appear in /agents?)
- No video/GIF demo — devs want to *see* parallel work in action
- Sample prompts doc is hidden (not linked from README)

### Voice and personality
- Squad's voice is confident, direct, opinionated — but README feels slightly sterile
- Opportunity to push personality earlier and louder (e.g., casting as a feature, not Easter egg)
