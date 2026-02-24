# UX Audit: Honest Quality Assessment

**Author:** Marquez (UX Auditor)
**Requested by:** Brady
**Date:** 2026-02-24
**Status:** Report delivered

## Overall Grade: B

Solid foundation with genuine design intent, but accumulated inconsistencies prevent it from reaching best-in-class status. ~2-3 days of focused polish to reach an A.

## P0 Fixes (Do Before Next Release)

### 1. Structure the help text
`cli-entry.ts` lines 70-119 — Group commands into "Core" (init, status, help) and "Advanced" (triage, loop, export, import, copilot, aspire, etc). Add section headers.

### 2. Remove stub commands from help
`triage`, `loop`, `hire` print placeholder messages. Either ship them or hide them from `squad help`. Showing "(full implementation pending)" destroys user trust.

### 3. One tagline
Line 55 says "Add an AI agent team to any project." Line 71 says "Team of AI agents at your fingertips." Pick one.

## P1 Fixes (Next Sprint)

### 4. Unify status vocabulary
Three different status label systems across AgentPanel (compact), AgentPanel (normal), and `/agents` command. Standardize to one vocabulary.

### 5. Unify separator characters
Dashes (`-`) in AgentPanel/MessageStream, rounded box-drawing in App.tsx header, Unicode `─` in init. Pick one.

### 6. Fix `/agents` vs AgentPanel mismatch
Same data, different presentation. The slash command should mirror the panel's visual language.

### 7. Fix roster text wrapping
The header roster is a concatenated string that wraps mid-agent. Use flexWrap with individual nodes.

## P2 Fixes (Polish Pass)

8. `scrub-emails` defaults to `.ai-team` — should be `.squad`
9. `/clear` has no confirmation or feedback
10. `exit` works as bare word but `quit` doesn't
11. Goodbye message is a plain console.log — no session summary
12. `squad status` and `/status` show different information under the same label

## What Competitors Do Better

- **Session persistence** (Claude saves conversations)
- **Structured help** (gh groups by category)
- **Onboarding wizard** (first init should personalize)
- **Configuration command** (no `squad config`)

## What Squad Does Better Than Anyone

- Multi-agent orchestration panel (novel UI, no competitor has this)
- Init ceremony (genuinely delightful)
- Ghost retry (invisible reliability)
- NO_COLOR accessibility (professional-grade)
