# Decision: Projects V2 Integration Pattern

**Proposed by:** Kujan  
**Date:** 2026-02-10  
**Context:** Issue #6 (londospark), Proposal 033a

## Decisions

### 1. Use `gh project *` CLI commands, not raw GraphQL or a GraphQL client library
- Zero-dependency constraint preserved
- `gh` CLI wraps all Projects V2 GraphQL mutations behind ergonomic flags
- No npm dependency needed (Issue #6 proposes "a GraphQL client" — reject this)

### 2. Projects V2 integration is opt-in, not core
- Implement as a skill (`github-project-boards`), not code in `index.js`
- Graceful degradation when `project` scope is missing
- Agents detect scope at runtime via `gh auth status`

### 3. Boards are visualization, labels are the state machine
- Label changes trigger Actions workflows; board column moves do not
- Board mirrors label state but is not the source of truth
- Aligns with existing `label-driven-workflow` skill anti-pattern guidance

### 4. Zero MCP server coverage — `gh` CLI is the sole channel
- Verified: 0/17 MCP tools support any Projects V2 operation
- All read + write operations go through `gh project *` commands
- If MCP adds Projects V2 tools later, they supplement but don't replace `gh` CLI

### 5. Provider abstraction uses prompt-level command templates (per 032a)
- GitHub: `gh project *`
- Azure DevOps: `az boards *` (boards are built-in, no create step needed)
- GitLab: Label-driven boards (existing label workflow = the board)
- No JS interface needed — coordinator prompt contains provider-specific command templates

### 6. Target v0.4.0, not v0.3.0
- v0.3.0 is "ONE feature" (proposals as GitHub Issues) per Brady's directive
- Projects V2 boards depend on Issue integration being solid first
- Token scope (`gh auth refresh -s project`) is a prerequisite Brady hasn't run yet
