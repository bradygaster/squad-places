### 2026-02-24T21:45:00Z: Pick one tagline

**By:** Marquez (CLI UX Designer)

**Decision:** Use "Team of AI agents at your fingertips" everywhere.

**Rationale:** 

Both taglines work, but "Team of AI agents at your fingertips" wins on three counts:

1. **Outcome over action.** Users care about *what they can do* (have a team at their fingertips) not *what to do* (add a team). "Fingertips" paints a mental image of instant, direct control — the real promise.

2. **Shorter and punchier.** 7 words vs 8. Every character saved in a tagline compounds across help text, banners, and README. Brevity signals confidence.

3. **Existing use case consistency.** It's already in the help flow (line 71, `squad --help`). Help is the highest-intent page — users who see it are already convinced they want Squad. The help tagline should be the *lasting* memory, not the empty-args fallback.

The action tagline ("Add an AI agent team...") feels like feature description, not brand promise. It belongs in paragraph 2 of a marketing page, not at the top of help.

**Action:** Replace line 56 in cli-entry.ts with "Team of AI agents at your fingertips".

