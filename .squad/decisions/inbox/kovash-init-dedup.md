### First-run init messaging: single source of truth
**By:** Kovash (REPL & Interactive Shell)
**Date:** 2026-03-01
**Issue:** #625

**What:** When no roster exists, only the header banner tells the user about `squad init` / `/init`. The `firstRunElement` block returns `null` for the empty-roster case instead of showing a duplicate message. `firstRunElement` is reserved for the "Your squad is assembled" onboarding when a roster already exists.

**Why:** Two competing UI elements both said "run squad init" — visual noise that confuses the information hierarchy. Banner is persistent and visible; it owns the no-roster guidance. `firstRunElement` owns the roster-present first-run experience.

**Impact:** App.tsx only. No API or prop changes. Banner text reworded to prioritize `/init` (in-shell path) over exit-and-run.
