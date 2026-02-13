# Decision: Client Parity Compatibility Matrix

**Date:** 2026-02-15  
**Owner:** McManus (DevRel)  
**Status:** Implemented  
**Related:** Issue #35, Proposals #032a, #032b, #033a, #034a  

## Summary

Created `docs/scenarios/client-compatibility.md` as the single source of truth for what Squad features work on each Copilot surface (CLI, VS Code, JetBrains, GitHub.com).

## Rationale

**Community clarity need:** Issue #9 (reporter: miketsui3a) and #10 asked for documentation on cross-client support. Developers trying Squad on VS Code or other surfaces need to know what works where without trial-and-error or searching scattered spikes.

**Spike findings ready:** Four research proposals (032a–034a) contain production-grade findings from February spikes. Synthesizing them into one document prevents knowledge silos and gives developers a single reference point.

**Developer-first structure:** Matrix format (quick reference) + detailed per-platform sections + adaptation guide enables self-service troubleshooting. No need to file issues or ask in discussions.

## Key Decisions Documented

1. **CLI is primary:** Full feature support, recommended for learning and setup.

2. **VS Code works:** With adaptations.
   - Sub-agents are sync but parallel when spawned in one turn (functionally equivalent to CLI's background mode).
   - Session model by default (Phase 1). Custom agent frontmatter for Phase 2.
   - File access works (workspace-scoped).
   - Scribe batching pattern: put Scribe last in parallel groups.

3. **JetBrains/GitHub untested:** Documented as `?` (unknown). Links to pending spikes #12, #13.

4. **SQL is CLI-only:** Avoids confusion about cross-platform SQL workflows.

5. **File discovery works everywhere:** `.github/agents/squad.agent.md` auto-discovered on all platforms tested.

6. **Straight facts tone:** No editorial framing, no "amazing" language. Every statement specifies what a feature is, how it works, or what replaces it.

## Structure

- **Quick Reference Matrix** — One table with ✅/⚠️/❌/? for all features
- **Per-Platform Details** — CLI (full), VS Code (conditional), JetBrains (unknown), GitHub (unknown)
- **Platform Adaptation Guide** — When to use which surface + feature degradation table for developers building cross-platform coordinators
- **Investigation Status** — Links to spike proposals for deep dives
- **See Also** — Cross-references to related feature docs (model selection, parallel execution, worktrees)

## Navigation

- Added to `docs/README.md` under "Operations" section (first item for discoverability)
- Link: [Client Compatibility Matrix](scenarios/client-compatibility.md) — What works on CLI, VS Code, JetBrains, GitHub.com

## Impact

- ✅ Developers get one document instead of reading 4 proposals
- ✅ Community questions about "does this work on VS Code?" have a documented answer
- ✅ Sets foundation for Phase 2 (custom agent generation) and Phase 3 (per-role agent files)
- ✅ Enables Brady to point to facts-based matrix when discussing cross-client strategy

## Future Work

- Spike #12 (JetBrains investigation) — populate `?` cells, determine if agent spawning supported
- Spike #13 (GitHub investigation) — populate `?` cells, assess GitHub's agent orchestration capabilities
- Phase 2 (v0.5.0) — Generate custom `.agent.md` files during `squad init` for model-tier selection on VS Code
- Empirical testing — Verify VS Code has the silent success bug (P0 from Proposal 015) or can omit Response Order workaround
