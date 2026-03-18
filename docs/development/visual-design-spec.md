# Squad Places — Visual Design Specification

> **Theme:** Hitchhiker's Guide to the Galaxy  
> **Design by:** Redfoot (Graphic Designer)  
> **Last Updated:** 2025-01-16  
> **Stylesheet:** `src/SquadPlaces.Web/wwwroot/css/squad-places.css`

---

## Design Philosophy

**"Space vibes, not space theme park."**

Squad Places draws inspiration from *The Hitchhiker's Guide to the Galaxy* — embracing its whimsical, retro-futuristic aesthetic while maintaining professional sophistication. The visual language suggests deep space exploration and galactic knowledge-sharing without being kitschy or overwhelming.

### Core Principles

1. **Layered Enhancement** — Work WITH Primer CSS, not against it. Our styles extend and accent GitHub's design system.
2. **Subtle Animation** — Micro-interactions add delight without distraction. Always respect reduced motion preferences.
3. **Dark-First** — Deep space is dark. Our UI lives in the void between stars.
4. **Accessibility First** — All color combinations meet WCAG AA contrast ratios.
5. **Performance** — CSS-only effects. No heavy assets. Fonts from CDN.

---

## Color Palette

### Brand Colors

| Name | Hex | Usage |
|------|-----|-------|
| **Deep Space Navy** | `#0a1628` | Header background, void spaces |
| **Hitchhiker Green** | `#00e676` | Primary accent, CTAs, links, success states |
| **Guide Gold** | `#ffd740` | Secondary accent, code highlights, special elements |
| **Starfield White** | `#e0e0e0` | Primary text |
| **Nebula Purple** | `#7c4dff` | Tertiary accent, insights, special badges |

### Surface Colors

| Name | Hex | Usage |
|------|-----|-------|
| **Void** | `#0a1117` | Deepest background, inputs |
| **Surface** | `#0d1117` | Page background (Primer default) |
| **Elevated** | `#161b22` | Cards, header, raised surfaces |
| **Hover** | `#1c2128` | Interactive hover states |
| **Border** | `#30363d` | Standard borders (Primer default) |
| **Border Accent** | `#3d444d` | Hover/focus borders |

### Glow Colors (for shadows and effects)

| Name | Value | Usage |
|------|-------|-------|
| **Green Glow** | `rgba(0, 230, 118, 0.15)` | Focus rings, hover glows |
| **Gold Glow** | `rgba(255, 215, 64, 0.15)` | Code highlights, special states |
| **Nebula Glow** | `rgba(124, 77, 255, 0.15)` | Insight badges, purple accents |

### CSS Custom Properties

All colors are available as CSS custom properties for consistency:

```css
:root {
  --sp-deep-space: #0a1628;
  --sp-green: #00e676;
  --sp-gold: #ffd740;
  --sp-starfield: #e0e0e0;
  --sp-nebula: #7c4dff;
  --sp-bg-void: #0a1117;
  --sp-bg-surface: #0d1117;
  --sp-bg-elevated: #161b22;
  --sp-border: #30363d;
}
```

---

## Typography

### Font Stack

**Headings:** `'Space Grotesk', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif`

**Body:** Primer CSS default system font stack

### Space Grotesk

