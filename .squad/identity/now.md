---
updated_at: 2026-02-22T10:39:00Z
focus_area: Full fan-out — OTel, REPL fix, CI/CD, docs
active_issues: [265, 266, 267, 268, 303, 304, 305]
---

# What We're Focused On

**Priority order (Brady directive):** OTel + Aspire → Fix REPL → CI/CD npm publishing → Docs/website.

**⚠️ Repo: bradygaster/squad-pr ONLY — not bradygaster/squad.**

## Wave 1 (tonight)
- OTel Phase 4: #265 (Aspire cmd), #266 (public API), #267 (integration tests), #268 (file watcher)
- REPL fix: #303 — shell is placeholder, coordinator wiring missing
- CI/CD: #305 — GitHub Actions for npm publish + releases
- SquadOffice: #304 — telemetry integration with office visualization (C:\src\SquadOffice)

## Context
- OTel Phases 1-3 complete (#254-264 closed). 1940 tests passing.
- SquadOffice expects colon-separated EventBus events (session:*, agent:milestone, coordinator:routing, pool:health)
- Docs epic (#182) has 12 issues — last priority.
