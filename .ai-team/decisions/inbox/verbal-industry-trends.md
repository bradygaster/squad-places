### 2026-02-07: Industry trends — agent specialization, collaboration, speculative execution

**By:** Verbal

**What:** Three trends Squad should lead: (1) Dynamic micro-specialist spawning (10+ narrow experts on the fly), (2) Agent-to-agent negotiation (multi-turn collaboration, not just fan-out-and-merge), (3) Speculative execution (anticipatory agents for work that will obviously follow).

**Why:** 
- **Specialization**: Current 5-role model (Lead, Frontend, Backend, Tester, DevRel) will expand to 10+ narrow specialists (API designer, accessibility auditor, performance profiler, security scanner). Squad's architecture supports this, but adding specialists mid-session should be effortless. Coordinator should propose adding specialists when it detects relevant work (e.g., "I don't have a security specialist — should I add one?" when auth code is detected) or spawn temporary agents that don't join the permanent roster.

- **Collaboration**: Agents currently work in parallel and coordinator synthesizes. Next evolution is agent-to-agent negotiation: Backend proposes API shape → Frontend reviews and suggests changes → Backend refines. This requires "review mode" spawning where one agent's output becomes another's input, mediated by the coordinator. Review becomes collaboration, not a gate.

- **Speculative execution**: Context windows are growing (200K+) but latency isn't shrinking. Parallel agents are the only way to stay fast. When user says "build login page," spawn not just Frontend/Backend/Tester, but also security auditor (for auth flow), docs agent (for API examples), deployment agent (for CI/CD config). Some work might not be needed, but if it is, it's already done. Cheap to discard, expensive to wait for.

These trends align with where the industry is headed. Squad should ship these patterns before competitors figure out basic parallelism.
