### 2026-02-24T21:46:00Z: Unified status vocabulary

**By:** Marquez (CLI UX Designer)

**Decision:** Adopt the /agents command vocabulary — `[WORK]` / `[STREAM]` / `[ERR]` / `[IDLE]` — as the standard across ALL status surfaces. Replace AgentPanel's lowercase "streaming"/"working" and `[Active]` with this vocabulary.

**Rationale:**

Three status vocabularies currently exist:
- **AgentPanel compact:** `streaming` / `working` (lowercase text)
- **AgentPanel normal:** `[Active]` / `[Error]` (bracketed, collapsed)
- **/agents command:** `[WORK]` / `[STREAM]` / `[ERR]` / `[IDLE]` (bracketed, granular)

The /agents vocabulary wins because:

1. **Most granular.** It distinguishes between "streaming" (output flowing) and "working" (thinking/processing). AgentPanel collapses both into `[Active]`, losing critical information about *what type of work* is happening. Users need to know if an agent is still thinking or already returning results.

2. **NO_COLOR compatible.** Brackets work in monochrome terminals where only text styling is available. The emoji-based approach (● pulsing dots, ▶ arrow) requires color to be clear. Brackets are universally readable in 80x24 terminals and terminal emulators with color disabled.

3. **Text-over-emoji principle.** This respects the P2 decision in decisions.md: text status is primary, emoji is supplementary. `[STREAM]` is readable. ● alone is ambiguous. (● + text is best, ● alone in compact mode is worst.)

4. **Consistent across command context.** Users see `/agents` and AgentPanel simultaneously. When they see `[STREAM]` in the list and `[Active]` in the panel, it creates cognitive dissonance. One vocabulary everywhere eliminates friction.

5. **Future-proof for terminal edge cases.** Compact terminals (≤60 cols), SSH sessions with limited Unicode support, accessibility tools that strip ANSI codes — the bracket notation degrades gracefully in all cases.

**Implementation:**
- Replace AgentPanel compact status from `streaming`/`working` to `[STREAM]`/`[WORK]`
- Replace AgentPanel normal `[Active]` to match /agents format (use `[STREAM]` when `status === 'streaming'`, `[WORK]` when `status === 'working'`)
- Add `[ERR]` for error state (already present in /agents, missing in AgentPanel)
- Update test assertions for all three components (AgentPanel compact, normal, /agents command)

**Impact:** Reduces cognitive load for users scanning status across the shell interface. Makes Squad safer for use in low-color/accessibility-critical environments. No breaking changes to the /agents command output — only AgentPanel visuals change.

