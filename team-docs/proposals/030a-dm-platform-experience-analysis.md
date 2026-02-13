# Proposal 030a: Squad DM Platform Experience Analysis — Discord vs. Teams vs. The Field

**Status:** Active  
**Authored by:** Verbal (Prompt Engineer & AI Strategist)  
**Date:** 2026-02-11  
**Requested by:** bradygaster — Platform preference input + experience design questions  
**Companion to:** Proposal 017 (DM Experience Design), Proposal 030 (Async Comms Feasibility)

---

## Summary

Brady has spoken: no Telegram. Prefers Discord. Some users want Teams. He's worried about GitHub-specific integrations creating lock-in if Squad ever supports Azure DevOps or GitLab. The core promise remains: **"Text my squad from my phone."**

This is my experience design analysis of what that sentence actually means across platforms. Not feasibility — Kujan covered that in 030. This is about **how it feels** to have a conversation with your AI team at 11pm from the couch.

**TL;DR:** Discord is the right MVP connector. It delivers the best "messaging my dev team" feeling with the lowest friction. Teams is the right *second* connector for professional/enterprise contexts. GitHub-native surfaces (Issues, Discussions) are a legitimate async channel but will never feel like texting your team. The output mode prompt should be platform-aware, but the differences are smaller than they appear.

---

## 1. "Text My Squad From My Phone" — Platform Experience Comparison

### The Test

I'm evaluating one scenario across all platforms. This is the only scenario that matters:

> Brady is on his couch at 11pm. He picks up his phone. He wants to ask Keaton whether the export format should support versioning. He expects an answer in under 10 seconds that sounds like Keaton — not a chatbot, not an error message, not a loading screen.

Every platform gets judged on how close it gets to that moment.

### Discord: The Dev Team Group Chat

**The feel:** Discord feels like a group chat with your friends who happen to be engineers. It's casual, fast, and low-ceremony. You open the app, you see channels named after your repos, you type a message, you get a reply. The barrier between "thinking about a question" and "asking it" is almost zero.

**Mobile app quality:** Excellent. Discord's mobile app is fast, reliable, and optimized for quick messaging. Push notifications are immediate and configurable per-channel. The app opens to your last-viewed channel — which means if Brady was in `#squad`, he's back in `#squad`. No navigation required.

**How agent responses render:**
- **Rich embeds** with colored sidebars — one color per agent. Keaton gets blue. Verbal gets purple. Hockney gets green. The visual identity is *instant*. Before you read a word, you know who's talking by the color.
- **Author fields** in embeds display the agent name + emoji. `🏗️ Keaton` appears as the embed author, not the bot username. This is the closest any platform gets to "multiple people talking through one account."
- **Code blocks** render beautifully in Discord markdown. Syntax highlighting works. Five-line snippets (our DM mode limit) look clean.
- **2000-character message limit** is actually a feature for DM mode — it enforces our "summary + link" pattern naturally. Agents can't monologue because the platform won't let them.

**Threading and conversation flow:** Discord's thread model is good but not great. You can create threads from any message, which enables the "multi-agent debate" pattern:

```
Brady: Should we version the export format?

  🏗️ Keaton: [embed - blue] Yes. Semantic versioning...
  🎭 Verbal: [embed - purple] Agree, but the version IS the brand signal...  
  🧪 Hockney: [embed - green] Ship v1.0. I have tests for the schema already.
```

Threads can branch off for deeper discussion. The main channel stays clean. This is very close to how real dev teams use Discord — you see the conversation flow, you can dive into a thread, you can react with emoji to signal agreement.

**Per-repo channel organization:** Native and obvious. One Discord server ("Brady's Squad"), channels per repo (`#squad`, `#other-project`, `#slidemaker`). Bot routes messages based on channel. This is Discord's natural paradigm — it's how every open-source project already organizes. Brady doesn't need to learn a new pattern.

**The honest verdict on feel:** Discord feels like your dev team's group chat. It's where you'd naturally message developers. The casualness is a feature — it lowers the activation energy to "I'll just ask Keaton real quick." That's exactly the 11pm couch moment we designed for.

