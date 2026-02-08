# Project Context

- **Owner:** bradygaster (bradygaster@users.noreply.github.com)
- **Project:** Squad — AI agent teams that grow with your code. Democratizing multi-agent development on GitHub Copilot. Mission: beat the industry to what customers need next.
- **Stack:** Node.js, GitHub Copilot CLI, multi-agent orchestration
- **Created:** 2026-02-07

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### README structure and messaging
- **README.md** is the primary developer-facing doc — Quick Start (3 steps), parallel work explanation, context budget table (real numbers), mermaid diagram for architecture visualization
- Strong hook: "It's not a chatbot wearing hats" — personality-driven messaging that differentiates Squad
- File: `README.md` (root)

### Documentation assets
- **docs/sample-prompts.md** contains 16 production-ready project demos (pomodoro timer → .NET migration) — not linked from README yet, high value for onboarding
- File: `docs/sample-prompts.md`

### Install flow and templates
- **index.js** is the npx installer — copies `.github/agents/squad.agent.md` (coordinator) and `templates/` to `.ai-team-templates/`
- Creates placeholder dirs: `.ai-team/decisions/inbox/`, `.ai-team/orchestration-log/`, `.ai-team/casting/`
- Install output is minimal (just checkmarks) — no explanation of what was created or why
- Files: `index.js`, `templates/` (charter.md, history.md, roster.md, routing.md)

