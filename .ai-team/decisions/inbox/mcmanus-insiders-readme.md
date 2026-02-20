# Decision: Expanded Insiders Program Section in README

**Author:** McManus (DevRel)  
**Requested by:** Brady  
**Date:** 2025

## What Changed

Expanded the "Insider Program" section in README.md (lines 365–386) from a brief mention to a full, actionable guide for new and existing Squad users.

## Why

The original README had only 8 lines on insiders with a reference to a non-existent external doc (`docs/insider-program.md`). Users needed clear, in-README guidance on:
1. How to install the insider build (`npx github:bradygaster/squad#insider`)
2. How to upgrade existing squadified repos (`npx github:bradygaster/squad#insider upgrade`)
3. What gets preserved during upgrade (`.ai-team/` state)
4. What to expect (pre-release, may be unstable)
5. Release tagging and how to pin versions

## What's Included

- **Install command** — `npx github:bradygaster/squad#insider`
- **Upgrade command** — `npx github:bradygaster/squad#insider upgrade`
- **Preservation guarantee** — `.ai-team/` (team.md, agents, decisions, casting) is never touched
- **Stability caveat** — "may be unstable, intended for early adopters and testing"
- **Release tags** — explains pre-release format (e.g., `v0.4.2-insider+abc1234`)
- **Pinning versions** — how to target specific tagged releases
- **Links** — insider branch on GitHub + bug reporting in CONTRIBUTORS.md

## Tone & Placement

Kept Squad's confident, developer-friendly voice. Placed right after the regular `upgrade` section since they're related workflows (install → upgrade, regular → insider upgrade). No nested docs — all essential info is in-README.

## Validation

- Section reads naturally after "### Upgrade"
- Commands are copy-paste ready
- Preserves consistency with existing README prose style
- Addresses all key facts Brady requested
