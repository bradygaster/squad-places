# Decision: Dual-root path resolution (projectDir / teamDir)

**Date:** 2026-02-23  
**Author:** Fenster  
**Issue:** #311  
**Status:** Implemented

## Context

Remote squad mode requires separating project-local state (decisions, logs) from team identity assets (agents, casting, skills). A project's `.squad/config.json` can now point to an external team directory via a relative `teamRoot` path.

## Decision

Added `resolveSquadPaths()` alongside the existing `resolveSquad()` in `packages/squad-sdk/src/resolution.ts`.

### Types

- **`SquadDirConfig`** — schema for `.squad/config.json` (`version: number`, `teamRoot: string`, `projectKey: string | null`). Named `SquadDirConfig` to avoid collision with the existing `SquadConfig` type in `config/schema.ts` and `runtime/config.ts`.
- **`ResolvedSquadPaths`** — `{ mode, projectDir, teamDir, config, name, isLegacy }`.

### Resolution rules

1. Walk up from startDir checking `.squad/` then `.ai-team/` (legacy fallback).
2. If `.squad/config.json` exists with a valid `teamRoot` string → **remote mode**: `teamDir = path.resolve(projectRoot, config.teamRoot)` where projectRoot is the parent of the `.squad/` directory.
3. Otherwise → **local mode**: `projectDir === teamDir`.
4. Malformed JSON or missing/invalid `teamRoot` → graceful fallback to local mode.

### Backward compatibility

- `resolveSquad()` is unchanged — returns `string | null` as before.
- `resolveSquadPaths()` is a new, additive export.
- No existing tests or callers affected.

## Constraints

- ESM-only, strict TypeScript.
- Uses `node:fs` and `node:path` — no string concatenation for path building.
- No symlinks — config.json with relative paths only.
- `teamRoot` resolved relative to the project root, not relative to `.squad/`.
