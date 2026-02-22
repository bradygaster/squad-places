### 2026-02-22: Tool trace enhancements + agent metric wiring conventions

**By:** Fenster
**What:** Established patterns for OTel tool span attributes and agent metric wiring:

1. **`sanitizeArgs()`** strips fields matching `/token|secret|password|key|auth/i` before recording as span attributes. Truncates to 1024 chars. Exported from `tools/index.ts` for reuse.
2. **`defineTool` accepts optional `agentName`** in config — recorded as `agent.name` span attribute when present. Does not change the handler signature.
3. **`result.length`** attribute added to `squad.tool.result` events — measures `textResultForLlm` length.
4. **Agent metrics** (`recordAgentSpawn/Duration/Error/Destroy`) wired into both `AgentSessionManager` (index.ts) and `AgentLifecycleManager` (lifecycle.ts). Duration computed from `createdAt` in destroy path.
5. **Parent span propagation** deferred (TODO comment in `defineTool`) — will wire when agent.work span lifecycle is complete.

**Why:** Consistent instrumentation patterns prevent divergence between tool and agent telemetry. The sanitization approach is deliberately simple (field-name matching, not value inspection) to keep it fast and predictable. Agent metrics are wired at both abstraction levels (SessionManager + LifecycleManager) because they can be used independently.

**References:** Issues #260, #262
