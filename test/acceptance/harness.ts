import { spawn, ChildProcess } from 'node:child_process';
import { EventEmitter } from 'node:events';

/**
 * Terminal harness for E2E testing the Squad CLI.
 * Uses child_process.spawn with pipes as a pragmatic cross-platform solution.
 * Can be upgraded to node-pty later for true PTY support when CI has build tools.
 */
export class TerminalHarness extends EventEmitter {
  private process: ChildProcess | null = null;
  private outputBuffer = '';
  private exitCode: number | null = null;
  private closed = false;

  private constructor(
    private readonly command: string,
    private readonly args: string[],
    private readonly options: { cols?: number; rows?: number; env?: Record<string, string> }
  ) {
    super();
  }

  /**
   * Spawn the CLI in a child process.
   */
  static async spawn(options?: {
    cols?: number;
    rows?: number;
    env?: Record<string, string>;
  }): Promise<TerminalHarness> {
    const cols = options?.cols ?? 80;
    const rows = options?.rows ?? 24;
    const env = {
      ...process.env,
      COLUMNS: String(cols),
      LINES: String(rows),
      TERM: 'dumb', // Disable interactive features
      NO_COLOR: '1', // Disable colors for cleaner output
      ...options?.env,
    };

    const harness = new TerminalHarness('node', ['packages/squad-cli/dist/cli-entry.js'], {
      cols,
      rows,
      env,
    });

    await harness.start();
    return harness;
  }

  /**
   * Spawn CLI with specific arguments (for non-interactive commands).
   */
  static async spawnWithArgs(
    args: string[],
    options?: { cols?: number; rows?: number; env?: Record<string, string> }
  ): Promise<TerminalHarness> {
    const cols = options?.cols ?? 80;
    const rows = options?.rows ?? 24;
    const env = {
      ...process.env,
      COLUMNS: String(cols),
      LINES: String(rows),
      TERM: 'dumb',
      NO_COLOR: '1',
      ...options?.env,
    };

    const harness = new TerminalHarness(
      'node',
      ['packages/squad-cli/dist/cli-entry.js', ...args],
      { cols, rows, env }
    );

    await harness.start();
    return harness;
  }

  private async start(): Promise<void> {
    this.process = spawn(this.command, this.args, {
      cwd: process.cwd(),
      env: this.options.env,
      stdio: ['pipe', 'pipe', 'pipe'],
      windowsHide: true,
    });

    this.process.stdout?.on('data', (data: Buffer) => {
      const text = data.toString();
      this.outputBuffer += text;
      this.emit('data', text);
    });

    this.process.stderr?.on('data', (data: Buffer) => {
      const text = data.toString();
      this.outputBuffer += text;
      this.emit('data', text);
    });

    this.process.on('exit', (code) => {
      this.exitCode = code ?? 0;
      this.emit('exit', code);
    });

    this.process.on('error', (err) => {
      this.emit('error', err);
    });

    // Give it a moment to start
    await new Promise((resolve) => setTimeout(resolve, 100));
  }

  /**
   * Send keystrokes to the CLI (for interactive mode).
   */
  async sendKeys(input: string): Promise<void> {
    if (!this.process || !this.process.stdin) {
      throw new Error('Process not started or stdin not available');
    }
    this.process.stdin.write(input);
    await new Promise((resolve) => setTimeout(resolve, 50));
  }

  /**
   * Send text + Enter.
   */
  async sendLine(text: string): Promise<void> {
    await this.sendKeys(text + '\n');
  }

  /**
   * Wait for specific text or regex to appear in output, with timeout.
   */
  async waitForText(pattern: string | RegExp, timeoutMs = 10000): Promise<string> {
    const startTime = Date.now();
    const regex = typeof pattern === 'string' ? new RegExp(pattern, 'i') : pattern;

    while (Date.now() - startTime < timeoutMs) {
      if (regex.test(this.outputBuffer)) {
        return this.outputBuffer;
      }

      // If process exited, stop waiting
      if (this.exitCode !== null) {
        break;
      }

      await new Promise((resolve) => setTimeout(resolve, 50));
    }

    throw new Error(
      `Timeout waiting for pattern: ${pattern}\nCurrent output:\n${this.outputBuffer}`
    );
  }

  /**
   * Wait for process to exit with optional timeout.
   */
  async waitForExit(timeoutMs = 10000): Promise<number> {
    const startTime = Date.now();

    while (Date.now() - startTime < timeoutMs) {
      if (this.exitCode !== null) {
        return this.exitCode;
      }
      await new Promise((resolve) => setTimeout(resolve, 50));
    }

    throw new Error('Timeout waiting for process to exit');
  }

  /**
   * Capture current terminal frame (ANSI-stripped).
   */
  captureFrame(): string {
    return this.stripAnsi(this.outputBuffer);
  }

  /**
   * Capture raw frame (with ANSI codes).
   */
  captureRawFrame(): string {
    return this.outputBuffer;
  }

  /**
   * Resize terminal (no-op in pipe mode, but maintained for API compatibility).
   */
  async resize(cols: number, rows: number): Promise<void> {
    // In pipe mode, we can't resize. This is a no-op.
    // If upgraded to node-pty, this would call pty.resize(cols, rows)
  }

  /**
   * Get all accumulated output.
   */
  getOutput(): string {
    return this.outputBuffer;
  }

  /**
   * Get exit code (null if still running).
   */
  getExitCode(): number | null {
    return this.exitCode;
  }

  /**
   * Close/kill the process.
   */
  async close(): Promise<void> {
    if (this.closed) return;
    this.closed = true;

    if (this.process) {
      this.process.kill('SIGTERM');
      
      // Wait for exit or force kill after 2s
      const killTimeout = setTimeout(() => {
        if (this.process) {
          this.process.kill('SIGKILL');
        }
      }, 2000);

      await new Promise<void>((resolve) => {
        if (this.exitCode !== null) {
          clearTimeout(killTimeout);
          resolve();
          return;
        }
        this.once('exit', () => {
          clearTimeout(killTimeout);
          resolve();
        });
      });
    }
  }

  /**
   * Strip ANSI escape codes from text.
   */
  private stripAnsi(text: string): string {
    // eslint-disable-next-line no-control-regex
    return text.replace(/\x1B\[[0-9;]*[a-zA-Z]/g, '');
  }
}
