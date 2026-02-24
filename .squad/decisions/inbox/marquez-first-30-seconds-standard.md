# First 30 Seconds UX Standard

**By:** Marquez (CLI UX Designer)
**Date:** 2026-02-24
**Status:** Proposed

## Decision

Squad must delight users in the first 30 seconds. Any moment that creates confusion, frustration, or "is it broken?" anxiety is a blocker for release.

**The First 30 Second Rule:**
1. Help text must be scannable in ≤3 seconds
2. Default behavior must be obvious without reading docs
3. Wait states >2 seconds must show status ("Connecting...", "Waking up [Agent]...")
4. Animations must never block input
5. The primary action path (install → init → first message → first response) must feel **instant** at every step

## Why This Matters

Brady's directive: "The impatient user bails at second 7."

Analysis of current first-run experience shows users bail at 0:18 (6 seconds after hitting enter) because:
- No feedback during SDK connection (5-second silence)
- Spinner with no context ("what is it doing?")
- Help text is a 50-line wall (can't find "how to start")

**Industry standard:** Modern CLIs (gh, docker, npm) provide instant feedback at every async operation. Squad matches this or loses users.

## What Changes

Filed 6 issues (#419-#423, #426) targeting first-30-second pain points:
- Help text structure (P0)
- SDK connection feedback (P0)
- Spinner context (P0)
- Default behavior clarity (P0)
- Welcome animation speed (P1)
- Tagline consistency (P1)

All P0 issues must be resolved before v1.0 release.

## Future UX Gates

Consider adding CI test:
- Simulated cold-start timing (measure time from `squad` launch to prompt ready)
- Help text line length (enforce ≤40 lines, grouped sections)
- Every spinner must have accompanying status text

## Team Impact

- **Cheritto:** Welcome animation, spinner context
- **Fenster/Edie:** Help text structure
- **Kujan:** SDK connection status
- **Marquez:** UX review approval on PRs that touch first-run experience
