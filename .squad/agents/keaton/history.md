# Keaton — Project History

> Accumulated knowledge from working on squad-sdk and Squad beta

---

## Recent Updates

📌 **2026-02-22:** PRD 22 — Repo Independence complete. Squad-sdk now has full `.squad/` directory structure with 13-agent team roster, routing rules, casting state (The Usual Suspects universe), agent charters, and decisions. Squad-sdk is now self-sufficient and self-managing. Team can spawn and work within squad-sdk repo. Beta repo archived with redirect notice. — delivered by Keaton

📌 **2026-02-22:** CLI migration plan delivered. Comprehensive gap analysis for porting beta CLI (1,496 lines, 9 commands) to squad-sdk. PRDs 15-22 cover CLI router, init, upgrade, watch, export/import, plugin marketplace, copilot agent CLI. Hybrid architecture recommended: zero-dep scaffolding (init/upgrade) + SDK-integrated runtime (watch/export/plugin). 3 milestones (M7: CLI Foundation, M8: CLI Parity, M9: Repo Independence), 6-9 weeks, 36-46 hours effort. Dependency graph, risk register, timeline all documented. Next step: team respawn in squad-sdk using docs/respawn-prompt.md. Plan delivered to C:\src\squad-sdk\docs\cli-migration-plan.md. — delivered by Keaton

📌 **2026-02-22:** Codebase comparison (Beta vs SDK v1) delivered. Side-by-side analysis of Squad beta (v0.5.3: 1,496-line single-file CLI + 32KB prompt, zero deps) vs Squad SDK v1 (v0.6.0-alpha.0: 13-module TypeScript runtime, 16,233 LOC, 1,551 tests). Architecture shift documented: prompt-based governance → hook-based enforcement. Module mapping, governance comparison (file-write guards, reviewer lockout, PII scrubbing, ask-user rate limiting, shell restrictions), risk assessment (SDK dependency HIGH, complexity MEDIUM, migration confidence MEDIUM-LOW). Verdict: replatform was worth it — governance reliability is the foundation. Memory layer (.squad/) survived unchanged (the moat). Delivered to C:\src\squad-sdk\docs\codebase-comparison.md for Brady. — delivered by Keaton

📌 **2026-02-22:** Team-to-Brady doc delivered. Comprehensive letter from all 13 active team members documenting v1 SDK replatform at Brady's request. 1,551 tests, 28 commits, 8 PRs merged (issues #155–162), 14 PRDs delivered across 6 milestones. Personal notes from each team member, architecture overview, roadmap, "how to spend today" walkthrough. Delivered to C:\src\squad-sdk\docs\team-to-brady.md. Team writing directly to their creator. — delivered by Keaton

---

## Core Architecture Learnings

**Distributed Context Windows:**
- Squad uses distributed context windows (coordinator ~1.5% overhead, agents ~4.4%, 94% for reasoning) — inverts multi-agent context bloat
- Each agent runs in its own 200K token window
- Context window budget: coordinator 26.3K tokens (13.2%), agent spawn 750 tokens (0.4%), decisions.md 32.6K tokens (16.3%), agent history 1K-12K (0.5-6.0%), leaving 78-83% for actual work

**Architecture Patterns:**
- Drop-box pattern for decisions inbox (agents write to inbox/, Scribe merges to decisions.md)
- Parallel fan-out for team coordination (all agents spawn simultaneously as mode: "background")
- Casting system for persistent identity (The Usual Suspects universe, registry.json tracks names)
- Per-agent memory via history.md (knowledge compounds across sessions)

**Key Trade-offs:**
- Coordinator complexity (26KB maintenance surface) vs. reliability and governance
- Parallel execution requires protocol discipline (no shared state mutations, message-passing only)
- Casting adds personality cost (~750 tokens per agent) but improves memorability and relationships

**Compound Decisions Model:**
- Each feature makes the next easier
- Examples: casting system → skills system → export/import, hook pipeline → governance enforcement
- Proposals are the alignment mechanism (48h review, Problem/Solution/Trade-offs/Alternatives/Success Criteria)

