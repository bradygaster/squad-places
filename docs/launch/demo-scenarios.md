# Demo Scenarios

> **⚠️ INTERNAL ONLY — DO NOT PUBLISH**

**Issue:** #44 (M6-7)

---

## Overview

Five demo scenarios with step-by-step scripts for showcasing Squad SDK capabilities.

---

## Demo 1: Initialize a New Squad

**Goal:** Show project scaffolding with typed config.

```typescript
import { initSquad } from '@squad/sdk';

// Step 1: Scaffold the project
await initSquad({
  name: 'demo-squad',
  agents: ['backend', 'frontend', 'devops'],
  format: 'typescript',
});

// Step 2: Verify generated files
// - squad.config.ts (typed config with 3 agents)
// - .squad/agents/ (agent directories)
// - .squad/skills/ (skill templates)

// Step 3: Load and validate
import { loadConfig } from '@squad/sdk';
const config = await loadConfig();
console.log(`Team: ${config.team.name}, Agents: ${Object.keys(config.agents).length}`);
// → Team: demo-squad, Agents: 3
```

---

## Demo 2: Cast a Team for a Task

**Goal:** Show dynamic agent assembly with the casting engine.

```typescript
import { CastingEngine } from '@squad/sdk';

const engine = new CastingEngine(config.agents);

// Step 1: Assemble a team for a specific task
const cast = engine.assemble({
  universe: 'usual-suspects',
  task: 'Build a REST API with authentication',
});

// Step 2: Inspect the cast
cast.forEach(member => {
  console.log(`${member.role}: model=${member.model}, tools=[${member.tools}]`);
});
// → backend: model=claude-sonnet-4, tools=[route,memory,decision]
// → devops: model=gpt-4.1, tools=[route,status]

// Step 3: Review casting history
import { CastingHistory } from '@squad/sdk';
const history = new CastingHistory();
console.log(`Total castings: ${history.records.length}`);
```

---

## Demo 3: Route a Message Through the Coordinator

**Goal:** Show compiled routing, response tiers, and policy enforcement.

```typescript
import { Coordinator } from '@squad/sdk';

const coordinator = new Coordinator(config);

// Step 1: Route a simple status query (direct tier)
const decision1 = coordinator.route('What is the team status?');
console.log(`Tier: ${decision1.tier}, Targets: ${decision1.targets}`);
// → Tier: direct, Targets: []

// Step 2: Route a backend task (standard tier)
const decision2 = coordinator.route('Implement the /users API endpoint');
console.log(`Tier: ${decision2.tier}, Targets: ${decision2.targets}`);
// → Tier: standard, Targets: [backend]

// Step 3: Route a complex task (full tier, parallel)
const decision3 = coordinator.route('Refactor the auth system across frontend and backend');
console.log(`Tier: ${decision3.tier}, Parallel: ${decision3.parallel}`);
// → Tier: full, Parallel: true
```

---

## Demo 4: Browse and Install from Marketplace

**Goal:** Show marketplace discovery, security validation, and installation.

```typescript
import { MarketplaceBrowser, validateRemoteAgent } from '@squad/sdk';

const browser = new MarketplaceBrowser(fetcher);

// Step 1: Search the marketplace
const results = await browser.search({
  text: 'security scanner',
  category: 'Security',
  sort: 'downloads',
});
console.log(`Found ${results.entries.length} agents`);

// Step 2: Validate security before install
const report = await validateRemoteAgent(results.entries[0].agent);
console.log(`Risk score: ${report.riskScore}, Blocked: ${report.blocked.length}`);

// Step 3: Install if safe
if (report.blocked.length === 0) {
  const installResult = await browser.install(results.entries[0], '.squad/');
  console.log(`Installed: ${installResult.createdFiles.length} files`);
}
```

---

## Demo 5: Migrate from Beta to v0.6.0

**Goal:** Show automated migration from markdown config to typed config.

```typescript
import {
  migrateMarkdownToConfig,
  parseTeamMarkdown,
  detectDrift,
  loadConfig,
} from '@squad/sdk';

// Step 1: Show existing beta config
import { readFileSync } from 'fs';
const teamMd = readFileSync('.ai-team/team.md', 'utf-8');
console.log('Beta config (markdown):\n', teamMd.slice(0, 200));

// Step 2: Run automated migration
const migrated = await migrateMarkdownToConfig('.ai-team/');
console.log('Generated squad.config.ts with', Object.keys(migrated.agents).length, 'agents');

// Step 3: Validate the migrated config
const config = await loadConfig();
console.log(`Config valid: team=${config.team.name}`);

// Step 4: Check for drift between old docs and new config
const drift = detectDrift(config);
console.log(`Drift entries: ${drift.entries.length}`);
// → Drift entries: 0 (clean migration)
```

---

## Running the Demos

Each demo is self-contained. To run:

```bash
# Ensure SDK is installed
npm install @squad/sdk

# Run any demo script
npx tsx demos/demo-1-init.ts
npx tsx demos/demo-2-cast.ts
npx tsx demos/demo-3-route.ts
npx tsx demos/demo-4-marketplace.ts
npx tsx demos/demo-5-migrate.ts
```
