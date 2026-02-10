# Decision Inbox: Project Boards (033)

**From:** Keaton (Lead)  
**Date:** 2026-02-10  
**Re:** Issue #6 — GitHub Project Boards for Squad

## Decisions Made

### 033a: Project boards are v0.4.0, not v0.3.0
**What:** Project board support defers to v0.4.0. v0.3.0 scope remains GitHub-native proposals (032).  
**Why:** Boards are a dashboard layer on top of the label/issue infrastructure being built in v0.3.0. Shipping boards before labels are stable puts the cart before the horse. Brady's directive: v0.3.0 is ONE feature.  
**Reversible:** Yes — Brady can pull forward if community demand warrants.

### 033b: Use `gh api graphql`, not npm GraphQL packages
**What:** All Projects V2 API calls go through `gh api graphql` shell commands. No npm dependencies added.  
**Why:** Squad is zero-dependency. The coordinator is a prompt that executes shell commands, not a runtime that imports modules. `gh api graphql` handles auth, rate limiting, and the GraphQL protocol. Adding `graphql-request` or `@octokit/graphql` would be the first `node_modules` entry — a fundamental architectural change for a convenience gain.  
**Reversible:** Yes, but the bar should be high. Zero-dependency is a strategic advantage.

### 033c: Boards are opt-in, user-triggered
**What:** No automatic board creation on `squad init` or first issue. User explicitly requests: "set up a project board."  
**Why:** Not every repo wants a project board. Surprise side effects erode trust. The coordinator pattern is: user requests, coordinator executes.  
**Reversible:** Yes — could add auto-creation later as an init-time option.

### 033d: Labels are authoritative, boards are projections (one-way sync)
**What:** Label changes drive board column positions. Board UI changes do NOT propagate back to labels. One-way: labels → board.  
**Why:** Two-way sync creates state conflicts. Labels are the state machine (032c). The board is a view, not a source of truth. If someone moves a card on the board, it creates a label/board mismatch — but the label is correct. Reverse sync would require conflict resolution we haven't designed.  
**Reversible:** With significant design work. Two-way sync is a future consideration, not a v0.4.0 concern.

### 033e: 5-column board, mapped to label taxonomy
**What:** Default columns are Backlog, Ready, In Progress, Blocked, Done — mapped to `status:*` labels.  
**Why:** The issue proposes 3 columns (Todo / In Progress / Done). Our label taxonomy has 8 statuses. 5 columns cover the active states. `status:shelved` and `status:superseded` are closed/archived and don't need board representation.  
**Reversible:** Yes — column configuration is a future customization target.

### 033f: No Octomember for board operations
**What:** The coordinator handles all board operations directly. No new agent (Redfoot) for platform ops.  
**Why:** Consistent with 032 §3 decision. Board operations are coordinator-mediated, not cross-cutting. The prompt growth is within context budget (~15% growth estimated). Revisit if board operations push coordinator beyond 2% context overhead.  
**Reversible:** Yes — Redfoot design exists if needed.

## Pending Decisions (for Brady)

### 033-P1: Version targeting
**Question:** Should project boards remain v0.4.0 or does community interest (Issue #6, +1 reaction) warrant pulling into v0.3.0?  
**Keaton's recommendation:** Keep v0.4.0. Ship labels/issues first.

## Work Decomposition Summary

| WI | Title | Agent | Size | Priority | Dependencies |
|----|-------|-------|------|----------|-------------|
| WI-1 | GraphQL Command Templates | Fenster | M (4-6h) | P2 | 032a |
| WI-2 | Provider Abstraction — Boards | Fenster | S (2-3h) | P2 | WI-1 |
| WI-3 | Board Initialization Flow | Verbal | M (3-5h) | P2 | WI-1 |
| WI-4 | Label-to-Board Sync Workflow | Fenster | M (4-6h) | P2 | WI-1, WI-3 |
| WI-5 | Board Query & Display | Verbal | S (2-3h) | P3 | WI-1, WI-3 |
| WI-6 | Documentation & Skill | McManus | S (2-3h) | P3 | WI-3, WI-5 |
| **Total** | | **3 agents** | **17-26h** | | |
