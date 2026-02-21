# Team Decisions

> Historical decisions archived in `decisions/archive/`. Recent 30 days and permanent decisions stay here.

---

### 2026-02-22: Repo Independence (PRD 22)

**By:** Keaton (Lead)

**What:** Squad-sdk becomes the primary working repository. The team now lives in squad-sdk with full `.squad/` directory structure. Squad beta enters archive mode.

**Why:** The v1 SDK replatform is complete (1,551 tests, 13 modules, typed hooks, session management). The team needs to work in the repo they're building. Self-referential: squad-sdk's Squad team manages squad-sdk.

**Implementation:**
- `.squad/` directory created with full team roster (13 specialists + Scribe + Ralph)
- Routing rules for all 13 modules
- Casting state with The Usual Suspects universe
- Agent charters seeded with beta knowledge
- Beta repo README updated with archive notice

**Status:** ✅ Implemented

---

### 2026-02-22: CLI Migration Plan — Hybrid Architecture

**By:** Keaton (Lead)

**What:** Port beta CLI (1,496 lines, 9 commands) to squad-sdk using hybrid approach:
- Zero-dep scaffolding (init, upgrade, templates, version stamping)
- SDK-integrated runtime commands (watch, export/import, plugin, copilot)

**Why:** Preserves zero-dep constraint where it matters (init/upgrade must work without SDK installed) while leveraging SDK primitives for runtime features (type-safe, tested).

**Trade-offs:**
- Two dependency profiles (scaffolding vs runtime) requires clear separation
- More complex than pure port or pure rewrite
- Better than alternatives: pure port carries tech debt, pure rewrite breaks zero-dep

**Timeline:** 6-9 weeks across 3 milestones (M7: CLI Foundation, M8: CLI Parity, M9: Repo Independence)

**Status:** ✅ Approved, PRDs 15-22 defined

---

### 2026-02-20: SDK Distribution Model — GitHub-Native

**By:** Brady (via directive)

**What:** Keep distributing via GitHub + npx (`npx github:bradygaster/squad`). Do NOT move to npmjs.com. The SDK/CLI stays on GitHub for distribution.

**Why:** Continuity with existing distribution model. GitHub-native workflow aligns with GitHub Copilot platform. Zero-config install experience.

**Status:** 🔒 Permanent decision

---

### 2026-02-20: The Usual Suspects — Locked Universe

**By:** Team consensus (Keaton, Kujan, McManus, Verbal)

**What:** The Usual Suspects (1995) is the permanent universe for Squad. Scribe is always Scribe. Ralph is always Ralph. All other names cast from this universe.

**Why:** 
- Names are memorable and distinct (Keaton, Verbal, Fenster, Kujan, McManus, Hockney, Edie, Kobayashi, Fortier, Rabin, Baer, Redfoot, Strausz)
- Team identity persists across projects and repos
- Accumulated knowledge tied to specific agent names
- Developers build relationships with agents over time

**Status:** 🔒 Permanent decision

---

### 2026-02-20: Type Safety Standards

**By:** Edie (TypeScript Engineer)

**What:** TypeScript strict mode with zero tolerance:
- `strict: true` in tsconfig
- `noUncheckedIndexedAccess: true`
- No `@ts-ignore` allowed without explicit review
- Declaration files are public API contracts

**Why:** Types are runtime guarantees. If it compiles, it works. Strict mode catches bugs at compile time, not in production.

**Status:** 🔒 Permanent decision

---

### 2026-02-20: Hook-Based Governance (Not Prompt-Based)

**By:** Baer (Security)

**What:** Security, PII scrubbing, file-write guards, reviewer lockouts enforced via hooks module. Rules run BEFORE tools execute. Not suggestions in prompts — actual code that blocks invalid operations.

**Why:** Prompt instructions are interpretable and ignorable. Hooks are code. They enforce rules at runtime. No agent (compromised or confused) can bypass them.

**Examples:**
- File-write guards block writes outside allowed paths
- PII scrubber redacts emails/secrets from logs
- Reviewer lockout prevents original author from re-editing rejected work
- Ask-user rate limiter prevents stall loops

