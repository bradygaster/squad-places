### Per-command --help/-h: intercept-before-dispatch pattern
**By:** Fenster (Core Dev)
**Date:** 2025-07-14
**PR:** #533
**Closes:** #511, #512

**What:** All CLI subcommands now support `--help` and `-h` flags. Help is intercepted in a single block *before* the command routing switch, so destructive commands (like `squad init`) never execute when help is requested.

**Why:** Brady reported that `squad init --help` actually ran init instead of showing help. This is a safety issue — users expect `--help` to be non-destructive. The intercept-before-dispatch pattern guarantees no side effects.

**Convention:** Any new CLI command added to the router MUST have a corresponding entry in the `getCommandHelp()` map in `cli-entry.ts`. Help text should include: usage line, 1-2 sentence description, options, and at least 2 examples.

**Source:** Draft help text from `.squad/agents/fenster/cli-command-inventory.md`.
