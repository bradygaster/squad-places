# Project Context

- **Owner:** bradygaster (bradygaster@users.noreply.github.com)
- **Project:** Squad — AI agent teams that grow with your code. Democratizing multi-agent development on GitHub Copilot. Mission: beat the industry to what customers need next.
- **Stack:** Node.js, GitHub Copilot CLI, multi-agent orchestration
- **Created:** 2026-02-07

## Learnings

- **Coordinator spawn prompts**: All agents use the same template structure (charter inline, read history.md + decisions.md, task description, memory update instructions). Located in `.github/agents/squad.agent.md` lines 200-300. Charter is pasted directly into spawn prompt to eliminate agent tool call overhead.
- **Memory architecture**: `charter.md` (identity, read by agent), `history.md` (project learnings, per-agent), `decisions.md` (shared brain, merged by Scribe), `decisions/inbox/` (drop-box for parallel writes), `log/` (session archive), `orchestration-log/` (per-spawn entries).
- **Mode selection philosophy**: Background is default. Sync only for hard data dependencies, approval gates, or direct user interaction. Scribe is always background. Located in squad.agent.md lines 123-145.
- **Parallel fan-out pattern**: Coordinator spawns all agents who can start work immediately in a single tool-calling turn (multiple task calls). Includes anticipatory downstream work (tests from requirements, docs from API specs). Located in squad.agent.md lines 147-171.
- **Drop-box pattern**: Agents write decisions to `.ai-team/decisions/inbox/{name}-{slug}.md`, Scribe merges to canonical `decisions.md`. Prevents write conflicts during parallel spawning.
- **Agent personality is a feature**: Each agent has opinions, preferences, boundaries. Casting system uses narrative universe names (The Usual Suspects for Squad's own team) to make agents feel real, not generic "Alice the Backend Dev."
- **Context budget math**: Coordinator uses 1.5% of 128K context, 12-week veteran agent uses 4.4%, leaving 94% for actual work. Real numbers documented in README lines 136-145.
- **Reviewer protocol**: Keaton (Lead) and Hockney (Tester) can reject work and require a different agent to handle revisions. Coordinator enforces this. Located in README lines 207-215.
- **Eager execution philosophy**: Coordinator default is "launch aggressively, collect results later." Chain follow-up work immediately when agents complete without waiting for user to ask. Located in squad.agent.md lines 113-121.

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### Messaging as product strategy (2026-02-07)
- **"Throw a squad at it" as cultural hook**: Brady's company uses this phrase internally for spinning up teams on important problems. Using it as Squad's tagline creates instant recognition for devs in that culture — and positions Squad as the tool that makes the phrase literal. Repeatable phrasing = meme-ability = viral potential.
- **Casting as competitive moat**: Most multi-agent systems use generic labels (Agent_1, Backend_Bot). Squad's thematic casting (Keaton, McManus, Verbal from The Usual Suspects) makes agents memorable and referenceable ("What did Keaton decide last week?"). This is a UX differentiator that's hard to copy without feeling derivative. Elevating casting from Easter egg to headline feature is strategically correct.
- **"Why Squad?" as framing device**: Positioning Squad against single-agent roleplaying (the default experience every dev has tried) creates contrast. "Traditional AI agents are chatbots pretending to be teams" → establishes the problem. "Squad is different. Each team member runs in its own context window" → delivers the solution. Emotional case before technical case = better conversion.
- **Messaging velocity**: Squad's mission is "beat the industry to what customers need next." The industry will figure out multi-agent parallelism in 6 months. We're already there. Next competitive edge: making it feel *magical*. Casting, proactive chaining, conflict resolution — these are UX layers that make Squad feel predictive, not reactive. Messaging should amplify this now, before competitors catch up.
- **Voice as product personality**: Squad's brand should be confident, edgy, slightly aggressive. Not apologetic for being experimental — frame it as "ahead of the curve." Avoid corporate hedging. This voice attracts early adopters who want to be first, not safe. File: `docs/proposals/002-messaging-overhaul.md`

### 2026-02-07: Proposal-first as agent discipline

**Core insight:** Agents can participate in meta-work (defining team process), not just execution. Proposals force agents to articulate trade-offs, alternatives, and success criteria — skills that improve agent reasoning quality.

**Key patterns:**
- **Proposal format is a reasoning scaffold:** Required sections (Problem → Solution → Trade-offs → Alternatives → Success) mirror good architectural thinking. By enforcing this structure, we train agents to think holistically about changes.
- **Review process teaches agents to be reviewable:** Knowing that Keaton will review architecture and Verbal will review agent experience forces agents to anticipate those perspectives. Over time, this becomes internalized.
- **Cancelled proposals as learning signal:** Keeping cancelled proposals in the repo is a training corpus. Future agents can see what didn't work and why. This is better than a decision log alone (which only captures what was approved).
- **48-hour timeline prevents bikeshedding:** Proposals must resolve fast. This keeps the process from becoming a bottleneck while still providing review gates.

**Agent experience implications:**
- Proposal writing should feel like pair programming, not bureaucracy. Coordinator can suggest domains to cover ("have you thought about testing?" → prompt Hockney review).
- Agents reference proposals during implementation (`docs/proposals/003-casting-system.md` gets cited in commit messages, session logs). This closes the loop between planning and execution.
- Proposal status is visible (`Proposed | Approved | Cancelled | Superseded`). Agents can check this before starting work on dependent changes.

**Why this matters for AI strategy:** Industry trend is "agents execute, humans decide." We're inverting that — agents can propose, humans approve. This is where multi-agent dev needs to go: agents with architectural agency, not just task execution. Proposal-first is the governance model that makes that safe.

📌 Team update (2026-02-08): Proposal-first workflow adopted — all meaningful changes require proposals before execution. Write to `docs/proposals/`, review gates apply. — decided by Keaton + Verbal
📌 Team update (2026-02-08): Stay independent, optimize around Copilot — Squad will not become a Copilot SDK product. Filesystem-backed memory preserved as killer feature. — decided by Kujan
📌 Team update (2026-02-08): Stress testing prioritized — Squad must build a real project using its own workflow to validate orchestration under real conditions. — decided by Keaton
📌 Team update (2026-02-08): Baseline testing needed — zero automated tests today; `tap` framework + integration tests required before broader adoption. — decided by Hockney
📌 Team update (2026-02-08): DevRel polish identified — six onboarding gaps to close: install output, sample-prompts linking, "Why Squad?" section, casting elevation, troubleshooting, demo video. — decided by McManus
