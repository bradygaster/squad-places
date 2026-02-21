# Migrating to Squad v0.6.0: Step by Step

> **⚠️ INTERNAL ONLY — DO NOT PUBLISH**

**Milestone:** M6 · **Issue:** #54 (M6-16)
**Date:** Sprint 6

---

## Overview

v0.6.0 replaces the markdown-driven `.ai-team/` directory with typed configuration in `.squad/` and `squad.config.ts`. This guide walks through the migration. If you're not ready to migrate, the **legacy fallback** keeps your existing setup running — no changes required.

## Step 1: Assess Your Current Setup

A beta Squad project typically has:

```
.ai-team/
  team.md
  routing.md
  agents/
    agent-name/charter.md
  decisions/
    inbox/
    ...
```

The migration converts `team.md` → `TeamConfig`, `routing.md` → `RoutingConfig`, and each `charter.md` → `AgentConfig` inside a unified `squad.config.ts`.

## Step 2: Run the Automated Migration

`migrateMarkdownToConfig()` reads your `.ai-team/` directory and produces a typed `SquadConfig`:

```typescript
import { migrateMarkdownToConfig } from '@bradygaster/squad';

const config = await migrateMarkdownToConfig('.ai-team');
// Returns a fully typed SquadConfig object
```

Under the hood, this calls `parseTeamMarkdown()` to extract team metadata, `parseCharterMarkdown()` for each agent, and `parseRoutingMarkdown()` for routing rules. The result is passed through `generateConfigFromParsed()` to produce the final config.

## Step 3: Write the Config File

Use `defineConfig()` to create your `squad.config.ts`:

```typescript
import { defineConfig } from '@bradygaster/squad';

export default defineConfig({
  version: '0.6.0',
  team: { name: 'My Team', root: '.squad' },
  agents: [
    { name: 'alice', role: 'developer', model: 'sonnet' },
    { name: 'bob', role: 'reviewer', model: 'haiku' },
  ],
  routing: {
    workTypes: [{ pattern: 'feat/*', agents: ['alice'] }],
    issueLabels: [{ label: 'bug', agents: ['alice', 'bob'] }],
  },
});
```

`defineConfig()` merges your partial config with `DEFAULT_CONFIG`, providing sensible defaults for any field you omit.

## Step 4: Migrate Agent Charters

Agent charters move from `.ai-team/agents/<name>/charter.md` to `.squad/agents/<name>/charter.md`. The content format is unchanged — charters are still markdown. What changes is how they're discovered: `LocalAgentSource` reads from `.squad/agents/` and `parseAgentDoc()` extracts structured metadata.

## Step 5: Use the MigrationRegistry for Version Chains

If you're migrating across multiple versions, `MigrationRegistry` chains transforms:

```typescript
import { MigrationRegistry } from '@bradygaster/squad';

const registry = new MigrationRegistry();
// Register migrations for each version step
// Then run the chain from your current version to target
```

Each `Migration` specifies a `from` version, `to` version, and a transform function. The registry finds the shortest path and applies migrations in order.

## Step 6: Verify with detectDrift

After migration, run `detectDrift()` to compare your new typed config against the original markdown. It reports mismatches — missing agents, changed routing rules, divergent model settings — so you can fix them before going live.

## The Safety Net: Legacy Fallback

Not ready to migrate? `LegacyFallback` detects `.ai-team/` directories at runtime and transparently routes through the markdown-based pipeline. Your project works exactly as before. When you're ready, run the migration at your own pace.

The fallback is not deprecated — it's a first-class path. Beta projects that never migrate will continue to work. But new features (skills, marketplace, typed routing) are only available through the typed config path.
