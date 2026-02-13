---
name: "cross-platform-agent-spawning"
description: "How to spawn sub-agents across CLI and VS Code using platform detection and conditional logic"
domain: "platform-integration"
confidence: "high"
source: "earned"
---

## Context

Squad runs on multiple Copilot surfaces (CLI, VS Code, potentially JetBrains and GitHub.com). Each surface provides a different sub-agent spawning mechanism. The coordinator (`squad.agent.md`) must detect the platform at runtime and use the correct tool. This skill applies whenever Squad's coordinator needs to spawn agents on any platform.

## Patterns

- **Detect platform via tool availability.** At session start, check which spawning tool is present:
  - `task` tool available → CLI mode
  - `agent` / `runSubagent` tool available → VS Code mode
  - Neither available → Fallback inline mode (no delegation)
- **CLI spawning:** Use `task` tool with structured parameters: `agent_type`, `mode`, `model`, `description`, `prompt`. Supports `mode: "background"` for async fan-out. Results collected via `read_agent`.
- **VS Code spawning:** Use `runSubagent`/`agent` tool with prompt text referencing custom agents by name. Configuration lives in `.agent.md` files (not parameters). Supports parallel spawning (multiple sub-agents in same turn). All sub-agents are synchronous individually.
- **Role mapping:** CLI uses `agent_type` ("explore", "task", "general-purpose", "code-review"). VS Code uses custom `.agent.md` files with `name`, `tools`, and `model` frontmatter. Define one `.agent.md` file per Squad role.
- **Model selection divergence:** CLI passes `model` as a spawn parameter. VS Code declares `model` in `.agent.md` frontmatter. Per-agent model selection in VS Code requires the experimental setting `chat.customAgentInSubagent.enabled: true`.
- **MCP tool inheritance divergence:** CLI sub-agents do NOT inherit parent MCP tools. VS Code sub-agents DO inherit parent tools by default. Use `tools` restrictions in `.agent.md` to limit access for security-sensitive roles.
- **Parallel fan-out:** CLI uses multiple `task` calls with `mode: "background"` in a single response. VS Code uses multiple sub-agent requests in a single prompt turn. Both achieve concurrency; collection semantics differ (CLI: incremental via `read_agent`; VS Code: all-at-once).

## Examples

**Platform detection in coordinator instructions:**
```markdown
### Platform-Aware Spawning
Detect your platform at session start:
- If `task` tool is available → CLI mode. Use `task` tool with structured parameters.
- If `agent` tool is available → VS Code mode. Use custom agents via `runSubagent`.
- If neither → Fallback inline mode. Work without delegation.
```

**CLI spawn:**
```yaml
agent_type: "general-purpose"
model: "claude-sonnet-4"
mode: "background"
description: "Fenster: implement auth API"
prompt: "You are Fenster, the Core Dev..."
```

**VS Code equivalent (custom agent file):**
```yaml
---
name: Squad Worker
tools: ['editFiles', 'search', 'read', 'terminalLastCommand']
model: 'Claude Sonnet 4 (copilot)'
user-invokable: false
---
You are a Squad team member. Execute the assigned task.
```

**VS Code spawn prompt:**
```
Use the Squad Worker agent to implement the auth API.
Task: You are Fenster, the Core Dev...
```

## Anti-Patterns

- Hardcoding `task` tool usage without platform detection — breaks on VS Code
- Assuming `mode: "background"` exists everywhere — VS Code has no equivalent; use parallel spawning instead
- Passing `model` as a spawn parameter in VS Code — it must be in the `.agent.md` frontmatter
- Assuming sub-agents lack MCP tools in VS Code — they inherit everything by default, which may be too permissive
- Creating a compatibility shim that translates `task` calls to `runSubagent` at runtime — too fragile. Use conditional logic in the coordinator instructions instead
- Building a code-level abstraction layer for spawn parity — prompt-level conditional instructions in `squad.agent.md` are sufficient and more maintainable
- Skipping the Response Order bug workaround on VS Code without testing — the silent success bug may manifest differently; keep the block until empirically verified unnecessary

## Validated Findings (Proposal 032b, 2026-02-13)

Full parameter parity analysis confirmed all patterns above. Additional validated details:
- **5 spawn patterns map successfully:** Standard, Lightweight, Explore, Scribe, Ceremony Facilitator
- **VS Code `runSubagent` invocation control:** `user-invokable: false` and `disable-model-invocation: true` provide Squad-relevant restrictions for internal agents
- **VS Code `agents` frontmatter:** Coordinator can restrict which custom agents subagents can invoke — useful for Squad role isolation
- **Scribe on VS Code:** Becomes synchronous (blocking). Mitigation: batch Scribe as last subagent in parallel group
- **Nuclear model fallback on VS Code:** Omit custom agent → session model applies (equivalent to CLI's "omit model param")
