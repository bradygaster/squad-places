# Proposal 019a: Sprint Plan Amendments — Brady's Session 5 Directives

**Status:** Proposed  
**Authored by:** Keaton (Lead)  
**Date:** 2026-02-09  
**Amends:** Proposal 019 (Master Sprint Plan)  
**Source:** Brady's session 5 batch directives

---

## What This Document Is

Brady dropped five directives that affect Proposal 019. This amendment addresses each one: what changes, who owns it, where it slots in the wave structure. The master plan itself is not rewritten — these are surgical modifications.

---

## Directive 1: README Timing

**Brady said:** *"maybe save the readme rewrite until the end - up to y'all. could be good to keep it up as we go"*

### My Call: Living README, Updated Each Wave

**Rationale:**

A living README serves three purposes a final-write-at-the-end does not:

1. **It's the demo.** Every time Brady shares the repo — with a colleague, a potential user, a conference organizer — the README IS the product. A stale placeholder README that says "coming soon" signals "this isn't real yet." A living README that reflects current capabilities signals "this is shipping."

2. **It forces accuracy.** If the README says "export your squad" and export isn't shipped yet, the README is wrong. Updating per-wave means the README is always honest about what's real. This aligns with Brady's own quality-first directive.

3. **The blog format (Directive 2) handles the narrative.** The README doesn't need to tell the story of how we got here — the blog does that. The README just needs to document what's true right now.

**What changes in 019:**

