# Proposal 024: Per-Agent Model Selection

**Status:** Draft — Deferred to Horizon
**Author:** Verbal
**Date:** 2026-02-08

## Problem

Every agent spawn uses the same model — `claude-sonnet-4` via `general-purpose` agent type. This is wrong for at least three cases:

1. **Capability mismatch.** Redfoot (Graphic Designer) needs a vision-capable model to reason about imagery, color systems, and visual composition. Claude Sonnet is a text-first model. Spawning Redfoot on Sonnet is like hiring a painter and handing them a typewriter.

2. **Cost mismatch.** Scribe merges inbox files into `decisions.md` — mechanical file manipulation. Burning premium Sonnet tokens on Scribe is like flying first class to the mailbox. Haiku handles this in a fraction of the cost and latency.

3. **Depth mismatch.** Keaton (Lead) making an architecture decision that propagates across 8 agents and 3 sprints should get Opus-tier reasoning. A quick test scaffold from Hockney doesn't need that depth. One-size-fits-all wastes money on simple tasks and undersells complex ones.

Brady's directive: *"We don't want Redfoot using Claude Sonnet to design imagery."* The model must match the agent's capabilities.

## Design

### 1. Charter-Level Model Field

Add an optional `model` field to the charter template under a new `## Model` section:

```markdown
## Model

- **Preferred:** claude-opus-4.5
- **Rationale:** Vision-capable model required for graphic design work — image analysis, color reasoning, visual composition
```

**Field semantics:**
- `Preferred` — the model the coordinator SHOULD use when spawning this agent. Not a hard constraint — the coordinator can override based on task complexity or user directive.
- `Rationale` — WHY this model. Forces the charter author to justify the choice. Prevents cargo-culting "just use opus for everything."
- If the `## Model` section is omitted entirely, the coordinator falls through to the auto-selection algorithm (Section 5).

**Charter template change** (in `templates/charter.md`):

```markdown
## Model

- **Preferred:** {model or "auto"}
- **Rationale:** {Why this model — capability needs, cost considerations, or "auto" for coordinator selection}
```

Setting `Preferred` to `auto` explicitly opts into auto-selection. Omitting the section entirely has the same effect.

### 2. Model Auto-Selection Algorithm

When no charter model is specified (or `auto` is set), the coordinator applies a deterministic algorithm. Three inputs, in priority order:

**Priority 1 — User override (highest)**
If the user says "use opus for everything" or "use haiku to save costs," that directive overrides all other logic. The coordinator notes the override in the spawn and applies it.

**Priority 2 — Charter preference**
If the charter has a `## Model` section with a specific model (not `auto`), use it.

**Priority 3 — Role-based default (auto-selection)**

Role categories mapped to model tiers:

| Role Category | Default Model | Rationale |
|---|---|---|
| **Lead / Architect** | `claude-sonnet-4` | Deep reasoning for cross-cutting decisions. Upgrade to Opus for architecture proposals. |
| **Core Dev / Backend / Frontend** | `claude-sonnet-4` | Good balance of reasoning and speed for implementation work. |
| **Tester / QA** | `claude-haiku-4.5` | Test generation is structured and pattern-heavy. Speed > depth. |
| **Designer / Visual** | `claude-opus-4.5` | Vision-capable. Required for image reasoning, color analysis, visual composition. |
| **DevRel / Writer** | `claude-sonnet-4` | Prose quality needs good reasoning. Not as deep as architecture. |
| **Scribe / Logger** | `claude-haiku-4.5` | Mechanical file operations. Cheapest model that can follow instructions. |
| **Platform / Infra** | `claude-sonnet-4` | Standard reasoning for platform analysis and feasibility. |
| **Prompt Engineer / AI** | `claude-sonnet-4` | Meta-reasoning about agent design. Sonnet is sufficient; Opus for complex proposals. |

**Priority 4 — Task complexity override**

Even after role-based selection, the coordinator can bump the model tier based on task signals:

