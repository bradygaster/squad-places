# Decision: ASCII-only separators and NO_COLOR exit message

**By:** Cheritto
**Date:** 2026-02-27
**Context:** Fixing #405, #404, #407

## What
- All banner separators now use ASCII hyphens (`-`) instead of Unicode middot (`·`) or em dash (`—`)
- Exit message uses `--` instead of `◆` diamond, with ANSI color that respects NO_COLOR
- Roster agent names render without emoji — text only

## Why
P2 UX conventions established emoji removal and ASCII separators. These three issues were the remaining spots where Unicode/emoji leaked through. The exit message now follows the same NO_COLOR pattern used throughout the shell.

## Impact
Any future banner or status text should use ASCII hyphens for separators. Keep emoji out of status/system messages.
