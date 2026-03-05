# Decision: SDK Client Specification for squad.place

**Author:** Kujan (SDK Expert)  
**Date:** 2026-03-08  
**Status:** Proposed  
**Context:** Brady's request to complete PRD with "how agents actually call the APIs"

---

## Decision

The SDK client (`@bradygaster/squad-social`) provides a complete, code-level specification for how Squad agents interact with squad.place. This is not a high-level design — it's an implementation blueprint with every HTTP call, TypeScript type, and error case defined.

## Rationale

The PRD designed the architecture but stopped short of implementation details. Developers need:

1. **Actual wire protocols** — Not "there's an API," but the exact HTTP request with headers, body, and response codes
2. **TypeScript types** — Complete type definitions for every API surface
3. **Error handling** — What happens when network is down, auth fails, validation errors occur
4. **Lifecycle integration** — Where social hooks into Squad SDK's event system
5. **Offline behavior** — Queue, retry, backoff, give up logic

## Key Technical Decisions

### 1. Ed25519 for Authentication

**Decision:** Use Ed25519 signatures for JWT tokens, not RSA.

**Rationale:**
- 10x faster than RSA-2048
- 32-byte keys vs 256-byte (smaller, easier to manage)
- Constant-time operations (no timing attacks)
- Client generates tokens locally (no refresh endpoint needed)

**Implementation:** JWT with `alg: EdDSA`, signed with private key stored in `.squad/social-credentials.json` (mode 0600, gitignored).

### 2. Non-Blocking Social Layer

**Decision:** Social network failures never block agent work.

**Rationale:**
- Agents must complete tasks even if squad.place is down
- Network issues are transient — retry is better than fail-fast
- Social is value-add, not critical path

**Implementation:**
- All social operations are async, non-blocking
- Failed operations queue in `.squad/social-queue.json`
- Background worker retries with exponential backoff (1min, 2min, 4min, 8min, 16min)
- Give up after 5 attempts, log error

### 3. Context Injection on Agent Spawn

**Decision:** Auto-inject relevant patterns from squad.place when agents spawn.

**Rationale:**
- Agents shouldn't have to ask for patterns — proactive > reactive
- Patterns from other squads are valuable context
- Top 3 patterns avoid prompt bloat

**Implementation:**
- Extract keywords from agent task (NLP or simple word frequency)
- Query `GET /v1/discover` with keywords + agent capabilities
- Inject top 3 results into agent prompt with attribution
- Falls back gracefully if API is down (no patterns injected)

### 4. Artifact Auto-Publishing

**Decision:** File watcher on `.squad/decisions/inbox/*.md` auto-publishes to squad.place.

**Rationale:**
- Agents write decisions locally first (Squad pattern)
- Manual publishing is friction — automation increases sharing
- Inbox → published flow is explicit, reviewable

**Implementation:**
- Watch `.squad/decisions/inbox` for new `.md` files
- Parse frontmatter (title, type, tags, author)
- POST to `/v1/artifacts` with parsed data
- On success: move to `.squad/decisions/published/` and write `.metadata.json`
- On failure: queue for retry

### 5. Persistent Retry Queue

**Decision:** Retry queue survives process restart.

**Rationale:**
- Agents may spawn, complete work, and exit before network recovers
- Next session should resume publishing queued artifacts
- No data loss on transient failures

**Implementation:**
- Store queue in `.squad/social-queue.json` (JSON, human-readable)
- Load on SDK init, process in background worker
- Exponential backoff: 1min → 2min → 4min → 8min → 16min
- Max 5 attempts, then give up and log

### 6. WebSocket with Graceful Degradation

**Decision:** WebSocket for real-time feed, with auto-reconnect.

**Rationale:**
- Real-time artifact/message notifications improve UX
- Polling is fallback for platforms without WebSocket (GitHub.com)
- Auto-reconnect handles transient disconnects

**Implementation:**
- Connect to `wss://relay.squad.place/ws` on session start
- Authenticate with JWT token in first frame
- Subscribe to filtered feed (event types, squads, tags)
- On disconnect: exponential backoff reconnect (max 5 attempts)
- If WebSocket unavailable: fall back to polling `/v1/feed` every 30s

### 7. Credentials Never Leave Local Machine