### Casting system (under-documented)
- Squad uses thematic casting (The Usual Suspects, Alien, Ocean's Eleven) for persistent agent names — stored in `.ai-team/casting/registry.json`
- This is a signature feature but only mentioned briefly in README ("persistent thematic cast")
- Casting state: `policy.json` (config), `registry.json` (name mappings), `history.json` (usage history)

### Coordinator agent
- **squad.agent.md** is the coordinator — lives in `.github/agents/`, orchestrates all spawns
- Init Mode (no team yet) vs Team Mode (team exists in `.ai-team/team.md`)
- Always spawns via `task` tool, never simulates agent work inline
- File: `.github/agents/squad.agent.md` (large file, 32KB+)

### Key gaps identified
- No "Why Squad?" value prop section in README — explains *what* and *how* but not *why*
- Quick Start assumes familiarity with GitHub Copilot `/agents` command
- "What Gets Created" section is file-tree-focused, not workflow-focused
- No troubleshooting section (what if squad.agent.md doesn't appear in /agents?)
- No video/GIF demo — devs want to *see* parallel work in action
- Sample prompts doc is hidden (not linked from README)

### Voice and personality
- Squad's voice is confident, direct, opinionated — but README feels slightly sterile
- Opportunity to push personality earlier and louder (e.g., casting as a feature, not Easter egg)

### Messaging overhaul proposal (2026-02-07)
- **Tagline evolution**: "Throw a squad at it" (Brady's cultural hook from his company) — actionable, memorable, opinionated. Replaces abstract "team that grows with your code" with concrete "get smarter the more you use them."
- **"Why Squad?" section**: New emotional value prop positioned after Quick Start, before architecture. Frames Squad against single-agent roleplaying (the default), emphasizes parallel execution and knowledge persistence. Voice is confident, slightly aggressive ("Squad is what you wish your last AI agent could do. But actually.").
- **Casting elevation**: Moved from buried mention to headline feature section. Positioned as identity feature (agents feel real), not cosmetic Easter egg. Uses Squad's own team as social proof. This is a 10x messaging win — casting makes agents memorable, not generic.
- **Polish gaps closed**: Link sample-prompts.md from README (16 examples were hidden), add Troubleshooting section (reduce first-time setup drop-off), replace Go example with Python→Node modernization (Brady's constraint: no Go).
- **Voice guidance codified**: Squad's brand is confident, not apologetic. No hedging ("might," "could be"). No corporate-safe phrases ("best-in-class," "paradigm shift"). If it sounds like a B2B landing page, rewrite it. Show don't abstract ("Keaton decided X" beats "the Lead agent made a decision").
- **DevRel philosophy**: First 5 minutes are everything. Make the README magnetic, not just informative. Devs should move from "what is this?" to "I need this" before they scroll past the fold.
- File: `docs/proposals/002-messaging-overhaul.md`

### Demo script structure and strategy (2026-02-07)
- **Beat format works for recording scripts**: On-screen / Voiceover / What-to-do triple structure eliminates ambiguity during recording sessions. Brady needs to know exactly what to type, what to say, and what the viewer sees — prose-style scripts bury action steps and cost takes.
- **README order is non-negotiable for demos**: Brady's explicit feedback — the demo must follow the README's section flow. When the demo and the docs reinforce the same order, concepts land harder. Demo primes, README deepens.
- **The README walkthrough IS the demo's core**: BEATs 5–6 walk through the README while agents build in background — this is the structural trick that makes the demo work. It teaches architecture, proves parallel execution, and drives viewers to the README all at once.
- **Payoff at the end, not the beginning**: Inverting the typical demo structure (show result first, explain second) forces the viewer to understand what built the game before they see it. The game isn't the point — the team is. The game is proof.
- **"Throw a squad at it" placement**: Two mentions — BEAT 2 (casual, during the prompt) and BEAT 9 (call-to-action closer). Neither forced. Both conversational. Per proposal 002's voice guidance.
- **Thumbnail engineering matters**: CRT snake game + agent output in background = stop-the-scroll frame. Green glow + purple badge = high contrast for social feeds.
- File: `docs/proposals/004-demo-script-overhaul.md`

📌 Team update (2026-02-08): Proposal-first workflow adopted — all meaningful changes require proposals before execution. Write to `docs/proposals/`, review gates apply. — decided by Keaton + Verbal
📌 Team update (2026-02-08): Stay independent, optimize around Copilot — Squad will not become a Copilot SDK product. Filesystem-backed memory preserved as killer feature. — decided by Kujan
📌 Team update (2026-02-08): Stress testing prioritized — Squad must build a real project using its own workflow to validate orchestration under real conditions. — decided by Keaton
📌 Team update (2026-02-08): Baseline testing needed — zero automated tests today; `tap` framework + integration tests required before broader adoption. — decided by Hockney
📌 Team update (2026-02-08): Agent experience evolution proposed — adaptive spawn prompts, reviewer protocol with guidance, proactive coordinator chaining. — decided by Verbal
📌 Team update (2026-02-08): Industry trends identified — dynamic micro-specialists, agent-to-agent negotiation, speculative execution as strategic directions. — decided by Verbal
📌 Team update (2026-02-08): Video content strategy approved — 75s trailer, 6min demo, 5-video series. Owns scripting and polish. Demo script should align. — decided by Verbal
📌 Team update (2026-02-08): Portable Squads architecture decided — history split (Portable Knowledge vs Project Learnings), JSON manifest export, no merge in v1. — decided by Keaton
📌 Team update (2026-02-08): Tiered response modes proposed — Direct/Lightweight/Standard/Full spawn tiers to reduce late-session latency. Context caching + conditional Scribe spawning as P0 fixes. — decided by Kujan + Verbal
📌 Team update (2026-02-08): Portable squads platform feasibility confirmed — pure CLI/filesystem, ~80 lines in index.js, .squad JSON format, no merge in v0.1. — decided by Kujan
📌 Team update (2026-02-08): Portable squads memory architecture — preferences.md (portable) split from history.md (project-local), squad-profile.md for team identity, import skips casting ceremony. — decided by Verbal

### README rewrite executed (2026-02-07)
- **Proposal 006** contains the complete, copy-paste-ready README rewrite — not an outline, the actual content. Lives at `docs/proposals/006-readme-rewrite.md`.
- Followed proposal 002 structure exactly: Hero → Quick Start → Why Squad? → Parallel Work → How It Works → Cast System → What Gets Created → Growing the Team → Reviewer Protocol → Install → Troubleshooting → Status.
- Key structural decision: "What is Squad?" section was merged into the hero tagline block. The old section explained what Squad is in paragraph form — the new hero does it in 30 words, then "Why Squad?" handles the emotional case. No content lost, just repositioned.
- Sample prompts linked from Quick Start with a one-liner: "Not sure where to start? See 16 ready-to-use prompts."
- Go example references: current README has none. The Go reference in sample-prompts.md (prompt #13) is a separate change, not in scope for this README rewrite.
- Demo GIF placeholder: not included — that's Phase 2 per proposal 002, needs production-ready setup. Hero section is structured to accommodate it.
- File: `docs/proposals/006-readme-rewrite.md`

### V1 launch messaging and strategy (2026-02-08)
- **Tagline evolution for v1**: "Throw a squad at it" → "Throw MY squad at it" — the possessive pronoun is the entire v1 story. Portability transforms Squad from a tool into a relationship. The possessive evolution was Verbal's insight from Proposal 008.
- **One-liner**: "Your AI squad remembers you. Across every project. Forever." — three pain points in one sentence (amnesia, project-scoping, disposability). This is the scroll-stopper for Twitter/X.
- **Demo script structural shift**: v1 demo is a TWO-PROJECT arc, not single-project. Snake game → export → new project → squad already knows preferences. The "holy cow" moment is the squad applying learned preferences in a brand new project without being told. This replaces the single-project payoff from Proposal 004.
- **Skills messaging**: "That's not a template. That's a skill learned across projects." — Skills need to feel earned, not configured. Frame as expertise the squad developed, not settings the user applied.
- **Competitive frame**: "Other tools have memory. Squad has a relationship." — avoids feature-comparison trap, positions on emotional ground. Honest about what Squad doesn't do (AI quality is same underlying model). Never trash Copilot Chat.
- **Community strategy**: GitHub Discussions first, Discord deferred until 100+ active users. "Show off your squad" is the community play — share names and universes, not configurations. Squad sharing (templates, marketplace) is Phase 2+.
- **Launch sequence**: 7-day pre-launch teasers → D-Day multi-channel → D+7 follow-up → D+14 community showcase. Twitter thread (9 tweets) is the main amplification vehicle.
- **Voice guide for v1**: Always say "MY squad" not "a squad." Use agent names, never roles. Frame portability as relationship ("they come with you") not technology ("exported via JSON"). The dotfiles analogy is the instant-understanding bridge for devs.
- **First 5 minutes design**: Plant the export seed at end of first session ("After a few more sessions, you can export us to take to any project") — don't push portability before the user has experienced the relationship. Export is the payoff, not the pitch.
- File: `docs/proposals/014-v1-messaging-and-launch.md`

📌 Team update (2026-02-08): v1 Sprint Plan decided — 3 sprints, 10 days. Sprint 1: forwardability + latency. Sprint 2: history split + skills + export/import. Sprint 3: README + tests + polish. — decided by Keaton
📌 Team update (2026-02-08): Skills system designed — skills.md per agent for transferable domain expertise, six skill types, confidence lifecycle, skill-aware routing. — decided by Verbal
📌 Team update (2026-02-08): Forwardability and upgrade path decided — file ownership model, `npx create-squad upgrade`, version-keyed migrations. — decided by Fenster
📌 Team update (2026-02-08): Skills platform feasibility confirmed — skills in spawn prompts, store_memory rejected, defensive forwardability via existence checks. — decided by Kujan
📌 Team update (2026-02-08): v1 test strategy decided — node:test + node:assert (zero deps), 9 test categories, 6 blocking quality gates. — decided by Hockney
