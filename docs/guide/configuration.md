# Configuration Guide

**Issue:** #36 (M2-14)

---

## Overview

Squad v1 uses `squad.config.ts` as the single source of truth for team configuration. This guide covers the config file, `defineConfig()`, routing rules, model selection, agent sources, and validation.

## squad.config.ts

Create a config file at your project root:

```typescript
import { defineConfig } from '@squad/sdk';

export default defineConfig({
  team: {
    name: 'my-squad',
    root: '.squad',
    description: 'Product engineering team',
  },
  agents: {
    backend: {
      model: 'claude-sonnet-4',
      tools: ['route', 'memory', 'decision'],
      status: 'active',
    },
    frontend: {
      model: 'gpt-4.1',
      tools: ['route', 'skill'],
      status: 'active',
    },
  },
});
```

`defineConfig()` merges your partial config with `DEFAULT_CONFIG`, providing type inference and editor autocomplete.

## Routing Configuration

Define work-type and label-based routing rules:

```typescript
export default defineConfig({
  routing: {
    workTypes: [
      { pattern: /\bAPI|backend\b/i, targets: ['backend'], tier: 'standard' },
      { pattern: /\bUI|CSS|React\b/i, targets: ['frontend'], tier: 'standard' },
      { pattern: /\bstatus|help\b/i, targets: [], tier: 'direct' },
    ],
    issueLabels: [
      { labels: ['bug', 'backend'], targets: ['backend'] },
    ],
  },
});
```

`compileRoutingRules()` precompiles patterns into a fast regex-based router. `matchRoute()` evaluates messages; `matchIssueLabels()` handles GitHub issue routing.

## Model Configuration

`ModelRegistry` catalogs 17 models across tiers (`premium`, `standard`, `fast`). Configure preferences and fallback chains:

```typescript
export default defineConfig({
  models: {
    default: 'claude-sonnet-4',
    fallbackChains: {
      premium: ['claude-opus-4', 'gpt-4.1'],
      standard: ['claude-sonnet-4', 'gpt-4.1'],
      fast: ['claude-haiku-3.5', 'gpt-4.1-mini'],
    },
  },
});
```

`resolveModel()` applies 4-layer priority: user override → charter → task auto-select → config default. `getModelInfo()`, `getFallbackChain()`, and `isModelAvailable()` provide runtime model queries.

## Agent Sources

`AgentRegistry` discovers agents from multiple sources:

- **LocalAgentSource** — `.squad/` directory on disk
- **GitHubAgentSource** — Remote GitHub repositories
- **MarketplaceAgentSource** — Squad marketplace

`parseAgentDoc()` parses `.agent.md` into `AgentDocMetadata`. `syncDocToConfig()` merges doc metadata into config; `syncConfigToDoc()` generates markdown from config. `detectDrift()` reports mismatches.

## Validation

`loadConfig()` discovers, loads, and validates configuration. `validateConfig()` runs runtime type guards. Invalid config throws `ConfigurationError` with the exact field and reason:

```
ConfigurationError: agents.backend.model — "gpt-5-turbo" is not in the model catalog.
Available models: claude-sonnet-4, claude-opus-4, gpt-4.1, ...
```

Use `loadConfigSync()` for synchronous loading in scripts and CLI tools.
