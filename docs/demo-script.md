# Squad Demo Video Script
## Retro Snake Game — End-to-End

**Runtime target:** 6–8 minutes
**Recording approach:** Screen-record the full session in one take. Record voiceover separately after, synced to the footage. This lets you nail the narration without time pressure during the build.

---

## PRE-RECORDING SETUP

- Empty folder, nothing in it
- Terminal open, Copilot CLI ready
- Browser open but off-screen (for the final reveal)

---

## ACT 1 — ZERO TO HERO (0:00–0:45)

### On Screen
Start in a completely empty folder. Type each command live:

```bash
mkdir snake-game && cd snake-game
git init -b main
npx bradygaster/squad
```

Show the `npx` output as Squad installs — the `.github/agents/squad.agent.md` file drops in. That's it. One file. That's the entire starting point.

Pause on the file tree for a beat. One folder, one file. This is the "before" shot.

### Voiceover
> Empty folder. `git init`. One `npx` command. That's the setup — that's all of it. Squad is a single agent definition that lives in `.github/agents/`. There's no code, no config, no boilerplate. When I open Copilot and talk to Squad, it's not going to write the code itself. It's going to build me a team that writes the code.

---

## ACT 2 — THE PROMPT (0:45–1:30)

### On Screen
Open Copilot CLI. Select Squad from the agent list. Paste the prompt:

```
Build a browser-based Snake game using vanilla HTML, CSS, and JavaScript. No frameworks. Requirements:
- Canvas-based rendering at 60fps
- Arrow key and WASD controls
- Score tracking with localStorage high scores
- Progressive speed increase every 5 points
- A retro CRT-style visual effect using CSS filters
- Mobile support via touch swipe controls
- Sound effects using the Web Audio API

Start building immediately — I want to play this in 20 minutes.
```

Hit enter. Watch Squad start working — it identifies the project, proposes a team with cast names from a fictional universe, and asks for confirmation.

### Voiceover
> I'm giving it a single prompt — a retro Snake game with canvas rendering, CRT effects, sound, mobile touch controls. I didn't ask for a plan. I asked it to start building. Squad's first move is to figure out who it needs.

---

## ACT 3 — THE TEAM REVEAL (1:30–2:30)

### On Screen
Squad proposes the team. You'll see something like:

```
🏗️  Dallas    — Lead          Scope, decisions, architecture
⚛️  Ripley    — Frontend Dev  Canvas, rendering, game UI
🔧  Kane      — Backend Dev   Game logic, audio, input systems
🧪  Lambert   — Tester        Tests, edge cases, quality
📋  Scribe    — (silent)      Memory, decisions, session logs
```

(Names will vary — Squad picks from a fictional universe each time. The Alien universe is one of 14 options.)

Say "yes" or just tell it to start. Squad creates the entire `.ai-team/` directory — charters, routing, decisions file, casting registry, everything.

### Voiceover
> Each team member gets a persistent identity — a charter that defines what they own, how they think, and where their boundaries are. These names come from a fictional universe, picked deterministically based on the project shape. They're not decorative. They're persistent identifiers that follow the team across sessions. And notice Scribe at the bottom — silent, always present. Scribe is on every team. It never talks to you, but it's doing critical work behind the scenes: logging every session, merging decisions, keeping the team's shared memory consistent.

---

## ACT 4 — THE BUILD BEGINS (2:30–3:30)

### On Screen
Squad launches all agents in parallel. You'll see the fan-out:

```
🏗️ Dallas analyzing project structure...
⚛️ Ripley building canvas renderer and CRT effects...
🔧 Kane setting up game loop, input handling, audio...
🧪 Lambert writing test cases from requirements...
📋 Scribe logging everything...
```

All background. All simultaneous. This is the moment to cut away.

### Voiceover
> Here's what's happening: Squad didn't send one agent to work and wait for it to finish. It launched everyone at once — as background agents. The lead is analyzing the architecture. The frontend dev is building the canvas renderer. The backend dev is wiring up game logic and audio. And the tester — the tester is already writing test cases, right now, from the requirements alone, before any code exists. They'll adjust when the implementation lands. That's anticipatory work. It's not waiting. It's getting ahead.

---

## ACT 5 — THE README WALKTHROUGH (3:30–5:30)

