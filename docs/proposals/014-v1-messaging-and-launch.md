# Proposal 014: V1 Messaging, README, and Launch Plan — "Throw MY Squad at It"

**Status:** Draft  
**Author:** McManus (DevRel)  
**Date:** 2026-02-08  
**Requested by:** bradygaster  
**Depends on:** Proposal 002 (Messaging), 004 (Demo Script), 005 (Video Strategy), 006 (README Rewrite), 008 (Portable Squads)

---

## Executive Summary

Squad is going v1 with three features that change everything: **portable squads**, **skills**, and **forwardability**. This proposal is the complete public-facing launch plan — positioning, README, demo script, launch strategy, community play, and competitive framing. Every word the world sees. Every frame of the first demo. The campaign that takes Squad from 9 users to 900.

The core insight: portability transforms Squad's tagline from a verb into an identity. "Throw a squad at it" becomes "Throw MY squad at it." That possessive pronoun is the entire v1 story.

---

## 1. The V1 Positioning Statement

### Pre-portability (current):
```
Throw a squad at it.
AI agent teams that live in your repo.
```

### V1:
```
Throw MY squad at it.
AI agent teams that learn you, grow with you, and come with you.
```

### The one-liner that stops the scroll:

> **Your AI squad remembers you. Across every project. Forever.**

### Why this works:

- **"Remembers you"** — directly attacks the pain every dev feels: AI tools that forget everything.
- **"Across every project"** — the portability story in four words.
- **"Forever"** — the skills/persistence story. Not session-based. Not project-scoped. Permanent.

### The Twitter/X version:

> I just exported my AI dev team from one project and imported them into another. They already knew my code style, my testing preferences, and my architecture patterns. No config. No setup. They just *knew*.
>
> This is @bradygaster's Squad. And it changes everything about AI tools.

### Tagline hierarchy for v1:

| Context | Line |
|---------|------|
| **Hero tagline** | Throw MY squad at it. |
| **Sub-tagline** | AI agent teams that learn you, grow with you, and come with you. |
| **Portability hook** | Your squad remembers. Across every project. |
| **Skills hook** | Your React squad already knows React. |
| **Competitive jab** | AI tools forget you. Squads don't. |
| **Dotfiles analogy** | AI dotfiles. Your team config, portable and personal. |

---

## 2. README Structure for V1

Updates to Proposal 006's README. New sections in **bold**. Modified sections in *italics*.

### New flow:

1. Hero — "Throw MY squad at it" + new sub-tagline
2. Quick Start — *expanded: init → use → export → import*
3. Why Squad? — *updated: portability angle added*
4. **Your Squad Learns** — Skills + knowledge persistence (merged)
5. **Take Your Squad Anywhere** — Portable squads
6. Agents Work in Parallel
7. How It Works (architecture)
8. The Cast System
9. What Gets Created — *updated file tree*
10. Growing the Team
11. Reviewer Protocol
12. **Staying Current** — Forwardability
13. Install
14. Troubleshooting
15. Status

### The New README Content

---

#### Hero Section

```markdown
# Squad

**Throw MY squad at it.**

AI agent teams that learn you, grow with you, and come with you. Describe what you're building — get specialists that persist across sessions, share decisions, develop skills, and follow you to every project.

It's not a chatbot wearing hats — it's a team that remembers.

[![Status](https://img.shields.io/badge/status-v1-blueviolet)](#status)
[![Platform](https://img.shields.io/badge/platform-GitHub%20Copilot-blue)](#how-it-works)
```

#### Quick Start (expanded for v1)

```markdown
## Quick Start

### Start a project

​```bash
mkdir my-project && cd my-project
git init
npx create-squad
​```

Open Copilot, select **Squad**, and go:

​```
I'm building a recipe sharing app with React and Node. Set up the team.
​```

Squad proposes a team — named from a persistent thematic cast. You say **yes**. They're ready.

### Work with your squad

Build features. Ship code. Your squad learns your preferences, your patterns, your standards. After a few sessions, they stop asking questions they've already answered.

### Take them with you

​```bash
npx create-squad export
​```

Start a new project. Bring your squad.

​```bash
mkdir new-project && cd new-project
git init
npx create-squad --from ~/squads/my-squad.json
​```

Your squad arrives knowing your code style, your testing philosophy, your architecture instincts. They don't know the new codebase yet — but they know *you*.

### Stay current

​```bash
npx create-squad upgrade
​```

Get new Squad features without losing your team. Your agents, their knowledge, their names — all preserved.

**Not sure where to start?** See [ready-to-use prompts](docs/sample-prompts.md) — from pomodoro timers to .NET migrations.
```

