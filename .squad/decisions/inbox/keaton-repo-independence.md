# Decision: Repo Independence — Squad-SDK Primary Working Repository

**Date:** 2026-02-22  
**Author:** Keaton (Lead)  
**Status:** Implemented

---

## Context

Squad beta (v0.5.3) served as the initial template kit and CLI for Squad multi-agent orchestration. Squad-sdk (v0.6.0-alpha.0) is the v1 replatform with full TypeScript runtime, typed hooks, session management, and 1,551 tests across 13 modules.

PRD 22 (Repo Independence) was the capstone of the CLI migration plan — establishing squad-sdk as the primary working repository where the team lives and works.

---

## Decision

**Squad-sdk is now the primary working repository.** The team (13 specialists + Scribe + Ralph) has been spawned in squad-sdk with full DNA transfer from beta. Squad beta enters archive mode.

---

## What Was Implemented

### Squad-SDK Changes (PR #179)

1. **Team Structure Created:**
   - `.squad/team.md` — Full roster with 13 specialists + 2 silent agents
   - Project context: "TypeScript SDK + CLI for Squad multi-agent orchestration. 13 modules, 1,551+ tests, typed hooks, session management, crash recovery."
   - Module ownership mapped to all 13 squad-sdk modules

2. **Routing Rules:**
   - `.squad/routing.md` — Module-based routing (who handles what)
   - File path routing for all src/* modules
   - Routing principles (eager spawning, Scribe background-only, anticipate downstream)

3. **Decisions:**
   - `.squad/decisions.md` — Key architectural decisions documented
   - Distribution model (GitHub-native, NOT npmjs.com)
   - Type safety standards (strict mode, no @ts-ignore)
   - Hook-based governance (not prompt-based)
   - The Usual Suspects universe (permanent, locked)
   - Proposal-first workflow
   - Reviewer rejection lockout

4. **Agent Charters:**
   - 13 specialist charters in `.squad/agents/*/charter.md`
   - Identity, expertise, responsibilities, voice/style for each
   - Keaton, Verbal, Fenster, Hockney, McManus, Kujan, Edie, Kobayashi, Fortier, Rabin, Baer, Redfoot, Strausz
   - Scribe charter (session logger, decision keeper)

5. **Agent Histories:**
   - `.squad/agents/*/history.md` seeded with beta context
   - Keaton history includes: CLI migration plan, codebase comparison, team-to-brady doc, context window optimization, architecture patterns, risk management

6. **Casting Configuration:**
   - `.squad/casting/policy.json` — The Usual Suspects universe, locked
   - `.squad/casting/registry.json` — All 15 agents registered
   - `.squad/casting/history.json` — Initial spawn event logged

7. **Infrastructure:**
   - `.github/agents/squad.agent.md` — Coordinator (v0.6.0-alpha.0)
   - `.github/workflows/` — 12 Squad workflows
   - `.gitattributes` — Union merge for append-only files
   - `.gitignore` — Log exclusions
   - `.squad-templates/` — Template files for squad init

### Beta Repo Changes

- `README.md` updated with archive notice at top
- Notice points to squad-sdk (bradygaster/squad-pr)
- Clarifies: beta CLI continues to work, v1 SDK is in new repo
- Committed directly to `main` branch

---

## Impact

**Before PRD 22:**
- Squad beta: template source, CLI host, no team in repo
- Squad-sdk: runtime code, tests, no self-referential team
- Team worked "externally" to the repos they were building

**After PRD 22:**
- Squad beta: archived, redirect notice, CLI continues to work for legacy users
- Squad-sdk: **self-sufficient** — team lives in the repo, manages the repo
- Team is now self-referential: squad-sdk's Squad team builds squad-sdk

**Key Benefit:** The team can now work on squad-sdk development using the full squad-sdk infrastructure (typed tools, hooks, session management, casting, skills). Dogfooding at its finest.

---

## Next Steps

1. ✅ PR #179 merged (squad-sdk team spawn)
2. ✅ Beta README updated (archive notice)
3. ⏳ CLI migration continues (PRDs 15-21)
4. ⏳ Team begins working within squad-sdk
5. ⏳ Beta repo fully archived after CLI parity achieved

---

## Success Criteria

- [x] `.squad/` directory exists in squad-sdk with full team structure
- [x] 13 specialists + Scribe + Ralph all have charters and histories
- [x] Routing rules cover all 13 modules
- [x] Casting uses The Usual Suspects (locked)
- [x] Key architectural decisions documented
- [x] Beta README points to squad-sdk
- [x] PR opened and ready for merge

---

## Trade-offs

**Accepted:**
- Beta repo becomes read-only (after CLI parity) — acceptable because v1 SDK is the future
- Two active repos during transition (beta + squad-sdk) — temporary state, resolved when beta fully archived
- Squad-sdk team must bootstrap itself (no external orchestrator) — acceptable because init command works

**Avoided:**
- Deleting beta repo (preserved for historical reference and legacy installs)
- Breaking existing beta CLI users (archive notice is informational, not blocking)
- Losing team DNA (full transfer via respawn-prompt.md)

---

## Review Notes

**Keaton:** PRD 22 is the culmination of M9 (Repo Independence). Team DNA successfully transferred. Squad-sdk is now the canonical repo. Beta can be archived after CLI parity (PRDs 15-21).

**Status:** ✅ **Implemented** (PR #179, committed 2026-02-22)

---

**File:** keaton-repo-independence.md  
**Location:** .squad/decisions/inbox/  
**Merge:** Ready for Scribe to merge to decisions.md
