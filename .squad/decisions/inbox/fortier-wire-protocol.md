# Wire Protocol Decision — Fortier

**Date:** 2026-03-05  
**Author:** Fortier (Node.js Runtime)  
**Context:** Brady requested wire protocol specification (Section 23) for api.squad.place

---

## Decision: SSE + Ed25519 Signing for Phase 1

### What

The wire protocol for squad.place uses:
- **HTTPS** for REST API (request/response)
- **SSE** (Server-Sent Events) for real-time streaming at `GET /v1/stream`
- **Ed25519** signatures on every request (prevents replay attacks)
- **JSON** payloads (gzip >1KB)
- **Exponential backoff** with jitter for reconnection

### Why

**SSE over WebSocket:**
- Simpler protocol (one-way push, no handshake)
- Native reconnection via `Last-Event-ID` header
- HTTP/2 multiplexing (multiple streams per connection)
- Firewall-friendly (pure HTTP, no upgrade)
- WebSocket reserved for Phase 2 (<50ms bidirectional latency)

**Ed25519 Signing:**
- Fast verification (<1ms server-side)
- Small signature size (88 chars base64)
- Prevents replay attacks (timestamp validation: 5-minute window)
- No shared secrets (public-key cryptography)

**Backpressure as First-Class:**
- Per-connection buffer: 1,000 events (~2 MB)
- Tiered dropping: keep heartbeats, drop reactions first
- Client gets 24h to catch up (REST API fallback)

**Latency Targets:**
- REST: P95 <200ms (TLS session resumption + HTTP/2 reuse)
- SSE: P95 <500ms publish-to-delivery (in-memory fanout)
- Heartbeat every 30s, timeout at 90s

**Bandwidth Budget (Phase 1):**
- 500 agents = 55 GB/month (~$5.50)
- Idle agent: 8.64 MB/month (heartbeat only)
- Active agent: 72 MB/month (50 events/hr)

### Implications

**SDK Implementation (Fenster):**
- Ed25519 signing library required (libsodium or tweetnacl)
- EventSource wrapper with Last-Event-ID tracking
- Bounded queue (100 events) to prevent memory exhaustion
- Exponential backoff (5s → 60s) with jitter (0–1s)

**Server Implementation (Fortier):**
- Node.js + Fastify (HTTP/2 native)
- SSE connection pool manager (1,000 connections = 1 GB buffers)
- Ed25519 signature verification middleware
- Event fanout (publisher → subscribers in <10ms)
- 24h event retention (SQLite for Phase 1)

**Security (Baer):**
- Certificate pinning (Let's Encrypt root CA)
- Timestamp skew validation (<5 min)
- Rate limiting: 100 req/min per squad
- Connection limits: 10 SSE streams per squad

**Testing (Keaton):**
- Load test: 500 concurrent agents, 1,000 events/sec
- Latency test: measure P50/P95/P99 across geo regions
- Chaos test: network flakiness, server restarts, slow clients
- Backpressure test: verify graceful degradation (drop reactions, keep messages)

### Alternatives Considered

**WebSocket instead of SSE:**
- Rejected for Phase 1: overkill for one-way push
- Reserved for Phase 2 when bidirectional <50ms needed

**ActivityPub federation protocol:**
- Rejected: designed for human-scale social networks
- Agents need <500ms latency, ActivityPub is seconds-scale

**GraphQL Subscriptions:**
- Rejected: adds query complexity, not needed for event streams
- REST + SSE is simpler for feed consumption

**JWT instead of Ed25519 signing:**
- Rejected: shared secrets are harder to rotate
- Public-key crypto eliminates secret distribution problem

### Next Steps

1. **Fortier:** Implement Phase 1 SSE server (Node.js/Fastify)
2. **Fenster:** Implement client SDK (`@bradygaster/squad-social`)
3. **Baer:** Security audit (Ed25519 + timestamp validation)
4. **Keaton:** Load testing suite (500 agents, backpressure scenarios)

### References

- PRD Section 23: `docs/prd/sections/23-wire-protocol.md`
- PRD Section 08: Performance architecture (Fortier)
- PRD Section 04: Security model (Baer)
- PRD Section 07: Federation & SDK integration (Kujan)

---

**Status:** ✅ DECIDED — Ready for implementation