**Status:** 🔒 Permanent decision

---

### 2026-02-20: Node.js ≥20, ESM-Only

**By:** Fortier (Node.js Runtime)

**What:** Runtime target is Node.js ≥20. ESM-only (no CommonJS shims). No dual-package hazards.

**Why:** Node 20 LTS provides stable async/await, top-level await, native fetch. ESM is the future — CJS adds complexity and compatibility risk. Single module format simplifies build and distribution.

**Status:** 🔒 Permanent decision

---

### 2026-02-20: `.squad/` Directory Standard

**By:** Team consensus

**What:** `.squad/` is the team state directory. NOT `.ai-team/` (beta convention). Migration path provided for legacy repos.

**Why:** Clearer naming (`squad` vs `ai-team`). Beta used `.ai-team/` before Squad had a name. Now that Squad is the product, the directory should match.

**Migration:** `npx github:bradygaster/squad upgrade --migrate-directory`

**Status:** ✅ Standard for v1+, `.ai-team/` deprecated

---

### 2026-02-20: Proposal-First Workflow

**By:** Keaton (Lead)

**What:** Meaningful changes require proposals before execution. Format: Problem/Solution/Trade-offs/Alternatives/Success Criteria. 48-hour review window.

**Why:** Prevents regret decisions. Forces articulation of trade-offs. Creates searchable decision history. Aligns team before work starts.

**Scope:** Applies to:
- Architecture changes
- API additions/breaking changes
- New features with user impact
- Non-trivial refactors

**Exceptions:** Bug fixes, tests, documentation updates (unless controversial)

**Status:** ✅ Standard practice

---

### 2026-02-20: Reviewer Rejection Lockout

**By:** Keaton (Lead), Baer (Security)

**What:** If a reviewer (Keaton, Hockney, Baer) rejects work, the original author may be locked out of that artifact. A different agent must handle revisions.

**Why:** Prevents self-review after rejection. Ensures fresh eyes on rejected work. Enforces protocol for quality gates.

**Enforcement:** Via hooks module (`reviewer-lockout.ts`)

**Status:** ✅ Enforced

---

### 2026-02-20: Tone Ceiling — ALWAYS

**By:** McManus (DevRel)

**What:** No hype. No hand-waving. No unsubstantiated claims. All assertions must be:
- Backed by evidence (code, tests, docs, benchmarks)
- Specific, not vague
- Measured, not exaggerated

**Examples:**
- ❌ "Squad is blazing fast"
- ✅ "Squad's session pool reduces spawn overhead from 2.3s to 180ms (1,551 tests, benchmarks in runtime/benchmarks.ts)"

**Why:** Trust is earned through precision. Developers smell BS immediately. Better to under-promise and over-deliver.

**Status:** 🔒 Permanent decision

---

### 2026-02-20: Scribe Merges Decisions Inbox

**By:** Team consensus

**What:** Decisions written to `decisions/inbox/` are merged to `decisions.md` by Scribe (silent agent). Scribe runs in background after substantial work.

**Why:** Prevents merge conflicts. Single merge authority. Scribe has full context across all sessions and can deduplicate/organize.

**Workflow:**
1. Agent writes decision to `decisions/inbox/{agent}-{topic}.md`
2. Scribe merges to `decisions.md` in next background run
3. Source file archived or deleted

**Status:** ✅ Standard practice

---

### 2026-02-22: Module Boundaries — 13 Modules

**By:** Keaton (Lead), Edie (TypeScript Engineer)

**What:** Squad SDK has 13 modules with clear ownership:
- adapter, agents, build, casting, cli, client, config, coordinator, hooks, marketplace, ralph, runtime, sharing, skills, tools

**Why:** Clear boundaries prevent sprawl. Each module has a single owner (from team roster). Imports across module boundaries are explicit and reviewable.

**Ownership:** See `team.md` Module Ownership section

**Status:** ✅ Current architecture

---

**Last Updated:** 2026-02-22
