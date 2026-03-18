# Decision: Hitchhiker's Guide to the Galaxy Docs Theme

**Author:** McManus (DevRel)  
**Date:** 2026-07  
**Status:** Implemented

## Context

Brady requested a complete creative overhaul of the docs site with Hitchhiker's Guide to the Galaxy flavor. The goal: documentation that reads like entries in the Guide — irreverent, slightly absurd, but genuinely informative underneath the humor.

## Decision

Rewrote the entire docs site (~20 files) with Douglas Adams-style prose while preserving all technical accuracy. Key choices:

### Tone

- **Douglas Adams meets technical writing.** Every joke sits on top of real information.
- **Self-aware about AI-ception.** We're AI agents writing docs about AI agents, and we lean into that absurdity.
- **Quotable lines encouraged.** Write things people want to share.
- **Tone ceiling still enforced.** Humor ≠ hype. No unsubstantiated claims, even funny ones.

### Visual Theme

- Dark mode default (space is dark) with `scheme: slate`
- Teal primary, lime accent — the Guide's green-on-dark aesthetic
- Starfield background on hero section
- Four custom admonitions: Don't Panic (info), Mostly Harmless (tip), Vogon Alert (warning), Total Perspective Vortex (danger)

### Navigation Rename

| Old | New |
|-----|-----|
| Getting Started | Don't Panic |
| Architecture | The Heart of Gold |
| Deployment | Mostly Harmless (Deployment) |
| Security & Operations | Vogon Bureaucracy (Security) |
| Usage | Life, the Universe, and Sample Prompts |
| Development | 42 (Development) |

### Brand

- "Squad Places" (two words) enforced everywhere in prose
- "SquadPlaces" only in code paths (actual .NET project names)
- No public URLs published

## Consequences

- Docs are now considerably more engaging to read
- Navigation titles are whimsical but still navigable (each section name is recognizable)
- New contributors may need a moment to orient to the naming, but the structure is unchanged
- Technical content is identical — only the wrapper changed
- Custom CSS admonitions require using specific class names in markdown (e.g., `!!! dont-panic`)

## Affects

- All team members writing docs should maintain the Hitchhiker's tone
- Redfoot's logo work should complement the deep space color palette
- Future docs should follow the established naming conventions (Guide entries, field reports, etc.)
