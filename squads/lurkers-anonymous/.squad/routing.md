# Work Routing

How to decide who handles what.

## Routing Table

| Work Type | Route To | Examples |
|-----------|----------|----------|
| Caching behavior | CacheBuster | Cache hit/miss, TTL/expiry, ETag & conditional requests (304), staleness |
| Search | SearchSpecialist | Query params, filters, relevance/ranking observation, geo/text search edge cases |
| Pagination | PaginationTester | offset/limit, page-number, cursors, boundaries, completeness, page stability |
| Scope & priorities | ReadLead | What to test next, trade-offs, read-only contract enforcement |
| Code review | ReadLead | Review polling scripts, enforce reads-only (no POST/PUT/PATCH/DELETE) |
| Session logging | Scribe | Automatic — never needs routing |
| RAI review | Rai | Content safety, credential detection, ethical review |
| Verification / Devil's Advocate | Fact Checker | Verify claims, challenge assumptions, pre-mortem |

## Read-Only Contract

**Hard rule for this team:** every script and request is read-only. GET/HEAD (and conditional read headers) only — never POST, PUT, PATCH, or DELETE against the Places API. ReadLead rejects any change that mutates the API during review.

## Issue Routing

| Label | Action | Who |
|-------|--------|-----|
| `squad` | Triage: analyze issue, assign `squad:{member}` label | ReadLead |
| `squad:{name}` | Pick up issue and complete the work | Named member |

### How Issue Assignment Works

1. When a GitHub issue gets the `squad` label, **ReadLead** triages it — analyzing content, assigning the right `squad:{member}` label, and commenting with triage notes.
2. When a `squad:{member}` label is applied, that member picks up the issue in their next session.
3. Members can reassign by removing their label and adding another member's label.
4. The `squad` label is the "inbox" — untriaged issues waiting for ReadLead review.

## Rules

1. **Eager by default** — spawn all agents who could usefully start work, including anticipatory downstream work.
2. **Scribe always runs** after substantial work, always as `mode: "background"`. Never blocks.
3. **Quick facts → coordinator answers directly.** Don't spawn an agent for trivial lookups.
4. **When two agents could handle it**, pick the one whose domain is the primary concern (cache vs. search vs. pagination).
5. **"Team, ..." → fan-out.** Spawn all relevant agents in parallel as `mode: "background"`.
6. **Anticipate downstream work.** Large search result sets hand off from SearchSpecialist to PaginationTester; cached responses loop in CacheBuster.
7. **Issue-labeled work** — when a `squad:{member}` label is applied, route to that member. ReadLead handles all `squad` (base label) triage.
