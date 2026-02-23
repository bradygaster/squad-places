#!/usr/bin/env node

import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';
import MarkdownIt from 'markdown-it';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const md = new MarkdownIt({ html: true, linkify: true, typographer: true });

// Nav sections: directory name → display title (order matters)
const SECTIONS = [
  { dir: 'guide', title: 'Getting Started' },
  { dir: 'cli', title: 'CLI' },
  { dir: 'sdk', title: 'SDK' },
  { dir: 'features', title: 'Features' },
  { dir: 'scenarios', title: 'Scenarios' },
  { dir: 'blog', title: 'Blog' },
];

// Parse optional YAML-style frontmatter (--- fenced)
function parseFrontmatter(raw) {
  const match = raw.match(/^---\r?\n([\s\S]*?)\r?\n---\r?\n?([\s\S]*)$/);
  if (!match) return { meta: {}, body: raw };
  const meta = {};
  for (const line of match[1].split('\n')) {
    const idx = line.indexOf(':');
    if (idx > 0) meta[line.slice(0, idx).trim()] = line.slice(idx + 1).trim();
  }
  return { meta, body: match[2] };
}

// Extract first H1 as fallback title
function extractTitle(markdown) {
  const m = markdown.match(/^#\s+(.+)$/m);
  return m ? m[1] : null;
}

// Derive a human-readable title from a filename
function titleFromFilename(filename) {
  return filename
    .replace(/\.md$/, '')
    .replace(/[-_]/g, ' ')
    .replace(/\b\w/g, c => c.toUpperCase());
}

// Discover docs from all configured section directories
function discoverDocs(docsDir) {
  const sections = [];
  for (const { dir, title } of SECTIONS) {
    const sectionDir = path.join(docsDir, dir);
    if (!fs.existsSync(sectionDir)) continue;
    const items = fs.readdirSync(sectionDir)
      .filter(f => f.endsWith('.md'))
      .map(f => {
        const name = f.replace('.md', '');
        const raw = fs.readFileSync(path.join(sectionDir, f), 'utf-8');
        const { meta, body } = parseFrontmatter(raw);
        const docTitle = meta.title || extractTitle(body) || titleFromFilename(f);
        return { name, dir, title: docTitle, href: `${dir}/${name}.html` };
      });
    // index.md first; blog sorts by filename (chronological); rest alphabetical
    items.sort((a, b) => {
      if (a.name === 'index') return -1;
      if (b.name === 'index') return 1;
      if (dir === 'blog') return a.name.localeCompare(b.name);
      return a.title.localeCompare(b.title);
    });
    if (items.length > 0) {
      sections.push({ title, dir, items });
    }
  }
  return sections;
}

// Compute relative prefix from a subdir back to the dist root
function assetsPrefix(subdir) {
  if (!subdir) return '';
  const depth = subdir.split('/').filter(Boolean).length;
  return '../'.repeat(depth);
}

function buildNavHtml(sections, activeDir, activeFile) {
  const prefix = assetsPrefix(activeDir);
  let html = '<nav class="sidebar" id="sidebar">\n';
  html += `<div class="sidebar-header"><a href="${prefix}guide/index.html" class="logo"><img src="${prefix}assets/squad-logo.png" alt="Squad" class="sidebar-logo-img"></a><button class="sidebar-close" onclick="toggleSidebar()">X</button></div>\n`;
  html += '<div class="sidebar-content">\n';
  // Home link outside sections
  const homeCls = (activeDir === 'guide' && activeFile === 'index') ? ' class="active"' : '';
  html += `<a href="${prefix}guide/index.html"${homeCls}>Home</a>\n`;
  for (const section of sections) {
    html += '<details class="nav-section" open>\n';
    html += `<summary>${section.title}</summary>\n`;
    for (const item of section.items) {
      if (item.dir === 'guide' && item.name === 'index') continue; // Home already above
      const href = `${prefix}${item.href}`;
      const cls = (item.dir === activeDir && item.name === activeFile) ? ' class="active"' : '';
      html += `<a href="${href}"${cls}>${item.title}</a>\n`;
    }
    html += '</details>\n';
  }
  html += '</div>\n</nav>';
  return html;
}

function buildSearchIndex(docsDir, sections) {
  const index = [];
  for (const section of sections) {
    for (const item of section.items) {
      const raw = fs.readFileSync(path.join(docsDir, item.dir, `${item.name}.md`), 'utf-8');
      const { body } = parseFrontmatter(raw);
      const preview = body.replace(/^#+\s+.+$/gm, '').replace(/[`*_\[\]()#>|\\-]/g, '').replace(/\s+/g, ' ').trim().substring(0, 200);
      index.push({ title: item.title, href: item.href, preview });
    }
  }
  return index;
}

// Rewrite .md links to .html in rendered HTML
function rewriteLinks(html) {
  return html.replace(/href="([^"]*?)\.md(#[^"]*)?"/g, (_, base, hash) => {
    return `href="${base}.html${hash || ''}"`;
  });
}

