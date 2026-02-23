#!/usr/bin/env node

import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';
import MarkdownIt from 'markdown-it';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const md = new MarkdownIt({ html: true, linkify: true, typographer: true });

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
  return m ? m[1] : 'Squad Documentation';
}

function generateNav() {
  return [
    { section: 'Getting Started', items: [
      { title: 'Home', file: 'index' },
      { title: 'Installation', file: 'installation' },
      { title: 'CLI Install', file: 'cli-install' },
      { title: 'Configuration', file: 'configuration' },
      { title: 'CLI Shell', file: 'shell' },
    ]},
    { section: 'Guides', items: [
      { title: 'SDK Integration', file: 'sdk-integration' },
      { title: 'Tools & Hooks', file: 'tools-and-hooks' },
      { title: 'Marketplace', file: 'marketplace' },
      { title: 'Upstream Inheritance', file: 'upstream-inheritance' },
      { title: 'VS Code Integration', file: 'vscode-integration' },
    ]},
    { section: 'Reference', items: [
      { title: 'Architecture', file: 'architecture' },
      { title: 'SDK API Reference', file: 'sdk-api-reference' },
    ]},
    { section: 'Migration', items: [
      { title: 'Migration Guide', file: 'migration' },
      { title: 'Feature Migration', file: 'feature-migration' },
    ]},
  ];
}

function buildNavHtml(nav, activeFile) {
  let html = '<nav class="sidebar">\n';
  for (const group of nav) {
    html += `<h4>${group.section}</h4>\n<ul>\n`;
    for (const item of group.items) {
      const href = `${item.file}.html`;
      const cls = item.file === activeFile ? ' class="active"' : '';
      html += `<li><a href="${href}"${cls}>${item.title}</a></li>\n`;
    }
    html += '</ul>\n';
  }
  html += '</nav>';
  return html;
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
  const guideDir = path.join(__dirname, 'guide');
  const distDir = path.join(__dirname, 'dist');
  const templatePath = path.join(__dirname, 'template.html');
  const assetsDir = path.join(__dirname, 'assets');

  // Create dist directory
  if (!fs.existsSync(distDir)) {
    fs.mkdirSync(distDir, { recursive: true });
  }

  // Copy assets into dist/
  if (fs.existsSync(assetsDir)) {
    copyDirSync(assetsDir, path.join(distDir, 'assets'));
  }

  const template = fs.readFileSync(templatePath, 'utf-8');
  const nav = generateNav();

  // Get all markdown files
  const files = fs.readdirSync(guideDir)
    .filter(f => f.endsWith('.md'))
    .map(f => f.replace('.md', ''));

  for (const file of files) {
    const mdPath = path.join(guideDir, `${file}.md`);
    const htmlPath = path.join(distDir, `${file}.html`);

    const raw = fs.readFileSync(mdPath, 'utf-8');
    const { meta, body } = parseFrontmatter(raw);
    const title = meta.title || extractTitle(body);
    const content = md.render(body);

    const navHtml = buildNavHtml(nav, file);

    const html = template
      .replace('{{TITLE}}', title)
      .replace('{{CONTENT}}', content)
      .replace('{{NAV}}', navHtml);

    fs.writeFileSync(htmlPath, html);
    console.log(`✓ Generated ${file}.html`);
  }

  console.log(`\n✅ Docs site generated in ${distDir}`);
}

build().catch(err => {
  console.error('Build failed:', err);
  process.exit(1);
});