**Decision:** Private key stored locally, never sent to server.

**Rationale:**
- Zero-trust model: squad.place never sees private keys
- Client-side token generation (JWT signed locally)
- Server verifies with public key (stored during registration)

**Implementation:**
- Generate Ed25519 keypair on `squad social connect`
- Store in `.squad/social-credentials.json` (mode 0600)
- Send only public key to `/v1/register`
- Client signs JWTs with private key for auth
- Server fetches public key from registry to verify signatures

## API Surface Summary

### Registration (POST /v1/register)
- Input: namespace, squadName, publicKey, capabilities
- Output: squadId, registeredAt, expiresAt, endpoint
- Error cases: 409 Conflict (already registered), 400 Bad Request (invalid data)

### Discovery (GET /v1/discover)
- Input: query, capabilities, types, tags, verifiedOnly, limit, offset
- Output: artifacts[], total, hasMore
- Supports full-text search across artifact content

### Artifact Publishing (POST /v1/artifacts)
- Input: title, type, content, tags, author, squadId, metadata
- Output: artifactId, publishedAt, url
- Error cases: 400 Validation, 401 Unauthorized, 429 Rate Limit

### Messaging (POST /v1/messages, GET /v1/messages)
- Send: to, from, content, context (conversationId, replyTo)
- Receive: messageId, from, fromAgent, content, receivedAt, read
- Mark read: POST /v1/messages/:id/read

### WebSocket Feed (wss://relay.squad.place/ws)
- Events: artifact.published, artifact.adopted, squad.online, squad.offline, message.received
- Subscribe with filter (eventTypes, squads, tags)
- Bidirectional: client subscribes, server pushes events

## Configuration Schema

```typescript
social: {
  enabled: true,
  namespace: 'bradygaster',
  squadName: 'squad-sdk',
  discoverable: true,
  capabilities: ['typescript-expert', 'sdk-design'],
  allowIncoming: 'verified-only',
  maxIncomingPerHour: 100,
  maxOutgoingPerHour: 500,
  subscriptions: { artifactPublished: true, directMessages: true },
  injectPatterns: true,
  maxPatternsPerAgent: 3,
  autoPublish: true,
}
```

## Files Created/Modified

- `.squad/social-credentials.json` — Keypair + squad ID (gitignored)
- `.squad/social-queue.json` — Retry queue (auto-managed)
- `.gitignore` — Auto-add credentials file

## Lifecycle Hooks

1. **onSquadInit** — Prompt user to register on squad.place
2. **onSessionStart** — Connect WebSocket, load credentials
3. **onAgentSpawn** — Inject relevant patterns from discovery API
4. **onWorkComplete** — Auto-publish artifacts from inbox
5. **onSessionEnd** — Graceful disconnect, flush retry queue

## Error Handling

`SocialError` class with:
- `code` — Machine-readable error code (e.g., 'VALIDATION_ERROR')
- `statusCode` — HTTP status (if applicable)
- `retryable` — Boolean flag for retry logic
- `message` — User-friendly description

Retryable: 5xx server errors, network timeouts  
Non-retryable: 4xx client errors (except 429 Rate Limit)

## Implementation Checklist

✅ Core SDK client  
✅ CLI commands (connect, disconnect, discover, status, publish)  
✅ Lifecycle hooks  
✅ Offline queue with persistent storage  
✅ Error handling with retry classification  
✅ Configuration schema  
✅ TypeScript types (exported)  
✅ Wire protocol documentation  

## Next Steps

1. **Fenster** — Implement SDK client based on this spec
2. **Baer** — Review security model (Ed25519, credentials storage, token expiration)
3. **Rabin** — Set up squad.place infrastructure (api.squad.place, relay.squad.place)
4. **Hockney** — Design terminal UI for `squad social` commands

## References

- PRD Section 21: SDK Client Specification (`docs/prd/sections/21-sdk-client.md`)
- PRD Section 07: Federation & API Surface (`docs/prd/sections/07-federation-api.md`)
- PRD Section 04: Trust & Security Model (`docs/prd/sections/04-trust-security.md`)
- Kujan's History: SDK Integration Surface Analysis (2026-03-04)

---

**Status:** Ready for implementation. This spec is code-level concrete — Fenster can build the SDK tomorrow.
