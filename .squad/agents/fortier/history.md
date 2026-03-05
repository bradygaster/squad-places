# Fortier — History

## Project Context

- **Owner:** Brady
- **Project:** squad-sdk — the programmable multi-agent runtime for GitHub Copilot (v1 replatform)
- **Stack:** TypeScript (strict mode, ESM-only), Node.js ≥20, @github/copilot-sdk, Vitest, esbuild
- **Created:** 2026-02-21

## Core Context

**SDK Architecture & OTel (Feb 21–22):** Implemented StreamingPipeline bridge + ShellRenderer for streaming event handling. Implemented ShellLifecycle for agent discovery from team.md + state management. Decided runtime/event-bus.ts as canonical (colon-notation, error isolation) vs client/event-bus.ts. Implemented Coordinator + RalphMonitor with EventBus subscriptions + cleanup patterns. Wired full OTel provider (NodeSDK) + bridge (TelemetryEvent → OTel spans) with version skew mitigation. Wired session traces (sendMessage span parent/child, closeSession alias) + latency metrics (TTFT, duration, tokens/sec) with opt-in tracking via markMessageStart(). Phase 2 shipped in parallel with Fenster/Edie/Hockney (1940 tests passing). Wired REPL Shell coordinator (lazy session creation, parallel MULTI routing via Promise.allSettled). **[CORRECTED — Feb 24 work summary, final state]** Wave 2 polish: rich welcome header (brand + version + team roster with emoji + focus), compact inline AgentPanel (flexWrap + role emoji + status indicators), MessageStream (cyan/dim/green user/system/agent messages, thin separators, ThinkingIndicator with braille spinner), InputPrompt dynamic prompt. All startup data loading non-blocking via useEffect + filesystem reads. Role-to-emoji mapping lives in lifecycle.ts alongside team manifest parsing (design cohesion).

## Learnings

### From Beta (carried forward)
- Event-driven over polling: always prefer event-based patterns
- Streaming-first: async iterators over buffers — this is a core design principle
- Graceful degradation: if one session dies, others survive
- Node.js ≥20: use modern APIs (structuredClone, crypto.randomUUID, fetch, etc.)
- ESM-only: no CJS shims, no dual-package hazards
- Cost tracking and telemetry: runtime performance is a feature, not an afterthought

### Architecture Patterns (Issues #239, #240, #303)

---

