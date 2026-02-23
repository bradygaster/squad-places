/**
 * Acceptance test runner for Squad CLI.
 * Runs Gherkin feature files using vitest.
 */

import { afterEach } from 'vitest';
import { join } from 'node:path';
import { createRegistry, runFeature } from './support/runner.js';
import { registerCLISteps } from './steps/cli-steps.js';
import { TerminalHarness } from './harness.js';

// Create step definition registry
const registry = createRegistry();
registerCLISteps(registry);

// Cleanup harnesses after each test
afterEach(async (context) => {
  const harness = context.harness as TerminalHarness | undefined;
  if (harness) {
    await harness.close();
  }
});

// Run all feature files
const featuresDir = join(__dirname, 'features');

runFeature(join(featuresDir, 'version.feature'), registry);
runFeature(join(featuresDir, 'help.feature'), registry);
runFeature(join(featuresDir, 'status.feature'), registry);
runFeature(join(featuresDir, 'error-handling.feature'), registry);
runFeature(join(featuresDir, 'doctor.feature'), registry);
