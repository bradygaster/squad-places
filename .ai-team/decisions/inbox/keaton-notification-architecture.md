### 2026-02-11: Squad Notification Architecture — MCP Integration Pattern

**By:** Keaton (Lead)

**What:** Squad agents can notify humans via external channels (Teams, iMessage, Discord, webhooks) when work is blocked, errors occur, or decisions are needed. Implemented as an MCP integration pattern — Squad ships ZERO notification infrastructure.

**Why:**

1. **Brady's vision:** "It needs to feel like I'm not in the team room, they are, and they need me so they pinged me." When agents hit a wall requiring human input, they should ping the human's phone, not just pause in the terminal.

2. **MCP integration preserves Squad's architecture:** Zero dependencies, filesystem-authoritative, git-native. The consumer brings their own notification MCP server (Teams, iMessage, etc.). Squad teaches agents WHEN and HOW to notify via a skill at `.ai-team/skills/human-notification/SKILL.md`.

3. **Platform-agnostic design:** Works with ANY notification MCP server — Teams (primary path), iMessage (Mac-only secondary), Discord, generic webhooks. Squad never hardens against a specific platform. When new platforms emerge (Slack, Mattermost, Signal), the consumer installs the right MCP server and Squad's skill detects the tools automatically.

4. **Zero maintenance burden:** The consumer owns the MCP server, credentials, and delivery mechanism. When Teams changes their API, the MCP server maintainer updates the server — not Squad. Squad just teaches the notification pattern and lets the platform handle delivery.

5. **Graceful degradation:** If no MCP server is configured, agents log the notification attempt and continue. Notifications are an enhancement, not a requirement. Squad works perfectly without them.

**Architecture:**

- **Layer 1:** Notification skill (`.ai-team/skills/human-notification/SKILL.md`) teaches agents when to ping (BLOCKED, ERROR, DECISION, COMPLETE) and how to compose rich, agent-branded notifications.
- **Layer 2:** MCP tool abstraction — agents detect which notification tools are available (`send_teams_message`, `send_imessage`, `post_webhook`) and use the right format for each platform.
- **Layer 3:** Consumer's MCP server (configured in `.vscode/mcp.json`, VS Code settings, etc.) handles actual delivery.

**Message format (platform-agnostic):**

- **Who:** Agent name + emoji (Keaton 🏗️)
- **Why:** Type badge (🚫 BLOCKED, ⚠️ ERROR, 🤔 DECISION, ✅ COMPLETE)
- **Context:** Brief explanation (1-2 sentences)
- **Action:** What the human should do next
- **Link:** URL to GitHub issue/PR/proposal if applicable

**Platform-specific renderers:**

- **Teams:** Adaptive Card JSON with color-coded theme (red for ERROR, orange for BLOCKED, blue for DECISION, green for COMPLETE)
- **iMessage:** Plain text with emoji and signature
- **Webhook:** Structured JSON payload that consumer routes to their chosen backend (Slack, Discord, SMS, push notifications)

**Integration with existing features:**

- **Human Team Members:** When work routes to a human team member, the assigned agent sends a BLOCKED notification on their behalf.
- **Ralph (Work Queue Monitor):** Ralph can escalate stale work via notifications (opt-in — default OFF).
- **Coordinator Handoffs:** When an agent returns blocked, the coordinator triggers the notification BEFORE prompting the user in terminal (ensures Brady gets the ping even if not watching terminal).

**Primary path: Microsoft Teams**

Brady said Teams is "ideal, especially per-repo channels." Teams channels-within-a-Team map perfectly to repos. Microsoft ships official MCP support: `@microsoft/teams.mcp` npm package and https://github.com/microsoft/IF-MCP-Server-for-Microsoft-Teams. Setup: create Incoming Webhook URL, configure MCP server, Squad detects `send_teams_message` tool and sends Adaptive Cards.

**Secondary path: iMessage (Mac-only)**

Zero account setup, instant delivery, native to Apple ecosystem. Limitations: requires macOS with Messages.app running, cannot run headless. MCP server exists: `imessage-mcp` or `imsg` CLI tool. Squad detects `send_imessage` tool and sends plain text with agent signature.

**Trade-offs:**

- **No auto-configuration:** Consumer must manually wire up MCP server and credentials. This is a setup burden but preserves Squad's zero-dependency constraint.
- **Single channel per repo:** All notifications from a repo go to ONE configured channel/recipient. Per-agent channels would fragment the notification stream (Brady doesn't want to monitor 5 channels per repo).
- **COMPLETE notifications opt-in:** Completion notifications can be noisy. Default is OFF. Consumers enable explicitly if they want visibility into finished work.

**Sprint estimate:** 1.8 squad-days (core) + 0.3 squad-days (Ralph integration, optional). Target version: 0.3.0 (alongside GitHub-native proposals).

**Success criteria:**

1. Notification skill exists at `.ai-team/skills/human-notification/SKILL.md`
2. Skill teaches all four trigger types (BLOCKED, ERROR, DECISION, COMPLETE)
3. `docs/notifications.md` exists with Teams and iMessage setup guides
4. Agents gracefully degrade when no MCP server configured
5. At least ONE real-world test: Brady configures Teams, receives notification from his squad

**Key file paths:**

- `team-docs/proposals/034-notification-architecture.md` — full design specification
- `.ai-team/skills/human-notification/SKILL.md` — agent-facing skill (teaches when/how to notify)
- Future: `docs/notifications.md` — consumer setup guide (Teams, iMessage, Discord, webhook walkthroughs)

**Future enhancements (post-0.3.0):**

- Discord support in primary docs (currently secondary tier)
- Slack support for enterprise customers
- Per-agent notification preferences (e.g., "only notify for Keaton's blockers")
- Digest mode (daily/weekly summary email instead of real-time pings)
- Two-way communication (reply to notification via Teams/iMessage and have Squad ingest response — requires connector architecture, not just MCP tools)
