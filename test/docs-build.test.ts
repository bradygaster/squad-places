/**
 * Tests for docs site build (markdown-it upgrade) and markdown validation
 * Verifies docs/build.js execution, markdown-it output quality, and structure compliance
 */

import { describe, it, expect, beforeAll, afterAll } from 'vitest';
import { readdirSync, readFileSync, existsSync, rmSync } from 'node:fs';
import { execSync } from 'node:child_process';
import { join, basename } from 'node:path';

const DOCS_DIR = join(process.cwd(), 'docs');
const GUIDE_DIR = join(DOCS_DIR, 'guide');
const DIST_DIR = join(DOCS_DIR, 'dist');
const BUILD_SCRIPT = join(DOCS_DIR, 'build.js');
const TEMPLATE_PATH = join(DOCS_DIR, 'template.html');

const EXPECTED_GUIDES = [
  'index', 'installation', 'cli-install', 'configuration', 'shell',
  'sdk-integration', 'tools-and-hooks', 'marketplace', 'upstream-inheritance',
  'vscode-integration', 'architecture', 'sdk-api-reference', 'migration',
  'feature-migration',
];

function getMarkdownFiles(): string[] {
  if (!existsSync(GUIDE_DIR)) return [];
  return readdirSync(GUIDE_DIR)
    .filter(f => f.endsWith('.md'))
    .map(f => join(GUIDE_DIR, f));
}

function readFile(filepath: string): string {
  return readFileSync(filepath, 'utf-8');
}

// --- Source Markdown Validation (always runs) ---

