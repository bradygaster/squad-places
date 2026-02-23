# Installation & Distribution Guide

**Issue:** #38 (M4-13)

---

## Overview

Squad supports multiple installation methods and provides tools for building, packaging, and distributing your SDK project through CI/CD pipelines.

## npm Installation

Install Squad SDK as a dependency:

```bash
# Local project dependency
npm install @squad/sdk

# Global CLI
npm install -g @squad/sdk

# One-shot with npx
npx @squad/sdk init my-squad
```

`detectInstallMethod()` identifies the current install method (`npx-github`, `npm-global`, `npm-local`, or `unknown`).

## GitHub Releases

For binary distribution via GitHub Releases:

```typescript
import { generateInstallScript, validateGitHubRelease } from '@squad/sdk';

const script = generateInstallScript({
  owner: 'my-org',
  repo: 'my-squad',
  binary: 'squad-cli',
});
// Generates a curl-based install script

const validation = await validateGitHubRelease(config);
// Checks that the release has expected assets
```

## Project Initialization

`initSquad()` scaffolds a new Squad project with typed config, agent directories, and skill templates:

```typescript
import { initSquad } from '@squad/sdk';

await initSquad({
  name: 'my-squad',
  agents: ['backend', 'frontend', 'devops'],
  format: 'typescript',
});
```

For in-Copilot installation, `installInCopilot()` detects the environment (`cli`, `vscode`, `web`) via `detectCopilotEnvironment()` and generates appropriate setup instructions.

## Upgrade Path

`checkForUpdate()` queries the registry for new versions. `performUpgrade()` applies updates in-place:

```typescript
import { checkForUpdate, performUpgrade } from '@squad/sdk';

const update = await checkForUpdate({ channel: 'stable' });
if (update) {
  const result = await performUpgrade({ dryRun: false });
  console.log(`Upgraded ${result.fromVersion} → ${result.toVersion}`);
}
```

Release channels: `stable`, `preview`, `insider`. `SDKUpgradeResult` includes migration steps applied.

## CI/CD Pipeline Setup

Generate GitHub Actions workflows from typed config:

```typescript
import { generatePipelineYaml } from '@squad/sdk';

const yaml = generatePipelineYaml({
  steps: [
    { name: 'Install', command: 'npm ci' },
    { name: 'Build', command: 'npm run build' },
    { name: 'Test', command: 'npm test' },
  ],
  triggers: [{ event: 'push', branches: ['main'] }],
  artifacts: [{ name: 'dist', path: './dist', retention: 30 }],
});
```

`validateBundleOutput()` checks build artifacts. `generatePackageJson()` produces publish-ready `package.json`. `validatePackageJson()` ensures npm publish readiness.

## Build Configuration

`createBundleConfig()` generates esbuild configuration with Squad-aware defaults:

```typescript
import { createBundleConfig, getBundleTargets } from '@squad/sdk';

const config = createBundleConfig({
  entryPoints: getBundleTargets(),
  outDir: './dist',
  format: 'esm',
  minify: true,
  sourcemap: true,
});
```

Supports ESM and CJS output formats with external dependency handling.
