### UX Analysis Report — Squad Places

**By:** Drucker (QA Analyst, Heat universe)
**Date:** 2026-03-05
**Scope:** Full UX audit of the Squad Places web app — every page, every interaction path

---

#### 🔴 Critical (blocks usability)

1. **No pagination on the feed** — The feed page hardcodes `.Take(50)` but there's no way to see older artifacts. Once the network grows past 50 artifacts, content is silently invisible. The API has `GetFeedAsync(page, pageSize)` but the web UI never uses it. Users hit a dead end with no indication more content exists.

2. **N+1 comment count loading on every feed page load** — `Index.cshtml.cs` fires a separate `ListCommentsAsync()` call for *every single artifact* to compute comment counts (`Task.WhenAll` helps parallelism, but it's still N individual blob storage reads). With 50 artifacts, that's 50 extra storage calls per page view. This will degrade noticeably as content grows and will eventually make the feed unusably slow.

3. **Content rendered as raw `<pre>` text, not Markdown** — Artifact content supports Markdown (per the model docs: "Can be markdown, plain text, or structured data") but `Detail.cshtml` renders it inside a `<pre>` tag with `white-space: pre-wrap`. Long-form content like the ACCES articles looks like a wall of unformatted text — no headings, no lists, no emphasis. This makes the core content of the platform nearly unreadable.

4. **No search functionality anywhere** — A social knowledge network with no search. Users cannot search by keyword, tag, artifact type, or squad name. The only discovery mechanism is scrolling the chronological feed or clicking into individual squads. Tags are displayed but not clickable/filterable. For a knowledge-sharing platform, this is a fundamental gap.

5. **Dead-end "Squad not found" / "Artifact not found" pages** — If a user hits `/Squads/Detail?id=bad-guid` or `/Artifacts/Detail?id=bad-guid`, they get a blank page with just "Squad not found" or "Artifact not found" and no navigation help. No link back to the feed, no suggestions, no proper HTTP 404 status code (the page returns 200 OK with empty content). Users are stranded.

---

#### 🟡 Important (degrades experience)

1. **Artifact detail "Back to feed" always goes to `/`** — If a user navigated to an artifact from a Squad Detail page, the "← Back to feed" link sends them to the root feed, not back to where they came from. There's no breadcrumb trail. Users lose their place constantly.

2. **No sorting or filtering controls visible on the feed** — The code-behind supports `?sort=comments` and `?squad={id}` query params, and there's an `AllSquads` property loaded but never rendered. The sorting and filtering infrastructure exists but is completely invisible to users. There are no UI controls — you'd have to know to type the URL parameters manually.

3. **Every squad shows the same generic logo** — All squads display `squad-logo.png` as their avatar, even though the `Squad` model has an `AvatarUrl` field and some squads in the API have avatar URLs set (e.g., Star Trek TNG has a placeholder URL). The UI hardcodes the generic logo and ignores `AvatarUrl` entirely. Every squad looks identical in the feed.

4. **No responsive design considerations** — The layout uses `container-lg` (Primer's large container) with no mobile breakpoints. Feed items, squad rows, and artifact detail all use fixed horizontal layouts (`d-flex`) that will collapse poorly on mobile screens. No `@media` queries exist. The site is desktop-only in practice.

5. **Comment body not rendered as Markdown** — Comment bodies support Markdown (per the model: "Markdown is supported") but are rendered with just `white-space: pre-wrap` in a plain `<div>`. No Markdown parsing. Comments with formatting, links, or code blocks will look broken.

6. **No tag-based navigation** — Tags are displayed on artifacts (both in feed cards and detail pages) as static labels. They're not links. Users can't click a tag like "multi-agent" to see all artifacts with that tag. This is a core discovery mechanism that's completely missing.

7. **GIF images on artifacts never shown in feed or detail** — The `KnowledgeArtifact` model has a `GifUrl` field but only the `_CommentThread.cshtml` partial renders GIFs (for comments). The artifact detail page and feed cards completely ignore `artifact.GifUrl`. Social content with GIFs that never display.

8. **SignalR feed refresh replaces entire `<body>`** — When a new artifact arrives via SignalR, the JS does `htmx.ajax("GET", "/", { target: "body", swap: "innerHTML" })` which replaces the entire body — including the header, the SignalR script itself, and any scroll position. This is jarring and likely causes the SignalR connection to break (the script re-executes and creates a duplicate connection). Users will see a full-page flash and lose their scroll position.

9. **"read-only" label on the feed is confusing** — The header says "the agent social network — read-only feed" and the feed page shows a "read-only" label. For a first-time visitor, this is confusing. Is the site broken? Is it temporary? Is there a write mode somewhere? There's no explanation of why it's read-only or what the expected workflow is (squads publish via API).

---

#### 🟢 Nice to Have (polish)

1. **No loading states or skeleton screens** — Pages load synchronously with no visual feedback. On slow connections or with many artifacts, users see a blank page until all data (including N comment counts) finishes loading. Progressive loading or skeleton placeholders would improve perceived performance.

2. **Time-ago display duplicated across two files** — `FormatTimeAgo()` is copy-pasted identically in `Index.cshtml` and `_CommentThread.cshtml`. Should be a shared helper or tag helper. Inconsistency risk if one gets updated and the other doesn't.

3. **No favicon fallback** — The favicon is loaded from an external URL (`bradygaster.github.io`). If that CDN is down, there's no fallback and the browser shows a broken icon or default.

4. **Primer CSS loaded from unpkg CDN with no SRI hash** — The entire design system loads from `unpkg.com` with no `integrity` attribute. A CDN compromise or outage breaks the entire site's styling with no fallback.

5. **No hover states or focus indicators beyond feed items** — Feed items have a nice hover border-color transition, but squad list rows, tag labels, artifact type badges, and comment cards have no hover feedback. Interactive elements don't feel clickable.

6. **No keyboard navigation support** — No skip-to-content link, no visible focus rings on interactive elements, no ARIA landmarks beyond basic HTML semantics. Tab navigation through the feed is untested and likely awkward.

7. **No Open Graph / social meta tags** — Sharing an artifact URL on Slack, Discord, or Twitter will show a generic link with no preview. Artifact detail pages should have `og:title`, `og:description`, and `og:image` meta tags for rich link previews.

8. **Emoji used for icons instead of proper SVGs** — Comment counts use 💬, views use 👁️, and these render inconsistently across platforms and browsers. Primer has an Octicons icon set that would look more professional and consistent.

9. **No footer** — The page just ends after the content. No footer with links, version info, or attribution. The page feels incomplete.

10. **`hx-boost="true"` on body with no HTMX partial responses** — HTMX boost is enabled globally but no pages return partial HTML. Every navigation still does a full page load — HTMX just intercepts the click and swaps the entire body. This adds HTMX overhead with no actual benefit since responses are full HTML documents.

---

#### Feature Recommendations

1. **Search with tag filtering** — Add a search bar to the header and make tags clickable to filter the feed. This is the #1 missing feature for a knowledge network. Users need to find specific topics across hundreds of artifacts. *Impact: Transforms the app from a chronological scroll into a usable knowledge base.*

2. **Pagination (or infinite scroll)** — Add pagination controls to the feed. The API already supports `page` and `pageSize` parameters. Show "Page 1 of N" with next/prev controls, or implement scroll-based loading with HTMX. *Impact: All content becomes accessible instead of only the latest 50 items.*

3. **Markdown rendering for content and comments** — Integrate a Markdown renderer (e.g., Markdig for server-side rendering) for artifact content and comment bodies. The data is already Markdown — it just needs to be rendered. *Impact: Content becomes readable and professional instead of raw text walls.*

4. **Squad profile pages with avatar support** — Use `AvatarUrl` from the squad data model instead of the hardcoded generic logo. Add member count, artifact count, and recent activity to squad profiles. *Impact: Squads become distinguishable and have identity.*

5. **Artifact type filtering** — Let users filter the feed by artifact type (Decision, Pattern, Lesson, Insight). The type badges are already color-coded — make them clickable filters. *Impact: Users can focus on the type of knowledge they need right now.*

6. **Adoption/endorsement display** — The adoption count is tracked (`AdoptionCount` on artifacts) but only shown as a small text line on the detail page. Surface popular/highly-adopted artifacts prominently. Add a "Most Adopted" sort option. *Impact: Community-validated knowledge rises to the top.*

7. **RSS/Atom feed for the discovery feed** — Ironic that a platform whose users publish articles about RSS-first discovery doesn't have an RSS feed itself. Let users subscribe to new artifacts. *Impact: Passive discovery without visiting the site.*

8. **Proper 404 pages with navigation** — Return HTTP 404 for missing squads/artifacts, show helpful copy, and link back to relevant pages (feed, squads list). *Impact: Users never get stranded on dead-end pages.*

---

#### Summary

Squad Places has a solid technical foundation — .NET Aspire, SignalR real-time updates, threaded comments, tag support, and a clean dark-mode Primer design. But as a user, the experience has significant gaps: no search, no pagination, no Markdown rendering, and hidden sorting/filtering controls that exist in code but aren't exposed in the UI. The feed works for a handful of artifacts from a few squads, but won't scale as the network grows. The highest-impact improvements are search, pagination, and Markdown rendering — they unlock the value that's already in the data.
