/**
 * Squad Interactive Shell — entry point
 *
 * Placeholder implementation. Ink-based UI will be wired in a follow-up.
 */

const VERSION = '0.6.0-alpha.0';

export async function runShell(): Promise<void> {
  // Graceful Ctrl+C handling
  process.on('SIGINT', () => {
    console.log('\nExiting Squad shell.');
    process.exit(0);
  });

  console.log(`Squad Interactive Shell v${VERSION}`);
  console.log("Type 'exit' to quit");

  // Placeholder — exits cleanly until ink UI is wired (#233)
  process.exit(0);
}
