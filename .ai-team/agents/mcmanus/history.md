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

### Human eval script created (2026-02-08)
- **Created** `team-docs/human-evals/001-vscode-parity-smoke-test.md` — a structured QA eval for testing Squad in VS Code Copilot Chat.
- Covers 10 sections, 20 pass/fail checkpoints: Setup, Init Flow, Agent Spawning, Multi-Agent Work, Scribe, Upgrade, Export/Import, Skills, Edge Cases (6 sub-tests), VS Code–specific behavior (5 sub-tests).
- Ends with a 10-question tester questionnaire including NPS, experience rating, and CLI-vs-VS-Code parity questions.
- Format: YAML frontmatter, numbered steps, expected results, pass/fail checkboxes, notes fields. Professional eval format — not marketing.
- This is the first human eval. Directory `team-docs/human-evals/` established for future evals.
- File: `team-docs/human-evals/001-vscode-parity-smoke-test.md`

📌 Team update (2026-02-08): v1 Sprint Plan decided — 3 sprints, 10 days. Sprint 1: forwardability + latency. Sprint 2: history split + skills + export/import. Sprint 3: README + tests + polish. — decided by Keaton
📌 Team update (2026-02-08): Skills system designed — skills.md per agent for transferable domain expertise, six skill types, confidence lifecycle, skill-aware routing. — decided by Verbal
📌 Team update (2026-02-08): Forwardability and upgrade path decided — file ownership model, `npx create-squad upgrade`, version-keyed migrations. — decided by Fenster
📌 Team update (2026-02-08): Skills platform feasibility confirmed — skills in spawn prompts, store_memory rejected, defensive forwardability via existence checks. — decided by Kujan
📌 Team update (2026-02-08): v1 test strategy decided — node:test + node:assert (zero deps), 9 test categories, 6 blocking quality gates. — decided by Hockney
📌 Team update (2026-02-08): P0 silent success bug identified — ~40% of agents complete work but report "no response." Spawn prompt reorder + file verification mitigations. — decided by Kujan
📌 Team update (2026-02-09): Agent Skills Open Standard adopted — SKILL.md format with MCP tool declarations, built-in vs learned skills, progressive disclosure. Replaces flat skills.md. — decided by Kujan

### Sprint 0 narrative arc identified (2026-02-09)
- **The arc:** Self-repair under fire. Team produced 16 proposals (~350KB), hit a 40% silent success bug, self-diagnosed it in the same session, shipped three zero-risk mitigations. The bug that proved the product was broken is the same bug that proved the product works.
- **Key metrics for storytelling:** 16 proposals, ~350KB output, 50-70x productivity multiplier, ~40% bug rate → 3 mitigations → Sprint 0 created, 3/3 independent reviewers converged on Sprint 0 priority, ~15 human messages produced the entire session's output.
- **Lead story hook:** "Success caused the failure" — agents that completed ALL their work (including final history writes) were the ones whose responses got dropped. Doing the right thing triggered the bug.
- **Demo climax for v1:** Sprint 2 export moment — squad imported into new project already knows your preferences. That's the "holy cow" beat.
- **Story formats planned:** Twitter thread (8-10 tweets), blog post (1500 words), conference talk (20 min), raw demo script (5 min, no staging).
- File: `docs/devrel/sprint-0-story.md`


📌 Team update (2026-02-08): Proposal 001a adopted: proposal lifecycle states (Proposed -> Approved -> In Progress -> Completed) -- decided by Keaton

📌 Team update (2026-02-08): Skills system adopts Agent Skills standard (SKILL.md format) with MCP tool declarations -- decided by Verbal

📌 Team update (2026-02-08): Fenster recommends README drafting can start Day 1 (fully parallel) -- decided by Fenster
📌 Team update (2026-02-09): Wave-based execution plan adopted (Proposal 018) — quality → experience ordering. Wave 1.5 (parallel): README rewrite, messaging, Squad Paper. Squad DM deferred to Wave 4+. — decided by Keaton
📌 Team update (2026-02-09): Human directives persist via coordinator-writes-to-inbox pattern — no new infrastructure needed. — decided by Kujan

