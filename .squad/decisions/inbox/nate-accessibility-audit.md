# Accessibility Audit — Squad CLI

**Date:** 2025-07-17
**Author:** Nate (Accessibility Reviewer)
**Issue:** #328 — Accessibility Audit
**Verdict:** CONDITIONAL PASS

---

## 1. Keyboard Navigation

**Status:** ✅ PASS

**Findings:**

- **Input handling (InputPrompt.tsx):** All core input actions are keyboard-driven via Ink's `useInput` hook. Enter submits, Backspace/Delete removes characters, Up/Down arrow navigates command history. No mouse dependency detected.
- **Command history:** Up/Down arrow history navigation is implemented correctly (lines 49–68). History persists within the session and behaves like a standard shell.
- **Shortcut bar (App.tsx:161):** The welcome banner includes a shortcut hint line: `↑/↓ history · @Agent to direct · /help commands · Ctrl+C quit`. This is good — shortcuts are discoverable on first launch.
- **Ctrl+C exit (App.tsx:72–76):** Graceful exit is wired via `useInput`. Works correctly.
- **Autocomplete (autocomplete.ts):** Tab-completion infrastructure exists for `@Agent` names and `/slash` commands. However, the `InputPrompt.tsx` component does not wire up Tab key handling — the autocomplete module is defined but appears unused in the Ink shell. Users cannot trigger autocomplete via Tab.
- **No Escape key handling:** There is no Escape key binding to cancel the current input or dismiss states. Minor issue for power users.

**Recommendations:**
1. **P2 — Wire up Tab autocomplete** in `InputPrompt.tsx`. The `createCompleter` function exists but is not called from the input component.
2. **P3 — Add Escape to clear current input line** as a convenience shortcut.

---

## 2. Color Contrast & NO_COLOR

**Status:** ❌ FAIL

**Findings:**

