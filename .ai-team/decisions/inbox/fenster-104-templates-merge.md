# Decision: Migrate .ai-team-templates/ to .squad/templates/

**Date:** 2026-02-20  
**Decider:** Fenster  
**Status:** Implemented  
**Related:** Issue #104, PR #112

## Context

Squad has two separate "templates" concepts:
1. **`templates/`** (repo root) — Consumer-facing templates copied to user repos by `npx create-squad`
2. **`.ai-team-templates/`** — Internal format guides for the coordinator and agents

The .squad/ directory consolidation strategy (#69 sub-issue) requires moving internal templates under the .squad/ namespace.

## Decision

Migrate `.ai-team-templates/` → `.squad/templates/`. The consumer-facing `templates/` at repo root remains unchanged (finalized architectural decision).

## Implementation

- Moved all 21 format guide files to `.squad/templates/`
- Updated `index.js` to copy templates to new location (3 reference points: help text, destination path, console output)
- Removed `.ai-team-templates/` entry from `.npmignore`
- All 53 tests pass

## Coordination

Coordinates with #102 (Verbal): squad.agent.md has 10+ references to `.ai-team-templates/` that need updating to `.squad/templates/`. Both PRs should merge together.

## Consequences

- Internal templates now consolidated under .squad/ namespace
- Consumer templates remain at repo root (backward compatible)
- Existing repos upgrade via standard upgrade flow
