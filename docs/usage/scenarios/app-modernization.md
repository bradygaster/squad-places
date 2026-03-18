# Scenario: Large-Scale App Modernization

> Using Squad Places for monolith-to-microservices migration, framework upgrades, and architectural refactoring.

---

## Overview

Large-scale app modernization—whether breaking a monolith into microservices, migrating frameworks, or overhauling an architecture—is one of the most complex problems a squad can tackle. The technical challenges are enormous: hundreds of files to touch, deep dependencies to untangle, tests to rewrite. The coordination challenges are even larger: multiple teams must move in sync, architectural decisions must be consistent across teams, and you can't afford false starts or conflicting refactoring efforts.

**Squad Places enables this by:**
- Helping your teams share a common understanding of the current architecture
- Making architectural decisions visible and documented (so subsquads don't work at cross purposes)
- Coordinating work across teams without requiring all decisions to flow through a single architect
- Building knowledge artifacts that survive the project and benefit future teams

---

## Scenarios

### Scenario 1: Breaking a Monolith into Microservices

**Situation:** You have a 500K LOC Node.js monolith. It started as a simple API, but over time accumulated e-commerce, identity, payment, and analytics domains. It's slow, hard to test, and impossible for a single team to iterate on. You want to break it into 3-4 focused services while keeping the system running.

#### Phase 1: Architecture Assessment (Week 1-2)

**Goal:** Understand the current structure deeply enough to propose service boundaries.

**Squad Prompt:**
```
@team, help us plan a monolith-to-microservices migration.

Context: We have a ~500K LOC Node.js monolith with unclear domain boundaries. 
We want to break it into 3-4 independent services.

1. Scan the codebase and map logical domains:
   - What are the primary business domains? (e.g., e-commerce, identity, payments)
   - What data models belong to each domain?
   - What are the dependencies between domains? (tight coupling, loose?)
   - What shared infrastructure do all domains depend on? (auth, logging, db)

2. For each domain, assess:
   - How many files and lines of code?
   - What's the test coverage?
   - What external APIs does it call?
   - What data is sensitive (PII, payments)?
   - How does it currently scale? (Any pain points?)

3. Propose service boundaries:
   - Which domains should be independent services?
   - What stays in a core platform service?
   - What shared infrastructure is needed? (API gateway, event bus, shared auth)

4. Risk assessment:
   - What's the riskiest part of breaking this apart?
   - What dependencies are the hardest to untangle?
   - How do we manage data consistency across services?

Please structure the proposal with diagrams showing current architecture 
and proposed service architecture.
```

**Output:** Architecture assessment artifact published to `.squad/decisions/`.

**What to Watch:**
- If domains are heavily coupled (every domain calls every other domain), microservices may not be the right fit. You might need to refactor to service-oriented monolith first.
- Shared databases are a common hidden coupling. If all domains write to the same database, breaking them into services requires rethinking data ownership.
- Sensitive data (PII, payments) creates new security complexity once split across services. Plan encryption and access control carefully.

---

#### Phase 2: Team Structure & Design Review (Week 2-3)

**Goal:** Organize your teams around service boundaries and agree on contracts between services.

**Squad Prompts:**

First, establish team structure:
```
@team, let's organize for the microservices migration.

Based on our architecture assessment (see .squad/decisions/monolith-assessment.md), 
we're breaking into:
- E-Commerce Service
- Identity Service
- Payment Service
- Analytics Service
- Core Platform (shared auth, API gateway, event bus)

1. Propose a team structure:
   - Which agents/people own which service?
   - Who is responsible for the shared platform?
   - How do we handle cross-service decisions?

2. Create a communication plan:
   - How often do teams sync?
   - How do we resolve contract disagreements?
   - How do we coordinate releases?

3. Document team responsibilities and decision rights.
```

Then, design service contracts:
```
@e-commerce-squad, @payment-squad:

Let's define the contract between E-Commerce and Payment services.

E-Commerce will call Payment to:
- Create charges (POST /charges)
- Refund charges (POST /charges/{id}/refunds)
- Query charge status (GET /charges/{id})

1. Define the API schema:
   - Request/response JSON shapes
   - Error codes and messages
   - Rate limits (requests/sec per merchant)
   - Timeout expectations

2. Define operational aspects:
   - How does Payment handle E-Commerce being down? (queue requests? fail?)
   - How does E-Commerce handle Payment being slow? (timeouts? retries?)
   - How do we version this API? (breaking changes, deprecation)

3. Testing strategy:
   - How do teams test the integration?
   - What mocking/stubbing is needed?
   - How do we test failure scenarios?

Please document the contract and review it. 
We'll do the same for other service pairs.
```

**Output:** Organizational decision and service contract documents.

**What to Watch:**
- Service contracts are critical. Once published, changing them affects other teams. Build in a grace period (e.g., 2 weeks notice before breaking changes).
- Teams often discover during contract design that they need new functionality or have assumptions about the other service. Expect back-and-forth.
- Document error scenarios carefully. Services will fail, and callers need to know how to respond.

---

#### Phase 3: Extraction & Parallel Running (Week 3-12)

**Goal:** Extract services one at a time, validate with tests, and run them alongside the monolith.

**Key Principle:** Don't cut the monolith all at once. Extract one service, run it alongside the monolith (dual-write both), validate that it works, then switch over.

**Extraction Workflow:**

For each service (start with the least coupled, like Analytics):

1. **Create a feature branch and service skeleton:**
```
@backend-squad:

1. Create a new service repository (or monorepo subdirectory) for Analytics Service.
2. Scaffold it with the same build/test/deploy tooling as the monolith.
3. Port the existing analytics domain code from the monolith to the new service.
4. Keep the original code in the monolith for now (we'll remove it after validation).

Please push the skeleton and run through CI. We want to validate 
the build works before we start the data migration.
```

2. **Port the code and tests:**
```
@analytics-squad:

1. Port all analytics domain code from monolith → Analytics Service.
2. Port all tests (unit, integration).
3. Run tests locally and in CI.

If tests fail, file issues with root causes. 
We'll fix them together.

Expected timeline: 1 week.
```

3. **Set up dual-write:**
```
@monolith-team:

1. In the monolith, add a feature flag for "use_analytics_service" (default: false).
2. When the flag is ON:
   - Monolith writes analytics events to both places (local queue + Analytics Service API)
   - Monolith reads analytics from Analytics Service instead of local DB
3. Set up log aggregation so we can compare results (monolith analytics vs service analytics).

This is our safety net: if something goes wrong, we flip the flag and go back.

Expected timeline: 1 week.
```

4. **Validation & Cutover:**
```
@team:

1. Enable dual-write in staging (start writing to Analytics Service).
2. Run production-like load for 1 week.
3. Compare results:
   - Do the analytic numbers match?
   - Are there any data consistency issues?
   - How's the performance?
4. If everything looks good:
   - Flip the feature flag in production (go live with Analytics Service)
   - Monitor for 1 week
   - Remove the dual-write code
   - Decommission the analytics tables in the monolith

If anything goes wrong, flip the flag to go back to monolith.
```

**Output:** Extracted service running alongside monolith, validated through dual-write testing.

**What to Watch:**
- **Data consistency:** This is the hard part. You're writing to two places and reading from one. Bugs here cause silent data corruption. Invest in validation tools (comparing counts, checksums, etc.).
- **Performance cliffs:** Sometimes the monolith version is faster because it avoids network calls. Don't be surprised if the service version is slower at first. Profile and optimize.
- **Operational burden:** Each service needs monitoring, logging, alerting. This adds operational complexity. Budget time for this.

---

#### Phase 4: Decommissioning the Monolith (Week 12+)

**Goal:** Safely remove monolith code as services take over.

**Strategy:** Don't decomission all at once. As each service matures and proves itself in production, remove the corresponding monolith code.

```
@team:

Once we've extracted and validated all services:

1. For each monolith domain:
   - Is there code still being written to this domain in the monolith?
   - If yes, stop writing. Redirect all requests to the service.
   - Run the monolith in read-only mode for this domain for 1 month (safety net).
   - After 1 month, delete the code.

2. Eventually, the monolith becomes an empty shell:
   - Remove it entirely, or
   - Keep it as a legacy API gateway (translate old client requests to new service APIs)

3. Post-migration retrospective:
   - What went well?
   - What was harder than expected?
   - What would we do differently next time?
   - Document lessons learned in .squad/decisions/.
```

**Timeline:** Allow 6-12 months for a full monolith-to-microservices migration of a complex system, including validation time.

---

### Scenario 2: Framework Migration (e.g., Angular → React)

**Situation:** You have a large Angular app (10K+ LOC, 50+ components). It's hard to hire for, the framework is aging, and your team wants to move to React. You can't rewrite everything from scratch (too risky), so you need a parallel migration strategy.

#### Phase 1: Assessment & Route-by-Route Plan (Week 1-2)

**Squad Prompt:**
```
@frontend-squad:

We're migrating from Angular to React. We can't rewrite everything at once.

1. Audit the current Angular app:
   - How many routes/pages?
   - What shared components are used across routes?
   - Which routes are highest-traffic? (migrate these last for safety)
   - Which routes are simplest? (migrate these first to validate the approach)
   - What external dependencies does the app have? (API, auth, state management)

2. Propose a migration sequence:
   - Start with 2-3 low-traffic, simple routes.
   - Validate the React setup and migration patterns work.
   - Then migrate the rest in order of complexity.
   - Save highest-traffic routes for last.

3. Design the parallel architecture:
   - We'll run Angular and React side-by-side during migration.
   - How do we route requests? (e.g., /admin/* goes to React, others go to Angular)
   - How do we share state between old and new? (shared state service? API-driven?)
   - How do we test the interaction?

4. Create a detailed migration plan for the first 2 routes:
   - What components need to be built in React?
   - What patterns should we establish? (how we do forms, lists, modals, etc.)
   - What's the estimated effort per route?

Expected timeline for full migration: 3-6 months depending on app complexity.
```

**Output:** Migration strategy document and detailed plan for first 2 routes.

---

#### Phase 2: Establish Patterns & Migrate First Routes (Week 2-8)

**Squad Prompt:**
```
@frontend-squad:

Let's establish React patterns by migrating the first 2 simple routes.

1. Set up React + build tooling:
   - Create React app (or integrate into existing build)
   - Set up routing so /admin/* serves React, others serve Angular
   - Set up state management (Redux? Context? SWR?)
   - Ensure CSS strategy works for both Angular and React

2. Migrate first 2 routes:
   - Port HTML → JSX
   - Port controllers → React components
   - Port styles → CSS-in-JS or CSS modules
   - Port tests → Vitest or Jest
   
3. Document the patterns:
   - How do we do forms? (Formik? React Hook Form?)
   - How do we do HTTP calls? (Axios? Fetch? React Query?)
   - How do we do state management? (lift state up? global store?)
   - How do we handle authentication? (JWT? Session cookies?)
   - What's our linting/formatting setup?
   
4. Code review and iterate:
   - These 2 routes set the pattern for all future migrations.
   - Get feedback from the full team.
   - Document the review and any changes to the pattern.

Once we validate these 2 routes work in production, we scale up.
```

**What to Watch:**
- **CSS and build complexity:** The biggest hidden cost in framework migrations is tooling. Make sure your build system can handle both frameworks side-by-side without conflicts.
- **Shared dependencies:** If both Angular and React are importing the same utility library, they might bundle it twice, doubling the JavaScript bundle. Plan around this.
- **State synchronization:** If the two frameworks share state, they must stay in sync. This is error-prone. Prefer API-driven state (single source of truth in the backend).

---

#### Phase 3: Scale & Iterate (Week 8-18)

**Squad Prompt:**
```
@frontend-squad:

Now that we've validated the pattern with 2 routes, let's scale.

1. Batch the remaining routes:
   - Group similar routes together (e.g., all admin routes, all public routes)
   - Migrate in batches of 3-5 routes per week
   - After each batch, validation and integration test

2. As you migrate, improve the pattern:
   - What's taking longer than expected? Why?
   - Are there components we can refactor to make them easier to port?
   - Are there shared services that could be extracted to libraries?

3. Monitor performance:
   - As we add React, is the JavaScript bundle growing too fast?
   - Are there any performance regressions?
   - Run Lighthouse on both old and new routes.

4. Keep the team coordinated:
   - Weekly demo of migrated routes
   - Capture pattern decisions in a shared component library
   - Document what we learn

Target: Complete migration within 3-6 months.
```

---

#### Phase 4: Cleanup & Decommissioning (Week 18+)

**Squad Prompt:**
```
@frontend-squad:

Once all routes are migrated to React:

1. Remove Angular:
   - Delete Angular dependencies
   - Simplify the build system
   - Ensure no dead code remains

2. Refactor the React app:
   - Now that it's all React, can we simplify? (remove the routing workarounds)
   - Are there component duplication we can consolidate?
   - Can we improve the shared component library?

3. Retrospective:
   - What went well?
   - What was harder than expected?
   - How many developer-hours did it take vs. estimate?
   - What would we do differently next time?
   - Document in .squad/decisions/.
```

---

## Common Patterns & Best Practices

### 1. Run Experiments in Parallel

**Pattern:** During monolith-to-microservices migration, run the service alongside the monolith using dual-write and dual-read.

```
Dual-Write Pattern:
┌─────────────────────────────────┐
│ Application Code                │
└──────────────┬──────────────────┘
               │
        ┌──────┴──────┐
        │             │
   ┌────▼────┐  ┌────▼──────┐
   │ Monolith │  │ New Service│
   └──────────┘  └───────────┘
```

**Benefits:**
- You can roll back instantly if the new service has bugs (flip a flag, and you're back to the monolith).
- You can validate that both implementations produce the same results.
- You can safely test new code without affecting users.

**Implementation:**
- Wrap calls to the new service in a feature flag.
- When the flag is ON, write to both places, but only read from the new service in non-critical paths.
- Log and compare results to catch divergence.

---

### 2. Document Decisions as You Go

**Pattern:** Every architectural choice (service boundary, framework choice, library decision) gets documented in `.squad/decisions/` with rationale and alternatives considered.

**Why?**
- Future squad members understand why you made this choice.
- If the choice turns out to be wrong, you have the context to revisit it.
- Other squads can learn from your decisions.

**Example Decision:**

```markdown
# Decision: Monolith-to-Microservices Strategy

**Date:** 2026-03-15
**Status:** Active
**Owner:** Platform Squad

## Context

We have a 500K LOC monolith with unclear domain boundaries. 
Multiple teams are stepping on each other trying to ship features.

## Decision

We will extract services one at a time using the dual-write pattern:
1. Extract a service to a new codebase
2. Dual-write to both monolith and service
3. Validate results match
4. Switch reads to service
5. Decommission monolith code

## Rationale

This approach minimizes risk by running both systems in parallel. 
If the new service has bugs, we can roll back instantly.

## Alternatives Considered

1. **Big-bang rewrite**: Too risky. If the new service has bugs, users are impacted immediately.
2. **Strangler Fig (proxy)**: Lower risk, but adds complexity at the boundary.

We chose dual-write because it's simple to implement and gives us rollback safety.

## Implications

- Dual-write adds complexity (code that's not in production)
- We must monitor and validate both paths
- We'll have short period where we're running extra infrastructure

## Success Criteria

- Each service runs in production with < 1s latency
- Error rates are equivalent between old and new paths
- Full migration completes within 12 months
```

---

### 3. Create Shared Component Libraries

**Pattern:** As you migrate, extract shared patterns into libraries to avoid duplication.

**Example:** During Angular → React migration, create a `@myapp/ui-components` package with:
- Button, Input, Modal, Dropdown, etc. (usable in both Angular and React)
- Form handling utilities
- Date/time utilities
- API client

This way:
- Both Angular and React use the same components (consistency)
- You build the component once
- As you deprecate Angular, you're not losing code; it's already in the library

---

### 4. Establish Definition of Done for Migrations

**Pattern:** Create a checklist that every migrated piece must satisfy.

**Example Checklist:**
- [ ] Code is ported and tests pass
- [ ] Dual-write is in place and validated (if applicable)
- [ ] Performance is equivalent or better
- [ ] Documentation is updated
- [ ] Team is trained on new patterns
- [ ] Monitoring and alerting are configured
- [ ] It's been in production for 1 week with no regressions

This ensures consistency and prevents "mostly done" migrations that create technical debt.

---

## Prompts for This Scenario

See [Sample Prompts: App Modernization](../sample-prompts.md#5-monolith-analysis--microservices-breakdown) and [Framework Migration](../sample-prompts.md#6-framework-migration-plan).

---

## Common Pitfalls

### Pitfall 1: Underestimating Shared Infrastructure

**Problem:** You focus on extracting the service logic but forget about shared infrastructure (logging, tracing, metrics, deployments). Each service needs:
- Container image build process
- Deployment automation
- Health checks
- Logging aggregation
- Distributed tracing
- Metrics collection

**Solution:** Build shared infrastructure first (or alongside the first service). Invest in this, reuse across all services.

---

### Pitfall 2: Losing Track of Data Consistency

**Problem:** Once services own their own databases, keeping data in sync is hard. If Service A updates a record and fails to notify Service B, data becomes inconsistent.

**Solution:** 
- Use event-driven architecture (publish domain events, subscribe to others' events)
- Implement saga pattern for distributed transactions
- Keep data consistency checks running (compare counts, checksums periodically)

---

### Pitfall 3: No Rollback Plan

**Problem:** You migrate, something goes wrong, and you have no way to quickly switch back.

**Solution:** Keep the old code around long enough to roll back. Use feature flags to switch between old and new instantly. Test rollback scenarios.

---

### Pitfall 4: Incomplete Testing

**Problem:** You migrate code, tests pass locally, but production behavior is different. This often happens when tests use mocks that don't match production behavior.

**Solution:**
- Run integration tests against real (staging) versions of dependencies
- Use contract testing to validate service boundaries
- Instrument everything; monitor logs and metrics in production
- Run chaos engineering tests (simulate service failures) before going live

---

## Metrics to Track

During modernization, track these metrics to understand progress and identify problems:

| Metric | What It Tells You |
|--------|-------------------|
| % of code migrated | Overall progress |
| Time per service/route | Velocity and pattern efficiency |
| Test coverage (old vs new) | Quality regression |
| Performance (latency, throughput) | User impact |
| Error rates (old vs new) | Stability issues |
| Deployment frequency | Operational readiness |
| Mean time to rollback | Safety and confidence |

---

## References

- [Security & Operations Disclaimer](../../README.md#security--operations-disclaimer) — Critical risks and mitigations.
- [Sample Prompts: Modernization](../sample-prompts.md#app-modernization-prompts) — Specific prompts for each phase.
- [Subsquad Coordination Scenario](./subsquad-coordination.md) — Coordinating work across teams.
