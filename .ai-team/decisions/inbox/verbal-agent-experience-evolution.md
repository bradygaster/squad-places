### 2026-02-07: Agent experience evolution — three strategic directions

**By:** Verbal

**What:** Identified three areas where Squad's agent design should evolve to stay ahead of the industry: (1) Role-specific spawn prompts with adaptive context loading, (2) Reviewer protocol with guidance and grace periods, (3) Proactive coordinator chaining and conflict resolution.

**Why:** Current spawn template is uniform across all agents. This works functionally but doesn't match how specialists actually work — Leads need trade-off context, Testers need edge case catalogs, etc. Adaptive context loading (tagging decisions by domain, injecting only relevant history) prevents agents from parsing noise. Proactive memory injection (reminding agents what they decided last week) prevents rediscovery loops.

Reviewer protocol is currently binary (accept/reject). Adding rejection with guidance + grace periods for minor fixes makes reviews collaborative handoffs, not just gates.

Coordinator launches agents in parallel (good) but waits passively after completion (missed opportunity). Chaining follow-up work automatically (e.g., Frontend finishes → spawn Tester for integration tests without waiting for user) and catching decision conflicts before the user sees them (e.g., Backend chose MongoDB, Frontend assumed Postgres → spawn Keaton to arbitrate) makes Squad feel predictive, not reactive.

These changes align with Squad's mission: beat the industry to what customers need next. The industry will figure out parallelism in 6 months. We're already there. Now we make it magical.
