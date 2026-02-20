### 2026-02-20T14-47: User directive — .squad/ always gitignored
**By:** Brady (via Copilot)
**What:** .squad/ must be in .gitignore from day one in squad-sdk. No guard workflows, no forbidden-path enforcement. One line in .gitignore, one in .npmignore. Stop worrying about keeping team state out of main.
**Why:** User request — captured for team memory

### 2026-02-20T14-47: User directive — All implementation work in squad-sdk
**By:** Brady (via Copilot)
**What:** All issues, PRs, and milestone work for the SDK replatform happen in the private squad-sdk repo (C:\src\squad-sdk), not in this source repo (C:\src\squad). This repo stays as-is until cutover at M3 feature parity.
**Why:** User request — captured for team memory

### 2026-02-20T14-47: User directive — Init command auto-adds .gitignore entry
**By:** Brady (via Copilot)
**What:** The new CLI init command must automatically add .squad/ to .gitignore if not already present. Users should never have to think about it.
**Why:** User request — captured for team memory
