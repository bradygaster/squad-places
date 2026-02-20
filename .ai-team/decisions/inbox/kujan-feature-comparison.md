# Decision: Feature Comparison Analysis Complete

**Author:** Kujan (Copilot SDK Expert)
**Date:** 2026-02-21
**Status:** For Brady review
**Output:** `.ai-team/docs/feature-comparison.md`

## Summary

Completed exhaustive feature comparison of current Squad (62 features) vs. SDK-replatformed Squad. Document is structured for McManus to produce Mermaid diagrams.

## Key Findings

1. **18 features are GRAVE** — no PRD coverage. Will be silently lost unless addressed. Biggest gaps: export/import portability (CLI-13/14), @copilot integration (CLI-6/7/8, AGT-16, GH-3), 12 workflow templates (DST-7), project-type detection (DST-5/6), identity system (STM-7/8), migration registry (STM-12), insider channel (DST-2).

2. **SDK enables 8 new capability categories** not possible today: programmatic session management, hooks system, custom agents with per-agent MCP, BYOK multi-provider, infinite sessions, streaming observability, permissions system, structured user input.

3. **CustomAgentConfig is the Squad team member primitive** — name, prompt (charter), mcpServers, tools filtering all align. But SDK has NO per-agent model field. Must use session-level model (Phase 1) or generate custom .agent.md files (Phase 2+).

4. **4-phase migration** estimated at 17–26 weeks total: v0.6.0 (3-5w) → v0.7.0 (8-12w) → v0.8.0 (4-6w) → v1.0.0 (2-3w).

## Recommendations

- **Write PRD 16** for export/import + marketplace (GRAVE items CLI-13, CLI-14, STM-9)
- **Fold @copilot integration** into PRD 5 or new PRD 15.5 (GRAVE items CLI-6–8, AGT-16, GH-3)
- **Add workflow template appendix** to PRD 14 (GRAVE item DST-7)
- **Add identity system** to PRD 14 (GRAVE items STM-7, STM-8)
- **Add migration registry** to PRD 12 or 14 (GRAVE item STM-12)
- **Preserve insider channel** in PRD 12 (GRAVE item DST-2)

## Blocking Decisions for Brady

1. Package name for npm registry (`@bradygaster/squad`?)
2. Model fallback chain specification per tier
3. Provider override scope (session vs. global default)
4. Quota routing (org-level cost budgets)
5. OTLP export for observability dashboards
6. PRD 16 scope — export/import only, or combined with agent marketplace?
