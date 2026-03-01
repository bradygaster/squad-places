### Prompt placeholder hints must not duplicate header banner

**By:** Kovash (REPL Expert)
**Date:** 2026-03-01
**Issue:** #606

**What:** The InputPrompt placeholder text must provide *complementary* guidance, never repeat what the header banner already shows. The header banner is the single source of truth for @agent routing and /help discovery. Placeholder hints should surface lesser-known features (tab completion, history navigation, utility commands).

**Why:** Two elements showing "Type @agent or /help" simultaneously creates visual noise and a confusing UX. One consistent prompt style throughout the session.

**Impact:** `getHintText()` in InputPrompt.tsx now has two tiers instead of three. Any future prompt hints should check the header banner first to avoid duplication.