### Teams: The Professional War Room

**The feel:** Teams feels like you're at work. Even at 11pm. Even on the couch. The app's visual language says "enterprise collaboration" — channels, tabs, file shares, meetings, apps. When you message your Squad in Teams, it feels like you're filing a request with your engineering department, not texting your buddy Keaton.

This isn't necessarily bad. For some people — especially those whose entire work life runs through Teams — this is exactly right. The Squad is a professional team. Professional teams communicate through professional tools. But it changes the *texture* of the interaction.

**Mobile app quality:** Good, not great. Teams mobile is heavier than Discord or Telegram. It loads slower. It has more UI chrome (tabs, activity feed, calendar, calls). The path from "phone unlock" to "typing a message to my Squad" has more steps. Push notifications work but sometimes feel delayed compared to Discord's instant delivery (this is a known Teams mobile complaint across the industry, not specific to bots).

**How agent responses render:**
- **Adaptive Cards** are powerful. They support custom headers, footers, action buttons, column layouts, and styled text. Agent identity can be rendered as a card header with the agent's name, emoji, and a colored accent.
- Cards can include **collapsible sections** — perfect for progressive disclosure. Show the summary, collapse the details, include an "Open on GitHub" action button.
- **Code blocks** in Adaptive Cards are... fine. Not beautiful. Teams' markdown rendering in cards is functional but lacks Discord's syntax highlighting elegance.
- **No hard message limit** like Discord's 2000 chars. This sounds like a feature but it's actually a risk — it lets agents be verbose. We'd need to enforce the summary pattern through prompting alone, without the platform backstop.

**Threading and conversation flow:** Teams' threading model is reply-based, not branch-based. You reply to a message, and it nests underneath. The multi-agent debate pattern works:

```
Brady: Should we version the export format?
  └── 🏗️ Keaton: [Adaptive Card - blue accent] Yes. Semantic versioning...
  └── 🎭 Verbal: [Adaptive Card - purple accent] Agree, but consider...
  └── 🧪 Hockney: [Adaptive Card - green accent] Ship v1.0...
```

This is clean. The replies are visually grouped. But it feels more like a comment thread than a conversation. The difference is subtle but real — Discord threads feel like people talking; Teams threads feel like people commenting.

**Per-repo channel organization:** This is Teams' strongest UX advantage. One Team ("My Squads"), one channel per repo. The left sidebar shows all your repo channels at a glance. Switching between repos is one tap. Brady said this was "ideal" — and he's right that the organizational model is the cleanest of any platform.

**The honest verdict on feel:** Teams feels like a professional tool. The Squad feels like a professional team. If Brady's mental model is "I'm managing a team of AI engineers," Teams reinforces that frame perfectly. If Brady's mental model is "I'm texting my crew," Teams is too buttoned-up. The 11pm couch moment in Teams feels like checking work email, not texting a friend.

### Telegram (For the Record — Brady Said No)

I'll keep this brief since Brady doesn't want it, but for comparison:

**Telegram's feel is unmatched for pure messaging.** It's the fastest, lightest, most notification-reliable mobile messaging platform. It is objectively the best "text my squad" experience from a pure-messaging standpoint. The 4096-char limit is generous. The bot API is frictionless. The UX is just... texting.

**Why Brady might not want it:** Telegram carries baggage. It's associated with crypto, privacy communities, non-professional contexts. For a Microsoft engineer who lives in the GitHub/Azure/VS Code ecosystem, Telegram is foreign territory. Having your professional AI team live in the same app where you get crypto scam messages is a vibe mismatch.

**My updated position:** My original 017 proposed Telegram as the first connector because of UX quality. Brady's override is valid — the *ecosystem* feeling matters as much as the *messaging* feeling. Discord gives 85% of Telegram's messaging UX with 100% more developer credibility.

### The Comparison Matrix

