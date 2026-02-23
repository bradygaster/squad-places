import { expect } from 'vitest';
import { existsSync } from 'node:fs';
import { join } from 'node:path';
import { TerminalHarness } from '../harness.js';
import { StepDefinitions, registerStep } from '../support/runner.js';

/**
 * CLI step definitions for Gherkin acceptance tests.
 */

export function registerCLISteps(registry: StepDefinitions): void {
  // Given steps
  registerStep(
    'Given',
    /the current directory has a "(.+)" directory/,
    async (stepText, context) => {
      const match = stepText.match(/the current directory has a "(.+)" directory/);
      if (!match) throw new Error('Pattern match failed');
      
      const dirName = match[1];
      const dirPath = join(process.cwd(), dirName);
      
      if (!existsSync(dirPath)) {
        throw new Error(`Directory ${dirPath} does not exist`);
      }
      
      context.squadDirExists = true;
    },
    registry
  );

  // When steps
  registerStep(
    'When',
    /I run "(.+)"/,
    async (stepText, context) => {
      const match = stepText.match(/I run "(.+)"/);
      if (!match) throw new Error('Pattern match failed');
      
      const command = match[1];
      const args = command.replace(/^squad\s*/, '').split(/\s+/).filter(Boolean);
      
      const harness = await TerminalHarness.spawnWithArgs(args);
      
      // Wait for exit (most CLI commands exit immediately)
      try {
        await harness.waitForExit(5000);
      } catch {
        // Timeout is okay, we'll check output anyway
      }
      
      context.harness = harness;
      context.output = harness.getOutput();
      context.exitCode = harness.getExitCode();
    },
    registry
  );

  // Then steps
  registerStep(
    'Then',
    /the output contains "(.+)"/,
    async (stepText, context) => {
      const match = stepText.match(/the output contains "(.+)"/);
      if (!match) throw new Error('Pattern match failed');
      
      const expectedText = match[1];
      const output = context.output as string;
      
      expect(output).toContain(expectedText);
    },
    registry
  );

  registerStep(
    'Then',
    /the output matches pattern "(.+)"/,
    async (stepText, context) => {
      const match = stepText.match(/the output matches pattern "(.+)"/);
      if (!match) throw new Error('Pattern match failed');
      
      const pattern = match[1];
      const output = context.output as string;
      
      expect(output).toMatch(new RegExp(pattern));
    },
    registry
  );

  registerStep(
    'Then',
    /the exit code is (\d+)/,
    async (stepText, context) => {
      const match = stepText.match(/the exit code is (\d+)/);
      if (!match) throw new Error('Pattern match failed');
      
      const expectedCode = parseInt(match[1], 10);
      const exitCode = context.exitCode as number | null;
      
      expect(exitCode).toBe(expectedCode);
    },
    registry
  );
}
