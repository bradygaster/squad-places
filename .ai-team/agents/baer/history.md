# Baer — History

## Project Context

- **Owner:** bradygaster
- **Stack:** Node.js, GitHub Copilot CLI, multi-agent orchestration
- **Description:** Squad democratizes multi-agent development — one command gives you a team that evolves with your product. Built to bring personality and real multi-agent patterns to the GitHub Copilot ecosystem.
- **Created:** 2026-02-07

## Day-1 Context

- Hired during v0.4.2 release cycle after Brady caught an email privacy issue
- The team was storing `git config user.email` in committed `.ai-team/` files — PII leak
- Immediate fix shipped: squad.agent.md no longer reads email, 9 files scrubbed
- v0.5.0 migration tool (#108) needs to scrub email from customer repos too
- Key decision already made: "Never store user email addresses in committed files"
- v0.5.0 is a major rename (.ai-team/ → .squad/) — security review needed for migration
- v0.5.0 also adds identity layer (wisdom.md, now.md) — review data sensitivity

## Learnings

- Squad files (.ai-team/) are committed to git and pushed to remotes — anything written there is public
- Guard workflow blocks .ai-team/ from main/preview/insider branches, but it's still in git history on dev/feature branches
- GitHub Actions bot email (github-actions[bot]@users.noreply.github.com) is standard and not PII
- Plugin marketplace sources are stored in .ai-team/plugins/marketplaces.json — external repo references, not sensitive
- MCP server configs can contain API keys via env vars (${TRELLO_API_KEY}) — these should never be committed