📌 Team update (2026-02-24T07:20:00Z): Wave D Batch 1 work filed (#488–#493). Cheritto: #488–#490 (UX precision — status display, keyboard hints, error recovery). Kovash: #491–#492 (hardening — message history cap, per-agent streaming). Fortier: #493 (streamBuffer cleanup on error). See .squad/decisions.md for details. — decided by Keaton

📌 Team update (2026-02-24T08:12:21Z): Wave D Batch 1 COMPLETE — all 3 PRs merged to main, 2930 tests passing (+18 new). Fortier: #499 shipped Per-Agent Streaming Content. — decided by Scribe


### 2026-02-24T17-25-08Z : Team consensus on public readiness
📌 Full team assessment complete. All 7 agents: 🟡 Ready with caveats. Consensus: ship after 3 must-fixes (LICENSE, CI workflow, debug console.logs). No blockers to public source release. See .squad/log/2026-02-24T17-25-08Z-public-readiness-assessment.md and .squad/decisions.md for details.

---

## Version Context — v0.6.0 vs v0.8.17

**[CORRECTED — 2026-03-03]:** Team discussions included confusion around public release version target. Brady initially directed v0.6.0, then EXPLICITLY REVERSED it. Correct target: v0.8.17 for both npm packages AND public repo GitHub tag. All migration documentation (docs/migration-checklist.md, docs/migration-guide-private-to-public.md) correctly reference v0.8.17. This is documented in .squad/decisions.md lines 787–1610 with detailed corrections.

---

## History Audit — 2026-03-03

**Audit Results:** 2 corrections made.
- Clarified Feb 24 wave work summary as final state (not intermediate)
- Added critical v0.6.0 vs v0.8.17 version context to prevent future spawn confusion

---

### 2026-03-05: Squad Social Network — Performance Architecture

**Project:** squad-social-network (Brady's AI-agent social network)  
**Task:** Wrote PRD Section 08 — Performance, Scale & Real-Time Communication

**What I learned:**

1. **Scale thinking for agent networks differs from human networks:**
   - Agents don't sleep (24/7 operational squads)
   - Agents post at machine speed (burst posting during sprints)
   - A "small" network of 1,000 agents can generate more traffic than 10,000 humans
   - Resource budgeting must account for ambient infrastructure (<20% host resources)

2. **SSE over WebSocket for MVP:**
   - Server-Sent Events (SSE) are simpler for one-way server→client push
   - Built-in reconnection with `Last-Event-ID` header
   - HTTP/2 multiplexing allows multiple streams over one connection
   - Reserve WebSocket for Phase 2 when bidirectional <50ms latency is needed

3. **Backpressure as first-class design:**
   - Bounded buffers per stream type (100 notifications, 1000 feed events)
   - Tiered dropping: keep critical (mentions, DMs), drop feed posts when slow
   - Send `buffer_warning` and `buffer_overflow` events to slow consumers
   - Agents can catch up later via API fetch (no blocking)

4. **Event-driven social network = event log with views:**
   - Everything is an event (post, reply, reaction, follow, mention, DM, heartbeat)
   - Fan-out routing: direct followers (guaranteed), squad feed (best-effort), global feed (sampled)
   - SQLite WAL mode for Phase 1 (10,000 writes/sec), Kafka/NATS for Phase 2
   - Monotonic event IDs enable replay from last ACK'd position

5. **Latency targets matter more than throughput:**
   - Target P95: <500ms agent-to-agent delivery
   - Sub-second perceived as "live" for conversational threading
   - Cascading reactions (agents reacting programmatically) amplify latency issues
   - 1,000 events/sec is enough for Phase 1 (1,000 agents)

6. **Cost model: $0.01/agent/month is sustainable:**
   - Compute: $36/month for 1,000 agents (single 4-core VM)
   - Storage: $0.60/month (30-day event retention in SQLite)
   - Network egress negligible (1 KB avg event size)
   - Social network is transport-only (no LLM token costs)

7. **Offline & reconnection resilience:**
   - 24-hour catch-up window (events older than 24h fetched via API)
   - Max 10,000 events on replay (prevents memory exhaustion)
   - Critical events (mentions, DMs) buffered 24h; feed events dropped after 1h
   - Agent SDK handles reconnection transparently (no app-level retry logic)

**Performance philosophy reinforced:**
- Event-driven over polling (agents subscribe, don't poll)
- Streaming-first (every feed is an async iterator)
- Backpressure as first-class (slow consumers don't crash the system)
- Degrade gracefully (drop feed events, keep notifications)
- Resource-aware (social network is ambient infrastructure)

This aligns perfectly with Squad SDK's existing patterns (event-bus.ts, streaming pipelines, graceful degradation). The social network should feel like a natural extension of the runtime.

## PIN: 2026-03-05 - 20-Agent PRD Design Session

**Event:** Historic parallel fanout - 20 agents designed squad-social-network PRD simultaneously.

**Contribution:** All agents participated. 20 PRD sections delivered.

**Outcome:**
- 20 PRD sections drafted (docs/prd/sections/{01-20}-*.md)
- 23 decisions merged to .squad/decisions.md
- 20 orchestration logs created
- Session log: .squad/log/2026-03-05T02-02-22Z-social-network-prd.md
- Inbox cleared

**Next Steps:** Keaton assembles final PRD, Brady reviews, implementation planning begins.

**Key Pattern:** Largest parallel fanout in Squad history. Loose coupling, clear domains, shared constraints.
