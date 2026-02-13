# Decision: VS Code Model & Background Parity Strategy

**By:** Kujan
**Date:** 2026-02-14
**Issue:** #34
**Proposal:** 034a

## Decision

VS Code model selection and background mode parity follows a three-phase approach:

1. **Phase 1 (v0.4.0):** Accept session model for all VS Code spawns. Use `runSubagent` (anonymous). No custom agent files required. Parallel sync subagents replace background mode.

2. **Phase 2 (v0.5.0):** Generate model-tier `.agent.md` files during `squad init` — `squad-fast` (haiku), `squad-standard` (sonnet), `squad-premium` (opus). Use `agent` tool to invoke named agents for model control.

3. **Phase 3 (v0.6.0+):** Per-role agent files if custom agent subagent support stabilizes out of experimental.

## Key Constraints

- `runSubagent` does NOT accept `model` param — frontmatter only
- VS Code "Background Agents" ≠ CLI `mode: "background"` — different concept entirely
- `chat.customAgentInSubagent.enabled` is still experimental
- Model names differ: CLI uses API names, VS Code uses display names with `(copilot)` suffix

## What This Means for the Team

- **Fenster:** When implementing VS Code spawn logic in `squad.agent.md`, use prompt-level conditional instructions (§5 of proposal). No code-level abstraction.
- **Verbal:** Spawn templates need VS Code-specific variants. Key differences: drop `mode`, `model`, `agent_type`, `description` params. Add "batch Scribe last" rule.
- **Keaton:** Phase 2 requires `squad init` to detect VS Code and generate `.agent.md` files. Plan for v0.5.0.
