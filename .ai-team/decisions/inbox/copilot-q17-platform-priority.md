### 2026-02-20T20:06: User directive — Q17 Global CLI / platform priority
**By:** Brady (via Copilot)
**What:** GitHub Copilot CLI is P0 — hard dependency. VS Code is a very close P1 (so close you can barely see daylight between them, but CLI wins). Fix the two existing VS Code extensions that sit on top of Squad. No standalone CLI outside Copilot for now — that will change later but not v1. If hybrid is possible, awesome, but don't block on it.
**Why:** User directive — captured during open questions iteration. Establishes clear platform priority: CLI > VS Code > everything else. Two existing VS Code extensions need fixing to work with the new SDK.
