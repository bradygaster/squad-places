### 2026-02-08: P0 — Silent success bug mitigation

**By:** Kujan
**Priority:** P0

**What:** ~40% of background agents complete all work (files written, histories updated) but `read_agent` returns "did not produce a response." Root cause: agent's final LLM turn is a tool call (writing history.md/inbox), not text. The `task` tool's response channel drops tool-call-only final turns. Three zero-risk mitigations proposed: (1) reorder spawn prompt so agents end with text summary after all file writes, (2) add silent success detection to coordinator's "After Agent Work" flow — check file existence when response is empty, (3) always use `read_agent` with `wait: true, timeout: 300`.

**Why:** This is the #1 trust-destroying bug. When the coordinator tells the user "agent failed" but the work is sitting on disk, it creates the exact "agents get in the way" perception Brady flagged. The mitigations are all additive, non-breaking, and can ship immediately. Expected to reduce silent success rate from ~40% to <15% (prompt fix) and catch remaining cases with file verification (detection fix). If rate stays above 10% after mitigations, escalate to Copilot team as platform bug.

**Proposal:** `docs/proposals/015-p0-silent-success-bug.md`
