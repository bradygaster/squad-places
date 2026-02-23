# Marquez — History

## Project Context
- **Project:** Squad — programmable multi-agent runtime for GitHub Copilot
- **Owner:** Brady
- **Stack:** TypeScript (strict, ESM), Node.js ≥20, Ink 6 (React for CLI), Vitest
- **CLI:** Ink-based interactive shell with AgentPanel, MessageStream, InputPrompt components
- **Key files:** packages/squad-cli/src/cli/shell/components/*.tsx

## Learnings

### 2026-02-23: Initial CLI UX Audit Completed

**Task:** Comprehensive UX audit of Squad CLI across all entry points and interactive shell components.

**Files Reviewed:**
- `packages/squad-cli/src/cli-entry.ts` — Command routing, help text, version output, error handling
- `packages/squad-cli/src/cli/shell/components/App.tsx` — Shell layout, header, welcome text, keyboard hints
- `packages/squad-cli/src/cli/shell/components/AgentPanel.tsx` — Agent roster, status display, separators
- `packages/squad-cli/src/cli/shell/components/MessageStream.tsx` — Message rendering, thinking indicators, timestamps
- `packages/squad-cli/src/cli/shell/components/InputPrompt.tsx` — Input handling, placeholders, disabled states
- `packages/squad-cli/src/cli/shell/commands.ts` — Slash command implementations (/status, /help, /agents)
- `packages/squad-cli/src/cli/shell/terminal.ts` — Terminal capability detection

**Key UX Patterns Identified:**

1. **Visual Hierarchy System:**
   - ◆ for primary prompts/system (cyan)
   - ❯ for user input (cyan)
   - Role emoji + name for agents (green)
   - Status indicators: pulsing dots for active, dim for idle

2. **Color Semantics:**
   - Cyan = user/system/prompts
   - Green = agent responses
   - Yellow = processing/warnings
   - Red = errors
   - Dim = secondary info/hints

3. **Information Density Philosophy:**
   - Header: identity + context (version, description, roster, focus)
   - Panel: current agent states + activity
   - Stream: conversation history with timestamps
   - Prompt: current input + placeholder hints

4. **Interaction Model:**
   - Natural language (coordinator routing)
   - @Agent direct addressing
   - Slash commands for meta-actions (/status, /help, /agents, /history, /clear, /quit)
   - Keyboard shortcuts (↑/↓ history, Ctrl+C quit)

**Issues Found:** 21 total (5 P0, 9 P1, 7 P2)

**Common UX Anti-Patterns Detected:**
- **Inconsistent verbs** in command descriptions (P1) — no pattern across help text
- **Hardcoded dimensions** (50-char separators) instead of terminal-aware layout (P1)
- **Technical jargon** exposed to users ("Connecting", "Routing" phase labels) (P2)
- **Redundant information** (agent count shown twice) (P1)
- **Missing remediation hints** in error messages (P0)
- **80-char violations** in help output (P0)
- **Inconsistent visual vocabulary** (◇ vs ●, ─ vs ┄, color emoji vs text) (P1-P2)

**UX Gates Defined:** 7 testable assertions for CI
1. Help line length (≤80 chars)
2. Version format (semver only)
3. Error remediation hints (must contain actionable verbs)
4. Empty state actionability (must mention "squad init")
5. Terminal width compliance (80x24 golden test)
6. Separator consistency (single character across components)
7. Command verb consistency (imperative verbs)

**Design Principles Extracted:**
- **Crisp, confident, delightful** — not just functional
- **Consistent visual language** reduces cognitive load
- **Every error includes next action** — never leave users stuck
- **Empty states are onboarding opportunities** — show the path forward
- **Terminal-aware layout** — respect width constraints, degrade gracefully
- **Minimal but meaningful** — information should earn its space

**Next Actions:**
- Breedan implements UX gates as Vitest tests
- Brady or assigned dev addresses P0 blockers before next release
- P1/P2 polish in subsequent iterations

**Files Written:**
- `.squad/decisions/inbox/marquez-ux-review-initial.md` — Full audit report with actionable diffs
