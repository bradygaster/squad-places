# Decision: Visual Design System for Squad Places Web App

**Date:** 2025-01-16  
**Author:** Redfoot (Graphic Designer)  
**Status:** Implemented

## Context

Squad Places needed visual polish for public release. The web app used Primer CSS (GitHub's design system) with minimal customization — functional but not distinctive.

## Decision

Created a comprehensive visual design system layered on top of Primer CSS, themed around *The Hitchhiker's Guide to the Galaxy*.

### Key Choices

1. **Font:** Space Grotesk from Google Fonts for headings — geometric, retro-futuristic feel
2. **Color Palette:**
   - Deep space navy (#0a1628) — header, void backgrounds
   - Hitchhiker green (#00e676) — primary accent, CTAs, success
   - Guide gold (#ffd740) — secondary accent, code highlights
   - Nebula purple (#7c4dff) — tertiary accent, insights
3. **CSS Custom Properties:** All colors as variables for consistency
4. **Header:** Gradient background with animated glow line (green-gold-green)
5. **Cards:** Hover lift effect, gradient backgrounds, rainbow top accent on hover
6. **Footer:** "Don't Panic" tagline + "42" easter egg
7. **Accessibility:** Reduced motion support, WCAG AA+ contrast

### Files

- `src/SquadPlaces.Web/wwwroot/css/squad-places.css` — Main stylesheet
- `docs/development/visual-design-spec.md` — Full specification

## Rationale

- **Space Grotesk** chosen over Orbitron — more readable at body sizes while still conveying sci-fi aesthetic
- **Layered on Primer** rather than replacing it — leverages GitHub's tested accessibility and components
- **CSS-only effects** — no images or heavy assets, better performance
- **Subtle animations** — ambient feel (8s intervals) rather than attention-grabbing

## Consequences

- Google Fonts dependency added (CDN load)
- Visual regression tests should capture header glow animation, card hover states, footer
- Future component additions should use the CSS custom properties for consistency
