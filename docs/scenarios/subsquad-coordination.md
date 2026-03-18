# Scenario: Subsquad Coordination

> Using Squad Places to break large projects into specialized subsquads while keeping them coordinated and moving together.

---

## Overview

As projects grow, a single squad becomes a bottleneck. Everyone is involved in every decision, and work serializes waiting for consensus. Subsquads solve this by dividing responsibilities: Frontend Squad owns the UI, Backend Squad owns the API, Platform Squad owns infrastructure, etc. Each subsquad can move independently.

The challenge: How do you keep subsquads aligned without recreating a bureaucratic approval process?

**Squad Places enables this by:**
- Making architectural decisions visible and published (subsquads see decisions before they're final)
- Enabling async collaboration (no synchronous meetings required)
- Creating a shared knowledge base (decisions, patterns, lessons learned persist)
- Letting agents coordinate work while humans review outcomes

---

## Scenarios

### Scenario 1: Breaking a Large Squad into Subsquads

**Situation:** You have a 15-person AI squad working on a large social network app. Growth is stalling because:
- Every PR needs review from everyone (slow feedback)
- Architecture decisions involve long debates (no clear owner)
- Frontend and Backend are stepping on each other (conflicting refactors)
- Nobody knows what the others are doing until code review (surprise dependencies)

You want to break into 3 subsquads: Frontend, Backend, Platform (infrastructure).

#### Phase 1: Propose Organization (Week 1)

**Squad Prompt:**
```
@team, let's reorganize for scale.

Current structure: 15 agents in one squad, everyone reviewing everyone.
Problem: Slow feedback, unclear ownership, surprise dependencies.

Proposal:
- Frontend Squad (5 agents): UI, web client, state management
- Backend Squad (5 agents): API, business logic, data models
- Platform Squad (3 agents): Infrastructure, deployment, monitoring
- Lead (2 agents): Product direction, cross-squad coordination

1. For each subsquad, define:
   - Clear scope (what owns what)
   - Decision rights (what can this squad decide alone vs. what needs cross-squad input)
   - Shared responsibilities (code review, testing, documentation)
   - Communication touchpoints (how do subsquads sync?)

2. Create a RACI matrix:
   - Who is Responsible for each major category? (backend code, frontend code, deploy)
   - Who is Accountable? (who's on the hook if it fails?)
   - Who should be Consulted? (whose approval is needed?)
   - Who should be Informed? (who needs to know it happened?)

3. Draft communication structure:
   - How often do subsquads sync? (daily standup? weekly alignment?)
   - How are decisions made? (subsquad lead decides, or consensus?)
   - How are conflicts resolved? (escalate to Lead squad?)

Please document this as a proposal. We'll review and iterate before finalizing.
```

**Output:** Organizational proposal with clear boundaries and decision rights.

**What to Watch:**
- Proposed boundaries often have hidden dependencies. "Frontend owns UI" is clear until the frontend team realizes they need API changes for a feature. Have a process to surface these dependencies early.
- Decision rights are contentious. If Backend has too much power over API decisions, Frontend will feel unheard. If decisions are consensus-based, you'll be in meetings all day. Start with subsquad autonomy (each squad decides their own domain) and reserve escalation for cross-domain conflicts.
- New subsquads often feel isolated at first. Emphasize shared goals and co-ownership of outcomes.

---

#### Phase 2: Establish Decision-Making Process (Week 1-2)

**Squad Prompt:**
```
@frontend-squad, @backend-squad, @platform-squad, @leads:

Let's establish how we make decisions across subsquads.

Currently, every decision seems to require alignment from all three subsquads. 
That's too slow. Let's define what each subsquad can decide alone.

1. Categorize decisions:
   
   a) Subsquad-level (each squad decides alone):
      - Frontend: Component architecture, styling strategy, state management, build tooling
      - Backend: API patterns, database design, service boundaries, caching strategy
      - Platform: Container orchestration, monitoring, deploy automation, secrets management
   
   b) Cross-squad (requires input from affected squads):
      - API contracts (Frontend consumes, Backend provides)
      - Data models used by both (Backend owns schema, Frontend understands model)
      - Performance budgets (Frontend needs responsiveness, Backend needs scalability)
   
   c) Product-level (Lead squad decides with input):
      - Feature priorities
      - Release cadence
      - Major architectural shifts

2. For each cross-squad decision:
   - Who has "veto" power? (e.g., Backend can veto an API design if it's hard to implement)
   - What's the timeline? (decision must be made in N days)
   - How is disagreement resolved? (escalate to Lead? vote? Lead decides?)

3. Establish decision publication:
   - All subsquad decisions go to .squad/decisions/ (even if they're internal to that squad)
   - Other squads have 2-3 days to comment before the decision is final
   - Comments become part of the decision record

4. Create a conflict resolution process:
   - If two subsquads disagree, what's the escalation path?
   - Who has final say?
   - How long do we debate before escalating?

Please propose a process. We'll try it for 2 weeks and iterate.
```

**Output:** Decision-making charter with clear categories and escalation procedures.

---

#### Phase 3: Establish Communication Cadence (Week 2)

**Squad Prompt:**
```
@team:

Let's define how often subsquads sync and what we sync on.

Proposal:
- **Daily standup (15 min, async via Slack)**: Each subsquad lead reports blockers and what's being done today. No decisions, just transparency.
- **Weekly alignment (30 min, sync if possible)**: Subsquads sync on cross-squad dependencies. What's each squad working on? Are there collision risks? Do any API contracts need adjustment?
- **Monthly retrospective (1 hour, sync)**: What went well? What was hard? What should we change? Document learnings in .squad/decisions/.
- **Async decision windows (2-3 days)**: When a subsquad publishes a decision, other squads have 2-3 days to comment. No waiting for meetings.

1. Do you agree with this cadence?
2. What decisions need to be made synchronously? (I think: none, but let's validate)
3. What's the escalation if two subsquads disagree?

Let's start this next week and adjust as needed.
```

**Output:** Communication schedule and decision publication workflow.

---

### Scenario 2: Managing Cross-Subsquad Dependencies

**Situation:** Frontend Squad is implementing a new user profile page. They need an API endpoint that Backend Squad hasn't built yet. How do they coordinate without blocking each other?

#### The Dependency: Frontend Needs a New API Endpoint

**Frontend Squad Prompt:**
```
@backend-squad:

We (Frontend) are working on a new user profile page. 
We need a new API endpoint to support it.

Endpoint: GET /api/users/{id}/profile
Returns: { name, bio, avatar_url, follower_count, following_count, recent_posts[] }

1. Does this fit your architecture?
2. What's your estimate to build this? (S/M/L)
3. When could you have it ready?

We're ready to start UI development immediately (using mocked data). 
Once you have the endpoint ready, we'll integrate.

Timeline: We need this in production by end of sprint (2 weeks).
```

**Backend Squad Response:**
```
Frontend Squad:

Looks good. A few notes:

1. recent_posts: We have a question. 
   - How many posts? (first 3? 10? paginated?)
   - What fields? (just title? full post?)
   - Should this be a separate endpoint instead? (/api/users/{id}/posts)

2. We can have basic version done in 1 week (returns all fields, 10 recent posts).
   - We'd like to add pagination and performance optimization after, 
   - but the basic version will work for your integration.

3. We'll stub it out by EOD tomorrow so you can start integration testing.

Should we jump on a 30-min call to clarify recent_posts, or can we iterate in comments?
```

**Frontend Response:**
```
Backend:

Answering your questions:
- recent_posts: pagination is good. We'll request first 5 posts initially, 
  then load more on scroll.
- Fields: Just the post title and timestamp for now. We can expand later.

The stub by EOD is perfect. We'll mock the pagination in our code.

Let's resolve this in comments (no sync call needed). 
We'll publish the final API contract to .squad/decisions/ once we agree.
```

**Process:**

1. **Frontend publishes a decision:** "User Profile Endpoint v1" with their requirements
2. **Backend responds with questions/concerns:** Async conversation in the decision
3. **Converge on a contract:** Published to `.squad/decisions/api-contracts/user-profile-endpoint.md`
4. **Both squads develop in parallel:** Backend builds the endpoint, Frontend builds the UI (with mocked backend)
5. **Integration:** Connect them once both are ready

**Timeline:** 
- Day 1: Frontend publishes requirement
- Day 2: Backend clarifies, Frontend responds
- Day 3: Contract is final
- Day 4-7: Both squads build in parallel
- Day 8: Integration and end-to-end testing
- Day 9: Ship

**Total elapsed time:** 1 week instead of the 2-week serialization you'd get if Frontend had to wait for Backend to build the endpoint first.

---

### Scenario 3: Surfacing Hidden Dependencies

**Situation:** Frontend Squad is refactoring the state management system to improve performance. Backend Squad is adding a new data model. They don't realize they affect each other until code review.

**How to Catch This Early:**

**Frontend Squad Publishes Decision:**
```
Decision: Migrate from Redux to Zustand

Current: Redux with thunks for async actions
Problem: Boilerplate is heavy, team is slower to iterate

Proposed: Zustand (simpler, smaller bundle, easier to test)

Timeline: 4 weeks (phase migration, route-by-route)

Affected: Data layer (how we fetch from API, how we cache)
```

**Other Squads Have a Comment Period (2-3 days):**

Backend Squad comments:
```
Question: How will this affect our data model API?

We're currently working on a new endpoint that returns nested data 
(user → posts → comments). Redux's normalization was helpful for this.

Will Zustand make it harder to work with nested data? 
Should we flatten the API response instead?

This might affect our API design (timing is tight).
```

Frontend Squad responds:
```
Great catch. Yes, Zustand doesn't have built-in normalization like Redux.

Two options:
1. Flatten the API response (Backend returns flat list of comments + post references)
2. We handle normalization in the client (same as we do now, but Zustand-compatible)

Option 1 is cleaner. Can we sync on this? (30-min call?)
```

Backend Squad agrees and adjusts their API design.

**Result:** The dependency was surfaced **before code was written**, saving a painful refactor later.

---

### Scenario 4: Coordinating a Major Release

**Situation:** You're shipping a big new feature (e.g., real-time collaboration) that requires changes across all subsquads. How do you coordinate?

#### Release Proposal (Week 1-2)

**Lead Squad Publishes:**
```
Decision: Real-Time Collaboration Feature (Q2 Release)

Vision: Users can see each other's edits in real-time (like Google Docs).

Architecture:
- WebSocket server (Platform Squad)
- Real-time sync algorithm (Backend Squad)
- Live cursor/selection rendering (Frontend Squad)

Timeline: 8 weeks

Dependencies:
- Platform: WebSocket infrastructure (weeks 1-2)
- Backend: Data model & sync algorithm (weeks 2-4)
- Frontend: UI (weeks 3-6)
- Integration & testing (weeks 6-8)

Success criteria:
- Sub-500ms latency for cursor updates
- Handles 1000+ concurrent editors
- Graceful degradation if WebSocket fails (fallback to polling)

Next steps:
- Each squad proposes detailed plan (architecture, data model changes, etc.)
- Subsquads surface dependencies
- We identify risks early
```

#### Subsquad Planning (Week 2-3)

**Backend Squad Proposes Data Model:**
```
Real-Time Collaboration: Data Model Design

Current: Single-version documents (linear edit history)
New: Operational transformation or CRDT for merge semantics

Proposed: CRDT (easier to implement, well-understood)

Data model:
- document { id, content (CRDT), users_editing[] }
- edit { user_id, timestamp, op_type, value }

All edits are published as events. Frontend consumes events in real-time.

Questions for Frontend:
- How quickly do you need to render edits? (100ms ok?)
- How do you handle conflicts? (last-write-wins? CRDT? both?)

Questions for Platform:
- What's the throughput we need? (concurrent messages/sec?)
- How do we persist events? (append-only log? relational db?)
```

**Frontend Squad Responds:**
```
Backend:

Good questions.

1. Render latency: 100ms is fine. We'll batch updates every 50ms for performance.
2. Conflicts: CRDT is great. We'll render the merged result.

Our concern: How do we handle offline edits?
If a user edits while disconnected, how do we merge those edits when they reconnect?

This might affect your CRDT design (you need to handle offline client state).
```

**Platform Squad Plans Infrastructure:**
```
Real-Time Collaboration: Infrastructure Plan

WebSocket server:
- Node.js + ws library (or similar)
- Runs alongside API
- Handles subscriptions + events

Event publishing:
- Events stored in a distributed log (Redis Stream or Kafka)
- All clients subscribe and consume in order
- Guarantees ordering + replay

Questions:
- How many messages per second? (estimate needed for capacity planning)
- How long do we keep event history? (1 hour? 1 day? forever?)
- Do we need event compression? (big edits could be large messages)
```

#### Dependency Resolution (Week 3)

Teams surface the hidden dependencies:
- **Frontend offline handling** affects Backend's CRDT design (need to timestamp ops)
- **Event ordering guarantees** needed for Frontend to maintain consistency
- **Message size limits** need to be defined (prevents runaway memory)

These get published to `.squad/decisions/` and resolved before coding starts.

#### Execution with Checkpoints (Weeks 4-12)

```
Week 4: Platform builds WebSocket server, Backend starts CRDT implementation
Week 5: Platform WebSocket goes live in staging, Backend publishes event schema
Week 6: Frontend starts building UI with mocked events, tests against Backend staging
Week 7: All three squads integrate in staging, identify performance issues
Week 8: Performance optimization (reduce message size, batch updates)
Week 9-10: Production rollout (gradual, with feature flags)
Week 11: Monitoring & incident response
Week 12: Post-mortem, publish learnings to .squad/decisions/
```

Each week:
- Subsquads publish status updates
- Any blockers are escalated immediately
- Cross-squad tests run continuously (catch integration issues early)
- Decisions about tradeoffs are published (not made in ad-hoc meetings)

---

## Tools & Patterns

### 1. API Contract Documents

**Pattern:** Every API between subsquads is documented in a decision with:
- Endpoint/method signature
- Request/response schema (with examples)
- Error codes and error messages
- Rate limits and latency requirements
- Version/deprecation strategy

**Example:**
```markdown
# API Contract: GET /api/users/{id}

**Owner:** Backend Squad
**Consumers:** Frontend Squad, Mobile Squad
**Version:** 2.0 (v1 deprecated 2026-03-15)

## Endpoint

GET /api/users/{id}

## Parameters

| Name | Type | Required | Description |
|------|------|----------|-------------|
| id   | string | yes | User ID |
| fields | string | no | Comma-separated fields to return. Default: all |

## Response

200 OK
```json
{
  "id": "user-123",
  "name": "Alice",
  "email": "alice@example.com",
  "bio": "Engineer",
  "avatar_url": "https://example.com/avatar.jpg"
}
```

400 Bad Request
```json
{
  "error": "INVALID_ID",
  "message": "User ID must be a valid UUID"
}
```

404 Not Found
```json
{
  "error": "USER_NOT_FOUND",
  "message": "No user with ID user-xyz"
}
```

## Rate Limits

- **Limit:** 100 requests/minute per IP
- **Headers:** X-RateLimit-Limit, X-RateLimit-Remaining, X-RateLimit-Reset

## Performance

- **Target latency:** < 100ms p99
- **Timeout:** 5 seconds

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 2.0 | 2026-03-01 | Added bio field, moved email to optional |
| 1.0 | 2025-12-01 | Initial release |
```

**Benefits:**
- No surprises at integration time (contract is clear upfront)
- Consumers can mock the API before it exists
- Breaking changes are explicit and documented
- Facilitates parallel development

---

### 2. Dependency Mapping

**Pattern:** Maintain a decision document listing all inter-squad dependencies, their status, and risks.

**Example:**
```markdown
# Cross-Squad Dependencies (Q1 2026)

| Frontend → Backend | Status | Risk | Owner |
|-------------------|--------|------|-------|
| User Profile endpoint | Design stage | Low (Backend estimates 1 week) | @backend-lead |
| Real-time sync events | Blocked (CRDT design pending) | Medium (affects Frontend architecture) | @backend-lead |
| Search API pagination | Designed | Low (stable for 2 weeks) | @frontend-lead |

| Backend → Platform | Status | Risk | Owner |
|------------------|--------|------|-------|
| Redis Cluster upgrade | In progress | High (might affect latency) | @platform-lead |
| Event log infrastructure | Design stage | High (critical path for real-time feature) | @platform-lead |
```

**Why maintain this:**
- Gives you a bird's-eye view of what's blocking whom
- Highlights high-risk dependencies (focus extra attention here)
- Makes it easy to replan if something slips
- Helps new team members understand the dependency graph

---

### 3. Subsquad Review Checklist

**Pattern:** When integrating across subsquads, have a clear checklist of what needs review.

**Example:**
```
Backend Squad Review Checklist for Frontend Integration

Before integrating Frontend's user profile page, Backend squad must verify:

- [ ] API endpoint is implemented and tested against spec
- [ ] Mocked data in Frontend matches production API response shape
- [ ] Error cases are handled (404 user not found, 401 unauthorized)
- [ ] Performance: latency < 100ms p99 under staging load
- [ ] Rate limiting is working (test with backend's rate limiting tool)
- [ ] Authentication: endpoint requires valid JWT/session
- [ ] Field validation: endpoint rejects malformed IDs
- [ ] Documentation is up to date (API docs, changelog updated)

Reviewer: @backend-lead
Deadline: Friday EOD
```

---

### 4. Conflict Resolution Protocol

**Pattern:** When two subsquads disagree (e.g., "Should API be REST or GraphQL?"), follow a structured process to resolve quickly.

**Steps:**

1. **Disagreement is surfaced in a decision comment** (not in Slack, not in a meeting)
   - Subsquad A explains their position and why it matters
   - Subsquad B explains their position and constraints

2. **Each subsquad proposes 1-2 concrete compromises** (within 24 hours)
   - "We could do REST now, migrate to GraphQL later"
   - "We could support both with a compatibility layer"

3. **If consensus, great.** If not, escalate to Lead Squad.

4. **Lead Squad decides within 48 hours**, with rationale published to `.squad/decisions/`
   - Decision includes what was sacrificed and why
   - Losing subsquad agrees to support it

**Benefits:**
- Disagreements are resolved in writing (no heated meetings)
- Decisions are documented (future context available)
- Escalation is rare (most things can be compromised)
- Losing subsquad can see the reasoning and often becomes buy-in champion

---

## Prompts for This Scenario

See [Sample Prompts: Subsquad Coordination](../sample-prompts.md#subsquad-coordination-prompts).

---

## Common Pitfalls

### Pitfall 1: Analysis Paralysis

**Problem:** Subsquads get stuck trying to perfect decisions before shipping. "We need to design the entire API before anyone writes code."

**Solution:** Set decision deadlines. "2 days to agree on API shape. After that, we proceed with what we have. We can change it later if needed."

Decision timelines:
- **Quick decisions** (build tooling, internal implementation): 24 hours
- **API contracts**: 3-5 days (affects multiple squads)
- **Architecture**: 1 week (this is the slow one)

---

### Pitfall 2: Cargo Cult Decisions

**Problem:** Subsquads copy decisions from other projects without understanding the context. "Let's use GraphQL because everyone else is."

**Solution:** Every decision must have a "Why?" section explaining the constraint or goal. If a subsquad can't explain why a decision matters to their squad, they don't copy it.

---

### Pitfall 3: Communication Breakdown

**Problem:** Subsquads stop talking to each other. Frontend doesn't know Backend is refactoring the data model, so Frontend's changes collide with Backend's changes.

**Solution:** 
- Publish all decisions (even internal ones) to `.squad/decisions/`
- Have a weekly 30-minute sync where subsquads report what they're working on
- Pair programming across squads when there's high interdependency

---

### Pitfall 4: No Clear Owner for Cross-Cutting Concerns

**Problem:** A bug affects both Frontend and Backend. Neither squad owns it. It doesn't get fixed.

**Solution:** In the RACI matrix, make sure every area has a clear owner (who is Accountable?). If it's murky, designate a "cross-squad owner" (usually Lead Squad).

---

## Metrics to Track

| Metric | What It Tells You |
|--------|-------------------|
| Avg. decision time | Coordination overhead |
| % of decisions with multi-squad input | Interdependency density |
| # of cross-squad bugs | Integration quality |
| Time from decision to implementation | Process friction |
| # of decision reversals | Decision quality |
| Subsquad autonomy score | How well squads are isolated |

---

## References

- [Security & Operations Disclaimer](../../README.md#security--operations-disclaimer) — Important risks when squads coordinate.
- [Sample Prompts: Subsquad Coordination](../sample-prompts.md#subsquad-coordination-prompts) — Practical prompts for this scenario.
- [App Modernization Scenario](./app-modernization.md) — Large-scale project that requires tight subsquad coordination.
