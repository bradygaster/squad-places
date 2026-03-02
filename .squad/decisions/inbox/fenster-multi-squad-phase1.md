# Decision: Multi-Squad Global Config Layout

**By:** Fenster (Core Dev)  
**Date:** 2025-07-24  
**Issue:** #652  
**PR:** #691  

## What

Squad now supports a global `squads.json` registry at the platform config root (`%APPDATA%/squad/` on Windows, `~/.config/squad/` on Linux/macOS). Each named squad is registered with a name, path, and creation timestamp. Resolution follows a 5-step chain: explicit name → `SQUAD_NAME` env var → active in squads.json → "default" → legacy `~/.squad` fallback.

## Why

Users need to manage multiple squads (personal, work, experiments) without conflicts. A global registry decouples squad identity from the current working directory and enables future CLI commands (`squad list`, `squad switch`, etc.) in Phase 2.

## Migration Strategy

Migration is **non-destructive and registration-only**. When `resolveSquadPath()` detects a legacy `~/.squad` layout without an existing `squads.json`, it registers that path as the "default" squad. No files are moved, copied, or renamed. This eliminates data loss risk on first upgrade.

## Impact

- All future squad path resolution should go through `resolveSquadPath()` from `multi-squad.ts`
- Existing `resolveSquad()` and `resolveSquadPaths()` in `resolution.ts` remain unchanged (project-local `.squad/` walk-up)
- Phase 2 CLI commands will consume these SDK functions directly
