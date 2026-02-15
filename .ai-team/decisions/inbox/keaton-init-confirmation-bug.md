### 2026-02-15: Init Mode confirmation skip — root cause analysis
**By:** Keaton
**What:** Analysis of why Init Mode skips user confirmation, with proposed fixes
**Why:** Issue #66 — this is a recurring UX problem that undermines user control during team setup

---

## Root Cause Analysis

The Init Mode confirmation skip is a **prompt design problem**, not a logic error. The coordinator prompt has all the right steps in the right order — but the surrounding prompt context creates overwhelming pressure for the LLM to execute the full sequence in a single turn. There are **five reinforcing causes**.

### Cause 1: Numbered List Completion Impulse

Init Mode steps 1–8 are a single numbered list. LLMs are trained to complete sequences. When the model reaches step 5 ("Ask: Look right?"), it generates the question text — but the next token prediction sees step 6 right there in context. The model treats the numbered list as a **procedure to execute**, not a **conversation to have**. It "asks" the question as output text, then immediately proceeds to step 6 because that's what comes next in the sequence.

This is the **primary driver**. The model doesn't distinguish between "emit this question and stop" vs "emit this question as part of completing the list."

### Cause 2: Step 6 Phrasing — "On confirmation" is Ambiguous

> `6. On confirmation (or if the user provides a task instead, treat that as implicit "yes"), create the .ai-team/ directory structure`

"On confirmation" reads as a **conditional within the same execution frame**, not as a "wait for the next user message." The parenthetical "(or if the user provides a task instead, treat that as implicit 'yes')" further weakens the gate — the model can rationalize that the user's *original message* (e.g., "I'm building a Node.js API") constitutes a task, triggering the implicit-yes bypass.

There is no explicit instruction to **stop generating**, **end the turn**, or **wait for user input**. The word "confirmation" is doing all the gate-keeping work, and it's not enough.

### Cause 3: Eager Execution Philosophy Creates Contradictory Pressure

Line 16:
> **Mindset:** **"What can I launch RIGHT NOW?"** — always maximize parallel work

Line 480-486 (Eager Execution Philosophy):
> The Coordinator's default mindset is **launch aggressively, collect results later.**
> ...launch follow-up agents without waiting for the user to ask.

Line 848:
> DO NOT stop. Do NOT wait for user input.

These are Team Mode instructions, but they're in the **same prompt context** during Init Mode. The model doesn't scope instructions to modes — it absorbs the entire prompt as its behavioral baseline. The repeated "don't wait," "launch immediately," "don't stop" instructions create a strong prior against pausing for any reason.

### Cause 4: The Parenthetical Escape Hatch

Step 5:
> *"(Or just give me a task to start!)"*

Step 6:
> *(or if the user provides a task instead, treat that as implicit "yes")*

Step 8 (post-setup):
> *These are additive. Don't block — if the user skips or gives a task instead, proceed immediately.*

These three parentheticals collectively communicate: "confirmation is optional, proceeding is fine." The model reads "Or just give me a task" as license to treat the user's initial project description as that task. Combined with step 6's implicit-yes clause, the model has a clean logical path from "user said what they're building" → "that's a task" → "implicit yes" → "create everything."

### Cause 5: No Structural Turn Boundary

The prompt has no mechanism that **forces** a turn boundary between step 5 and step 6. In a multi-turn conversation, the only thing that creates a real pause is:
1. The model choosing to stop generating (weak — easily overridden by completion impulse)
2. A tool call like `ask_user` that structurally requires user input before continuing
3. An explicit "END YOUR RESPONSE HERE" instruction

Init Mode relies on option 1 alone. Given causes 1-4, option 1 consistently fails.

---

## Proposed Fixes

### Fix A: Explicit STOP Gate (Minimum viable fix)

Replace the current step 5-6 boundary with a hard stop instruction:

```markdown
5. Propose the team roster (step 4 above), then ask:
   *"Look right? Say **yes**, **add someone**, or **change a role**."*

   **⚠️ STOP HERE. End your response. Do NOT proceed to step 6.**
   Wait for the user's reply before creating any files.

6. **[ONLY after the user replies]** On confirmation (explicit "yes", "looks good",
   or similar affirmative — OR if the user provides a task instead of confirming),
   create the `.ai-team/` directory structure.
```