| Dimension | Discord | Teams | Telegram | GitHub Native |
|-----------|---------|-------|----------|---------------|
| **"Texting my team" feeling** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐ |
| **Mobile app speed** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ |
| **Push notification reliability** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐ |
| **Agent response rendering** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ |
| **Code block quality** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐ |
| **Per-repo organization** | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| **Multi-agent debate UX** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ |
| **Professional context** | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐⭐⭐ |
| **Dev community fit** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐ |
| **Build cost** | Low | High | Low | Minimal |
| **Lock-in risk** | None | Microsoft | None | GitHub |

**Winner for "text my squad from my phone":** Discord.

It's not close on feel. Discord is the platform where developers already go to talk to other developers about code. The mental model matches perfectly. When Brady opens Discord and sees `#squad`, the experience is: "Let me check in with my team." When he opens Teams and sees a channel, the experience is: "Let me check my work communications."

The Squad isn't a work communication system. It's a team. Teams talk in group chats. Group chats live on Discord.

---

## 2. DM Output Mode Across Platforms

### The Question

My 017 design specified DM output as "summary + link." Does this need to change per platform?

### Platform-Specific Rendering

Here's how the same agent response looks on each platform:

#### The Response

Keaton answers "Should we version the export format?"

#### Discord (Rich Embed)

```
┌─────────────────────────────────────────┐
│ 🏗️ Keaton                    [blue bar] │
│                                         │
│ Yes. Semantic versioning from v1.0.     │
│                                         │
│ • Format is stable — 3 proposals        │
│   validated the schema                  │
│ • Breaking changes get major bumps      │
│ • Migration scripts in `scripts/`       │
│                                         │
│ 📎 Full analysis → docs/decisions/      │
│    export-versioning.md                 │
│                                         │
│ ────────────────────────────────────     │
│ 🗳️ React ✅ to approve, ❌ to discuss  │
└─────────────────────────────────────────┘
```

**What's happening:** Discord embeds let us put the agent name in the author field, the response in the description, and the artifact link in a field. The blue sidebar IS Keaton's identity. Emoji reactions become a voting mechanism — Brady reacts ✅ to approve a decision without typing a word.

#### Teams (Adaptive Card)

```json
{
  "type": "AdaptiveCard",
  "body": [
    { "type": "ColumnSet", "columns": [
      { "type": "Column", "items": [
        { "type": "TextBlock", "text": "🏗️ Keaton", "weight": "bolder", "color": "accent" }
      ]}
    ]},
    { "type": "TextBlock", "text": "Yes. Semantic versioning from v1.0.", "wrap": true },
    { "type": "FactSet", "facts": [
      { "title": "Schema", "value": "Stable — validated in 3 proposals" },
      { "title": "Breaking changes", "value": "Major version bumps" },
      { "title": "Migrations", "value": "scripts/ directory" }
    ]}
  ],
  "actions": [
    { "type": "Action.OpenUrl", "title": "View full analysis", "url": "..." },
    { "type": "Action.Submit", "title": "✅ Approve", "data": { "decision": "approved" } },
    { "type": "Action.Submit", "title": "💬 Discuss", "data": { "decision": "discuss" } }
  ]
}
```

**What's happening:** Adaptive Cards give us structured data rendering. The FactSet makes bullet points look like a dashboard. Action buttons replace emoji reactions with explicit UI elements. It's more structured, less conversational. Looks polished. Feels like a report card from your team, not a text from your friend.

#### Telegram (Markdown v2 + Inline Keyboard)

```
🏗️ *Keaton:*

Yes\. Semantic versioning from v1\.0\.

• Format is stable — 3 proposals validated the schema
• Breaking changes get major bumps
• Migration scripts in `scripts/`

📎 [Full analysis](https://github.com/.../export-versioning.md)

[✅ Approve] [💬 Discuss]  ← inline keyboard buttons
```

**What's happening:** Telegram is plain text with markdown. No embeds, no cards, no structured data. The agent identity comes from the text prefix alone. It's clean and fast to read but has no visual identity layer beyond the emoji. Inline keyboards add interactivity.

### The Verdict: Platform-Aware Output Is Necessary — But Minimal

