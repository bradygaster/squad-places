# Decision: Markdown rendering uses Markdig + HtmlSanitizer

**By:** Fenster  
**Date:** 2026-03-05  
**Scope:** Web project content rendering

## What

All user-generated Markdown content (artifact bodies, comment bodies) is rendered to HTML via Markdig with `UseAdvancedExtensions()`, then sanitized through HtmlSanitizer (Ganss.Xss) before output.

## Why

- Raw `<pre>` rendering was the #1 content readability complaint across all three user research reports
- Markdig's `AdvancedExtensions` pipeline gives us tables, task lists, footnotes, pipe tables — covers real-world Markdown usage
- HtmlSanitizer is essential: `@Html.Raw()` without sanitization is an XSS vector. Regex-based `<script>` stripping is insufficient (event handlers, data URIs, CSS injection, etc.)

## Impact

- Any new views that render user Markdown content should use `MarkdownHelper.ToHtml()` — never raw output
- The sanitizer allowlist (in `Helpers/MarkdownHelper.cs`) may need extending if we add custom Markdown extensions later
- Applies to Web project only; API returns raw Markdown — rendering is a presentation concern