describe('Docs Structure Validation', () => {
  describe('Markdown Files', () => {
    it('guide directory contains all 14 expected markdown files', () => {
      expect(existsSync(GUIDE_DIR)).toBe(true);
      const files = readdirSync(GUIDE_DIR).filter(f => f.endsWith('.md')).map(f => f.replace('.md', ''));
      for (const guide of EXPECTED_GUIDES) {
        expect(files).toContain(guide);
      }
      expect(files.length).toBe(14);
    });

    it('all markdown files have proper headings', () => {
      for (const file of getMarkdownFiles()) {
        const content = readFile(file);
        expect(/^#+\s+.+/m.test(content)).toBe(true);
      }
    });

    it('all code blocks are properly fenced (even count of ```)', () => {
      for (const file of getMarkdownFiles()) {
        const content = readFile(file);
        const fenceCount = (content.match(/```/g) || []).length;
        expect(fenceCount % 2).toBe(0);
      }
    });

    it('no empty markdown files', () => {
      for (const file of getMarkdownFiles()) {
        expect(readFile(file).length).toBeGreaterThan(10);
      }
    });
  });

  describe('Code Example Validation', () => {
    it('code blocks contain language specification or valid content', () => {
      for (const file of getMarkdownFiles()) {
        const codeBlocks = readFile(file).match(/```[\s\S]*?```/g) || [];
        for (const block of codeBlocks) {
          expect(block.split('\n').length).toBeGreaterThan(1);
        }
      }
    });

    it('bash examples have non-empty content', () => {
      for (const file of getMarkdownFiles()) {
        const bashBlocks = readFile(file).match(/```(?:bash|sh|shell)[\s\S]*?```/g) || [];
        for (const block of bashBlocks) {
          const lines = block.split('\n').filter(l => l.trim() && !l.startsWith('```'));
          expect(lines.length).toBeGreaterThan(0);
        }
      }
    });
  });
});

// --- Build Script Tests (markdown-it upgrade contract) ---

describe('Docs Build Script (markdown-it)', () => {
  // Run build once before all tests in this suite, clean up after
  beforeAll(() => {
    if (!existsSync(BUILD_SCRIPT)) return;
    if (existsSync(DIST_DIR)) {
      rmSync(DIST_DIR, { recursive: true, force: true });
    }
    execSync(`node "${BUILD_SCRIPT}"`, { cwd: DOCS_DIR, timeout: 30_000 });
  });

  afterAll(() => {
    if (existsSync(DIST_DIR)) {
      try { rmSync(DIST_DIR, { recursive: true, force: true }); } catch { /* Windows ENOTEMPTY race */ }
    }
  });

  // Helper: skip entire suite gracefully if build.js isn't upgraded yet
  function requireBuild() {
    if (!existsSync(BUILD_SCRIPT)) {
      return false;
    }
    return existsSync(DIST_DIR);
  }

  function readHtml(name: string): string {
    return readFile(join(DIST_DIR, `${name}.html`));
  }

  // --- 1. Build execution ---

  it('docs/build.js exists', () => {
    expect(existsSync(BUILD_SCRIPT)).toBe(true);
  });

  it('build.js runs without errors (exit code 0)', () => {
    if (!existsSync(BUILD_SCRIPT)) return;
    // Build already ran in beforeAll; re-run to confirm idempotent zero-exit
    expect(() => {
      execSync(`node "${BUILD_SCRIPT}"`, { cwd: DOCS_DIR, timeout: 30_000 });
    }).not.toThrow();
  });

  // --- 2. All 14 guide files produce HTML output ---

  it('all 14 guide files produce HTML in docs/dist/', () => {
    if (!requireBuild()) return;
    for (const guide of EXPECTED_GUIDES) {
      const htmlPath = join(DIST_DIR, `${guide}.html`);
      expect(existsSync(htmlPath), `Missing: ${guide}.html`).toBe(true);
    }
  });

  it('no extra unexpected HTML files in dist/', () => {
    if (!requireBuild()) return;
    const htmlFiles = readdirSync(DIST_DIR).filter(f => f.endsWith('.html'));
    // Allow index.html as homepage redirect/copy, but all should map to guides
    for (const f of htmlFiles) {
      const name = f.replace('.html', '');
      expect(EXPECTED_GUIDES).toContain(name);
    }
  });

  // --- 3. markdown-it output quality ---

  describe('markdown-it output: code blocks with language class', () => {
    it('fenced code blocks have language class on <code> element', () => {
      if (!requireBuild()) return;
      // configuration.md has ```typescript blocks
      const html = readHtml('configuration');
      // markdown-it renders: <pre><code class="language-typescript">
      expect(html).toMatch(/<code\s+class="language-typescript"/);
    });

    it('bash code blocks get language-bash class', () => {
      if (!requireBuild()) return;
      const html = readHtml('cli-install');
      expect(html).toMatch(/<code\s+class="language-bash"/);
    });
  });

  describe('markdown-it output: table markup', () => {
    it('tables render as proper <table> HTML', () => {
      if (!requireBuild()) return;
      // cli-install.md has markdown tables
      const html = readHtml('cli-install');
      expect(html).toMatch(/<table>/);
      expect(html).toMatch(/<thead>/);
      expect(html).toMatch(/<tbody>/);
      expect(html).toMatch(/<th>/);
      expect(html).toMatch(/<td>/);
    });
  });

  describe('markdown-it output: nested lists', () => {
    it('nested list items produce nested <ul> or <ol> elements', () => {
      if (!requireBuild()) return;
      // vscode-integration.md has nested lists (indented - items)
      const html = readHtml('vscode-integration');
      // markdown-it nests <ul> inside <li> for indented items
      expect(html).toMatch(/<li>[\s\S]*?<ul>/);
    });
  });

  describe('markdown-it output: inline formatting', () => {
    it('bold text renders as <strong>', () => {
      if (!requireBuild()) return;
      const html = readHtml('index');
      expect(html).toMatch(/<strong>/);
    });

    it('inline code renders as <code> without language class', () => {
      if (!requireBuild()) return;
      const html = readHtml('configuration');
      // Inline code: <code> without class attribute
      expect(html).toMatch(/<code>[^<]+<\/code>/);
    });

    it('links render as <a> with href', () => {
      if (!requireBuild()) return;
      const html = readHtml('index');
      expect(html).toMatch(/<a\s+href="/);
    });
  });

  // --- 4. Assets copied to dist/assets/ ---

  it('assets directory is copied to dist/assets/', () => {
    if (!requireBuild()) return;
    const distAssets = join(DIST_DIR, 'assets');
    expect(existsSync(distAssets), 'dist/assets/ should exist').toBe(true);
    const assetFiles = readdirSync(distAssets);
    expect(assetFiles).toContain('style.css');
    expect(assetFiles).toContain('app.js');
  });

  // --- 5. index.html as homepage ---

  it('index.html is generated as the homepage', () => {
    if (!requireBuild()) return;
    const indexPath = join(DIST_DIR, 'index.html');
    expect(existsSync(indexPath)).toBe(true);
    const html = readFile(indexPath);
    expect(html).toMatch(/<!DOCTYPE html>/i);
    // Should contain the index guide content (has "Squad Documentation" heading)
    expect(html).toMatch(/Squad Documentation/);
  });

  // --- 6. Frontmatter parsing (title extraction from --- fences) ---

  it('frontmatter title is extracted and used (not rendered as raw ---)', () => {
    if (!requireBuild()) return;
    // Guide files like configuration.md have --- separators that could be frontmatter
    // After markdown-it upgrade, if frontmatter is used, raw --- should not appear as <hr> at top
    const html = readHtml('configuration');
    // The page should have a title — either from frontmatter or from h1
    expect(html).toMatch(/Configuration/);
    // The title should appear somewhere in the page (heading or <title>)
    expect(html).toMatch(/<h1[^>]*>.*Configuration.*<\/h1>|<title>.*Configuration.*<\/title>/s);
  });

  it('page title is populated in the HTML (not left as {{TITLE}})', () => {
    if (!requireBuild()) return;
    for (const guide of EXPECTED_GUIDES) {
      const html = readHtml(guide);
      expect(html).not.toContain('{{TITLE}}');
    }
  });

  // --- 7. Nav links and all guide files in navigation ---

  it('every generated page contains a <nav> element', () => {
    if (!requireBuild()) return;
    for (const guide of EXPECTED_GUIDES) {
      const html = readHtml(guide);
      expect(html, `${guide}.html missing <nav>`).toMatch(/<nav[\s>]/);
    }
  });

  it('navigation contains links to guide pages', () => {
    if (!requireBuild()) return;
    const html = readHtml('index');
    // Nav should have links to at least the core guides
    expect(html).toMatch(/href="installation\.html"/);
    expect(html).toMatch(/href="configuration\.html"/);
    expect(html).toMatch(/href="shell\.html"/);
  });

  it('all 14 guide files appear as links in navigation', () => {
    if (!requireBuild()) return;
    const html = readHtml('index');
    for (const guide of EXPECTED_GUIDES) {
      // Each guide should have a nav link (href="guide.html")
      const linkPattern = new RegExp(`href="${guide}\\.html"`);
      expect(html, `Nav missing link to ${guide}.html`).toMatch(linkPattern);
    }
  });

  it('active page is marked in navigation', () => {
    if (!requireBuild()) return;
    const html = readHtml('configuration');
    // The current page link should have an "active" class
    expect(html).toMatch(/class="active".*configuration\.html|configuration\.html.*class="active"/);
  });

  // --- 8. Template substitution ---

  it('{{CONTENT}} placeholder is replaced with actual content', () => {
    if (!requireBuild()) return;
    for (const guide of EXPECTED_GUIDES) {
      const html = readHtml(guide);
      expect(html).not.toContain('{{CONTENT}}');
      // Should have real HTML content from markdown
      expect(html).toMatch(/<h[1-6]|<p>|<pre>|<ul>|<ol>/);
    }
  });

  it('{{NAV}} placeholder is replaced with navigation HTML', () => {
    if (!requireBuild()) return;
    for (const guide of EXPECTED_GUIDES) {
      const html = readHtml(guide);
      expect(html).not.toContain('{{NAV}}');
    }
  });

  it('no raw template placeholders remain in output', () => {
    if (!requireBuild()) return;
    for (const guide of EXPECTED_GUIDES) {
      const html = readHtml(guide);
      expect(html).not.toMatch(/\{\{[A-Z_]+\}\}/);
    }
  });

  // --- HTML structure validation ---

  it('all HTML files have proper DOCTYPE and closing tags', () => {
    if (!requireBuild()) return;
    for (const guide of EXPECTED_GUIDES) {
      const html = readHtml(guide);
      expect(html).toMatch(/<!DOCTYPE html>/i);
      expect(html).toMatch(/<\/html>/i);
      expect(html).toMatch(/<\/body>/i);
    }
  });

  it('HTML contains <main> content area from template', () => {
    if (!requireBuild()) return;
    for (const guide of EXPECTED_GUIDES) {
      const html = readHtml(guide);
      expect(html).toMatch(/<main[\s>]/);
    }
  });
});