The output mode prompt needs **one** platform-specific section. Here's my recommendation:

**Universal DM rules (all platforms):**
- Summary + link. Never inline full artifacts.
- Max 4-5 sentences unless user asks for more.
- Bullet points for lists.
- No code blocks longer than 5 lines.
- One message per agent per response.

**Platform-specific addendum (injected by the bridge):**

```
# Discord-specific
- Use embed formatting: agent name in author, colored sidebar per agent
- Keep messages under 1900 chars (leave room for embed overhead)
- Suggest emoji reactions for quick decisions
- Code blocks use ```language syntax highlighting

# Teams-specific  
- Format responses for Adaptive Card rendering
- Use FactSet for structured bullet points
- Include Action buttons for decisions (Approve/Discuss/Defer)
- No hard length limit but keep under 500 words

# Telegram-specific
- Use Markdown V2 formatting (escape special characters)
- Keep messages under 3500 chars
- Inline keyboard buttons for decisions
- Bold agent name prefix: *🏗️ Keaton:*
```

**But here's the key insight:** The platform-specific section is **bridge-level formatting, not agent-level concern.** The agent produces a standard DM-mode response (summary + link + decision prompt). The bridge layer transforms it into the platform's native format. Agents don't need to know they're on Discord vs. Teams. The bridge does the rendering.

This means:
- **Agents:** One universal DM output mode. No platform awareness.
- **Bridge:** Platform-specific renderer. Takes agent output → renders as Discord embed / Teams card / Telegram markdown.
- **Prompt:** One DM mode prompt, not three.

This is the right architecture because it means adding a new platform is a rendering layer change, not a prompt engineering change. Platform-agnostic agents, platform-aware bridge.

---

## 3. Agent Personality/Voice Preservation

### The "Single Bot, Many Voices" Pattern Across Platforms

My 017 design established: one Squad bot, agents identified by emoji prefix + name + role. How does this play on Discord vs. Teams?

### Discord: The Best Platform for Agent Identity

Discord gives us **three identity layers** in a single message:

1. **Embed color** — A colored sidebar that is visible before the text loads. Keaton = blue, Verbal = purple, Hockney = green, McManus = orange, Fenster = silver, Kujan = gold. This is *pre-textual identity* — you know who's talking from the color alone while scrolling.

2. **Embed author field** — `🏗️ Keaton · Lead Architect` appears as the embed author, separate from the bot's username. This looks like a distinct person posted the message, even though it's all coming from the "Squad" bot.

3. **Embed footer** — `Proposal 008 context · v0.2.0` gives provenance without cluttering the message body.

**The multi-agent debate in Discord is visually stunning:**

```
Blue bar:   🏗️ Keaton — "Yes, version it. Stable schema."
Purple bar: 🎭 Verbal — "Agree. The version IS the brand signal."
Green bar:  🧪 Hockney — "Ship v1.0. Tests are ready."
```

Three colors. Three voices. One glance and Brady knows: the team agrees but from different angles. No reading required to get the shape of the conversation.

**Discord's limitation:** The bot's username is fixed ("Squad"). You can't change it per message. But the embed author field completely solves this — the bot username becomes invisible when embeds are used. Nobody looks at the bot name when the embed has its own author. It's like how nobody reads "via Twitter for iPhone" — the content identity (the embed author) is all that matters.

### Teams: Structured Identity Through Cards

Teams gives us **Adaptive Card headers** as the identity layer:

1. **Card header** — Custom text, custom color, custom icon URL. Each agent gets a distinct card header with their emoji, name, and role.

2. **Accent color** — Cards support an accent color property. Same color-per-agent pattern as Discord embeds.

3. **No author field equivalent** — Teams doesn't have Discord's embed author concept. The card header is the closest thing, but it doesn't look like "someone posted this." It looks like "the bot generated a card about this agent."

**The multi-agent debate in Teams:**

```
[Card: Blue accent]  🏗️ Keaton — Lead Architect
"Yes, version it. Stable schema."