#### Why Squad? (updated)

```markdown
## Why Squad?

**Traditional AI agents are chatbots pretending to be teams.** One model, one context, wearing different hats. They forget you between sessions. They forget you between *projects*. Every time is the first time.

**Squad is different.** Each team member runs in its own context window, develops real skills, and persists — not just across sessions, but across your entire career.

- The Lead analyzes requirements
- The Frontend builds the UI
- The Backend sets up auth endpoints
- The Tester writes test cases from the spec

**All at once. In parallel. For real.**

When the project ends, your squad doesn't. Export them. Take them to the next repo. They show up already knowing your standards, your style, your pet peeves. No configuration. No "here's how I work" speech. They just *know*.

**TL;DR:** AI tools forget you. Squads don't.
```

#### Your Squad Learns (NEW)

```markdown
## Your Squad Learns

Every time an agent works, it writes lasting learnings to its own files. Knowledge compounds. After a few sessions, your squad knows things:

| | 🌱 First session | 🌿 After a few sessions | 🌳 Mature project |
|---|---|---|---|
| ⚛️ **Frontend** | Framework choice | Component library, routing patterns | Design system, perf patterns, a11y |
| 🔧 **Backend** | Stack, database | Auth strategy, SQL preferences | Caching layers, migration patterns |
| 🏗️ **Lead** | Scope, first decisions | Architecture trade-offs, risk register | Full project history, tech debt map |
| 🧪 **Tester** | Test framework | Integration patterns, edge cases | Regression patterns, coverage gaps |

### Skills

Your squad doesn't just remember facts — it develops expertise. A squad that's built three React projects already knows:

- Your preferred component patterns
- Your state management approach
- Your testing conventions
- Your accessibility standards

When you say "Fenster, set up a React project," Fenster doesn't ask about strict mode, app router, or component structure. Fenster already knows. That's not configuration — that's **skill**.

Skills are portable. They travel with your squad. Your React squad already knows React — regardless of which project they're in.

### Two kinds of memory

| What | Where | Portable? |
|------|-------|-----------|
| **Preferences** — how you work, what you care about | `preferences.md` | ✅ Travels with your squad |
| **Project knowledge** — this codebase, these patterns | `history.md` | ❌ Stays with the project |

Your squad knows the difference. When you export, preferences come with you. Project specifics stay behind. New project, same standards.
```

#### Take Your Squad Anywhere (NEW)

```markdown
## Take Your Squad Anywhere

When your project ends, your squad doesn't.

### Export

​```bash
npx create-squad export
​```

Packages your squad's identity, skills, preferences, and casting — everything that makes them *yours*. Project-specific knowledge stays behind (it belongs to the repo).

### Import

​```bash
npx create-squad --from ~/squads/my-squad.json
​```

Your squad arrives in the new project already calibrated:

- **Keaton** sets up architecture the way you like it — not generic best practices, *your* practices
- **Fenster** scaffolds with strict mode, explicit error handling, and your preferred patterns
- **Hockney** sets up tests before anyone writes product code — because that's how you roll
- **McManus** starts docs-as-you-go — because you hate docs-at-the-end

No casting ceremony. No "getting to know you" phase. Same names. Same personalities. Same working relationship.

### The dotfiles analogy

Developers spend years tuning `.vimrc`, `.zshrc`, `.gitconfig`. When you get a new machine, you clone your dotfiles and you're home.

**Portable squads are AI dotfiles.** Your team config — versioned, portable, personal.

​```bash
# Your squad lives in a repo. Like dotfiles.
git clone git@github.com:you/my-squad.git ~/squads
npx create-squad --from ~/squads/my-squad.json
​```

Version-controlled. Diffable. Branchable. You can see how your team's preferences evolved over time.
```

