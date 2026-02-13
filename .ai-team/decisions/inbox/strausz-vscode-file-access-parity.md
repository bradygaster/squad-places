# Decision: VS Code File Discovery and .ai-team/ Access Requires No Code Changes

**Author:** Strausz
**Date:** 2026-02-15
**Issue:** #33

## Decision

Squad's file discovery and `.ai-team/` access work in VS Code with zero code changes to `squad.agent.md`. The instruction-level abstraction (describing operations like "read this file" rather than hardcoding tool names like `view` or `readFile`) naturally works across both CLI and VS Code surfaces.

## Rationale

- VS Code auto-discovers `squad.agent.md` from `.github/agents/` — same location CLI uses
- Sub-agents inherit file tools by default — better than CLI (more tools available)
- Path resolution via `git rev-parse --show-toplevel` works in VS Code via `runInTerminal`
- All `.ai-team/` read/write operations are supported via VS Code's built-in tools

## Constraints Documented

- Multi-root workspaces: NOT supported for Squad (known VS Code bugs). Single-root only.
- Workspace Trust: Must be enabled. Document as prerequisite.
- First-session approval: Users see tool approval prompts on first file write. Document in onboarding.
- `sql` tool: CLI-only. Avoid in VS Code codepaths.

## Optional Enhancement (Not Blocking)

A small VS Code compatibility note can be added to `squad.agent.md` covering workspace scope, `sql` absence, and approval UX. This is recommended but not required for functionality.