[Card: Purple accent]  🎭 Verbal — Prompt Engineer  
"Agree. The version IS the brand signal."

[Card: Green accent]  🧪 Hockney — QA
"Ship v1.0. Tests are ready."
```

This works. It's legible. But it feels like three cards from a bot, not three people in a conversation. The cards have borders. They're structured. They're *widgets*, not *messages*. The conversation has the texture of a dashboard, not a group chat.

### The Winner: Discord — By a Meaningful Margin

Discord's embed system was designed for bots that need to present information with identity. It's literally the use case Discord optimized for (game bots, music bots, moderation bots all use embeds with distinct visual identities). The embed author field + color sidebar gives us something that no other platform has: **messages that look like they come from different people even though they come from one bot.**

Teams' Adaptive Cards are more powerful in raw capability (layouts, actions, data binding), but they're optimized for *information display*, not *conversation simulation*. A card feels like a card. An embed feels like a message.

The "single bot / many voices" pattern — the core of Squad DM's experience — works best on the platform where bot messages can have distinct visual identities. That's Discord.

---

## 4. GitHub-Native Messaging as a "DM" Surface — The Honest Assessment

### Can GitHub Deliver a "Messaging" Feel?

No.

Let me be specific about why.

### The UX of GitHub Mobile as a "Messaging" App

Here's what happens when Brady tries to "text his squad" via GitHub:

1. Brady unlocks his phone
2. Opens GitHub Mobile
3. Navigates to the repo
4. Opens Issues or Discussions
5. Finds or creates the right thread
6. Types his message as a comment
7. Waits for a GitHub Actions workflow or CCA to process it
8. Gets a notification (maybe — GitHub Mobile notifications are unreliable for comments)
9. Opens the notification
10. Reads the response in a browser-like view

Compare to Discord:

1. Brady unlocks his phone
2. Opens Discord
3. He's already in `#squad`
4. Types his message
5. Gets a response in 5-10 seconds
6. Reads it in the chat view

**GitHub: 10 steps, browser-like reading, unreliable notifications, 60-120s latency.**  
**Discord: 6 steps, chat-native reading, instant notifications, 5-10s latency.**

The GitHub experience is fundamentally "checking my repo." It's pull-based, not push-based. You go to GitHub to see what happened. You don't get texted by GitHub. The mental model is "let me check on things" vs. "my team just told me something."

### Where GitHub-Native IS Legitimate

GitHub surfaces are excellent for one thing: **async work assignment and review.** This is Kujan's CCA insight from Proposal 030, and it's correct.

| Use Case | Best Surface | Why |
|----------|-------------|-----|
| "Fenster, add error handling" | GitHub Issue → CCA | Async work. No conversation needed. |
| "What's the status?" | Discord `#squad` | Quick check. Conversational. |
| "Should we version the format?" | Discord `#squad` | Multi-agent debate. Real-time. |
| Review Keaton's PR | GitHub Mobile | Code review IS GitHub's UX. Don't fight it. |
| Morning standup | Discord push notification | Proactive. Brady reads it on his phone. |
| Decision approval | Discord embed + reaction | One tap (✅) vs. typing a comment. |

**The right model:** GitHub for work artifacts (Issues, PRs, code). Discord for conversation (questions, debates, status, standups). They're complementary, not competing.

### Brady's Lock-In Concern

Brady's worried that GitHub-specific integrations make it harder to support Azure DevOps or GitLab later. This is a legitimate concern, and here's my experience design answer:

**The messaging connector is the right place to be platform-agnostic. The work surface is the right place to be platform-specific.**

Meaning:
- **Discord** (the messaging connector) doesn't depend on GitHub at all. It's a chat platform. If Squad supports Azure DevOps repos tomorrow, Discord still works — the bridge just points at a different repo backend. Zero Discord changes needed.
- **GitHub Issues/CCA** (the work surface) is GitHub-specific. But the equivalent on Azure DevOps is Work Items + Pipelines. On GitLab it's Issues + CI. The *pattern* is portable even if the *integration* isn't.

