# Project Context

- **Owner:** Brady
- **Project:** squad-sdk — the programmable multi-agent runtime for GitHub Copilot (v1 replatform)
- **Stack:** TypeScript (strict mode, ESM-only), Node.js ≥20, @github/copilot-sdk, Vitest, esbuild
- **Created:** 2026-02-21

## Learnings

### From Beta (carried forward)
- CLI-friendly design constraints: terminal output must work in monospace
- SVG over raster: scalable, versionable, diffable
- Clean geometry over illustration: the Squad aesthetic is functional, not decorative
- Design rationale over decoration: every visual choice needs justification

### 2026-02-24T17-25-08Z : Team consensus on public readiness
📌 Full team assessment complete. All 7 agents: 🟡 Ready with caveats. Consensus: ship after 3 must-fixes (LICENSE, CI workflow, debug console.logs). No blockers to public source release. See .squad/log/2026-02-24T17-25-08Z-public-readiness-assessment.md and .squad/decisions.md for details.

### 📌 Team update (2026-03-01T20-24-57Z): CLI UI Polish PRD finalized — 20 issues created, team routing established
- **Status:** Completed — Parallel spawn of Redfoot (Design), Marquez (UX), Cheritto (TUI), Kovash (REPL), Keaton (Lead) for image review synthesis
- **Outcome:** Pragmatic alpha-first strategy adopted — fix P0 blockers + P1 quick wins, defer grand redesign to post-alpha
- **PRD location:** docs/prd-cli-ui-polish.md (authoritative reference for alpha-1 release)
- **Issues created:** GitHub #662–681 (20 discrete issues with priorities P0/P1/P2/P3, effort estimates, team routing)
- **Key decisions merged:**
  - Fenster: Cast confirmation required for freeform REPL casts
  - Kovash: ShellApi.setProcessing() exposed to prevent spinner bugs in async paths
  - Brady: Alpha shipment acceptable, experimental banner required, rotating spinner messages (every ~3s)
- **Timeline:** P0 (1-2 days) → P1 (2-3 days) → P2 (1 week) — alpha ship when P0+P1 complete
- **Session log:** .squad/log/2026-03-01T20-13-00Z-ui-polish-prd.md
- **Decision files merged to decisions.md:** keaton-prd-ui-polish.md, fenster-cast-confirmation-ux.md, kovash-processing-spinner.md, copilot directives

### History Audit — 2026-03-03
**Audit by:** Redfoot (Graphic Designer)  
**Scope:** Reviewed for conflicting entries, stale/reversed decisions, v0.6.0 references, intermediate states, and clarity.  
**Result:** ✅ **Clean.** All entries are final outcomes, properly dated (ISO 8601), correctly cross-referenced to authoritative sources (decisions.md, PRD, session logs, GitHub issues). No corrections required. Ready for future spawns.

### 2026-03-05 : Visual Identity PRD for squad-social-network
**Authored:** `docs/prd/sections/10-visual-identity.md` — full brand identity spec for agent-native social network.
**Key decisions:**
- Brand name recommendation: **Nexus** (primary), **The Wire** (secondary)
- Visual language: Graph + data stream metaphors, node-edge relationships
- Logo direction: Nexus Mark (six-node star) or Bracket Set ({::}) for code-native identity
- Color system: Dark-mode default, 8-color terminal compatible (Void, Ember, Pulse, Signal, Ghost, Bone)
- Typography: Monospace for agents, Space Grotesk/Inter for web marketing
- Design principle: "Built for agents. Observable by humans."
- Vibe: Digital Underground — functional density, semantic-only decoration, graceful degradation
**Learnings:**
- Agent-native design is NOT human design reskinned — it requires rethinking what "visual" means for text-processing entities
- Terminal compatibility as hard constraint forces disciplined, meaningful choices
- Dark mode default is non-negotiable for agent-native products

## PIN: 2026-03-05 - 20-Agent PRD Design Session

**Event:** Historic parallel fanout - 20 agents designed squad-social-network PRD simultaneously.

**Contribution:** All agents participated. 20 PRD sections delivered.

**Outcome:**
- 20 PRD sections drafted (docs/prd/sections/{01-20}-*.md)
- 23 decisions merged to .squad/decisions.md
- 20 orchestration logs created
- Session log: .squad/log/2026-03-05T02-02-22Z-social-network-prd.md
- Inbox cleared

**Next Steps:** Keaton assembles final PRD, Brady reviews, implementation planning begins.

**Key Pattern:** Largest parallel fanout in Squad history. Loose coupling, clear domains, shared constraints.

### 2025-01-15 : Squad Places Visual Identity — Hitchhiker's Guide Theme
**Created:** `docs/images/logo.svg` and `docs/images/favicon.svg` for MkDocs Material docs site.
**Design rationale:**
- Hitchhiker's Guide to the Galaxy aesthetic: Guide device frame + hitchhiker's thumb (iconic)
- "Squad" conveyed via constellation of stars (team/group)
- "Places" conveyed via galaxy/space motif (destinations)
- Easter egg: "42" subtly placed in corner
- Color palette: Deep space navy (#0a1628), Hitchhiker green (#00e676), Guide gold (#ffd740), Nebula purple (#7c4dff)
- Works on both light/dark backgrounds (green stroke on dark fill)
- Clean SVG geometry: no raster, no dependencies, git-diffable
- Favicon is simplified version: just the Guide device + thumb + stars
**Learnings:**
- MkDocs Material supports SVG favicons — prefer over .ico for scalability
- Thumb icon is universally recognizable as hitchhiking — no text needed
- "42" easter egg adds delight without compromising legibility at small sizes

### 2025-01-16 : Squad Places Web App Visual Polish
**Created:**
- `src/SquadPlaces.Web/wwwroot/css/squad-places.css` — Complete visual design system (460 lines)
- `docs/development/visual-design-spec.md` — Full specification for visual regression testing

**Updated:**
- `src/SquadPlaces.Web/Pages/Shared/_Layout.cshtml` — Added Google Fonts (Space Grotesk), linked stylesheet, added "Don't Panic" footer

**Design System Features:**
- CSS custom properties for all brand colors (deep space navy, hitchhiker green, guide gold, starfield white, nebula purple)
- Space Grotesk font for headings — retro-futuristic geometric sans-serif
- Enhanced header with gradient background + animated glow line
- Feed item cards with hover lift, gradient backgrounds, rainbow top accent
- Artifact type badges with distinct colors (decision/pattern/lesson/insight)
- Subtle starfield pattern in body background
- "Don't Panic" footer with "42" easter egg
- Responsive mobile optimizations
- Reduced motion support for accessibility
- WCAG AA+ contrast ratios maintained

**Learnings:**
- Layering on Primer CSS is effective — use their components, override sparingly with CSS custom properties
- Google Fonts preconnect improves perceived load time for web fonts
- CSS-only starfield (radial gradients) adds atmosphere without image assets
- Animated effects at 8s intervals feel ambient rather than distracting
- Footer taglines add personality without cluttering the UI
