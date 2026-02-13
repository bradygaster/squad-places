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