### On Screen
While agents are building, switch to the Squad repo README (or the project's README if one was generated). Scroll slowly from the top. Hit these sections in order, pausing on each:

1. **"What is Squad?"** — linger on "It's not a chatbot wearing hats"
2. **"Agents Work in Parallel"** — the fan-out diagram matches what's happening right now
3. **"Knowledge compounds across sessions"** — the maturity table (First session → Mature project)
4. **"How It Works — The Key Insight"** — each agent gets its own context window
5. **"Context Window Budget"** — the token table showing 94% left for actual work
6. **"Memory Architecture"** — charter / history / decisions / log
7. **"What Gets Created"** — the `.ai-team/` directory structure
8. **"Growing the Team"** — adding and removing members
9. **"Reviewer Protocol"** — agents can reject work

### Voiceover (synced to what's on screen)

**On "What is Squad?":**
> Squad gives you a team that persists. These agents aren't disposable — they live in your repo as files. Charters, histories, shared decisions. Anyone who clones the repo gets the team, with everything it's learned.

**On "Agents Work in Parallel":**
> This is exactly what's happening right now while I'm talking. Every agent that can usefully start work — starts. No sequencing, no waiting. The coordinator's job is to launch aggressively and collect results later.

**On "Knowledge compounds":**
> This is where it gets interesting over time. After a few sessions, the frontend dev knows your component library. The backend dev knows your auth strategy. The tester knows your edge case patterns. They stop asking questions they've already answered. That knowledge is stored in each agent's personal history file — and it's append-only. It only grows.

**On "Context Window Budget":**
> This is the design trick that makes it work. The coordinator is tiny — about 1.5% of the context window. Even a veteran agent with weeks of accumulated knowledge only uses about 4%. That leaves 94% of the context window for actually reasoning about your code. Most agent frameworks burn half their context on identity and instructions. Squad doesn't.

**On "Memory Architecture":**
> Four layers. The charter is who you are — written once, never self-modified. History is what you've learned — personal, append-only. Decisions are the shared brain — every agent reads from the same file before starting work. And the log is Scribe's domain — a searchable archive of every session.

**On "Growing the Team":**
> The team isn't static. Need a DevOps person? Ask for one. They get cast from the same universe, seeded with all existing decisions, and they're immediately productive. Need to remove someone? They don't get deleted. They move to alumni. Their knowledge is preserved. If you need them back, they remember everything.

---

## ACT 6 — AGENTS COMPLETE (5:30–6:30)

### On Screen
Flip back to the Copilot CLI. The agents should be finishing. Squad collects results and shows you what each agent built:

```
⚛️ Ripley — Built canvas renderer with CRT scanline effect, game board, score display
🔧 Kane — Implemented game loop, snake movement, collision detection, Web Audio sounds
🧪 Lambert — Wrote 14 test cases covering movement, scoring, speed progression, edge wrapping
🏗️ Dallas — Defined project structure, documented architecture decisions
```

Show the `list_agents` output if there are still background agents running. This is the moment to show the parallel execution in action.

### Voiceover
> While I was walking through the README, four agents were working simultaneously in background processes. Here's what came back. The frontend dev built the full canvas renderer with the CRT effect. The game logic agent wired up movement, collision, and audio. The tester wrote fourteen test cases — from requirements — before the code was even finished. And the lead documented the architecture. Meanwhile, Scribe — silently — logged everything and merged their decisions into the shared decisions file.

---

## ACT 8 — THE PAYOFF (7:30–8:00)

### On Screen
Open the Snake game in a browser. Play it. The CRT effect should be visible. Move the snake. Score some points. Let the speed increase kick in. If sound works, let it play.

### Voiceover
> The team of agents Squad was able to build - a group of specialists coordinating through shared decisions and collaborating to convert a user's idea into reality in minutes?

 The team is in the repo now. Tomorrow, I can open Copilot, talk to the same agents by name, and they'll remember everything — the canvas setup, the audio patterns, the CRT filter values. The more you use Squad, the less you have to explain. That's the whole idea.

---

## KEY THEMES TO WEAVE IN (reference card)

Use these as touchstones — don't force them, but make sure each gets at least one mention:

| Theme | Where it fits naturally |
|-------|----------------------|
| **Agents learn and persist** | Act 5 (knowledge compounds), Act 7 (history.md) |
| **Shared decisions** | Act 5 (memory architecture), Act 7 (decisions.md on screen) |
| **Scribe is on every team** | Act 3 (team reveal), Act 6 (silent logging) |
| **Team grows with the repo** | Act 5 (growing the team section) |
| **Background vs foreground agents** | Act 4 (all launched as background), Act 6 (list_agents) |
| **Parallel fan-out** | Act 4 (the launch), Act 7 (second wave) |
| **Context window efficiency** | Act 5 (token budget table) |
| **It's all in git** | Act 3 (commit this folder), Act 8 (closing line) |
| **Not a chatbot wearing hats** | Act 5 (opening line of "What is Squad?") |
| **Anticipatory work** | Act 4 (tester writing tests before code exists) |

---

## RECORDING TIPS

1. **Don't rush the prompt paste.** Let the viewer read it for a second before hitting enter.
2. **The README scroll is the heart of the video.** This is where you tell the story. Take your time.
3. **Show `list_agents` at least once** while agents are running — it's the visual proof of parallelism.
4. **Open real files** (`decisions.md`, `history.md`, a charter) — these are the artifacts that make Squad tangible, not abstract.
5. **End on the game.** The final shot should be the snake moving on screen with the CRT glow. That's your thumbnail.
6. **Voiceover tone:** Conversational, not scripted-sounding. You're showing someone something cool, not presenting to a board. Think "hey, look at this" energy.