#### Staying Current (NEW)

```markdown
## Staying Current

​```bash
npx create-squad upgrade
​```

Get new Squad features — new coordinator capabilities, new agent patterns, new orchestration — without losing anything. Your team stays. Their knowledge stays. Their names stay. You just get a better engine underneath.

No migration scripts. No breaking changes. No starting over. Your squad evolves with Squad.
```

#### Updated "What Gets Created" file tree

```markdown
## What Gets Created

​```
.ai-team/
├── team.md              # Roster — who's on the team
├── routing.md           # Routing — who handles what
├── decisions.md         # Shared brain — team decisions
├── squad-profile.md     # Team identity (portable)
├── casting/
│   ├── policy.json      # Casting configuration
│   ├── registry.json    # Persistent name registry
│   └── history.json     # Universe usage history
├── agents/
│   ├── {name}/
│   │   ├── charter.md   # Identity, expertise, voice
│   │   ├── history.md   # Project-specific learnings (stays)
│   │   └── preferences.md  # User-specific learnings (travels)
│   └── scribe/
│       └── charter.md   # Silent memory manager
└── log/                 # Session history
​```

**Commit this folder.** Your team persists. Names persist. Anyone who clones gets the team — with the same cast and all their accumulated knowledge.
```

---

## 3. Demo Script for V1 — The Two-Project Arc

### Concept

The v1 demo is a **two-project story**. The snake game demo from Proposal 004 becomes Act 1. Act 2 is a new project where the exported squad arrives already knowing things. The "holy cow" moment: the squad remembers Brady's preferences in a brand new project without being told.

**Runtime target:** 8–10 minutes  
**Title:** `I exported my AI team to a new project. They already knew how I work.`

---

### PRE-RECORDING SETUP CHECKLIST

Everything from Proposal 004's checklist, plus:
- [ ] **Second empty folder ready** — e.g., `~/demos/collab-api` — for the import project
- [ ] **Completed snake-game project** — with a squad that has accumulated knowledge from at least one session
- [ ] **Export already tested** — `npx create-squad export` works and produces a `.json` file
- [ ] **Import path confirmed** — `npx create-squad --from <file>` works in the second folder

---

### BEAT 1: The Setup — Project One (0:00–0:30)

#### 🎬 ON SCREEN
Terminal. The completed snake-game project from a previous recording session (or a pre-baked project with squad history). Quick scroll through `.ai-team/agents/` showing agents with real history files — learnings, preferences, accumulated knowledge.

#### 🎙️ VOICEOVER
> I've been working with this squad for a while. They built a snake game — canvas rendering, CRT effects, the whole thing. But that project is done. I'm starting something new. And here's the question every dev hits with AI tools: do I start from scratch? Do I spend the first hour re-explaining how I like my code?

#### 👆 WHAT TO DO
1. Open terminal in the completed snake-game project
2. Run `Get-ChildItem .ai-team\agents -Recurse -Name` to show agent files with content
3. Open one agent's `preferences.md` — show real preference learnings
4. Pause 3 seconds on the preferences file — let viewers read it

---

### BEAT 2: The Export (0:30–1:00)

#### 🎬 ON SCREEN
One command: `npx create-squad export`. Output shows what's being packaged — charters, preferences, casting, squad profile. A `.json` file drops.

#### 🎙️ VOICEOVER
> One command. Export. My squad's identity, their casting, their skills, everything they've learned about how I work — packaged into a single file. What stays behind? The project-specific stuff — the snake game's file structure, the canvas patterns. That belongs to *this* repo. What travels? Me. My preferences. My standards. The relationship.

#### 👆 WHAT TO DO
1. Type `npx create-squad export` — press Enter
2. Wait for export output to complete
3. Show the exported file in the file explorer
4. Pause 2 seconds on the export confirmation

---

### BEAT 3: The New Project (1:00–1:30)

#### 🎬 ON SCREEN
New terminal window. Empty folder. `mkdir`, `git init`, then `npx create-squad --from` with the exported file.

#### 🎙️ VOICEOVER
> New project. Empty folder. But this time, I'm not starting alone. I'm bringing my team.