function copyDirSync(src, dest) {
  fs.mkdirSync(dest, { recursive: true });
  for (const entry of fs.readdirSync(src, { withFileTypes: true })) {
    const srcPath = path.join(src, entry.name);
    const destPath = path.join(dest, entry.name);
    if (entry.isDirectory()) {
      copyDirSync(srcPath, destPath);
    } else {
      fs.copyFileSync(srcPath, destPath);
    }
  }
}

async function build() {
  const docsDir = __dirname;
  const distDir = path.join(__dirname, 'dist');
  const templatePath = path.join(__dirname, 'template.html');
  const assetsDir = path.join(__dirname, 'assets');

  // Clean and create dist directory
  if (fs.existsSync(distDir)) {
    fs.rmSync(distDir, { recursive: true });
  }
  fs.mkdirSync(distDir, { recursive: true });

  // Copy assets into dist/
  if (fs.existsSync(assetsDir)) {
    copyDirSync(assetsDir, path.join(distDir, 'assets'));
  }

  const template = fs.readFileSync(templatePath, 'utf-8');
  const sections = discoverDocs(docsDir);
  const searchIndex = buildSearchIndex(docsDir, sections);
  const searchIndexJson = JSON.stringify(searchIndex);

  let totalFiles = 0;

  for (const section of sections) {
    for (const item of section.items) {
      const mdPath = path.join(docsDir, item.dir, `${item.name}.md`);
      const outDir = path.join(distDir, item.dir);
      const htmlPath = path.join(outDir, `${item.name}.html`);

      fs.mkdirSync(outDir, { recursive: true });

      const raw = fs.readFileSync(mdPath, 'utf-8');
      const { meta, body } = parseFrontmatter(raw);
      const title = meta.title || extractTitle(body) || titleFromFilename(`${item.name}.md`);
      let content = md.render(body);
      content = rewriteLinks(content);

      const navHtml = buildNavHtml(sections, item.dir, item.name);
      const prefix = assetsPrefix(item.dir);

      const html = template
        .replace('{{TITLE}}', title)
        .replace('{{CONTENT}}', content)
        .replace('{{NAV}}', navHtml)
        .replace('{{SEARCH_INDEX}}', searchIndexJson)
        .replace(/href="assets\//g, `href="${prefix}assets/`)
        .replace(/src="assets\//g, `src="${prefix}assets/`);

      fs.writeFileSync(htmlPath, html);
      console.log(`✓ Generated ${item.dir}/${item.name}.html`);
      totalFiles++;
    }
  }

  // Root redirect to guide/index.html
  const redirectHtml = '<!DOCTYPE html><html><head><meta http-equiv="refresh" content="0;url=guide/index.html"><title>Redirecting...</title></head><body><p>Redirecting to <a href="guide/index.html">documentation</a>...</p></body></html>';
  fs.writeFileSync(path.join(distDir, 'index.html'), redirectHtml);
  console.log('✓ Generated index.html (redirect)');

  console.log(`\n✅ Docs site generated in ${distDir} (${totalFiles} pages)`);
}

build().catch(err => {
  console.error('Build failed:', err);
  process.exit(1);
});