| Signal | Effect |
|---|---|
| Task contains "architecture," "design system," "proposal" | Bump to Opus if currently Sonnet |
| Task contains "review," "audit," "security" | Bump to Sonnet if currently Haiku |
| Task contains "quick," "simple," "rename," "typo" | Drop to Haiku if currently Sonnet |
| Task is a reviewer gate (approval/rejection decision) | Bump to Sonnet minimum |
| Multi-file coordination (agent output feeds 3+ other agents) | Bump to Opus |

The coordinator applies AT MOST ONE bump. No cascading upgrades.

### 3. Registry Integration

Add a `model` field to `casting/registry.json` entries:

```json
{
  "agents": {
    "redfoot": {
      "persistent_name": "Redfoot",
      "universe": "The Usual Suspects",
      "created_at": "2026-02-08T17:58:00.000Z",
      "legacy_named": false,
      "status": "active",
      "model": "claude-opus-4.5"
    },
    "hockney": {
      "persistent_name": "Hockney",
      "universe": "The Usual Suspects",
      "created_at": "2026-02-07T23:18:31.762Z",
      "legacy_named": false,
      "status": "active",
      "model": "claude-haiku-4.5"
    }
  }
}
```

**Resolution order when both registry and charter specify a model:**

1. User override (always wins)
2. Charter `## Model` field (agent's own declaration — closer to the work)
3. Registry `model` field (team-level configuration — set during casting)
4. Auto-selection algorithm (fallback)

**Why both locations?**

- **Registry** is the team-level view. The coordinator reads it during routing to quickly see model assignments across all agents without opening every charter. It's also where the `casting` ceremony writes the initial model assignment.
- **Charter** is the agent-level declaration. The agent "knows" what model it needs and why. It's self-documenting — you read Redfoot's charter and immediately understand the model requirement.
- On conflict, charter wins over registry. The agent's self-declared needs (with rationale) are more authoritative than the casting-time default.

### 4. Coordinator Changes

**squad.agent.md spawn template modification:**

The coordinator already reads the charter before spawning (inline charter pattern). The change is: after reading the charter, extract the `## Model` field and pass it to the `task` tool's `model` parameter.

Updated spawn flow:

```
1. Read charter.md → extract ## Model → Preferred value
2. If no charter model → check registry.json → model field
3. If no registry model → run auto-selection algorithm (role + task signals)
4. If user override active → use that instead
5. Spawn with: task(agent_type: "general-purpose", model: "{selected_model}", ...)
```

**Spawn template addition** (new line in the prompt section):

```
- **`model`**: `"{selected_model}"` — selected from charter preference, registry, or auto-selection. Include rationale in coordinator's internal reasoning.
```

**Coordinator instruction addition** (new section in squad.agent.md):

```markdown
### Model Selection

Before spawning an agent, determine the model:

1. Check the user's message for model directives ("use opus," "save costs," "fast mode")
2. Read the agent's charter `## Model` section for a `Preferred` value
3. Check `casting/registry.json` for a `model` field on the agent entry
4. If none specified, apply role-based defaults:
   - Lead/Architect → claude-sonnet-4 (bump to opus for proposals/architecture)
   - Core Dev → claude-sonnet-4
   - Tester/QA → claude-haiku-4.5
   - Designer/Visual → claude-opus-4.5 (vision-capable required)
   - Scribe/Logger → claude-haiku-4.5
   - DevRel/Writer → claude-sonnet-4
5. Apply task complexity override (see Model Auto-Selection table)
6. Pass the selected model to the `task` tool's `model` parameter

Never auto-select a model the platform doesn't support. Valid models:
claude-opus-4.6, claude-opus-4.5, claude-sonnet-4.5, claude-sonnet-4,
claude-haiku-4.5, gpt-5.2, gpt-5.1-codex, gpt-5.1, gpt-5,
gpt-5.1-codex-mini, gpt-5-mini, gemini-3-pro-preview
```

### 5. Cross-Vendor Model Support

The `task` tool supports models beyond Claude. The auto-selection algorithm should be vendor-aware:

| Vendor | Models | Best For |
|---|---|---|
| **Anthropic** | opus-4.6/4.5, sonnet-4.5/4, haiku-4.5 | Default tier. Full range from premium to budget. |
| **OpenAI** | gpt-5.2, gpt-5.1-codex, gpt-5, gpt-5-mini | Cross-vendor diversity. Codex variants for heavy code gen. |
| **Google** | gemini-3-pro-preview | Alternative reasoning. Good for second opinions. |

**Charter model field accepts any valid model string**, not just Claude models. A team could configure:

```markdown
## Model

- **Preferred:** gpt-5.1-codex
- **Rationale:** OpenAI Codex excels at large-scale code generation tasks
```

The coordinator validates the model string against the known valid list before spawning. Unknown models fall through to auto-selection with a warning.

### 6. User-Facing Documentation

**What users see in the roster:**

When the user asks "show my team" or inspects the casting, model assignments should be visible:

```
🎬 Your Squad (The Usual Suspects):

  Keaton    — Lead              [claude-sonnet-4]
  Verbal    — Prompt Engineer   [claude-sonnet-4]
  McManus   — DevRel            [claude-sonnet-4]
  Fenster   — Core Dev          [claude-sonnet-4]
  Hockney   — Tester            [claude-haiku-4.5]
  Redfoot   — Graphic Designer  [claude-opus-4.5]
  Scribe    — Scribe            [claude-haiku-4.5]
```

**How users configure:**

1. **At casting time** — When a new agent is cast, the coordinator assigns a default model based on role. Written to `registry.json`.
2. **In the charter** — Users (or agents) can edit the `## Model` section of any charter to change the preference. Charter overrides registry.
3. **At spawn time** — Users can say "use opus for this task" or "save costs, use haiku" in their message. Overrides everything for that spawn.
4. **Globally** — Users can say "always use sonnet for everything" and the coordinator respects it for the session. Could be persisted to a team-level config in future.

**What users see at spawn time:**

The coordinator should note the model choice in its routing output:

```
Spawning Redfoot (claude-opus-4.5 — vision-capable, per charter preference) ...
Spawning Hockney (claude-haiku-4.5 — fast mode for test generation) ...
Spawning Keaton (claude-opus-4.6 — bumped from sonnet for architecture proposal) ...
```

This makes model selection transparent. Users can see WHY a model was chosen and override if they disagree.

### 7. Delegation Support

Model selection must work for **every spawn path**, not just coordinator → agent. There are three delegation patterns today, and model selection must be consistent across all of them:

**Pattern A: Coordinator → Agent (primary)**
The coordinator reads the charter, resolves the model, and passes it to the `task` tool. This is the flow described in Sections 1-6 above. Works today.

**Pattern B: Agent → Sub-agent (intra-team delegation)**
Some agents spawn other agents directly. For example:
- Lead delegates a sub-task to Backend during a complex architecture implementation
- Reviewer rejects work and spawns a different agent to revise
- Charter template says: *"On rejection, I may require a different agent to revise (not the original author) or request a new specialist be spawned."*

When an agent spawns another agent, it should read the target agent's charter `## Model` field and pass it to the `task` tool's `model` parameter — the same resolution order the coordinator uses. If the spawning agent doesn't have access to the registry or auto-selection table, it falls through to the charter preference or omits the `model` parameter (platform default).

**Charter template addition** for agents that may delegate:

```markdown
When spawning another agent, read their charter.md and extract the `## Model` 
section. Pass the `Preferred` value as the `model` parameter to the `task` tool.
If no `## Model` section exists, omit the `model` parameter (platform default applies).
```

**Pattern C: Ceremony-triggered spawns**
Ceremonies (e.g., review gates) can trigger agent spawns. The coordinator controls these spawns, so Pattern A applies — no special handling needed.

**Key principle:** The charter `## Model` field travels with the agent. Anyone spawning that agent — coordinator, fellow agent, or ceremony — reads the same charter and gets the same model preference. The agent's model needs are self-declared, not caller-determined.

**Delegation-specific auto-selection:**

When an agent (not the coordinator) spawns another agent, it likely doesn't have the full auto-selection algorithm. Simplified rule for agent-to-agent delegation:

1. Read the target agent's charter `## Model` → if present, use it
2. Otherwise, omit the `model` parameter → platform default (Sonnet)

This keeps delegation simple while still respecting charter preferences. The full auto-selection algorithm (role mapping, task complexity bumps) is coordinator-only — agents don't need that complexity.

## Trade-offs

| Decision | Trade-off |
|---|---|
| Charter model > Registry model | Agents can "demand" expensive models. Mitigated by coordinator judgment + user override. |
| Haiku default for Tester/Scribe | Risk of quality issues on complex test scenarios. Mitigated by task complexity bumps. |
| Deterministic algorithm over LLM judgment | Less flexible, but predictable and debuggable. Coordinator can still apply judgment on top. |
| Model strings in charter (not enums) | Future models work without charter format changes. Risk of typos. Mitigated by validation. |
| Simplified delegation model selection | Agent-to-agent spawns only check charter, not full auto-selection. Acceptable — most delegations are to known agents with charter model fields. |

## Alternatives Considered

1. **Model selection purely in registry.json** — Rejected. The charter is the agent's self-description; it should declare its own needs. Registry-only means the agent can't explain WHY it needs a particular model.

2. **LLM-based model selection** — Rejected for v1. Having the coordinator "think about" which model to use adds latency and unpredictability. Deterministic algorithm first; LLM judgment as a v2 refinement.

3. **Per-task model (no agent default)** — Rejected. Agents have inherent capability needs (Redfoot ALWAYS needs vision). Per-task-only would require the coordinator to re-derive this every spawn.

4. **Separate `model-config.json`** — Rejected. Splits configuration across too many files. Registry + charter is sufficient.

## Dependencies

- **Hard dependency: Model auto-selection must ship with or before any charter model field.** Without auto-selection, users must manually configure every agent's model. The auto-selection algorithm makes the feature zero-config by default.
- Coordinator instruction changes (`squad.agent.md`)
- Charter template update (`templates/charter.md`)
- Registry schema update (`casting/registry.json`)
- Forwardability: upgrade migration to add `model` field to existing registries (uses role-based defaults)

## Implementation Phases

**Phase 1 — Coordinator instructions + auto-selection (zero code changes)**
- Add Model Selection section to `squad.agent.md`
- Coordinator uses role-based defaults when spawning
- Pass `model` parameter to `task` tool calls
- No charter or registry changes yet — pure instruction change

**Phase 2 — Charter + registry integration**
- Add `## Model` section to `templates/charter.md`
- Add `model` field to `casting/registry.json` schema
- Coordinator reads charter model before applying auto-selection
- Migration: existing agents get model field based on role

**Phase 3 — User-facing polish**
- Model visibility in roster display
- Model choice logging in coordinator output
- User override persistence (session-level "always use X")
- Cost reporting (optional: show token estimates per model tier)

## Success Criteria

- Redfoot spawns on a vision-capable model (Opus) without any user configuration
- Scribe/Hockney spawn on Haiku by default, cutting cost for mechanical tasks
- User can say "use opus for everything" and it works
- User can say "save costs" and the coordinator drops all agents to the cheapest viable model
- Model choice is visible and explainable at spawn time
- Zero existing behavior breaks — agents without model config get Sonnet (current default)
- Agent-to-agent delegation respects the target agent's charter model preference
- Model selection works identically whether spawn originates from coordinator, fellow agent, or ceremony

## Open Questions

1. Should the coordinator log model selection reasoning to the orchestration log? (Probably yes — useful for debugging cost.)
2. Should model selection be part of the portable squad export? (Yes — it's part of the agent's identity.)
3. Should there be a team-level "budget mode" that caps all agents at Sonnet? (Deferred to v2.)
