# Decision: Paste detection via debounce in InputPrompt

**By:** Kovash (REPL Expert)
**Date:** 2026-03-02
**Context:** Multi-line paste was garbled because `useInput` fires per-character and `key.return` triggered immediate submission.

## Decision

InputPrompt uses a 10ms debounce on `key.return` to distinguish paste from intentional Enter:
- If more input arrives within 10ms → paste detected → newline preserved, accumulation continues
- If timer fires without more input → real Enter → submit accumulated value

A `valueRef` (React ref) mirrors all value mutations synchronously alongside React state, since closure-captured `value` is stale during rapid `useInput` calls. The ref is the source of truth for the debounced submit callback.

In disabled state, `key.return` now appends `\n` to the buffer instead of being ignored.

## Impact

- **UX:** 10ms delay on single-line submit is imperceptible. Multi-line paste is preserved intact.
- **Testing:** Hockney should verify paste scenarios use `jest.useFakeTimers()` or equivalent to control the 10ms debounce.
- **Future:** If Ink ever adds native bracketed-paste support, the debounce can be replaced with explicit paste start/end detection.