- **NO_COLOR not respected:** The `terminal.ts` capability detection (line 21) checks `FORCE_COLOR !== '0'` but does **not** check the `NO_COLOR` environment variable. The [NO_COLOR standard](https://no-color.org/) requires that when `NO_COLOR` is set (any value), color output must be suppressed. This is a compliance gap.
- **output.ts ANSI codes always emitted:** The `output.ts` utility emits raw ANSI escape codes (`\x1b[32m`, etc.) unconditionally. There is no gate on `NO_COLOR` or terminal capability. Users with `NO_COLOR=1` will see raw escape sequences in piped or non-TTY output.
- **Color-only meaning in AgentPanel.tsx:** Agent status uses color as the *primary* differentiator — green for active, red for error, dim for idle (lines 59–63). However, this is partially mitigated by emoji indicators (pulsing dot `●` for active, `✖` for error) and text labels ("working", "streaming", "idle"). The emoji fallback is acceptable.
- **Color-only meaning in MessageStream.tsx:** The `spinnerColor()` function (lines 25–29) cycles through cyan → yellow → magenta based on elapsed time. This is purely decorative and does not convey meaning, so it passes.
- **Message role differentiation:** User messages are cyan, system messages are dimColor, agent messages are green (MessageStream.tsx lines 118–134). Each role also has a text prefix (`❯ you:`, `◇ system:`, `{emoji} {name}:`), so color is not the sole differentiator. Pass.
- **Ink framework:** Ink (the React-for-CLI renderer) has its own color handling, but it does not auto-detect `NO_COLOR` unless explicitly configured. The shell does not pass `NO_COLOR` awareness to Ink's color system.

**Recommendations:**
1. **P1 — Respect `NO_COLOR` in `terminal.ts`:** Change `supportsColor` to: `isTTY && !process.env['NO_COLOR'] && process.env['FORCE_COLOR'] !== '0'`.
2. **P1 — Gate ANSI codes in `output.ts`:** Wrap all color constants so they return empty strings when `NO_COLOR` is set or stdout is not a TTY.
3. **P2 — Pass color capability to Ink:** When rendering the Ink app, disable Ink's color output if `NO_COLOR` is set.

---

## 3. Error Guidance

**Status:** ❌ FAIL

**Findings:**

- **Unknown command (commands.ts:41):** Returns `"Unknown command: /{command}. Type /help for available commands."` — this includes a remediation hint. ✅ Good.
- **Unknown CLI command (cli-entry.ts:286):** Returns `"Unknown command: {cmd}\nRun 'squad help' for usage information."` — includes remediation. ✅ Good.
- **Missing .squad/ directory (lifecycle.ts:57–59):** Returns `'No .squad/ directory found at "{path}". Run "squad init" to create a team.'` — includes remediation. ✅ Good.
- **Missing team.md (lifecycle.ts:63–65):** Returns `'No team.md found at "{path}". The .squad/ directory exists but has no team manifest.'` — describes the problem but does **not** suggest a fix. What should the user do? Re-run `squad init`? Manually create the file? ❌ No remediation hint.
- **Charter not found (spawn.ts:52):** Returns `'Charter not found for agent "{name}" at {path}'` — no remediation hint. ❌ User doesn't know whether to create the file, run a command, or check spelling.
- **No .squad/ directory (spawn.ts:47):** Returns `'No .squad/ directory found. Run "squad init" first.'` — includes remediation. ✅ Good.
- **SDK not connected (App.tsx:115):** Returns `'⚠️ SDK not connected — agent routing unavailable.'` — no remediation hint. ❌ User doesn't know how to connect the SDK.
- **Link command missing arg (cli-entry.ts:131):** Returns `'Usage: squad link <team-repo-path>'` — shows usage pattern, which serves as guidance. ✅ Acceptable.
- **Import command missing arg (cli-entry.ts:208):** Returns `'Usage: squad import <file> [--force]'` — shows usage. ✅ Acceptable.
- **Remote init missing path (cli-entry.ts:118):** Returns `'--mode remote requires a team root path. Usage: squad init --mode remote <team-root-path>'` — includes usage. ✅ Good.
- **Catch-all error handler (cli-entry.ts:289–295):** Catches `SquadError` and unknown errors, prints the message, and exits with code 1. No generic remediation hint like "Run 'squad doctor' to diagnose" is appended. ❌ Generic errors give no next step.

**Recommendations:**
1. **P1 — Add remediation hint to missing team.md error (lifecycle.ts:65):** Suggest `'Run "squad init" to recreate, or check that team.md exists in .squad/.'`
2. **P1 — Add remediation hint to charter-not-found error (spawn.ts:52):** Suggest `'Check the agent name spelling, or run "squad init" to regenerate agent files.'`
3. **P2 — Add remediation to SDK not connected (App.tsx:115):** Suggest what connects the SDK or how to check connectivity.
4. **P2 — Add fallback remediation to catch-all error handler (cli-entry.ts):** Append `'Run "squad doctor" to diagnose issues.'` to unhandled errors.

---

## 4. Help Text

**Status:** ✅ PASS (minor issues)

**Findings:**

- **CLI help (cli-entry.ts:52–98):** Comprehensive. Lists all 14 subcommands with descriptions, flags, and usage examples. Well-structured with clear grouping (Commands, Flags, Installation).
- **Shell help (commands.ts:83–99):** Lists all 6 slash commands with descriptions. Also documents `@AgentName` and `AgentName,` direct addressing syntax. Clear and complete for the shell context.
- **Keyboard shortcut listing:** The welcome banner (App.tsx:161) shows `↑/↓ history · @Agent to direct · /help commands · Ctrl+C quit`. However, the `/help` output (commands.ts:83–99) does **not** repeat keyboard shortcuts. Users who type `/help` expecting to see shortcuts will not find them there.
- **Line length:** CLI help text lines are within 80 characters. The longest lines in `cli-entry.ts` are the command descriptions, which stay under 80 columns when printed. ✅ Pass.
- **Placeholder prompt hint (InputPrompt.tsx:91):** Empty input shows `"type a message or /help"` — good discoverability for new users.
- **Missing agents hint:** When no agents are discovered, the welcome screen shows `"Run 'squad init' to set up your team"` (App.tsx:157). ✅ Good guidance.

**Recommendations:**
1. **P3 — Add keyboard shortcuts to `/help` output:** Include a "Keyboard shortcuts" section in the `/help` command output so users can reference shortcuts without scrolling back to the welcome banner.

---

## Summary

| Area | Status | Issues |
|------|--------|--------|
| Keyboard Navigation | ✅ PASS | Tab autocomplete wired but unused in shell |
| Color Contrast & NO_COLOR | ❌ FAIL | NO_COLOR not respected; ANSI codes always emitted |
| Error Guidance | ❌ FAIL | 3 error messages lack remediation hints |
| Help Text | ✅ PASS | Minor: /help doesn't list keyboard shortcuts |

**2 passes, 2 failures.** Verdict: **CONDITIONAL PASS** — ship after fixing P1 items.

### Priority Fixes (must fix before shipping)

1. **P1: Respect `NO_COLOR`** — Update `terminal.ts` to check `process.env['NO_COLOR']` and gate color output accordingly. Update `output.ts` to suppress ANSI codes when `NO_COLOR` is set.
2. **P1: Add remediation hints** — lifecycle.ts missing team.md error, spawn.ts charter-not-found error need actionable next-step guidance.

### Nice-to-Have (P2/P3)

3. P2: Wire Tab autocomplete into InputPrompt.tsx
4. P2: Add remediation to SDK-not-connected and catch-all errors
5. P3: Add Escape key to clear input
6. P3: Include keyboard shortcuts in `/help` output
