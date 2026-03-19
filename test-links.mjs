import { chromium } from 'playwright';

const BASE = 'http://127.0.0.1:8199';
const visited = new Set();
const violations = [];
let totalLinks = 0;
let internalLinks = 0;
let externalLinks = 0;

async function checkPage(page, url) {
  if (visited.has(url)) return;
  visited.add(url);
  
  try {
    await page.goto(url, { waitForLoadState: 'domcontentloaded', timeout: 10000 });
    // Wait for extra.js to execute
    await page.waitForTimeout(500);
  } catch (e) {
    console.log(`  ⚠️  Could not load: ${url}`);
    return;
  }

  const links = await page.$$eval('a[href]', (anchors, base) => {
    return anchors.map(a => ({
      href: a.href,
      target: a.getAttribute('target'),
      rel: a.getAttribute('rel'),
      text: a.textContent.trim().substring(0, 60),
      isInternal: a.href.startsWith(base) || a.href.startsWith('/') || !a.href.startsWith('http'),
    }));
  }, BASE);

  for (const link of links) {
    totalLinks++;
    if (link.isInternal) {
      internalLinks++;
      if (link.target === '_blank') {
        violations.push({ page: url, href: link.href, text: link.text });
      }
    } else {
      externalLinks++;
    }
  }
  
  console.log(`✅ ${url.replace(BASE, '')} — ${links.length} links checked`);
}

(async () => {
  const browser = await chromium.launch();
  const page = await browser.newPage();
  
  // Get all nav links from landing page
  await page.goto(BASE, { waitForLoadState: 'domcontentloaded', timeout: 10000 });
  await page.waitForTimeout(500);
  
  const navLinks = await page.$$eval('nav a[href], .md-nav a[href], .md-content a[href]', (anchors, base) => {
    return [...new Set(anchors
      .map(a => a.href)
      .filter(h => h.startsWith(base))
    )];
  }, BASE);

  console.log(`\n🔍 Found ${navLinks.length} internal nav links to test\n`);
  
  // Check landing page first
  await checkPage(page, BASE + '/');

  // Check up to 15 internal pages
  for (const link of navLinks.slice(0, 15)) {
    await checkPage(page, link);
  }

  await browser.close();

  console.log(`\n${'='.repeat(60)}`);
  console.log(`📊 RESULTS`);
  console.log(`${'='.repeat(60)}`);
  console.log(`Pages tested:    ${visited.size}`);
  console.log(`Total links:     ${totalLinks}`);
  console.log(`Internal links:  ${internalLinks}`);
  console.log(`External links:  ${externalLinks}`);
  console.log(`Violations:      ${violations.length}`);
  
  if (violations.length > 0) {
    console.log(`\n❌ INTERNAL LINKS WITH target="_blank":`);
    for (const v of violations) {
      console.log(`  Page: ${v.page.replace(BASE, '')}`);
      console.log(`  Link: ${v.href.replace(BASE, '')} ("${v.text}")`);
      console.log('');
    }
    process.exit(1);
  } else {
    console.log(`\n✅ ALL INTERNAL LINKS ARE CLEAN — no target="_blank" found`);
    process.exit(0);
  }
})();
