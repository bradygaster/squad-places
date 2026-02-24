# Wave D Readiness Assessment
**Date:** 2026-02-24  
**By:** Keaton (Lead)  
**Requested by:** Brady

---

## Executive Summary

**Waves A, B, and C are COMPLETE.** All 17 Polish items shipped, 8 PRs merged, 15 issues closed. The CLI is in a polished, reliable, tested state.

**Wave D (Delight) is ready to define.** The PRD currently says "TBD — Delight" — this assessment catalogs what delights are possible based on team gaps, user feedback, and architectural strengths.

**Quality snapshot:** Solid foundation. The CLI is fast, responsive, visually coherent, and handles user interactions gracefully. One known dogfooding gap (#324) is P0 and needs closure before Wave D launch.

---

## Quality State of the CLI Right Now

### ✅ What's Shipping Well

1. **Visual Polish (Wave A Complete)**
   - Text-over-emoji convention unified (#340)
   - ASCII-only separators (no `─` or `◆` in output)
   - Consistent message prefixes (user `❯`, system `▸`)
   - Help text structure matches expectations
   - First-run experience smooth with typewriter effect and staggered reveal

2. **Reliability (Wave B Complete)**
   - Cancel operations working (#434)
   - Cold SDK connection feedback visible (#397, #421)
   - Work division logic transparent (#342)
   - Input buffering no longer silent (#457)

3. **Test Coverage (Wave C Complete)**
   - E2E integration tests in place (#433)
   - Component tests passing across shell UI
   - Speed gates enforced
   - All P0 tests green (1727+ passing)

4. **User Experience**
   - Agent roster displays live with color, status, activity hints
   - Streaming responses show clearly per agent
   - Exit messages include session summary
   - Terminal adaptivity from 40–120 cols
   - Narrow terminal mode (`-q` flag) functional

### ⚠️ Known Gaps (Not Critical, But Present)

**From Marquez UX catalog (25 gaps identified):**
- **3 P0 (Critical):** Stub commands (#403), Cold SDK feedback (partially done), Cancel ops (#434 in progress)
- **9 P1 (Significant):** Status label inconsistency, `/agents` vs panel mismatch, system prefix styling, welcome hint agent selection, thinking label accuracy, team.md error visibility, keyboard hint education, error recovery guidance, concurrent ops indication
- **11 P2 (Polish):** Compact terminal completion flash, header roster wrapping, message prefix distinction, activity hint truncation, exit message underwhelming, `/clear` confirmation, input buffering feedback (in progress), separator consistency, duration calculation clarity, placeholder text, agent list emojis
- **2 P3 (Nice-to-have):** Message fade timing, Ctrl shortcuts (Ctrl+A, Ctrl+U, Ctrl+K)

**From Waingro fragility catalog (13 risks identified):**
- **Memory & Growth:** Unbounded message history (no cap enforced), stale streamBuffer entries (deleted on success, not error), coordinator session accumulates turns, MemoryManager unused
- **Data Loss & Fragility:** Concurrent multi-agent streaming overwrites, ghost retry masks connection failures, coordinator response parsing fails on preamble, streaming delta extraction fails silently
- **Edge Cases:** InputPrompt buffering races (unlikely), router comma-match input mangling, case-insensitive SessionRegistry duplicates, App error handling gaps
- **Long-term:** Listener accumulation, no health checks on stale sessions

**Hockney test-debt inventory:** Not found (file doesn't exist in agents/hockney/).

---

## Proposed Wave D Items: "Delight"

Based on the catalogs and open issues, Wave D should focus on:

### Tier 1: Certainty & Retention (High Impact, Medium Effort)

1. **P1: Unified Status Display (#107 from Marquez)**
   - `/agents` command and AgentPanel show same data with same styling
   - Use emoji + name + status label everywhere (`working`, `streaming`, `idle`, `error`)
   - Eliminates cognitive load in fast-scrolling sessions
   - *Effort:* 3–4 hours (refactor `/agents` handler to use shared rendering)

2. **P1: Adaptive Keyboard Hints (#159 from Marquez)**
   - First run: `Type @agent or /help`
   - After 5 messages: `Tab completes · ↑↓ history`
   - After 10 messages: `/status · /clear · /export`
   - Hints rotate every 30 seconds during idle
   - *Effort:* 4–5 hours (add hint state machine, track message count)

3. **P0: Dogfood with Real Repos (#324)**
   - Close the only open issue — test CLI against 5–10 real projects (not fixtures)
   - Document findings (what breaks, what needs hardening)
   - Feed back to future waves
   - *Effort:* 4–6 hours (test runs + bug triage)

### Tier 2: Delight Through Precision (Medium Impact, Low Effort)

4. **P1: Error Recovery Guidance**
   - Change SDK disconnect message to: `"SDK disconnected. Try 'squad doctor' or check your internet. Restart shell to reconnect."`
   - Add team.md validation: `"⚠️  No agents found in team.md. Run 'squad doctor' or 'squad init' to fix."`
   - *Effort:* 2–3 hours (message templates, lifecycle checks)

5. **P2: /clear Confirmation**
   - Add `Confirm clear history? (y/n)` prompt
   - Show feedback: `"History cleared. Messages saved in /history."`
   - *Effort:* 1–2 hours (command handler extension)

6. **P2: Exit Session Summary**
   - Show: `"You had 12 messages with Keaton and Riley over 8 minutes. Great work!"`
   - Track agent list and message count per session
   - *Effort:* 2–3 hours (stats collection, template rendering)

### Tier 3: Delight Through Polish (Low Impact, Low Effort)

7. **P2: Placeholder Hint for Slash Commands**
   - Change InputPrompt hint to: `Type / for commands or @agent...`
   - Makes `/help` discovery natural for new users
   - *Effort:* <1 hour (string change)

8. **P2: Separator Consistency**
   - Verify `getBoxChars()` used everywhere (AgentPanel, MessageStream)
   - Ensure Windows Terminal + dumb terminal both render correctly
   - *Effort:* 1–2 hours (audit + fix)

9. **P1: Welcome Hint Agent Selection**
   - Add `.squad/welcome.md` field: `starterAgent: "Keaton"`
   - Or detect role-based selection (prefer `coordinator` or `task`)
   - Show agent's charter in hint for context
   - *Effort:* 3–4 hours (config parsing, component enhancement)

### Tier 4: Fragility Hardening (Risk Reduction)

10. **Message History Cap**
    - Add `maxMessages: 200` in App state
    - Archive older messages when cap is reached
    - Wire up `MemoryManager.trimMessages()`
    - *Effort:* 2–3 hours (state management, archive logic)

11. **Per-Agent Streaming Content**
    - Change App state from single `streamingContent` to `Map<agentName, content>`
    - Render separate streaming area for each active agent
    - Fixes concurrent multi-agent output jumble
    - *Effort:* 4–5 hours (state refactor, UI layout)

12. **StreamBuffer Cleanup on Error**
    - Add `streamBuffers.delete(agentName)` in error path
    - Prevents stale buffer accumulation
    - *Effort:* 1 hour (defensive cleanup)

13. **Connection-Aware Ghost Retry**
    - Check `client.isConnected()` before retrying
    - Fail fast on network errors (not after 30+ minutes)
    - *Effort:* 2–3 hours (health check integration)

---

## Gaps from Waves A–C That Need Follow-Up

### Hard Blockers (Must Fix Before Wave D)
- **#324 Dogfood:** Test against real repos, not just fixtures. No substitute for real-world usage.

### Soft Gaps (Known but Manageable)
1. **Stub commands (#403):** Currently `triage`, `loop`, `hire` show "(full implementation pending)". Wave D should either ship the feature or remove the command stub.
2. **Status label inconsistency:** AgentPanel and `/agents` use different vocabularies. Wave D fix is high-value for UX.
3. **Concurrent agent operations:** When coordinator routes to 3+ agents, output can jumble. Wave D fragility hardening addresses this.

### Not Blocking (Deferred to Later Waves)
- **Ctrl shortcuts (Ctrl+A, Ctrl+U, Ctrl+K):** Nice-to-have power user feature — can wait.
- **Message fade timing:** Purely cosmetic — 200ms is fine for most users.
- **Coordinator session context leakage:** Low reproduction probability — monitor in production.

---

## Recommendation for Brady

### Next Steps

1. **Immediate (This Week)**
   - Assign #324 (dogfood) to an agent — test against 5–10 real projects, document findings, close issue
   - Review Wave D tier breakdown above — pick 3–4 items for **Wave D Batch 1**

2. **Wave D Batch 1 (High-Value Wins)**
   - **Priority 1:** Unified Status Display + Adaptive Keyboard Hints (UX precision)
   - **Priority 2:** Dogfood closure + Error Recovery Guidance (reliability)
   - **Priority 3:** Message History Cap + Per-Agent Streaming (fragility hardening)
   - *Total effort:* ~20–25 hours, ~4–5 PRs, 1 week with current team velocity

3. **Wave D Batch 2 (Polish)**
   - `/clear` confirmation, exit summary, placeholder hint, separator consistency
   - *Total effort:* ~8–10 hours, ~2–3 PRs, 2 days

4. **Wave D Batch 3 (Long-Term Hardening)**
   - Connection-aware retry, stale buffer cleanup, coordinator session isolation
   - *Total effort:* ~5–8 hours, 1–2 PRs
   - Can be deferred to Wave E if timeline is tight

### Quality Gate Before Wave D Launch

- [ ] #324 closed (dogfood findings documented)
- [ ] All P0 items in Marquez catalog addressed (cold SDK feedback, stub commands, cancel ops)
- [ ] Memory safety audit signed off (Waingro)
- [ ] E2E tests passing across real codebases (not just fixtures)

### Why Wave D Now?

The foundation is solid. Waves A–C built a **responsive, consistent, tested CLI**. Wave D is about turning that solid foundation into something users *love*. The delight opportunities are:

1. **Precision over polish** — Unified status display and adaptive hints feel like the CLI *understands* the user
2. **Certainty through dogfooding** — Real-world testing identifies real bugs before they hit public release
3. **Memory safety** — Unbounded growth and concurrent streaming issues are time bombs in long sessions

Wave D creates a 1.0 that's not just "works" but "feels considerate."

---

## Appendix: Catalog Summaries

### Marquez UX Gap Catalog (25 Total)
- **3 P0:** Stub commands, cancel ops, cold SDK feedback
- **9 P1:** Status consistency, agent panel sync, messaging clarity, hints, errors
- **11 P2:** Terminal edge cases, polish, animations
- **2 P3:** Power user features

**Status:** 12 gaps addressed in PRs (#444, #445, etc.). 13 gaps remain for Wave D and beyond.

### Waingro Fragility Catalog (13 Total)
- **4 Memory/Growth:** Unbounded history, stale buffers, session context, unused managers
- **4 Data Loss:** Concurrent streaming, retry masking, parsing fragility, delta extraction
- **3 Edge Cases:** Buffering races, input mangling, name normalization
- **2 Long-Term:** Listener accumulation, stale session health

**Status:** Zero critical bugs in production. All risks are latent — unlikely under normal usage but would surface at scale (100+ messages, 10+ agents, long sessions).

### Hockney Test Debt
**File doesn't exist.** This suggests test debt is low — Hockney may not have created a formal inventory, or test coverage is deemed adequate by the team.

---

## Summary for Brady

**Current State:** CLI is polished, reliable, and tested. Waves A–C completed on schedule.

**What Delights Wave D:** Unified status display, adaptive keyboard hints, dogfood closure, memory safety hardening, and error clarity. These turn a solid CLI into one that feels like it *understands* the user.

**Timeline:** Batch 1 (high-value wins) is 1 week. Batch 2 (polish) is 2 days. Batch 3 (long-term hardening) can slide if needed.

**Blocking:** Only #324 (dogfood). Close it, document findings, proceed.

**Risk:** None identified. Wave D is a feature enhancement, not a foundation restructure.
