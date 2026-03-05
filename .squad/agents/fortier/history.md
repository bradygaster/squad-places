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

---

### 2026-03-05: Wire Protocol Specification (Section 23)

**Task:** Write `docs/prd/sections/23-wire-protocol.md` — the bytes-on-the-wire specification for api.squad.place.

**What I learned:**

1. **SSE is the right choice for Phase 1:**
   - Built-in reconnection via `Last-Event-ID` header eliminates manual state tracking
   - HTTP/2 multiplexing allows multiple SSE streams over one TCP connection
   - Simpler than WebSocket for one-way push (no handshake complexity, no custom framing)
   - Works through corporate proxies and firewalls (pure HTTP, no upgrade protocol)
   - EventSource API is native in browsers and easy to polyfill in Node.js

2. **Request signing prevents replay attacks:**
   - Sign: `METHOD\nPATH\nTIMESTAMP\nBODY_SHA256`
   - Ed25519 signature is 88 characters base64-encoded (small overhead)
   - 5-minute timestamp skew window balances security vs clock drift
   - Signature verification on server is <1ms (Ed25519 is fast)
   - Authorization header format: `SquadSig squad_id=...,timestamp=...,signature=...`

3. **Backpressure must be first-class, not afterthought:**
   - Per-connection buffer: 1,000 events (FIFO queue, ~2 MB)
   - Send `buffer_warning` at 80% capacity, `buffer_overflow` at 95%
   - Drop low-priority events first (reactions before messages, messages before heartbeats)
   - Client SDK must use bounded queues (prevent memory exhaustion)
   - Slow clients get 24h to catch up via REST API before data loss

4. **Latency budgets drive architecture:**
   - P95 <200ms for REST calls requires TLS session resumption + HTTP/2 connection reuse
   - P95 <500ms for SSE event delivery requires in-memory fanout (no database per-event)
   - Geo-distributed servers drop cross-coast latency from 150ms to 20ms
   - Heartbeat every 30s keeps NAT holes open + detects dead connections
   - 90-second timeout balances fast failure detection vs spurious disconnects

5. **Bandwidth scales sublinearly with agents:**
   - Idle connection: 8.64 MB/month (heartbeat only)
   - Active agent: 72 MB/month (50 events/hr)
   - 500 agents = 55 GB/month total (~$5.50 at $0.10/GB egress)
   - Burst scenario (100 agents publish simultaneously): 8.5 MB/s (68 Mbps) peak
   - Social network is transport-only (no LLM token costs) — scales on compute/network, not AI inference

6. **Exponential backoff with jitter prevents thundering herd:**
   - Start at 5s, double each retry, cap at 60s
   - Add 0–1s random jitter (1,000 clients don't reconnect at exact same millisecond)
   - Reset delay on successful connection (don't punish stable clients)
   - Track `Last-Event-ID` across reconnects (resume, don't replay)

7. **Error handling is protocol design, not implementation detail:**
   - 429 rate limit → respect `Retry-After` header, don't DDoS yourself
   - 401/403 auth errors → re-authenticate once, alert user if still failing
   - 500/502 server errors → retry 3 times with backoff, then fail loudly
   - SSE connection lost → automatic reconnect, no user intervention
   - Event ID expired (>24h gap) → fetch via REST, then resume SSE

8. **Observability must be built-in from day one:**
   - Server: `sse_connections_active`, `sse_events_dropped_total`, `sse_buffer_depth`
   - Client: `sse_reconnect_count`, `sse_processing_lag_seconds`, `sse_events_received_total`
   - Metrics answer: "Why is my squad slow?" (buffer lag), "Why do I keep reconnecting?" (network instability)
   - Prometheus-compatible (standard labels: squad_id, event_type, reason)

**Protocol philosophy reinforced:**
- The wire protocol is not HTTP + JSON — it's latency budgets, backpressure contracts, and reconnection semantics
- Every millisecond of latency comes from somewhere (DNS, TLS, TCP, server processing) — measure and optimize the slowest
- Client SDKs don't "handle errors" — they implement the protocol's error recovery specification
- SSE is underrated for real-time (WebSocket is overkill until you need bidirectional <50ms)

This spec is implementation-ready. Next: build the server.
