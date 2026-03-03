# Decision: Migration Announcement Strategy

**Author:** McManus (DevRel)
**Date:** 2026-03-06
**Status:** Documented

## The Decision

Published two migration announcement documents:

1. **Blog post** (`docs/blog/021-the-migration.md`) — comprehensive, ~280 lines
2. **Launch announcement** (`docs/launch/migration-announcement.md`) — compact, shareable, ~77 lines

## Rationale

### Why Two Documents?

- **Blog post:** Tells the full story. Explains v0.5.4 → v0.8.18 jump. Details the benefits of npm distribution. Provides step-by-step upgrade instructions for beta users and getting-started instructions for new users. Links to reference docs (migration checklist, README, samples). This is the archive, the full context.

- **Announcement:** Shareable to social (LinkedIn, Discord, GitHub Discussions). Under 100 lines. Contains just enough context (what is Squad, what changed, how to start) with links back to the blog post for details. This drives traffic to the full post.

Two documents avoid forcing readers to choose between "get everything" and "get the gist." Different audiences, different goals.

### Messaging Tone

Applied tone ceiling throughout:
- **Version jump (v0.5.4 → v0.8.18):** Explained honestly, not positioned as "major release excitement."
- **Benefits (faster installs, semver):** Listed factually; no "now you finally can" language.
- **Public repository move:** Framed as enabling collaboration, not as a victory lap.
- **Beta user experience:** Acknowledged the version jump is significant; gave clear upgrade path.
- **New user experience:** Kept getting started quick and concrete (3 commands).

### Link Verification

Verified all links point to real files:
- `docs/migration-github-to-npm.md` ✓
- `docs/migration-checklist.md` ✓
- `README.md` ✓
- `CHANGELOG.md` ✓
- `samples/` directory ✓
- Public repo: `github.com/bradygaster/squad` ✓

No broken links in either document.

## What This Enables

1. **Social sharing** — announcement can go to LinkedIn, Discord, Twitter with full context in first 100 lines
2. **Archive** — blog post becomes the permanent explanation of why Squad migrated and how to upgrade
3. **SEO** — blog post indexed, searchable; announcement drives traffic there
4. **Community discovery** — links to samples, migration guide, and public repo support self-service onboarding

## What Wasn't Done

- **Didn't add the posts to the docs build** — That's Scribe's/build system's concern (`.squad/build/` or docs generator config)
- **Didn't update GitHub releases** — That's distribution team's work
- **Didn't write social media copy** — Announcement doc serves as source; anyone can adapt it

## Next Steps for Team

- **Keaton (Lead):** Decide when to publish (coordinate with GitHub release, npm publication timing)
- **Scribe:** Integrate blog post into docs site (ensure 021 appears in blog navigation)
- **Verbal/Community:** Adapt announcement for social channels (LinkedIn, Discord, Twitter)
- **Brady:** Review tone and messaging for any adjustments before publication
