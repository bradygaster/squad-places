# Decision: Squad Notifications Consumer Documentation

**Status:** Completed  
**Decided by:** McManus  
**Date:** 2026-02-12

## What Was Decided

Created `docs/features/notifications.md` — consumer-facing documentation for "Squad Pings You," the feature allowing users to receive instant messages when agents need human input.

## Rationale

Brady's vision: "It needs to feel like I'm not in the team room, they are, and they need me so they pinged me." This doc translates that into practical setup paths and concrete examples of what notifications look like.

## Key Design Decisions

### Zero-Auth Paths Come First
- Teams Incoming Webhook (just a URL, no API setup) listed as "Option A"
- Official Teams MCP server (full Azure AD auth) listed as "Option B" for completeness
- This lowers barrier to entry for most users

### Show, Don't Tell
- All quick-start sections include actual JSON configuration examples
- Notification format section shows concrete message examples with emoji and structure
- Troubleshooting uses real error scenarios ("MCP server failed to start", "wrong channel receiving")

### Architecture Transparency
- Explicit section explaining: skill-based (not hard-coded), bring-your-own MCP server (not managed service)
- Notes that users can customize the `human-notification` skill for advanced use cases
- Links to `.vscode/mcp.json` standard config file location

### Trigger Control Upfront
- Configuration section explains how to selectively enable/disable notification types
- Quiet hours concept introduced for advanced setups
- Test command provided for validation

### Platform Realistic
- iMessage marked as "Mac Only" with clear limitations
- All paths (Teams, Discord, webhook) tested for completeness
- Acknowledges three-tier setup complexity (webhook URL → MCP config → environment variable)

## File Location

`docs/features/notifications.md` — placed alongside other feature docs (skills, ralph, memory, etc.)

## Style Adherence

- **Brady's "straight facts" directive:** No editorial voice, no hype words, every sentence states what/how/depends-on
- **Devrel vision:** First 5 minutes to value (webhook option gets you going fastest), then depth for power users
- **Tone:** "Here's how to set it up" not "isn't this cool"
- **Brevity:** Dev-skimmable format with tables, code blocks, and scannable lists

## Cross-References

Links to related docs:
- Skills System (`docs/features/skills.md`)
- Copilot Environment Setup (`docs/guide.md`)
- Model Selection (`docs/features/model-selection.md`)

No README changes required — notifications is a features doc, not a headline change.

## Follow-Up

Consider adding `notifications` to the "What's New" section in README.md if this is a v0.4.0 feature launch. Coordinate with Brady on feature status and visibility.
