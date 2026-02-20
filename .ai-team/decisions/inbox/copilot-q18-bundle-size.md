### 2026-02-20T20:08: User directive — Q18 Bundle size target
**By:** Brady (via Copilot)
**What:** Lean target (~5MB) for global install. Stay close to current package footprint. Single-install is a priority — tree-shake aggressively, keep dependencies minimal. "I'd hate for us to get a lot bigger than we are today, especially if we can't be a single-install kind of thing."
**Why:** User directive — captured during open questions iteration. Distribution constraint that affects dependency choices and build pipeline.
