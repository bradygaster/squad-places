# M2: Configuring Your Squad in v1

> **⚠️ INTERNAL ONLY — DO NOT PUBLISH**

**Milestone:** M2 · **Issue:** #48
**Date:** Sprint 2

---

## Why Typed Config Matters

Beta Squad's configuration lived in markdown: `team.md`, `routing.md`, agent docs scattered across `.ai-team/`. It was readable but fragile. A typo in a model name silently fell back to defaults. A malformed routing table broke at runtime with no useful error. Editors couldn't autocomplete, and refactoring was grep-and-pray.

M2 introduced `squad.config.ts` — a single TypeScript file that replaces the entire `.ai-team/` directory as the source of truth.

## defineConfig()

The entry point is `defineConfig()`, a Vite-style helper that merges your partial config with `DEFAULT_CONFIG`:

```typescript
import { defineConfig } from '@squad/sdk';

export default defineConfig({
  team: { name: 'acme-squad', root: '.squad' },
  agents: {
    backend: { model: 'claude-sonnet-4', tools: ['route', 'memory'] },
    frontend: { model: 'gpt-4.1', tools: ['route', 'skill'] },
  },
  routing: {
    workTypes: [
      { pattern: /\bAPI\b/i, targets: ['backend'] },
      { pattern: /\bUI|CSS\b/i, targets: ['frontend'] },
    ],
  },
});
```

Full TypeScript inference, editor autocomplete, and compile-time checking — all for free.

## Validation

`validateConfig()` runs runtime type guards on the loaded config. `ConfigurationError` reports exactly which field failed and why. `loadConfig()` discovers the config file by walking up the directory tree, loads it, validates it, and returns a typed `SquadConfig` — or throws with actionable messages.

## Migration Path

For teams on beta, `migrateMarkdownToConfig()` converts `.ai-team/` files into a typed `SquadConfig`. `parseTeamMarkdown()` extracts team structure, `parseRoutingMarkdown()` converts routing tables, and `generateConfigFromParsed()` assembles the result. `MigrationRegistry` handles versioned migrations (v0.4 → v0.5 → v0.6) with composable transform functions.

## Result

M2 made configuration a first-class, typed, validated, auto-completable layer — eliminating an entire class of runtime errors.