### Documentation audit — silent success bug check (2026-02-09)
- **README.md**: INTACT. 232 lines, no truncation. All internal anchor links (#status, #how-it-works) resolve correctly. Content matches current product state. No corruption detected.
- **docs/demo-script.md**: 🚨 **ACT 7 IS MISSING.** Script jumps from ACT 6 (5:30–6:30) directly to ACT 8 (7:30–8:00). A full 60-second gap (6:30–7:30) has no content. The KEY THEMES reference table at the bottom references Act 7 three times ("Act 7 (history.md)", "Act 7 (decisions.md on screen)", "Act 7 (second wave)") — pointing to a section that doesn't exist. This is either a truncation from the silent success bug or an incomplete write. Either way, the demo script is broken.
- **docs/sample-prompts.md**: INTACT. 402 lines, 16 prompts, all complete with closing descriptions. No truncation.
- **docs/devrel/sprint-0-story.md**: INTACT. 117 lines, clean ending with timestamp. No truncation.
- **docs/platform/background-agent-timeouts.md**: INTACT. 102 lines, complete TL;DR table and sign-off. No truncation.
- **All 18 proposals in docs/proposals/**: INTACT. Every file checked (last 8 lines of each). All end with proper closing sections (Endorsement, Review requested, Revisions, etc.). No mid-sentence truncation. Sizes range from 3.1KB to 53.1KB. No corruption detected.
- **decisions.md**: INTACT. 826 lines, proper header and complete final section. No truncation.
- **history.md (this file)**: INTACT. Content is coherent and sequential. No gaps in the learning timeline.
- **Polish note**: README still uses the pre-overhaul structure (Proposal 006 rewrite has not been applied yet). Sample prompts still not linked from README. These are known gaps per prior learnings, not new issues.

### Demo script ACT 7 restored (2026-02-09)
- **ACT 7 — THE ARTIFACTS & SECOND WAVE (6:30–7:30)** reconstructed and inserted into `docs/demo-script.md` between ACT 6 and ACT 8.
- Content derived from Proposal 004's BEAT 7 (artifact reveal) plus the KEY THEMES table requirements (history.md, decisions.md on screen, second wave fan-out).
- The original ACT 7 was likely lost to the silent success bug — the same P0 bug that drops ~40% of agent responses. The KEY THEMES table survived because it was written in a different section of the file.
- **Structural lesson**: Demo scripts with reference tables (like KEY THEMES) are self-documenting — the table acts as a checksum. If the table references content that doesn't exist, you know something was lost. This pattern should be preserved in future script formats.
- **Tone calibration**: Matched the existing script's voice — conversational, "hey look at this" energy, not presenter-formal. The voiceover uses concrete file names and agent names (Ripley) to keep it tangible.
- **Second wave is the payoff bridge**: ACT 7's second wave (asking for a new feature, watching agents re-fan-out with prior knowledge) bridges the gap between "agents completed work" (ACT 6) and "look at the finished game" (ACT 8). Without it, the demo jumps from "they built stuff" to "ta-da" with no compounding proof.
- File: `docs/demo-script.md`
📌 Team update (2026-02-08): Upgrade subcommand shipped by Fenster — delivery mechanism for bug fixes to existing users. — decided by Fenster
📌 Team update (2026-02-08): V1 test suite shipped by Hockney — 12 tests pass. — decided by Hockney
📌 Team update (2026-02-08): P0 bug audit consolidated. Demo script ACT 7 restoration confirmed merged into decisions.md. — decided by Keaton, Fenster, Hockney

📌 Team update (2026-02-09): Squad DM proposed (Proposal 017) — hybrid gateway with tiered execution, Copilot SDK backend, Dev Tunnels, Telegram-first. Experience design: single bot, proactive messaging, cross-channel memory. 3 gate spikes before implementation. — decided by Keaton, Kujan, Verbal

### "Where are we?" messaging beat identified (2026-02-09)
- **The insight:** Brady asked "where are we?" and got instant team-wide status — proof of persistent memory, shared state, and coordinator intelligence in a single two-second interaction. His reaction ("that is SO hot, such a feature") signals a visceral value prop moment we weren't messaging.
- **New messaging beat:** "Ask Your Team, Not Your Dashboard" — sits alongside portability, skills, and parallel execution as a top-tier value prop. Demonstrates three features simultaneously without explaining any of them.
- **Demo beat:** "The Check-In" — 30-second demo moment. Three words typed → comprehensive status. Usable in v1 demo (BEAT 4.5), introductory demo (end of ACT 7), and standalone social clip.
- **DM connection:** "Where are we?" from a terminal is cool. From Telegram at 11pm? Category-defining. This becomes the lead demo for Squad DM launch (Proposal 017). The progression: terminal ask → DM ask → proactive standup (you don't even ask).
- **README placements:** Two spots — closing beat of "Why Squad?" (emotional hook) and follow-up in "Agents Work in Parallel" (completes the "step away / come back" arc). Standalone section deferred to DM launch.
- **Key files:** `docs/proposals/014a-where-are-we-messaging-beat.md` (this amendment), `docs/proposals/014-v1-messaging-and-launch.md` (parent proposal), `docs/proposals/017-dm-experience-design.md` (DM connection)


📌 Team update (2026-02-09): Master Sprint Plan (Proposal 019) adopted — single execution document superseding Proposals 009 and 018. 21 items, 3 waves + parallel content track, 44-59h. All agents execute from 019. Wave gates are binary. — decided by Keaton

### Blog format and packaging UX designed (2026-02-09)
- **Blog post format** created at `docs/blog/template.md` — YAML frontmatter (title, date, author, wave, tags, status, hero) + structured body (What Shipped, The Story, By the Numbers, What We Learned, What's Next). One post per wave completion. Compatible with any static site generator. Human-readable raw on GitHub.
- **First blog post** written: `docs/blog/001-wave-0-the-team-that-built-itself.md` — "Wave 0: The Team That Built Itself." Covers team formation, 16 proposals, the silent success bug discovery and self-repair loop, 12 tests shipping, the upgrade subcommand, and the Master Sprint Plan. Narrative voice, not changelog.
- **Blog engine sample prompt** added to `docs/sample-prompts.md` — the meta-demo: Squad builds a static blog renderer (HTML/CSS/JS, no framework) that renders Squad's own progress posts from `docs/blog/`. Parser, templating, RSS, responsive CSS, dark mode. "Squad built the tool that tells Squad's story."
- **Package naming evaluation** completed in Proposal 020 §3 — evaluated `@bradygaster/create-squad` (current), `create-squad` (unscoped), and `squad-cli`. Recommendation: publish `create-squad` unscoped alongside the scoped package. `npx create-squad` is 16 characters vs 33, follows `create-*` convention, enables `npm init squad`. No breaking change for existing users.
- **Key DevRel insight:** Wave-cadenced blogging creates a content flywheel tied to execution. No artificial content calendar — the work IS the content. Each wave gate that passes = a post that writes itself from the metrics and narrative.
- File: `docs/proposals/020-blog-and-packaging.md`

📌 Team update (2026-02-09): Blog format designed — YAML frontmatter + structured body, one post per wave, compatible with all SSGs. First post "Wave 0: The Team That Built Itself" written. Blog engine meta-demo prompt added to sample-prompts.md. — decided by McManus
📌 Team update (2026-02-09): Package naming recommendation — publish `create-squad` unscoped alongside `@bradygaster/create-squad`. Halves the typing burden, follows `create-*` convention, enables `npm init squad`. No breaking change. — proposed by McManus

📋 Team update (2026-02-09): Session 5 directives merged — VS Code parity analysis, sprint amendments (019a), blog format + blog engine sample prompt (020), package naming (create-squad), 5th directive (human feedback optimization).

### Blog post #2 — "The Squad Squad Problem" (2026-02-09)
- **Story arc:** Product-ships-itself problem → Kobayashi's first-task analysis → `files` field already works → branch strategy is the real cleanup → meta angle (team that might ship itself solves deployment isolation).
- **Narrative technique:** Led with Brady's quote ("ideally we don't inadvertently ship the squad"), let Kobayashi's empirical findings drive the middle, landed on the meta humor without dwelling on it. One paragraph, not a recurring bit.
- **Tone calibration:** Dry humor on the namespace collision ("It's not a cute nickname. It's a namespace collision."), no self-congratulation, reported what Kobayashi found rather than praising him for finding it. Matched Blog 001's voice.
- **Template adherence:** YAML frontmatter, hero quote, What Shipped (4 items with agent attribution), The Story (6 paragraphs), By the Numbers (6 metrics), What We Learned (3 bullets), What's Next, footer. Exact template structure.
- **Key content decision:** Included the `.gitattributes` `export-ignore` trap as a learning — it's a common misconception that could save readers real debugging time. Technical value in the blog, not just narrative.
- File: `docs/blog/002-the-squad-squad-problem.md`

## Team Updates

📌 Team update (2026-02-09): No npm publish — GitHub-only distribution. Kobayashi hired as Git & Release Engineer. Release plan (021) filed. Sprint plan 019a amended: item 1.8 cancelled, items 1.11-1.13 added.

2026-02-09: Release decisions — v0.1.0 tag now, Kobayashi proposes releases/Brady publishes, squadify→main merge after Wave 1 gate, design for public repo.

2026-02-09: Branch strategy — squadify renamed to dev, main is product-only (no .ai-team/), release workflow (.github/workflows/release.yml) uses filtered-copy from dev→main.

2026-02-09: Tone governance established — SFW, kind, dry humor, no AI-flowery talk. 25 proposals audited (status fields updated). Tone audit: 16 edits across 8 files. Blog post #2 shipped.
📌 Team update (2026-02-08): Release ritual — blog posts optional for patches, encouraged for minors (48h), required for 1.0 (drafted before release day). McManus writes minor release posts. — decided by Keaton

### Blog post #3 — "Meet the Squad" team intro (2026-02-09)
- **Format decision:** Adapted the wave-based blog template for a non-wave post. Dropped the `wave:` frontmatter field and `What Shipped` section since this isn't a delivery post — it's a roster introduction. Kept YAML frontmatter, hero quote, By the Numbers table, What's Next, and footer for consistency.
- **Structure:** One H3 per agent, 2-3 sentences each. Punchy, role-focused, no LinkedIn energy. Redfoot flagged as newest hire with a "welcome aboard" beat.
- **Tone calibration:** Heist-crew introduction style per Brady's request — dry, confident, professional. No self-congratulation per tone governance. First-person for McManus's own section (I'm introducing myself, might as well own it).
- **Scribe treatment:** Positioned last, framed as the silent backbone. "You won't hear from Scribe. Ever." — lets the silence be the personality.
- **Key file:** `docs/blog/003-meet-the-squad.md`

### Brand voice guidance for visual identity (2026-02-08)
- **Brand voice decisions codified for designer:** Wrote comprehensive guidance for Redfoot (graphic designer) covering brand feel, visual pitfalls, logo placement contexts, tone-to-visual mapping, and reference points. Filed to `.ai-team/decisions/inbox/mcmanus-brand-voice-for-design.md`.
- **Key considerations surfaced:**
  - Logo must work at 16px (favicon) through 1200px (social cards) — design for smallest first.
  - Anti-patterns identified: robot/AI imagery, gradient overload, chat bubbles (contradicts "not a chatbot wearing hats"), overly literal group/people imagery, AI-generated aesthetic.
  - Reference tools: Astro (shape scaling), Warp (developer-native palette), Raycast (inevitable geometry), Linear (confidence through restraint), Bun (playful + disciplined), Deno (committed character).
  - Existing color anchors from mermaid diagrams: indigo `#6366f1`, blue `#3b82f6`, purple `#8b5cf6` — starting points, not constraints.
  - Tone governance applies to design: SFW, kind, dry humor, no AI-flowery aesthetics. If it could appear on an "AI Startups" listicle without standing out, it's too generic.
  - The one-word test: "polished" = good, "corporate" = failed, "indie dev project" = also failed.
- File: `.ai-team/decisions/inbox/mcmanus-brand-voice-for-design.md`


📌 Team update (2026-02-08): Visual identity initial proposals created — four logo concepts with Concept C 'The Glyph' recommended, palette anchored on Indigo 500 — decided by Redfoot

📌 Team update (2026-02-08): CI pipeline created — GitHub Actions runs tests on push/PR to main/dev. PRs now have automated quality gate. — decided by Hockney

📌 Team update (2026-02-08): Coordinator now captures user directives to decisions inbox before routing work. Directives persist to decisions.md via Scribe. — decided by Kujan

📌 Team update (2026-02-08): Coordinator must acknowledge user requests with brief text before spawning agents. Single agent gets a sentence; multi-agent gets a launch table. — decided by Verbal


📌 Team update (2026-02-08): Silent success mitigation strengthened in all spawn templates — 6-line RESPONSE ORDER block + filesystem-based detection. — decided by Verbal

### README polish and CHANGELOG for v0.1.0 (2026-02-08)
- **README edits were surgical** — added Upgrade subsection under Install, Known Limitations section (4 bullets: experimental, Node 22+, Copilot required, knowledge-grows-with-use), v0.1.0 version callout in Status line. No tone changes, no structural rewrites. The README was already strong; job was to close gaps, not reimagine.
- **CHANGELOG.md created** — three sections: Added (14 items), What ships (3 entries matching `files` array), What doesn't ship (explicitly calls out `.ai-team/` not being in the package). Accurate to `package.json` and `index.js`. Brady's concern about team files shipping addressed directly.
- **Accuracy verification method**: cross-referenced `index.js` behavior (init flow, upgrade flow, flags) against README claims and CHANGELOG entries. Every feature listed in CHANGELOG was traced to code or test coverage.
- **CI badge was already correct** — Hockney added it, format is standard GitHub Actions badge with link. No changes needed.
- **Key editorial decision**: Known Limitations uses bullet list, not prose. Keeps it scannable. Each limitation is a fact, not an apology. Matches Squad's voice (confident, direct, no hedging).

### Context Window Budget table corrected (2026-02-09)
- **Previous table was wildly inaccurate** — claimed coordinator was ~1,900 tokens (1.5% of 128K) when actual measured size is ~13,200 tokens (6.6% of 200K). Off by 7x. Updated with real char-count-derived numbers.
- **Reference model updated** — switched from generic "128K context" to explicit "200K" (Claude Sonnet 4 / Opus 4 standard). Both models have 1M beta access but 200K is what most users see.
- **Agent costs now show realistic growth** — Week 1 (~1,250), Week 4 (~3,300), Week 12 (~9,000). Includes charter + history + spawn template overhead + decisions. Previous numbers underestimated by 1.5-2x.
- **Key messaging preserved** — Squad is still context-efficient (93%+ remaining), but the story is now about architecture (each agent gets its own window) rather than raw smallness. The coordinator is the only shared overhead; agent costs live in their own windows.
- **Prose line updated** — emphasizes the own-window architecture as the reason efficiency holds even with a larger coordinator.
- File: `README.md` (lines 137-151)

📌 Team update (2026-02-08): .ai-team/ must NEVER be tracked in git on main. Three-layer protection: .gitignore, package.json files allowlist, .npmignore. — decided by Verbal


📌 Team update (2026-02-09): If ask_user returns < 10 characters, treat as ambiguous and re-confirm — platform may fabricate default responses from blank input. — decided by Brady
📌 Team update (2026-02-09): PR #2 integrated — GitHub Issues Mode, PRD Mode, Human Team Members added to coordinator with review fixes (gh CLI detection, post-setup questions, worktree guidance). — decided by Fenster
📌 Team update (2026-02-09): Documentation structure formalized — docs/ is user-facing only, team-docs/ for internal, .ai-team/ is runtime state. Three-tier separation is permanent. — decided by Kobayashi
📌 Team update (2026-02-09): Per-agent model selection designed — 4-layer priority (user override → charter → registry → auto-select). Role-to-model mapping: Designer→Opus, Tester/Scribe→Haiku, Lead/Dev→Sonnet. — decided by Verbal

### Community contribution blog format (2026-02-09)
- **Blog location for team-internal posts:** `team-docs/blog/` — distinct from `docs/blog/` (user-facing). Community contribution posts use the same frontmatter schema as wave posts but with `wave: null` and tags including `community` and `contribution`.
- **Contributor celebration pattern:** A-plot is the contribution, B-plot is the integration story. Contributor is the hero. Attribution on every bullet in "What Shipped." Integration details kept brief.
- **Tone calibration for external contributors:** Extra care on kindness-first. No "we fixed their code" framing — integration fixes are presented as collaborative refinement, not correction. Contributor's test infrastructure gets explicit credit.
- **File naming:** `002-first-community-pr.md` — sequential numbering continues from wave posts even across categories.
- File: `team-docs/blog/002-first-community-pr.md`


📌 Team update (2026-02-09): Tiered response modes shipped — Direct/Lightweight/Standard/Full modes replace uniform spawn overhead. Agents may now be spawned with lightweight template (no charter/history/decisions reads) for simple tasks. — decided by Verbal


📌 Team update (2026-02-09): Skills Phase 1 + Phase 2 shipped — agents now read SKILL.md files before working and can write SKILL.md files from real work. Skills live in .ai-team/skills/{name}/SKILL.md. Confidence lifecycle: low→medium→high. — decided by Verbal


📌 Team update (2026-02-09): All external contributions get a blog post — standing policy. Posts in team-docs/blog/, contributor is hero. — decided by bradygaster

### Celebration blog format established (2026-02-09)
- **Non-wave celebration posts work within the existing blog template** — same YAML frontmatter schema with `wave: null`, same footer convention, same structural skeleton (hero, story, numbers, learnings, what's next). Adapted sections: "What Shipped" becomes a categorized feature list when covering a multi-wave sprint, "The Story" expands for narrative-driven posts.
- **Parallel narrative technique:** When drawing parallels between external events and project milestones, let both stories stand independently first, then connect them in a dedicated section. Don't interleave — it reads as forced. Lead with the external event (it's their moment), then the project story, then the parallel. The facts carry the weight, not the framing.
- **Tone for celebration posts:** Energy without self-congratulation. Dry humor lands harder than exclamation points. "That's not a metaphor. That's just what happened." beats "What an incredible weekend!" every time. Banned words still apply (amazing, incredible, brilliant, game-changing).
- File: `team-docs/blog/003-super-bowl-weekend.md`

### Belated PR #1 contribution blog (2026-02-09)
- **Retroactive celebration posts work.** PR #1 from amolchanov never got a blog despite Brady's standing policy. Writing it late is better than not writing it. The contribution details were recoverable from PR metadata and conversation history — nothing was lost by the delay except timeliness.
- **Infrastructure contributions need more narrative scaffolding.** Worktree awareness, `.gitattributes` merge=union rules, and Scribe auto-commit are invisible-when-working features. The blog had to explain *what breaks without them* to make the contribution legible to readers who've never hit those problems.
- **First PR framing carries weight.** PR #1 wasn't a typo fix — it was architectural. That contrast is the hook. Let the facts do the heavy lifting: 4 features, +365 lines, same-day merge, and the worktree foundation Squad still depends on.
- File: `team-docs/blog/001c-first-pr-amolchanov.md`


📌 Team update (2026-02-09): Contribution blog policy consolidated — retroactive PR #1 blog (001c) added. All contributions get a blog post, late is OK. — decided by McManus

### Feature showcase prompts added to sample-prompts.md (2026-02-09)
- **Added 7 new prompts (#17–#23)** in a new "Feature Showcases" section covering all 9 Wave 2-3 features: export/import, skills, smart upgrade, ceremonies, GitHub Issues Mode, PRD Mode, human team members, tiered response modes, and history summarization.
- **Prompts are real projects, not feature demos.** Each prompt naturally triggers the feature (e.g., "here's the PRD at..." triggers PRD Mode, "add my teammate Sarah as Designer" triggers Human Team Members, "pull issues from..." triggers GitHub Issues Mode). The "What it demonstrates" paragraph explains what's being showcased.
- **Multiple features per prompt keeps the section tight.** 7 prompts cover 9 features by combining naturally related features (PRD + Ceremonies, GitHub Issues + Human Team + History Summarization, etc.).
- File: `docs/sample-prompts.md`

### Super Bowl Weekend post — edit pass and honest assessment (2026-02-09)
- **Brady's instinct was right.** The original post leaned too heavily on the Seahawks parallel. The Super Bowl angle was the *reason* to write the post but not the *substance* of the post. "Both were supposed to be impossible" was forced hype — shipping a roadmap in a weekend is ambitious, not impossible.
- **What was cut:** The entire "The Game" section (3 paragraphs of Seahawks narrative), the "Yeah" closer with "That's it. That's the post." meme format, the "isn't this amazing" energy throughout. The "by halftime" quip about Shayne's PR was cute but performative.
- **What was kept/added:** The sprint metrics (real substance), the feature list (now includes Wave 2.5 explicitly), Seahawks get one line of context instead of three paragraphs. Added a "What We Learned" section with actual insight about weekend sprints and community contributions.
- **Status set to draft.** Brady said he might delete it — the rewrite gives him a version worth keeping, but it's his call. If the concept still doesn't land, the v0.2.0 release post covers the same features with more depth.
- **Tone lesson:** Celebration posts that rely on external events for energy are fragile. The sprint stands on its own. The Super Bowl is a time marker, not a metaphor.
- File: `team-docs/blog/003-super-bowl-weekend.md`

### v0.2.0 release blog post (2026-02-09)
- **Headline is the portability story.** "Your Squad Comes With You" — possessive pronoun per v1 messaging guidance. The hero sentence focuses on what persists across projects (preferences, decisions, earned skills), not on feature counts.
- **Structure follows blog template** with an added "Install / Upgrade" section (release posts need actionable commands). Hero, What Shipped (10 items with agent attribution), The Story (5 paragraphs), By the Numbers, What We Learned, Install/Upgrade, What's Next.
- **Shayne Boyer credited prominently.** Three features attributed to @spboyer with PR #2 callout on every bullet. The Story section gives the community contribution its own paragraph. Contributor is the hero of that part of the narrative.
- **Narrative arc:** v0.1.0 proved it works → v0.2.0 makes it yours. The portability + skills combination is framed as the reason to invest in a squad long-term. "Without portability, agents are disposable. With it, they're an asset that compounds."
- **Skills messaging:** "Earned skills beat configured skills" — aligns with v1 messaging ("not a template, that's a skill learned across projects"). Confidence lifecycle explained simply: low → medium → high.
- **Tone matches best existing posts.** Dry, factual, lets the features speak. No cheerleading. "The test suite tells the reliability story" instead of "incredible test coverage improvement."
- File: `team-docs/blog/004-v020-release.md`

