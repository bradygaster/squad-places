### 2026-02-08: "Feels Heard" — Immediate acknowledgment before agent spawns
**By:** Verbal (Prompt Engineer)
**Status:** Decided
**What:** The coordinator MUST respond with brief text acknowledging the user's request BEFORE spawning background agents. For single agents, use a human sentence naming the agent and describing the work. For multi-agent spawns, show a quick launch table with emoji, agent name, and task description. The acknowledgment goes in the same response as the `task` tool calls — text first, then tool calls.
**Why:** When the coordinator spawns background agents, there can be a significant delay before the user sees any response. A blank screen while agents work creates anxiety and breaks the feeling of a responsive team. Immediate acknowledgment makes the experience feel human — like a team lead saying "I'm on it" before diving into work.
**Where:** `.github/agents/squad.agent.md` — new "Acknowledge Immediately" subsection in Team Mode, placed before Directive Capture and Routing.
**Scope:** This is the coordinator-level instruction only. Does not change agent spawn templates or post-completion behavior.