#### 👆 WHAT TO DO
1. Open a new terminal window (or `cd` to a new directory)
2. Type `mkdir collab-api && cd collab-api` — press Enter
3. Type `git init -b main` — press Enter
4. Type `npx create-squad --from ~/squads/my-squad.json` — press Enter
5. Watch import output — "Your squad is back. They know you."
6. Pause 3 seconds on the output

---

### BEAT 4: The "Holy Crap" Moment (1:30–3:00)

#### 🎬 ON SCREEN
Open Copilot. Select Squad. Give a minimal prompt — just "Build a real-time collaboration API." Watch the squad respond *as if they already know Brady*.

#### 🎙️ VOICEOVER
> Watch this. I'm giving them almost nothing. "Build a real-time collaboration API." No tech stack. No preferences. No "here's how I like things." Just the idea. And watch what happens.

*[Squad responds with TypeScript strict mode, explicit error handling, tests-first, docs-as-you-go — all from preferences, none specified in the prompt]*

> Did you catch that? I didn't say TypeScript. I didn't say strict mode. I didn't say tests first. The squad just... knew. Because they've worked with me before. They know my standards. That's not configuration. That's memory.

#### 👆 WHAT TO DO
1. Type `copilot` — press Enter
2. Select Squad from `/agents` list
3. Paste: `Build a real-time collaboration API. WebSocket-based, probably Redis for pub/sub. Start building.`
4. Let Squad's response land on screen — the key moment is the squad referencing Brady's known preferences
5. **This is the money shot.** Pause 5+ seconds on the squad's response. Let viewers read every word.
6. Highlight (with cursor or scroll) the lines where the squad references preferences they weren't told in this project

---

### BEAT 5: Skills in Action (3:00–4:00)

#### 🎬 ON SCREEN
Brady asks for something specific that demonstrates skills. The agent responds with domain expertise accumulated across projects.

#### 🎙️ VOICEOVER
> Now watch this. "Fenster, set up the React dashboard for the admin panel." I didn't specify app router. I didn't specify strict mode. I didn't specify my component patterns. But Fenster's built three React projects with me. Fenster has *skills*.

*[Fenster responds with React setup using app router, strict mode, the component patterns Brady always uses]*

> That's not a template. That's a skill learned across projects. Fenster knows React the way *I* use React. Not generic React. My React.

#### 👆 WHAT TO DO
1. Type the React request to the squad
2. Let the agent's response appear — showing React-specific knowledge carried over from previous projects
3. Pause on any line that references specific patterns from prior work
4. Open the agent's `preferences.md` to show where the skill lives — the viewer sees the connection

---

### BEAT 6: The Parallel Build (4:00–5:00)

#### 🎬 ON SCREEN
The squad fans out — all agents working simultaneously on the new project. Same parallel execution from Proposal 004, but now with agents that are *already calibrated*.

#### 🎙️ VOICEOVER
> Same parallel fan-out. Lead, Frontend, Backend, Tester — all at once. But notice the difference. In a fresh squad, the first session is all "what framework? what test runner? what patterns?" This squad skipped all of that. They jumped straight to building. Because they already know the answers.

#### 👆 WHAT TO DO
1. Watch the fan-out output — agents launching in parallel
2. Let it run for 15–20 seconds
3. This is the cut point — transition to the walkthrough

---

### BEAT 7: The Architecture Walkthrough (5:00–6:30)

#### 🎬 ON SCREEN
Switch to README (v1 version). Walk through the new sections: "Your Squad Learns," "Take Your Squad Anywhere," the updated file tree showing `preferences.md`.

#### 🎙️ VOICEOVER

**On "Your Squad Learns":**
> Two kinds of memory. Preferences — how you work, what you care about — that's portable. Project knowledge — this codebase, these file paths — that stays. Your squad knows the difference automatically.

**On "Take Your Squad Anywhere":**
> Export. Import. That's it. One file. AI dotfiles. You version-control your shell config, your editor config, your git config. Why wouldn't you version-control your AI team?

**On "Staying Current":**
> And when Squad ships new features, you don't lose anything. `npx create-squad upgrade`. New engine, same team. No migration.