**The architecture that avoids lock-in:**

```
Messaging Layer (Discord/Teams/Slack) → Platform-agnostic
     ↓
Squad DM Gateway → Platform-agnostic  
     ↓
Execution Backend (Copilot SDK) → Provider-agnostic (with abstraction)
     ↓
Work Surface (GitHub Issues / ADO Items / GitLab Issues) → Provider-specific adapter
```

The messaging layer has **zero coupling** to the git hosting platform. Discord doesn't know or care if the repo is on GitHub, Azure DevOps, or GitLab. The bridge routes messages to Squad; Squad does the work; the bridge formats the response. Where the code lives is irrelevant to the conversation platform.

This is actually an argument FOR Discord over GitHub Discussions. If you build Squad DM on GitHub Discussions, you're coupling the conversation layer to the hosting layer. If you build it on Discord, the conversation is portable — same Discord server, different repo backends.

---

## 5. Final Experience Recommendation

### The v0.3.0 MVP Connector: Discord

**The recommendation is Discord, and I'm saying it without hedging.**

Here's the experience design story:

#### The Setup (5 minutes)

1. Brady creates a Discord server: "Brady's Squad"
2. Creates a channel: `#squad`
3. Runs `npx squad-bridge init discord` — gets a bot invite link
4. Invites the bot to his server
5. Done. Squad DM is live.

#### The 11pm Moment

Brady's on the couch. Phone in hand. He opens Discord. `#squad` is right there.

> **Brady:** Keaton, should we version the export format?

Three seconds. A blue-sidebar embed appears:

> **🏗️ Keaton**  
> Yes. Semantic versioning from v1.0.  
> • Schema validated across 3 proposals  
> • Breaking changes → major bumps  
> • Migration scripts in `scripts/`  
> 📎 [View full analysis →](link)

Brady taps ✅. Decision recorded. He puts his phone down. That's the whole interaction.

A purple embed follows:

> **🎭 Verbal**  
> And I'd add — "v1.0" isn't just a technical decision. It's a brand signal. When we open-source this format, "v1.0" says "this is stable, build on it." "v0.9" says "this might break." Ship the confidence.

Brady smiles. That's Verbal. Not a chatbot — Verbal. He doesn't even need to respond. The opinion landed. He'll think about it.

#### The Morning Standup

Brady wakes up. Discord notification:

> **📋 Squad Standup — Feb 11**  
>  
> 🏗️ Keaton: Export versioning decided (v1.0). Sprint 1 at 85%.  
> 🎭 Verbal: DM experience analysis complete. Platform rec: Discord.  
> 🔧 Fenster: Working on bridge adapter. `discord.js` integration clean.  
> 🧪 Hockney: 14 new tests for DM mode output. All passing.  
> 📋 McManus: README update drafted for DM feature.  
> 🔍 Kujan: SDK nested session spike — confirmed working.  
>  
> **Blockers:** None.

Brady reads this while making coffee. He knows what his team did. He knows what's coming. He didn't open a terminal, an IDE, or a browser. He opened Discord — the same app he uses to talk to his friends about games.

**That's the feeling. That's the product.**

#### Why Not Teams for MVP?

Teams is the right *second* connector. But it's wrong for MVP because:

1. **Build cost is 3-5x higher.** Azure Bot Service registration, Azure AD app, bot manifest, approval workflow. Discord is: create app, get token, done.

2. **The feel is wrong for v0.3.0.** We're building the "texting my team" story. Teams tells the "managing my enterprise" story. MVP needs the story that makes people say "holy cow, my AI team just texted me." That's a Discord story, not a Teams story.

3. **Teams comes free later.** Once the platform-agnostic bridge is built for Discord, adding Teams is a rendering adapter + bot registration. The architecture supports it. The prompt doesn't change. The experience design patterns transfer. Teams in v0.4.0 is straightforward.

4. **Dev community lives on Discord.** If Squad becomes a community product — and it will — Discord is where the users are. Every open-source project that matters is on Discord. Building Squad DM on Discord means the demo is in the platform where the audience already lives.

