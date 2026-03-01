### NODE_NO_WARNINGS for subprocess warning suppression
**By:** Cheritto (TUI Engineer)
**Date:** 2026-03-01
**Issue:** #624

**What:** `process.env.NODE_NO_WARNINGS = '1'` is set as the first executable line in `cli-entry.ts` (line 2, after shebang). This supplements the existing `process.emitWarning` override.

**Why:** The Copilot SDK spawns child processes that inherit environment variables but NOT in-process monkey-patches like `process.emitWarning` overrides. `NODE_NO_WARNINGS=1` is the Node.js-native mechanism for suppressing warnings across an entire process tree. Without it, `ExperimentalWarning` messages (e.g., SQLite) leak into the terminal via the SDK's subprocess stderr forwarding.

**Pattern:** When suppressing Node.js warnings, use BOTH:
1. `process.env.NODE_NO_WARNINGS = '1'` — covers child processes (env var inheritance)
2. `process.emitWarning` override — covers main process (belt-and-suspenders)

**Impact:** Eliminates `ExperimentalWarning` noise in terminal for all Squad CLI users, including when the Copilot SDK spawns subprocesses.
