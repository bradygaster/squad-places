# Strausz — VS Code Extension Expert

## Project Context

- **Project:** Squad — AI agent teams that grow with your code. Democratizing multi-agent development on GitHub Copilot.
- **Owner:** Brady (bradygaster)
- **Stack:** Node.js, GitHub Copilot CLI, multi-agent orchestration
- **Universe:** The Usual Suspects

## Learnings

- Joined the team 2026-02-13 to handle VS Code client parity (issues #32, #33, #34)
- VS Code is #1 priority for Copilot client parity per Brady's directive — JetBrains and GitHub.com are deferred to v0.5.0
- Keaton decomposed #10 into 5 sub-issues: #32 (runSubagent), #33 (file discovery), #34 (model selection), #35 (compatibility matrix), #36 (deferred surfaces)
- Key platform constraint: sub-agents spawned via `task` tool may NOT inherit MCP tools from parent session — this affects VS Code integration design
- Kujan handles Copilot SDK/CLI patterns; I handle VS Code extension-specific concerns — we collaborate on the overlap
- Squad is zero-dependency (no node_modules) — any VS Code integration must respect this constraint

### runSubagent API Research (2026-02-14, Issue #32)

- VS Code uses `runSubagent` (tool name: `agent`) instead of CLI `task` tool for sub-agent spawning
- `runSubagent` is **prompt-driven**, not parameter-driven — no `agent_type`, `mode`, or `model` parameters. Configuration lives in `.agent.md` files
- Sub-agents are synchronous (blocking) but VS Code supports **parallel spawning** — multiple sub-agents run concurrently when requested in the same turn
- No `mode: "background"` equivalent — Squad's Eager Execution maps to batched parallel sub-agent invocations
- **MCP tool inheritance is the default in VS Code** — sub-agents inherit parent's tools. This is the OPPOSITE of CLI behavior (CLI sub-agents do NOT inherit MCP tools). Net positive for Squad
- Model selection: via `.agent.md` `model` frontmatter field, not spawn-time parameter. Experimental setting required: `chat.customAgentInSubagent.enabled: true`
- Platform detection strategy: check tool availability — `task` tool = CLI, `agent`/`runSubagent` tool = VS Code, neither = fallback inline mode
- Custom agents (`.agent.md` files) provide **more granular control** than CLI agent types: tool restrictions, model selection, visibility control, handoff workflows
- Squad will need `.agent.md` files per role (worker, explorer, reviewer, runner) to replace CLI `agent_type` mapping
- Key VS Code-only capabilities: `agents` property (restrict which sub-agents a coordinator can spawn), `handoffs` (sequential workflow transitions), `user-invokable`/`disable-model-invocation` (visibility control)
- Open question: structured parameter passing to `runSubagent` is not supported — prompt is the only input channel