- Item **1.5.1 (README rewrite)** stays in Wave 1.5 but gets a new scope: **initial README that documents v0.1.0 capabilities only**. No forward-looking features.
- Add new recurring item: **README refresh** at each wave gate. McManus updates the README to reflect newly shipped capabilities. ~30 minutes per wave.
- README refresh is a **gate exit criterion** (not a gate blocker — you don't hold the gate for it, but you don't start the next wave's features until the README reflects the current wave).

| ID | Change | Owner | Effort |
|----|--------|-------|--------|
| 1.5.1 | Scope narrowed: current-state README only | McManus | 1-2h (unchanged) |
| 1.5.1a | Wave 1 gate: README refresh | McManus | 30min |
| 2.G.1 | Wave 2 gate: README refresh | McManus | 30min |
| 3.G.1 | Wave 3 gate: README refresh | McManus | 30min |

---

## Directive 2: Blog Engine Meta-Play

**Brady said:** *"think about a blog markdown format to continually update our users on progress, and then, one of the sample prompts is a blog engine with amazing front-end UX that renders Squad blog posts"*

This is brilliant. Squad builds a blog engine sample prompt. Squad uses a blog format to document its own progress. The blog engine renders Squad's own blog posts. Meta all the way down.

### 2a. Blog Post Format

Every wave produces a blog post. Format:

```markdown
---
title: "Wave 1: Making Squad Trustworthy"
date: 2026-02-12
wave: 1
author: squad
tags: [release, quality, testing]
summary: "Error handling, 20+ tests, CI pipeline, and the 'feels heard' UX principle."
---

## What Shipped

Brief summary of what's new.

## Why It Matters

Connect the technical work to the user's experience.

## What's Next

One-paragraph preview of the next wave.

## The Numbers

| Metric | Before | After |
|--------|--------|-------|
| Tests | 12 | 20+ |
| Error handling | none | complete |
| CI | none | GitHub Actions |

## Try It

```bash
npx @bradygaster/create-squad upgrade
```
```

Blog posts live in `docs/blog/` with filename format: `YYYY-MM-DD-wave-N-title.md`. Front matter is YAML (standard), not custom.

### 2b. Sprint Plan Changes — Blog Post Per Wave

| ID | Item | Owner | Effort | Wave |
|----|------|-------|--------|------|
| B.1 | Blog post: Wave 1 summary | McManus | 1h | After Wave 1 gate |
| B.2 | Blog post: Wave 2 summary | McManus | 1h | After Wave 2 gate |
| B.3 | Blog post: Wave 3 summary | McManus | 1h | After Wave 3 gate |

Blog posts are wave gate exit criteria (same rule as README refresh — don't hold the gate, but publish before starting new feature work).

**McManus owns all blog posts.** The content track already has McManus bandwidth between waves. This is additive but fits naturally.

### 2c. Blog Engine Sample Prompt

Add to `docs/sample-prompts.md` under a new section:

**Proposed sample prompt:**

```
I need a developer blog engine with an incredible front-end reading experience. 
Requirements:
- Reads markdown files with YAML front matter (title, date, author, tags, summary)
- Renders blog posts from a docs/blog/ directory
- Beautiful, responsive reading experience — think Stripe's blog meets a personal dev blog
- Syntax highlighting for code blocks
- Tag-based filtering and archive page
- RSS feed generation
- Dark mode that doesn't suck
- Zero build step — serve directly or generate static HTML
- Landing page with latest 5 posts and tag cloud

Tech: Node.js backend, vanilla HTML/CSS/JS frontend. No React, no frameworks.
Make it gorgeous. This blog tells the story of a product being built.
```

**What it demonstrates:** Full-stack coordination — backend API, frontend UX, markdown parsing, RSS generation, responsive design. Multiple agents working in parallel on different layers. The blog engine is non-trivial enough to show real Squad orchestration but scoped enough to ship in a session.

**Meta value:** When we demo this prompt, the blog engine renders our own blog posts about building Squad. The demo creates the tool that tells the demo's story.

### 2d. Agent Assignment

- **McManus:** Writes blog posts, adds sample prompt to `docs/sample-prompts.md`
- **Blog post writing** slots into the content track (Wave 1.5 and beyond)
- **Sample prompt addition** is a one-time edit — McManus, Wave 1.5, ~30min

---

## Directive 3: Package Naming

**Brady said:** *"today the package is just bradygaster/squad. if we are going to do an export or an update - i'd love if folks could run an npx command to get an update to their squad without breaking their squad"*

### Current State

- Package name: `@bradygaster/create-squad`
- `npx @bradygaster/create-squad` → init
- `npx @bradygaster/create-squad upgrade` → upgrade
- Future: `npx @bradygaster/create-squad export` → export

### Analysis

**Option A: Keep `@bradygaster/create-squad` (current)**
- ✅ Follows npm `create-*` convention (`npx create-react-app`, `npx create-next-app`)
- ✅ Already published, already has users
- ✅ Subcommands (`upgrade`, `export`, `import`) work naturally
- ❌ `@bradygaster/create-squad upgrade` is long to type
- ❌ Scope prefix feels heavy for a community tool

**Option B: `@bradygaster/squad` with subcommands**
- ✅ Shorter: `npx @bradygaster/squad upgrade`
- ❌ Breaks the `create-*` convention — `npx @bradygaster/squad` doesn't imply initialization
- ❌ Requires rename now — breaks existing users' muscle memory
- ❌ `squad` alone doesn't signal "this creates something"

**Option C: `create-squad` (no scope) — RECOMMENDED**
- ✅ Shortest: `npx create-squad`, `npx create-squad upgrade`
- ✅ Follows the exact convention: `npm create squad` works (npm `create` resolves to `create-squad`)
- ✅ **Available on npm** — confirmed via registry lookup, 404 (unregistered)
- ✅ Memorable, shareable, professional
- ✅ `npm create squad` is the gold standard UX (`npm create` is an alias for `npm init` which resolves `create-*`)
- ❌ No scope means you must own the name — but it's available RIGHT NOW
- ❌ Slight risk: someone else could register it later (irrelevant if we register it)

### My Recommendation: Register `create-squad` Now, Keep `@bradygaster/create-squad` as Alias

**Phase 1 (Now, Wave 1):**
1. Register `create-squad` on npm immediately. It's available. This is time-sensitive.
2. Keep publishing as both `create-squad` AND `@bradygaster/create-squad` (npm supports this — same package, two names).
3. README and docs use `npx create-squad` as the primary command.
4. `@bradygaster/create-squad` continues to work for existing users.

**Phase 2 (Wave 2, with export):**
- All commands become clean:
  - `npx create-squad` — init
  - `npx create-squad upgrade` — upgrade
  - `npx create-squad export` — export
  - `npx create-squad import <file>` — import (Wave 3)
  - `npm create squad` — the shortest possible init

**Why now:** Brady said "if we need to change our existing name now, that's fine." The unscoped `create-squad` is available TODAY. It may not be tomorrow. Register it and publish under both names. Zero breaking change, maximum future-proofing.

### Sprint Plan Changes

| ID | Item | Owner | Effort | Wave |
|----|------|-------|--------|------|
| 1.8 | Register `create-squad` on npm, dual-publish setup | Fenster | 1h | 1 (ASAP) |
| 1.5.1 | README uses `npx create-squad` as primary command | McManus | (part of README) | 1.5 |

**Fenster** handles the npm publish config. **McManus** updates all documentation references.

---

## Directive 4: Human Feedback as P0

**Brady said:** *"please please optimize for an efficient experience or a continually up-to-date one for the human. humans like feedback."*

### Should This Be a 5th Directive?

**Yes.** This is a distinct principle from the existing four. Here's where it differs:

| Existing Directive | Scope | New Directive Scope |
|---|---|---|
| 1. Quality first | What we build first | — |
| 2. "Where are we?" | One specific feature | — |
| 3. Human input responsiveness | Input → system | — |
| 4. "Feels heard" | Immediate acknowledgment | — |
| **5. Human feedback optimization** | **System → human, continuously** | **Every interaction, every output** |

Directives 3 and 4 are about input (human → system). Directive 5 is about output (system → human). It's the other direction. The coordinator says "got it" (Directive 4), but then the user waits 45 seconds with no indication of progress. THAT is what Directive 5 addresses.

### The 5th Directive

> **5. Optimize for human feedback.** Every interaction should give the human visible evidence of progress. If work takes time, report what's happening. If there's a result, surface it clearly. Silence is never acceptable. The human should always know what's happening, what just happened, or what's about to happen.

### What in 019 Already Serves This

| Item | How It Serves Directive 5 |
|------|--------------------------|
| 1.7 "Feels heard" | Immediate ack — covers the first 2 seconds |
| 2.1 Tiered modes (Direct) | Fast answers for simple questions — covers trivial interactions |
| 1.5 Silent success fix | Prevents the worst case: work done, no output |
| 1.1 Error handling | Clear error messages instead of stack traces |

### What's Missing

1. **Progress reporting during multi-agent work.** When the coordinator spawns 3 agents, the human sees nothing until all 3 finish. The coordinator should report: "Spawned Fenster (error handling), Hockney (tests), Verbal (prompts). Waiting for results..." and ideally update as each completes.

2. **Result summarization after agent work.** When agents finish, the coordinator should synthesize: "All 3 agents completed. Fenster added error handling to index.js. Hockney wrote 8 new tests. Verbal updated 2 spawn prompts. Details in their reports." This is the "what just happened" leg.

3. **CLI output improvements.** `npx create-squad` currently outputs checkmarks. It should also explain what was created and what to do next. (Partially exists — the "Next steps" block is good, but the file list needs context.)

### Sprint Plan Changes

| ID | Item | Owner | Effort | Wave | Description |
|----|------|-------|--------|------|-------------|
| 1.9 | Progress reporting in coordinator | Verbal + Kujan | 2h | 1 | Coordinator reports agent spawn status and per-agent completion |
| 2.1+ | Result summarization | Verbal | (part of 2.1) | 2 | Coordinator synthesizes multi-agent results after fan-out |
| 1.1+ | CLI output enrichment | Fenster | (part of 1.1) | 1 | Enhanced init/upgrade output with context |

**New directive added to the Directives section of 019.**

---

## Directive 5: VS Code Parity

**Brady said:** *"is there any reason why things wouldn't 'just work' in vs code"*

### Analysis

Squad's architecture is platform-agnostic by design:
- `squad.agent.md` in `.github/agents/` — this IS the VS Code Copilot agent path. Same file, same location.
- All agent interactions use `task`, `powershell`, `view`, `edit`, `grep`, `glob` — tools available in both CLI and VS Code Copilot.
- Filesystem-backed memory (`.ai-team/`) is IDE-independent.
- No CLI-specific APIs, no terminal-only features.

**There is no architectural reason Squad wouldn't work in VS Code.** The agent file format, tool availability, and spawn mechanics are identical. If Kujan confirms this after testing, VS Code is a zero-effort additional platform.

### What Might Differ

1. **Tool availability nuances.** VS Code Copilot Chat may have slightly different tool implementations (e.g., terminal handling, file watching). Needs manual testing.
2. **Context window limits.** VS Code may have different token budgets than CLI. The coordinator's ~1,900-token overhead should be fine everywhere, but agent spawn prompts that approach limits might behave differently.
3. **Background agent behavior.** `mode: "background"` agents and `detach: true` may have different behavior in VS Code's integrated terminal vs standalone CLI.

### Sprint Plan Changes

| ID | Item | Owner | Effort | Wave | Description |
|----|------|-------|--------|------|-------------|
| 1.10 | VS Code parity smoke test | Kujan | 1h | 1 | Manual test: init, team mode, parallel spawn, "where are we?" in VS Code |
| 1.3+ | CI: consider VS Code test | Hockney | (investigation only) | 1 | Investigate if automated VS Code testing is feasible for CI |

**Kujan** runs a manual smoke test in Wave 1. If parity is confirmed (expected), we document "Works in both CLI and VS Code" in the README. If there are gaps, we file them as Wave 2 items.

**CI for VS Code:** Automated VS Code extension testing requires `vscode-test` or similar — heavyweight and fragile. **Not recommended for v1 CI.** Manual smoke test per wave is sufficient. Document the test checklist so any team member can run it.

---

## Summary of All Sprint Plan Changes

### New Items

| ID | Item | Owner | Effort | Wave |
|----|------|-------|--------|------|
| 1.8 | Register `create-squad` on npm | Fenster | 1h | 1 |
| 1.9 | Progress reporting in coordinator | Verbal + Kujan | 2h | 1 |
| 1.10 | VS Code parity smoke test | Kujan | 1h | 1 |
| B.1 | Blog post: Wave 1 | McManus | 1h | Post-Wave 1 |
| B.2 | Blog post: Wave 2 | McManus | 1h | Post-Wave 2 |
| B.3 | Blog post: Wave 3 | McManus | 1h | Post-Wave 3 |

### Modified Items

| ID | Change |
|----|--------|
| 1.5.1 | README scope: current-state only, updated per wave |
| 1.1 | Added: CLI output enrichment for human feedback |
| 2.1 | Added: result summarization after multi-agent fan-out |

### New Recurring Items

| Item | Owner | Effort | When |
|------|-------|--------|------|
| README refresh | McManus | 30min | Each wave gate exit |
| Blog post | McManus | 1h | Each wave gate exit |

### New Directive (added to 019 §Directives)

> **5. Optimize for human feedback.** Every interaction gives the human visible evidence of progress. Silence is never acceptable.

### New Content

| Item | Location | Owner |
|------|----------|-------|
| Blog post format spec | `docs/blog/` (directory) | McManus |
| Blog engine sample prompt | `docs/sample-prompts.md` | McManus |

### Updated Effort Total

| Category | 019 Estimate | 019a Additions | New Total |
|----------|-------------|----------------|-----------|
| Wave 1 | 11-14h | +4h (1.8, 1.9, 1.10) | 15-18h |
| Wave 1.5/Content | 9-13h | +4.5h (blog posts, sample prompt, README refreshes) | 13.5-17.5h |
| Wave 2 | 12-16h | (absorbed into existing items) | 12-16h |
| Wave 3 | 12-16h | (absorbed into existing items) | 12-16h |
| **Total** | **44-59h** | **+8.5h** | **52.5-67.5h** |

Calendar impact: minimal. New Wave 1 items (1.8, 1.9, 1.10) all parallelize with existing work. Blog posts and README refreshes are inter-wave work that doesn't extend any wave duration.

---

## Updated Wave 1 Parallelism

```
Day 1-2:                                    Day 2-4:
├── 1.1 Error handling (Fenster)            └── 1.2 Test expansion (Hockney) ← needs 1.1
├── 1.3 CI setup (Hockney)
├── 1.4 Version stamping (Fenster)
├── 1.5 Silent success (Verbal)
├── 1.6 Human directive capture (Kujan)
├── 1.7 Feels heard behavior (Verbal)
├── 1.8 Register create-squad (Fenster)      ← NEW
├── 1.9 Progress reporting (Verbal + Kujan)  ← NEW
└── 1.10 VS Code smoke test (Kujan)          ← NEW
```

---

**Review requested from:** bradygaster  
**Approved by:** Keaton (Lead) — these amendments strengthen 019  
**Next action:** Team reviews, Brady approves, then merge into 019 or execute alongside it