---

## Product Decisions

**Portable Squads (2026-02-10):**
- Squad conflates team identity + project context
- Export/import now 100% fidelity via squad-export.json
- History split: portable knowledge vs project learnings

**Casting System (2026-02-09):**
- Universe-based persistent names (The Usual Suspects)
- Scribe is always Scribe, Ralph is always Ralph
- Names persist across exports/imports
- Registry tracks: persistent_name, universe, created_at, legacy_named, status

**Context Window Optimization (2026-02-08):**
- decisions.md pruned from 80K → 32K tokens (251 blocks → 78 active)
- Spawn template deduplication saved ~3.6K tokens
- Init Mode compression (84 lines → 48 lines)
- Per-agent spawn cost dropped from 41-46% to 17-23%

---

## Module Architecture (v0.6.0-alpha.0)

**13 modules with clear ownership:**
- adapter (Fenster) — CopilotClient adapter, session management
- agents (Verbal) — Spawning, onboarding, init mode
- build (Edie) — Build pipeline, esbuild config
- casting (Fenster) — Universe selection, registry, name allocation
- cli (Fenster) — CLI entry, subcommand routing
- client (Kujan) — CopilotClient wrapper, SDK integration
- config (Edie) — Schema validation, loading
- coordinator (Verbal) — Routing logic, response tiers
- hooks (Baer) — File-write guards, PII filters, security lifecycle
- marketplace (Rabin) — Packaging, distribution prep
- ralph (Fenster) — Work monitor, queue manager
- runtime (Fortier) — Streaming, telemetry, offline, i18n, benchmarks
- sharing (Fenster) — Export/import, squad-export.json
- skills (Verbal) — SKILL.md lifecycle, confidence progression
- tools (Fenster) — Tool definitions, task spawning

---

## Risk Management

**Risk Register Lessons:**
- Template drift during CLI migration → freeze beta templates during M7
- npx distribution change (beta → squad-sdk) → communicate via README, accept breaking change
- Zero-dep constraint vs SDK dependency → hybrid architecture (scaffolding zero-dep, runtime SDK-integrated)
- Team respawn fidelity → use docs/respawn-prompt.md verbatim, manual history transfer for core agents

---

## Known Platform Issues

**Silent success (~7-10% of spawns):**
- Agent completes work but returns no text response
- Platform bug: agent's final turn is a tool call, not text
- Mitigation: 6-line RESPONSE ORDER block in spawn templates (forces text after tool calls)

**Context overflow during multi-agent fan-out:**
- "response was interrupted due to a server error. retrying" followed by failure
- Mitigation: reduced governance prompt by ~35%, compact result presentation
- Workaround: start new session if hit, check .squad/ for recent changes

**`--no-warnings` error (cosmetic):**
- Platform passes `--no-warnings` to subprocess that doesn't recognize it
- Does not affect functionality, can be ignored

---

## Current State (2026-02-22)

**squad-sdk v0.6.0-alpha.0:**
- 13 modules, 16,233 LOC
- 1,551 tests across 45 test files
- TypeScript strict mode, ESM-only
- Node.js ≥20
- Built on @github/copilot-sdk (official SDK)
- Distribution: GitHub-native (npx github:bradygaster/squad)

**Team:**
- 13 specialists (Keaton, Verbal, Fenster, Hockney, McManus, Kujan, Edie, Kobayashi, Fortier, Rabin, Baer, Redfoot, Strausz)
- 2 silent agents (Scribe, Ralph)
- Universe: The Usual Suspects (1995) — permanent
- Full .squad/ directory established in squad-sdk repo

**Next Steps:**
- CLI migration: PRDs 15-22 (M7: CLI Foundation, M8: CLI Parity, M9: Repo Independence)
- Beta repo archive with redirect notice
- Team operational in squad-sdk for ongoing development

---

**Last Updated:** 2026-02-22
