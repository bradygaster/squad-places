# Decision: REPL Shell Polish Architecture

**By:** Fortier
**Date:** 2026-02-22
**Scope:** Shell UI components (`App.tsx`, `AgentPanel.tsx`, `MessageStream.tsx`, `InputPrompt.tsx`, `lifecycle.ts`)

## What

Role emoji mapping and welcome data loading live in `lifecycle.ts` alongside team manifest parsing. UI components import directly from `../lifecycle.js` — no new props needed on the shell entry point (`index.ts`). All filesystem reads happen inside React `useEffect` hooks at mount time.

## Why

- **Single source of team data**: `lifecycle.ts` already owns `parseTeamManifest()`. Adding `getRoleEmoji()` and `loadWelcomeData()` here keeps all team-data concerns in one module. Components don't need to know about `.squad/` directory structure.
- **No SDK wiring changes**: Loading welcome data inside `App.tsx` via `useEffect` avoids any changes to `index.ts` and the coordinator/session wiring. The welcome screen is purely additive UI.
- **Fail-safe**: `loadWelcomeData()` returns `null` on any error. Components gracefully degrade — if `.squad/team.md` or `.squad/identity/now.md` is missing, the header still shows version and hints.

## Team Impact

- **Component authors**: `AgentPanel` now accepts optional `streamingContent` prop; `MessageStream` accepts optional `agents` and `processing` props. Both are backward-compatible (props are optional with defaults).
- **Role emoji map**: New roles should be added to the `map` in `getRoleEmoji()` in `lifecycle.ts`. Unknown roles get `🔹` fallback.
- **ThinkingIndicator**: 80ms `setInterval` runs only while the indicator is mounted. Cleanup is automatic via React effect teardown. No event loop concerns.
