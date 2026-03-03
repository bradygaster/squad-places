# Decision: README.md "All Commands" section corrected to match CLI --help

**Author:** McManus (DevRel)  
**Date:** 2026-03-06  
**Impact:** Documentation accuracy, user discoverability  

## Problem

Brady flagged that README.md's "All Commands" section no longer matches the actual CLI. Three specific issues were noted:
1. `squad run <agent> [prompt]` listed in README but not in CLI --help output
2. `squad nap` in CLI --help but missing from README
3. Command aliases not documented in README (init→hire, doctor→heartbeat, triage→watch/loop)

## Investigation

Ran `node cli.js --help` and audited `packages/squad-cli/src/cli-entry.ts`:
- **14 core commands exist** in CLI (init, upgrade, status, triage, copilot, doctor, link, upstream, nap, export, import, plugin, aspire, scrub-emails)
- **4 aliases registered**: hire→init, heartbeat→doctor, watch→triage, loop→triage
- **`run` command does NOT exist** in codebase (no registration, no handler)
- **`nap` command IS implemented** (line 449: help text for nap command exists)

## Decision

**Removed `squad run`** from README's "All Commands" table.  
**Added `squad nap`** to "Utilities" section with flag documentation.  
**Added alias documentation** inline for init (hire), doctor (heartbeat), triage (watch, loop).  
**Verified command count remains 15** (not affected by removal of non-existent command).

### Why This Matters

- **Source of truth:** CLI --help is the implementation contract. Docs must reflect it.
- **User trust:** Listing non-existent commands erodes confidence and wastes user time on troubleshooting.
- **Consistency:** Aliases belong next to their base commands for discoverability.
- **Tone ceiling:** No hand-waving about features that don't exist; claims substantiated by implementation.

## Changes Made

**README.md lines 64–82:**
- Removed row: `squad run <agent> [prompt] | Send a message to a specific agent`
- Inserted (after upstream): `squad nap | Context hygiene — compress, prune, archive...`
- Updated init row to include: `alias: hire`
- Updated doctor row to include: `alias: heartbeat`
- Updated triage row to clarify: `(aliases: watch, loop)`
- Enhanced copilot, nap, plugin descriptions with flag details

**History:**
- Appended learning to `.squad/agents/mcmanus/history.md` under "Learnings" section
- Documented audit method and source references

## Affected Teams

- **Brady** (Project Lead) — confirmed the discrepancy; now resolved
- **DevRel** (McManus) — docs consistency maintained
- **Users** — CLI help and README now synchronized

## Follow-Up

1. **Recommendation:** Add docs-sync test to CI (compare CLI --help with README command table annually)
2. **Future:** If `squad run` is implemented, add back to README with full documentation (not just a flag)
3. **Monitor:** Next CLI feature release should include README update in the same PR (enforce via docs checklist)
