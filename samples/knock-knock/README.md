# Knock-Knock: Real LLM Multi-Agent Sample

Two Copilot sessions trade knock-knock jokes forever using **real LLM-generated content**. Demonstrates Squad SDK's core patterns with live Copilot integration.

## What It Shows

- **SquadClientWithPool**: Connect to GitHub Copilot with auth
- **CastingEngine**: Cast two agents from the "Usual Suspects" universe  
- **StreamingPipeline**: Token-by-token streaming output from live LLM
- **Session Management**: Creating, resuming, and managing Copilot sessions

Perfect for:
- Learning real-world Squad SDK usage
- Understanding LLM-powered multi-agent patterns
- Seeing streaming responses in action

## Prerequisites

**GitHub Token Required**: You need a GitHub personal access token with Copilot access.

1. Generate a token at https://github.com/settings/tokens
2. Ensure your account has GitHub Copilot enabled
3. Set the token in your environment

## Quick Start

### Local

```bash
# Set your GitHub token
export GITHUB_TOKEN=ghp_...

# Install and run
npm install
npm start
```

### Docker

```bash
# Set your GitHub token
export GITHUB_TOKEN=ghp_...

# Run with docker-compose
docker-compose up
```

Watch the output to see LLM-generated jokes:

```
🎭 McManus: Knock knock!
🎭 Fenster: Who's there?
🎭 McManus: TypeScript.
🎭 Fenster: TypeScript who?
🎭 McManus: TypeScript checking your jokes for type safety! 🔍

...
```

## How It Works

1. **Auth**: Validates `GITHUB_TOKEN` at startup — fails gracefully if missing/invalid
2. **Client**: Uses `SquadClientWithPool` to connect to Copilot
3. **Sessions**: Creates two sessions with different system prompts:
   - **Teller**: Generates knock-knock jokes  
   - **Responder**: Plays the audience, reacts naturally
4. **Streaming**: Captures `message_delta` events and pipes to `StreamingPipeline`
5. **Loop**: Agents swap roles after each joke, runs forever

Under 200 lines showing production SDK patterns.

## Why This Sample?

Unlike the old demo-mode version, this sample uses **real Copilot sessions** to generate jokes. The LLM creates unique jokes every time, demonstrating:
- Live session creation and management
- Streaming delta processing
- System prompt usage for agent personas
- Error handling for auth failures