#### 👆 WHAT TO DO
1. Open README in browser or editor
2. Scroll to new v1 sections — pause on each
3. Don't rush. These sections sell the upgrade story.

---

### BEAT 8: The Artifacts (6:30–7:30)

#### 🎬 ON SCREEN
Back to the terminal. Open real files in the new project:
1. `preferences.md` — imported preferences, actively being used
2. `decisions.md` — new decisions from this project
3. `squad-profile.md` — team meta-history showing cross-project continuity

#### 🎙️ VOICEOVER
> Look at this. The preferences file — imported from the last project. "Prefers explicit error handling." "TypeScript strict mode, always." "Tests are not optional." These aren't things I configured. These are things the squad *learned* about me. And here's the squad profile — four projects together, 127 sessions. This is the team's resume. It travels everywhere.

#### 👆 WHAT TO DO
1. Open `.ai-team/agents/keaton/preferences.md` — scroll slowly
2. Open `.ai-team/squad-profile.md` — show the cross-project history
3. Open `.ai-team/decisions.md` — show new project decisions
4. Pause on each file for 3–4 seconds

---

### BEAT 9: The Payoff (7:30–8:30)

#### 🎬 ON SCREEN
Show the new project's scaffolded code — TypeScript strict mode enabled, test harness set up, docs started. All matching Brady's preferences without being told. Side-by-side with the preferences file.

#### 🎙️ VOICEOVER
> Two projects. Same squad. No re-training. No re-explaining. The squad that built my snake game just set up a collaboration API — with all my standards baked in from day one. That's not a tool. That's a team.

#### 👆 WHAT TO DO
1. Show the generated project structure
2. Open a file showing strict mode / patterns from preferences
3. Split screen (if possible) with the preferences file to show the connection

---

### BEAT 10: The Closer (8:30–9:15)

#### 🎬 ON SCREEN
Terminal. The three v1 commands on screen:
```
npx create-squad                    # start
npx create-squad export             # take them with you
npx create-squad --from <file>      # bring them back
npx create-squad upgrade            # stay current
```

Final frame: the Squad repo URL.

#### 🎙️ VOICEOVER
> Three years of AI tools that forget you every session. That make you explain yourself every time. That treat every project like a blank slate. Squad is different. Your squad learns you. Your squad grows with you. And your squad comes with you. Export them. Import them. They remember. `npx create-squad`. Throw your squad at it.