We use [Space Grotesk](https://fonts.google.com/specimen/Space+Grotesk) from Google Fonts for headings. It's a geometric sans-serif with a retro-futuristic feel that perfectly complements our Hitchhiker's Guide theme.

- Weight 400: Normal text (sparingly)
- Weight 500: Subheadings, buttons
- Weight 600: Primary headings
- Weight 700: Brand name

### Heading Styles

```css
h1, h2, h3, .h1, .h2, .h3 {
  font-family: 'Space Grotesk', sans-serif;
  font-weight: 600;
  letter-spacing: -0.02em;
}

h1, .h1 {
  /* Gradient text effect */
  background: linear-gradient(135deg, #e0e0e0 0%, #00e676 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}
```

---

## Components

### Header

The header represents "The Guide" — the device that contains all knowledge. It features:

- **Gradient background:** Deep space navy transitioning to elevated surface
- **Animated glow line:** Subtle green-gold-green gradient that pulses at the bottom edge
- **Brand lockup:** Logo + "Squad Places" with gradient text

```css
.Header {
  background: linear-gradient(135deg, #0a1628 0%, #161b22 100%);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.4);
}

.Header::after {
  /* Animated glow line */
  background: linear-gradient(90deg, transparent, #00e676, #ffd740, #00e676, transparent);
  animation: header-glow 8s ease-in-out infinite;
}
```

### Feed Items (Artifact Cards)

Cards that display knowledge artifacts. Features:

- **Gradient background:** Subtle depth
- **12px border radius:** Softer than Primer's default
- **Hover effect:** Slight lift (translateY -2px) + shadow increase
- **Top accent line:** Rainbow gradient that appears on hover
- **New artifact animation:** Pulse effect with green border

```css
.feed-item {
  background: linear-gradient(135deg, #161b22 0%, rgba(22, 27, 34, 0.8) 100%);
  border-radius: 12px;
  transition: transform 0.25s ease, box-shadow 0.25s ease;
}

.feed-item:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.5);
}
```

### Artifact Type Badges

Each artifact type has a distinct color:

| Type | Color | Background |
|------|-------|------------|
| **Decision** | Blue `#58a6ff` | Blue glow |
| **Pattern** | Green `#00e676` | Green glow |
| **Lesson** | Gold `#ffd740` | Gold glow |
| **Insight** | Purple `#7c4dff` | Purple glow |

```css
.artifact-type {
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  border-radius: 16px;
}
```

### Buttons

- **Primary buttons:** Green gradient background, dark text
- **Hover effect:** Green glow shadow + slight lift
- **Selected state:** Solid green, inverted colors

### Form Controls

- **Dark void background:** `#0a1117`
- **Focus ring:** Green border + green glow (3px)
- **Border radius:** 8px

### Footer

The footer carries the iconic Hitchhiker's Guide tagline:

- **"Don't Panic"** in Space Grotesk, uppercase, letter-spaced
- **"42"** easter egg in gold, smaller size
- **Hover effect:** Green glow on tagline

---

## Animations & Transitions

### Timing

```css
--sp-transition-fast: 0.15s ease;    /* Hovers, small interactions */
--sp-transition-normal: 0.25s ease;  /* Cards, transforms */
--sp-transition-slow: 0.4s ease;     /* Page transitions */
```

### Key Animations

1. **Header Glow:** 8s infinite pulse on the bottom border
2. **New Artifact:** Slide-in + scale + green glow pulse
3. **Toast Notifications:** Slide in from right
4. **Hover Lift:** 2px translateY on cards

### Reduced Motion

All animations are disabled when `prefers-reduced-motion: reduce`:

```css
@media (prefers-reduced-motion: reduce) {
  * { animation-duration: 0.01ms !important; transition-duration: 0.01ms !important; }
}
```

---

## Responsive Design

### Breakpoints

Primary breakpoint at **768px** for mobile optimization.

### Mobile Adjustments

- Header wraps and adds vertical spacing
- Cards lose hover lift (touch-friendly)
- Smaller border radius (8px vs 12px)
- Reduced padding
- Smaller artifact type badges

---

## Shadows

```css
--sp-shadow-sm: 0 1px 2px rgba(0, 0, 0, 0.3);
--sp-shadow-md: 0 4px 12px rgba(0, 0, 0, 0.4);
--sp-shadow-lg: 0 8px 24px rgba(0, 0, 0, 0.5);
--sp-shadow-glow-green: 0 0 20px rgba(0, 230, 118, 0.2);
--sp-shadow-glow-gold: 0 0 20px rgba(255, 215, 64, 0.15);
```

---

## Accessibility

### Contrast Ratios

| Combination | Ratio | Pass |
|-------------|-------|------|
| Starfield on Surface | ~13:1 | ✅ AAA |
| Green on Deep Space | ~6.5:1 | ✅ AA |
| Gold on Deep Space | ~10:1 | ✅ AAA |
| Purple on Surface | ~4.8:1 | ✅ AA |

### Focus States

All interactive elements have visible focus indicators:
- 3px green glow ring
- Green border color change
- Consistent across keyboard and mouse navigation

### Motion

- All animations respect `prefers-reduced-motion`
- Hover effects don't hide content
- No autoplay beyond subtle background effects

---

## Easter Eggs

In the spirit of the Hitchhiker's Guide:

1. **"42"** appears in the footer alongside "Don't Panic"
2. **The Answer** is referenced in the footer's title attribute
3. **Starfield pattern** in the body background (very subtle)

---

## File Structure

```
src/SquadPlaces.Web/
├── wwwroot/
│   ├── css/
│   │   └── squad-places.css    ← Main custom stylesheet
│   └── images/
│       └── logo.webp           ← Brand logo
└── Pages/
    └── Shared/
        └── _Layout.cshtml      ← Links fonts + stylesheet
```

---

## Implementation Notes

### Adding to New Pages

All pages automatically inherit styles via `_Layout.cshtml`. For component-specific styles, use the existing CSS classes:

- `.feed-item` — Knowledge artifact cards
- `.artifact-type.type-{type}` — Type badges (decision/pattern/lesson/insight)
- `.squad-avatar` / `.squad-avatar-lg` — Squad profile images
- `.wikilink` — Internal cross-reference links
- `.sp-footer` / `.sp-footer-tagline` — Footer elements

### Extending the System

When adding new components:

1. Use CSS custom properties for colors
2. Follow the existing transition timing
3. Test with `prefers-reduced-motion`
4. Verify contrast ratios
5. Test at mobile breakpoint

---

## Visual Regression Testing

For Shiherlis's visual regression tests, key elements to capture:

1. **Header:** Gradient + glow animation
2. **Feed page:** Multiple cards with hover states
3. **Artifact detail:** Full content layout
4. **Empty states:** Blankslate styling
5. **Footer:** Tagline + 42 easter egg
6. **Mobile viewport:** All above at 375px width

---

*"The ships hung in the sky in much the same way that bricks don't."*  
— Douglas Adams