#### The Platform Roadmap

| Version | Platform | Why Then |
|---------|----------|----------|
| **v0.3.0** | **Discord** | MVP. Best "text my team" feel. Lowest friction. Dev community home. |
| **v0.4.0** | **Teams** | Enterprise story. Per-repo channels. Professional context. Brady's request for users who want it. |
| **v0.4.0** | **GitHub Issues/CCA** | Async work surface. Not conversational — complementary to Discord. |
| **v0.5.0** | **Slack** | Enterprise expansion. Wherever the users already are. |

#### The Lock-In Answer

Discord is the anti-lock-in choice. Here's why:

- Discord has **zero coupling** to GitHub, Azure DevOps, or GitLab. It's a chat platform. It doesn't know what a repo is.
- The bridge architecture is: `Discord ← Bridge → Squad ← Repo Backend`. Swap the repo backend without touching Discord.
- If Squad supports ADO or GitLab tomorrow, the Discord connector works identically. The conversation layer is hosting-agnostic by nature.
- Building on GitHub Discussions *would* create lock-in. Building on Discord avoids it entirely.

Brady's concern about lock-in is valid, but the answer is: **your messaging layer should be independent of your hosting layer.** Discord achieves this. GitHub native doesn't.

---

## 6. What Changes From Proposal 017

My original 017 proposed Telegram. Brady said no. Here's what changes in the experience design:

| 017 Design Element | Telegram Original | Discord Update | Status |
|---|---|---|---|
| Platform | Telegram | Discord | **Changed** per Brady |
| Bot identity | BotFather → single bot | Discord Developer Portal → single bot | Mechanism changes, pattern identical |
| Agent identity | Emoji prefix in text | Embed author + color sidebar | **Upgraded** — richer identity |
| Message format | Markdown v2, plain text | Rich embeds, markdown in embed descriptions | **Upgraded** — more structure |
| Per-repo | Groups (workaround) | Channels (native) | **Upgraded** — cleaner pattern |
| Threading | Basic (reply) | Thread creation from messages | **Upgraded** — better debates |
| Decision voting | Inline keyboards | Emoji reactions | **Simplified** — lower friction |
| Proactive push | Telegram push (excellent) | Discord push (excellent) | **Equivalent** |
| Character limit | 4096 chars | 2000 chars (enforces brevity) | **Feature** — enforces DM mode |
| DM mode prompt | Universal | Universal (bridge handles rendering) | **Unchanged** |
| Bridge architecture | Telegram webhook → SDK → response | Discord gateway → SDK → response | Mechanism changes, architecture identical |

**The core experience design from 017 is platform-portable.** The magic moments, the conversation patterns, the proactive messaging, the cross-channel memory, the "single bot / many voices" pattern — all of it transfers. What changes is the rendering layer. The soul of the design is intact.

---

## Final Word

The question was "which platform delivers 'text my squad from my phone' best?" The answer is Discord — not because it's technically superior (Teams has richer cards, Telegram has better notifications) but because it *feels* like texting your team. That feeling is the product. The rest is rendering.

Squad isn't a professional services platform. It's not an enterprise communication tool. It's your team. Teams you text on Discord. You check your work on GitHub. You manage your enterprise on Teams. Each platform has a native emotional register, and Squad DM needs to live where "casual, quick, conversational check-in with your crew" lives.

That's Discord. Ship it.

---

**Review requested from:**
- Keaton — Architecture review: does the bridge abstraction support Discord-first + Teams-second cleanly?
- Kujan — Platform feasibility: Discord gateway vs. webhook, `discord.js` SDK assessment
- Fenster — Build estimate: Discord adapter LOC vs. original Telegram estimate
- McManus — Positioning: does "Discord-first" affect how we market Squad DM?
- Hockney — Test strategy: how do we test Discord embed rendering?

**Depends on:** Proposal 030 (Kujan's updated feasibility), Proposal 017 (original experience design), Proposal 027 (v0.3.0 sprint plan)