**Pros:** Minimal change, preserves existing flow.
**Cons:** Still relies on the model obeying a text instruction. LLMs can and do ignore "STOP" instructions, especially with competing pressure from the Eager Execution sections.

### Fix B: Structural Turn Break via Two-Phase Init (Recommended)

Split Init Mode into two clearly separated phases with an explicit turn boundary:

```markdown
## Init Mode — Phase 1: Cast the Team

No team exists yet. Propose one.

1. **Identify the user.** Run `git config user.name` and `git config user.email`.
2. Ask: *"What are you building? (language, stack, what it does)"*
3. **Cast the team** (see Casting & Persistent Naming algorithm).
4. Propose the team roster.
5. Ask: *"Look right? Say **yes**, **add someone**, or **change a role**."*

**Your response for Phase 1 ENDS here. Do not create any files or directories.**

---

## Init Mode — Phase 2: Create the Team

**Trigger:** User replied to the Phase 1 roster with confirmation or a task.

6. Create the `.ai-team/` directory structure...
7. Say: *"✅ Team hired..."*
8. Post-setup input sources...
```

**Pros:** The section boundary (horizontal rule + new heading) creates a structural signal that these are separate response turns. The model is much less likely to "complete" across section breaks than within a numbered list.
**Cons:** Slightly more verbose prompt. Requires the model to re-enter Init Mode Phase 2 on the next turn (but `team.md` doesn't exist yet, so the Init Mode check still triggers).

### Fix C: `ask_user` Tool Instruction (Strongest guarantee)

If the platform supports an `ask_user` tool that forces a turn boundary:

```markdown
5. Propose the team roster, then call the `ask_user` tool with:
   *"Look right? Say **yes**, **add someone**, or **change a role**."*
   The `ask_user` tool will pause execution until the user responds.
   Do NOT proceed to step 6 until `ask_user` returns.
```

**Pros:** Structural guarantee — the tool call mechanism forces a real pause. The model cannot "complete past" a tool call the way it can ignore text instructions.
**Cons:** Depends on `ask_user` being available on all platforms (CLI, VS Code, etc.). May not exist in all Copilot client contexts. Needs a fallback for platforms without `ask_user`.

### Fix D: Remove Competing Signals (Complementary — do with A, B, or C)

Add an Init Mode exception to the Eager Execution section:

```markdown
### Eager Execution Philosophy

> **Exception:** Eager Execution does NOT apply during Init Mode.
> Init Mode requires explicit user confirmation before creating the team.
> See Init Mode step 5 for the required pause.

The Coordinator's default mindset is **launch aggressively, collect results later.**
```

Also tighten the step 6 implicit-yes clause to prevent the original message from qualifying:

```markdown
6. On confirmation (explicit "yes"/"looks good"/affirmative in response to step 5's question,
   OR if the user's **reply to step 5** is a task instead of confirming), create...
```

The key change: "reply to step 5" — not the original message.

---

## Recommendation

**Implement Fix B (two-phase split) + Fix D (remove competing signals).**

Fix B is the most robust text-only solution because it uses structural formatting (section breaks, separate headings) to create a turn boundary, rather than relying on the model obeying an instruction it has competing reasons to ignore. Fix D removes the contradictory pressure that causes the model to rationalize skipping the pause.

Fix C (`ask_user`) is the strongest technical guarantee but has platform portability concerns. Add it as an enhancement once client parity (Issue #10) is resolved — at that point, we'll know which clients support `ask_user`.

Fix A alone is insufficient. The model already has an instruction to ask and wait (step 5). Adding more emphasis to the same instruction pattern is unlikely to change behavior when the root causes (completion impulse, eager execution pressure, implicit-yes escape hatch) remain.

---

## Validation Approach

After implementing the fix:
1. Test with 5+ fresh repos (no `.ai-team/` directory) across CLI and VS Code
2. Verify the coordinator stops after proposing the roster and does NOT create files
3. Test the implicit-yes path: respond to the roster with a task instead of "yes" — files should be created
4. Test modification: respond with "add a designer" — coordinator should re-propose, not create