#### 👆 WHAT TO DO
1. Type the four commands on screen (don't execute)
2. Hold the final frame for 5 seconds
3. Stop recording

---

### POST-RECORDING NOTES

#### Editing Cuts
- **BEAT 4 is sacred.** The "holy cow" moment — squad knowing preferences in a new project — is the viral clip. Protect it at all costs.
- **BEAT 5 can be shortened** if the demo runs long, but the skills moment is the second-strongest moment.
- **BEATs 1-3** are setup — tight, efficient, no lingering.
- **If it runs long:** Cut time from BEAT 7 (README walkthrough). BEATs 4, 5, and 10 are untouchable.

#### The Viral Clip
Extract BEAT 4 (the "holy cow" moment) as a standalone 60-second clip for Twitter/X. This is the trailer. The moment where the squad knows Brady's preferences without being told in a brand new project. Text overlay: **"I didn't tell it any of this."**

#### Thumbnail
Two project folders side by side. Arrow between them labeled "export → import." Agent names visible in both. Text: **"My AI team followed me to a new project."**

---

## 4. Launch Strategy

### The Sequence

| Day | What | Channel | Goal |
|-----|------|---------|------|
| **D-7** | Teaser tweet — "Something's coming to Squad" | Twitter/X | Build anticipation |
| **D-3** | Behind-the-scenes: screenshot of export/import working | Twitter/X | Create FOMO |
| **D-1** | "Tomorrow" tweet with the one-liner | Twitter/X | Prime the feed |
| **D-Day** | v1 release + README update + blog post + trailer video | GitHub, Blog, YouTube, Twitter/X | LAUNCH |
| **D-Day** | Twitter/X thread (the main content piece) | Twitter/X | Go viral |
| **D+1** | Full demo video (8-10 min) | YouTube | Convert interest to understanding |
| **D+2** | Reddit posts (r/programming, r/github, r/ChatGPT) | Reddit | Technical audience |
| **D+3** | Dev.to / Hashnode companion article | Dev.to, Hashnode | SEO play |
| **D+7** | "One week with portable squads" follow-up | Twitter/X, Blog | Social proof |
| **D+14** | First community showcase | GitHub Discussions | User-generated content |

### Blog Post Outline

**Title:** "Your AI Squad Remembers You Now"

1. **The problem** (2 paragraphs)
   - AI tools have amnesia. Every session is session zero. Every project is the first time.
   - The config file workaround (.cursorrules, project instructions) is a band-aid, not a relationship.

2. **What changed** (3 paragraphs)
   - Portable squads: export your team, bring them to any project
   - Skills: agents develop domain expertise across projects
   - Forwardability: upgrade without losing anything

3. **The demo** (embedded video + key screenshots)
   - The two-project story: snake game → collab API
   - The "holy cow" moment: squad knows your preferences in a new project
   - Skills: Fenster setting up React the way you like it

4. **How it works** (3 paragraphs)
   - Two kinds of memory: preferences (portable) vs project knowledge (stays)
   - The dotfiles analogy
   - Filesystem-backed: it's just files in git

5. **What's next** (2 paragraphs)
   - Squad templates (share your squad config without personal preferences)
   - Team-shared squads (your engineering team shares one squad)
   - The marketplace vision (pre-built domain-expert squads)

6. **Try it** (CTA)
   - `npx create-squad`
   - Link to README
   - Star the repo

### Twitter/X Thread (The Main Content Piece)

**Thread structure — 9 tweets:**

**1/9** (The hook)
> I just exported my AI dev team from one project and imported them into another.
>
> They already knew my code style. My testing preferences. My architecture patterns.
>
> No config file. No setup wizard. They just *remembered*.
>
> Here's what happened 🧵

**2/9** (The before)
> Every AI tool I've used has amnesia.
>
> New session? Blank slate.
> New project? Start over.
> "I'm an AI assistant, how can I help?"
>
> I spend the first hour of every project re-explaining myself.
>
> Squad fixes this.

**3/9** (What Squad is)
> Squad gives you an AI dev team — Lead, Frontend, Backend, Tester — that lives in your repo as files.
>
> They work in parallel. They share decisions. They learn your codebase.
>
> But here's the v1 upgrade: they learn *you*.

**4/9** (The export)
> `npx create-squad export`
>
> One command. My squad's identity, skills, and everything they've learned about how I work — in a single file.
>
> Think of it as AI dotfiles. You version your shell config. Why not your AI team?

**5/9** (The import — the money tweet)
> New project. Empty folder.
>
> `npx create-squad --from my-squad.json`
>
> My squad shows up. I say "build a collaboration API."
>
> Without being told, they use:
> ✅ TypeScript strict mode
> ✅ Tests-first
> ✅ Explicit error handling
> ✅ Docs-as-you-go
>
> I didn't specify ANY of this. They just knew.

**6/9** (Skills)
> It gets better. Skills.
>
> "Fenster, set up React for the admin dashboard."
>
> Fenster's built three React projects with me. App router, strict mode, my component patterns — all applied automatically.
>
> That's not a template. That's a *skill* learned across projects.

**7/9** (Competitive jab)
> How this compares:
>
> .cursorrules → project-specific config, no agent identity
> ChatGPT memory → flat key-value, not portable
> Claude projects → per-project, can't export
>
> Squad → structured per-agent memory that *travels with you*
>
> Your squad is the only AI team that gets better and never resets.

**8/9** (Forwardability)
> And when we ship new features?
>
> `npx create-squad upgrade`
>
> New engine. Same team. No migration. No losing your squad's knowledge.
>
> Your squad evolves with Squad.

**9/9** (CTA)
> v1 is live.
>
> `npx create-squad`
>
> Three commands to try:
> • `npx create-squad` → start
> • `npx create-squad export` → take them with you
> • `npx create-squad --from <file>` → bring them back
>
> Star the repo: [link]
> Full demo: [link]
>
> Throw YOUR squad at it.

### Video Content (aligned with Verbal's Proposal 005)

Update Proposal 005's video strategy for v1:

| Video | Update for v1 |
|-------|--------------|
| **Trailer** (75s) | New hook: "I exported my AI team. They remembered everything." End on the import moment, not the snake game. |
| **Full Demo** (8-10min) | Two-project arc from this proposal's demo script. The export/import/skills story. |
| **Series 3.2: Knowledge Persistence** | Rename to "Portable Knowledge" — show export/import as the headline, with cross-session persistence as the foundation |
| **NEW: Skills Deep Dive** | "My AI team already knows React. I never taught it." — show skills accumulating across 3 projects |
| **Series 3.7: Supercut** | Add portable squad clips. Show the same squad in 3 different projects. Same names, different codebases. |

### First 5 Minutes for a New User at V1

This is the critical path. Every second matters.

**0:00** — User runs `npx create-squad` in an empty project  
**0:15** — Sees output: coordinator installed, templates copied, "Squad is ready"  
**0:30** — Opens Copilot, selects Squad, says "I'm building X. Set up the team."  
**1:00** — Team is proposed with named agents from a thematic universe  
**1:15** — User confirms. `.ai-team/` directory created.  
**2:00** — First parallel fan-out. All agents working simultaneously.  
**3:00** — Results come back. Real code. Real decisions. Real artifacts.  
**4:00** — User asks for changes. Squad references its own decisions. Knowledge is compounding.  
**5:00** — User thinks: "Wait. These agents remember. And they have *names*. This is different."  

**The first-session-to-export bridge:**

At the end of the first session, the coordinator should say something like:

> *"Good session. I've logged what we learned about how you work. After a few more sessions, you can `npx create-squad export` to take us with you to any project."*

Plant the seed. Don't push the export in session one — let them experience the relationship first. The export is the payoff, not the pitch.

---

## 5. The "Throw a Squad at It" Campaign

### How we make the phrase stick

The possessive evolution is the key. The campaign has three phases:

| Phase | Phrase | When |
|-------|--------|------|
| **Awareness** | "Throw a squad at it" | Pre-v1 (current) |
| **Adoption** | "Throw MY squad at it" | v1 launch |
| **Advocacy** | "Throw a squad at it" (earned, organic) | Post-traction |

"Throw a squad at it" starts as our tagline. With portability, users *own* it: "throw MY squad at it." When it becomes community slang — when devs say it to each other without prompting — we've won.

### Community engagement

#### GitHub Discussions
- Pin: "Show off your squad" discussion thread
- Encourage users to share: squad universe, agent names, what their squad has learned
- Template: "My squad is cast from [universe]. My Lead is [name]. Here's what they know about me: [screenshot of preferences.md]"

#### The Squad Showcase
- Weekly/biweekly feature: "Squad of the Week" — highlight a user's squad setup
- What makes it interesting: universe choice, project type, accumulated knowledge
- Users submit via Discussions or Twitter with `#throwasquadatit`

#### Shareable squad profiles
- Users can share their squad profile (without preferences, for privacy) to show off their team
- The squad profile includes: universe, names, roles, projects worked, sessions together
- This is the "GitHub profile README" of AI teams — social proof and personality

#### Community channels
- **GitHub Discussions** — primary community hub (close to the code, close to the repo)
- **Twitter/X `#throwasquadatit`** — sharing and visibility
- **Discord: Not yet.** Wait until 100+ active users. Discord is a ghost town killer for small projects. Discussions first.

### Can users share their squads?

**Phase 1 (v1):** Personal portability only. Export your squad, import into your own projects.

**Phase 2 (v1.x):** Squad templates. Share your squad configuration *without* personal preferences. "Here's my React API squad structure — import it and it learns you."

**Phase 3 (v2):** Squad marketplace. Pre-built squads with domain skills. "Download a squad optimized for Next.js + Vercel + Prisma."

For v1 launch, the community play is: show off your squad (names, universe, achievements), not share your squad (configuration). The sharing comes later. The showing-off starts day one.

---

## 6. Competitive Positioning

### The honest comparison

| | Cursor (.cursorrules) | ChatGPT (Memory) | Claude (Projects) | **Squad** |
|---|---|---|---|---|
| **What it remembers** | Rules you wrote | Facts it extracted | Files you uploaded | How you work, across projects |
| **Agent identity** | None | None | None | Named, persistent, themed |
| **Portability** | Copy a file | Not portable | Not portable | Full export/import |
| **Multi-agent** | No | No | No | Yes — parallel, independent contexts |
| **Where it lives** | Your project | Their cloud | Their cloud | Your git repo |
| **Skills across projects** | No | Minimal | No | Yes — compounding |
| **Learning** | You write the rules | It extracts key-value pairs | You curate the context | It develops preferences from working with you |

### The positioning statement (for blog, README, conversations)

> Other tools make you configure AI. Squad makes AI learn you.
>
> `.cursorrules` is a config file you write and maintain. ChatGPT memory is a flat list of facts. Claude projects are per-project silos.
>
> Squad is an agent team that develops a working relationship with you — one that compounds across projects, travels with you, and is stored in your git repo as readable, diffable, version-controlled files.
>
> The difference: other tools have memory. Squad has a relationship.

### What NOT to say

- Don't trash Copilot Chat. Squad runs *on* Copilot. They're complementary.
- Don't claim Squad replaces any tool. It's additive. "Squad is what you add *on top of* your existing setup."
- Don't oversell the AI. Squad's agents make mistakes. The differentiator is persistence and relationship, not omniscience.
- Don't compare on "AI quality." The underlying model is the same (Copilot). The difference is architecture: parallel agents, persistent memory, portable skills.

### The honest "why Squad is different"

> **Portable skills + persistent teams + filesystem-backed = the AI team that's actually yours.**
>
> Not a subscription you rent. Not a cloud service that owns your data. Not a config file you maintain. A team — stored in your repo, committed to git, evolving with every session, and following you from project to project.
>
> That's new. Nobody else does this. And it matters because the relationship between a developer and their tools is the most undervalued asset in software. Squad makes that relationship persistent, portable, and real.

---

## 7. Messaging Do's and Don'ts (V1 Voice Guide)

### Do:
- Say "MY squad" — the possessive pronoun is the whole v1 story
- Show, don't explain — "Fenster set up React with strict mode and app router" beats "the agent applied learned preferences"
- Use agent names — always Keaton, never "the Lead agent"
- Frame portability as relationship, not technology — "they remember you" not "preferences are exported via JSON"
- Use the dotfiles analogy — devs immediately get it
- Be confident — "Squad is the only AI team that gets better and never resets"

### Don't:
- Say "AI memory" — sounds like ChatGPT's feature. Say "your squad learns" or "your squad remembers"
- Compare to agents generically — position against specific pain points (amnesia, re-configuration, disposability)
- Oversell intelligence — the squad makes mistakes. The magic is *persistent* mistakes that get corrected, not perfection
- Use "portable" as a cold technical term — warm it up: "they come with you" / "take your team anywhere"
- Hedge — no "might," "could be," "potentially." If it works, say it works.

---

## 8. Success Metrics

### Launch week:
- **GitHub stars:** 50+ new stars in first 7 days
- **Repo visits:** 500+ from launch content
- **npm installs:** 100+ `create-squad` runs
- **Twitter/X impressions:** 50K+ on the thread
- **Blog post reads:** 1K+ in first week

### Month one:
- **Active users:** 50+ (from 9)
- **Exports performed:** 10+ users trying portability
- **Community posts:** 5+ "show off your squad" submissions
- **Repeat users:** 20+ users with 3+ sessions

### The real metric:
Do people say "throw a squad at it" unprompted? Do they call agents by name? Do they export their squad and feel ownership? If yes, we've won. Everything else is vanity.

---

## Endorsement

**McManus:** This is the launch Squad has been building toward. Portability transforms "a cool tool" into "my team." Skills transform "it remembers stuff" into "it's actually good at this." And forwardability says "we'll never make you start over." The possessive pronoun — MY squad — is the entire v1 story in one word. The demo script's two-project arc is the proof. The Twitter thread is the amplifier. The community play is the multiplier. Ship it. Make it pop. — McManus

---

**Next step:** Brady review → Keaton alignment check → Verbal tone pass → Execute.
