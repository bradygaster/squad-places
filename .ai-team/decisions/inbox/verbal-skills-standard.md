# Decision: Skills System — Agent Skills Standard + MCP Tool Declarations

**By:** Verbal  
**Date:** 2026-02-09  
**Proposal:** 010-skills-system.md (Revision 2)

**What:** Squad skills will use the **Agent Skills standard** (SKILL.md format) instead of a custom format. Skills live in `.ai-team/skills/{skill-name}/SKILL.md` as standard directories — not per-agent `skills.md` files. Each SKILL.md has YAML frontmatter (`name`, `description` required; `metadata.confidence`, `metadata.projects-applied`, `metadata.acquired-by`, `metadata.mcp-tools` for Squad extensions) and a markdown body with earned knowledge. The coordinator injects `<available_skills>` XML into spawn prompts for progressive disclosure (~50 tokens per skill at discovery, full SKILL.md loaded on demand). MCP tool dependencies are declared in `metadata.mcp-tools` — skills specify which MCP servers they need and why. Skills are generated organically from real work (agents create SKILL.md after completing tasks), can be explicitly taught, and follow a lifecycle: acquisition → reinforcement → correction → deprecation. The standard format means skills are portable beyond Squad — works in Claude Code, Copilot, any compliant tool.

**Why:** Brady's directive: *"skills that adhere to the anthropic 'skills.md' way"* and *"tell copilot which mcp tools our skills would need."* The original Proposal 010 invented a custom format. This revision adopts the open standard, gaining interoperability (skills work everywhere), progressive disclosure (cheap discovery, on-demand activation), and MCP awareness (skills declare their tool dependencies). Squad's unique value isn't the format — it's that Squad GENERATES standard-compliant skills from real work. Everyone else authors them by hand. The flat `skills/` directory replaces per-agent skill files because skills are team knowledge, not agent-siloed. MCP declarations in metadata are spec-compliant (arbitrary key-value) and solve the tool-wiring problem. Implementation phased across 6 releases, starting with template + instruction changes only.

**Key changes from Revision 1:**
- Storage: `skills/*/SKILL.md` (standard directories) replaces `agents/*/skills.md` (per-agent markdown)
- Format: YAML frontmatter + markdown body (standard) replaces freeform markdown (custom)
- Context: `<available_skills>` XML injection (standard progressive disclosure) replaces full content inlining
- MCP: `metadata.mcp-tools` array for declaring MCP server dependencies (NEW — addresses Brady's request)
- Portability: Skills work in any Agent Skills-compliant tool (Claude Code, Copilot, etc.)

**Depends on:** Proposal 008 (Portable Squads) for export/import; Proposal 007 (Persistence) for progressive summarization; Proposal 012 (Skills Platform) for Kujan's forwardability analysis.
