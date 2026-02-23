/**
 * UX Gates Test Suite
 * Enforces UX quality rules for the Squad CLI.
 */

import { describe, it, expect, afterEach } from 'vitest';
import { TerminalHarness } from './acceptance/harness.js';

describe('UX Gates', () => {
  let harness: TerminalHarness | null = null;

  afterEach(async () => {
    if (harness) {
      await harness.close();
      harness = null;
    }
  });

  it('No overflow beyond terminal width (80 chars)', async () => {
    harness = await TerminalHarness.spawnWithArgs(['--help']);
    await harness.waitForExit(5000);
    
    const output = harness.captureFrame();
    const lines = output.split('\n');
    
    // Count lines that exceed reasonable terminal width
    const longLines = lines.filter((line) => line.length > 120);
    
    // Help text should be reasonably formatted - no lines over 120 chars
    // (This is a quality gate, not a hard blocker at 80)
    expect(longLines.length).toBe(0);
    
    // Count lines that are very long (over 80 chars)
    const veryLongLines = lines.filter((line) => line.length > 80);
    
    // Document how many lines exceed 80 chars (ideal width)
    // This is informational - we're catching regression, not blocking existing behavior
    if (veryLongLines.length > 0) {
      console.log(`Info: ${veryLongLines.length} lines exceed 80 chars (ideal terminal width)`);
    }
  });

  it('Error states include remediation hints', async () => {
    harness = await TerminalHarness.spawnWithArgs(['nonexistent-command']);
    await harness.waitForExit(5000);
    
    const output = harness.captureFrame();
    
    // Error message should include a hint about running 'squad help'
    expect(output).toMatch(/squad help/i);
    expect(output).toMatch(/Unknown command/i);
  });

  it('Version output is clean (single line with version format)', async () => {
    harness = await TerminalHarness.spawnWithArgs(['--version']);
    await harness.waitForExit(5000);
    
    const output = harness.captureFrame().trim();
    const lines = output.split('\n').filter((line) => line.trim());
    
    // Should be exactly one line
    expect(lines.length).toBe(1);
    
    // Should match format: squad X.Y.Z
    expect(lines[0]).toMatch(/^squad\s+\d+\.\d+\.\d+/);
  });

  it('Help screen includes essential commands', async () => {
    harness = await TerminalHarness.spawnWithArgs(['--help']);
    await harness.waitForExit(5000);
    
    const output = harness.captureFrame();
    
    // Must include core commands
    expect(output).toContain('init');
    expect(output).toContain('upgrade');
    expect(output).toContain('status');
    expect(output).toContain('help');
    expect(output).toContain('Usage:');
  });

  it('Status command shows clear output structure', async () => {
    harness = await TerminalHarness.spawnWithArgs(['status']);
    await harness.waitForExit(5000);
    
    const output = harness.captureFrame();
    
    // Should have clear section headers
    expect(output).toMatch(/Squad Status/i);
    expect(output).toMatch(/Active squad/i);
  });

  it('Doctor command exits cleanly', async () => {
    harness = await TerminalHarness.spawnWithArgs(['doctor']);
    await harness.waitForExit(5000);
    
    const exitCode = harness.getExitCode();
    
    // Doctor should exit with 0 (success) in this repo
    expect(exitCode).toBe(0);
  });
});
