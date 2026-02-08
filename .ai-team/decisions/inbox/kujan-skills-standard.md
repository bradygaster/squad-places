### 2026-02-09: Adopt Agent Skills Open Standard with MCP tool declarations

**By:** Kujan

**What:** Squad will adopt the Agent Skills Open Standard (agentskills.io) for its skills system instead of the previously proposed flat `skills.md` file. Skills use the standard SKILL.md format with YAML frontmatter, standard directory layout (SKILL.md + scripts/ + references/ + assets/), and progressive disclosure. MCP tool dependencies are declared via `metadata.mcp-servers` in SKILL.md frontmatter. Two skill categories: built-in (shipped with Squad, upgradable via `create-squad upgrade`, prefixed `squad-`) and learned (created by agents/users, never overwritten by upgrades). Skills discovery uses the standard's `<available_skills>` XML injection into spawn prompts.

**Why:** Brady clarified that "skills" means Claude-and-Copilot-compliant skills adhering to the Anthropic SKILL.md standard. Adopting the open standard gives Squad three advantages: (1) ecosystem compatibility — skills published on agentskills.io can be dropped into `.ai-team/skills/` and just work, (2) progressive disclosure — coordinator loads only name + description at discovery (~100 tokens per skill), full SKILL.md on activation, keeping spawn prompts lean, (3) future-proofing — the standard's extensible metadata field cleanly accommodates our MCP server declarations without inventing a proprietary format. The standard's filesystem-native design (directories with markdown files) aligns perfectly with Squad's filesystem-backed memory architecture.

**Key decisions within this decision:**
- Skills directory layout: `.ai-team/skills/` (team-wide) + `.ai-team/agents/{name}/skills/` (per-agent)
- MCP declaration: `metadata.mcp-servers` array in SKILL.md frontmatter with `name`, `reason`, `optional`, and `fallback` fields
- Built-in skills: shipped in `templates/skills/`, prefixed `squad-`, upgradable
- Learned skills: agent-created or user-taught, never touched by upgrades
- Skills directory layout is a frozen API contract (like charter file paths)
- Progressive disclosure over full inlining — XML summary at discovery, full read on activation
- No MCP auto-configuration (security concern) — skills document requirements, users configure servers

**Proposal:** `docs/proposals/012-skills-platform-and-copilot-integration.md` (Revision 2)
