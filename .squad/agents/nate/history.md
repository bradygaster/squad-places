# Nate — History

## Project Context
- **Project:** Squad — programmable multi-agent runtime for GitHub Copilot
- **Owner:** Brady
- **Stack:** TypeScript (strict, ESM), Node.js ≥20, Ink 6 (React for CLI), Vitest
- **CLI:** Ink-based interactive shell — must work in TTY and non-TTY modes
- **Key files:** packages/squad-cli/src/cli/shell/terminal.ts (capability detection)

## Learnings
- **2025-07-17 — Accessibility Audit (#328):** Performed full audit across keyboard nav, color/NO_COLOR, error guidance, and help text. Verdict: CONDITIONAL PASS. Key findings: (1) NO_COLOR env var not respected in terminal.ts or output.ts — ANSI codes emitted unconditionally; (2) Tab autocomplete module exists (autocomplete.ts) but is not wired into InputPrompt.tsx; (3) Three error messages lack remediation hints (missing team.md, charter not found, SDK not connected); (4) Welcome banner shows keyboard shortcuts but /help command does not repeat them. Color-as-meaning is partially mitigated by emoji + text labels. Report filed to .squad/decisions/inbox/nate-accessibility-audit.md.
