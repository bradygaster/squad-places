#!/usr/bin/env node

const fs = require('fs');
const path = require('path');

const GREEN = '\x1b[32m';
const DIM = '\x1b[2m';
const BOLD = '\x1b[1m';
const RESET = '\x1b[0m';

const root = __dirname;
const dest = process.cwd();
const pkg = require(path.join(root, 'package.json'));
const cmd = process.argv[2];

// --version / --help
if (cmd === '--version' || cmd === '-v') {
  console.log(pkg.version);
  process.exit(0);
}

if (cmd === '--help' || cmd === '-h' || cmd === 'help') {
  console.log(`\n${BOLD}squad${RESET} v${pkg.version} — Add an AI agent team to any project\n`);
  console.log(`Usage: npx github:bradygaster/squad [command]\n`);
  console.log(`Commands:`);
  console.log(`  ${BOLD}(default)${RESET}  Initialize Squad (skip files that already exist)`);
  console.log(`  ${BOLD}upgrade${RESET}    Update Squad-owned files to latest version`);
  console.log(`             Overwrites: squad.agent.md, .ai-team-templates/`);
  console.log(`             Never touches: .ai-team/ (your team state)`);
  console.log(`  ${BOLD}help${RESET}       Show this help message`);
  console.log(`\nFlags:`);
  console.log(`  ${BOLD}--version, -v${RESET}  Print version`);
  console.log(`  ${BOLD}--help, -h${RESET}     Show help\n`);
  process.exit(0);
}

function copyRecursive(src, target) {
  if (fs.statSync(src).isDirectory()) {
    fs.mkdirSync(target, { recursive: true });
    for (const entry of fs.readdirSync(src)) {
      copyRecursive(path.join(src, entry), path.join(target, entry));
    }
  } else {
    fs.mkdirSync(path.dirname(target), { recursive: true });
    fs.copyFileSync(src, target);
  }
}

const isUpgrade = cmd === 'upgrade';

// Copy agent file (Squad-owned — overwrite on upgrade)
const agentSrc = path.join(root, '.github', 'agents', 'squad.agent.md');
const agentDest = path.join(dest, '.github', 'agents', 'squad.agent.md');

if (isUpgrade) {
  fs.mkdirSync(path.dirname(agentDest), { recursive: true });
  fs.copyFileSync(agentSrc, agentDest);
  console.log(`${GREEN}✓${RESET} ${BOLD}upgraded${RESET} .github/agents/squad.agent.md`);
} else if (fs.existsSync(agentDest)) {
  console.log(`${DIM}squad.agent.md already exists — skipping (run 'upgrade' to update)${RESET}`);
} else {
  fs.mkdirSync(path.dirname(agentDest), { recursive: true });
  fs.copyFileSync(agentSrc, agentDest);
  console.log(`${GREEN}✓${RESET} .github/agents/squad.agent.md`);
}

// Pre-create drop-box, orchestration-log, and casting directories (additive-only)
const inboxDir = path.join(dest, '.ai-team', 'decisions', 'inbox');
const orchLogDir = path.join(dest, '.ai-team', 'orchestration-log');
const castingDir = path.join(dest, '.ai-team', 'casting');
fs.mkdirSync(inboxDir, { recursive: true });
fs.mkdirSync(orchLogDir, { recursive: true });
fs.mkdirSync(castingDir, { recursive: true });

// Copy default ceremonies config
const ceremoniesDest = path.join(dest, '.ai-team', 'ceremonies.md');
if (!fs.existsSync(ceremoniesDest)) {
  const ceremoniesSrc = path.join(root, 'templates', 'ceremonies.md');
  fs.copyFileSync(ceremoniesSrc, ceremoniesDest);
  console.log(`${GREEN}✓${RESET} .ai-team/ceremonies.md`);
} else {
  console.log(`${DIM}ceremonies.md already exists — skipping${RESET}`);
}

// Append merge=union rules for append-only .ai-team/ files
const gitattributes = path.join(dest, '.gitattributes');
const unionRules = [
  '.ai-team/decisions.md merge=union',
  '.ai-team/agents/*/history.md merge=union',
  '.ai-team/log/** merge=union',
  '.ai-team/orchestration-log/** merge=union',
];
const existing = fs.existsSync(gitattributes) ? fs.readFileSync(gitattributes, 'utf8') : '';
const missing = unionRules.filter(rule => !existing.includes(rule));
if (missing.length) {
  const block = (existing && !existing.endsWith('\n') ? '\n' : '')
    + '# Squad: union merge for append-only team state files\n'
    + missing.join('\n') + '\n';
  fs.appendFileSync(gitattributes, block);
  console.log(`${GREEN}✓${RESET} .gitattributes (merge=union rules)`);
} else {
  console.log(`${DIM}.gitattributes merge rules already present — skipping${RESET}`);
}

// Copy templates (Squad-owned — overwrite on upgrade)
const templatesSrc = path.join(root, 'templates');
const templatesDest = path.join(dest, '.ai-team-templates');

if (isUpgrade) {
  copyRecursive(templatesSrc, templatesDest);
  console.log(`${GREEN}✓${RESET} ${BOLD}upgraded${RESET} .ai-team-templates/`);
} else if (fs.existsSync(templatesDest)) {
  console.log(`${DIM}.ai-team-templates/ already exists — skipping (run 'upgrade' to update)${RESET}`);
} else {
  copyRecursive(templatesSrc, templatesDest);
  console.log(`${GREEN}✓${RESET} .ai-team-templates/`);
}

if (isUpgrade) {
  console.log(`\n${DIM}.ai-team/ untouched — your team state is safe${RESET}`);
}

console.log();
console.log(`${BOLD}Squad is ${isUpgrade ? 'upgraded' : 'ready'}.${RESET}${isUpgrade ? ` (v${pkg.version})` : ''}`);
console.log();
if (!isUpgrade) {
  console.log(`Next steps:`);
  console.log(`  1. Open Copilot:  ${DIM}copilot${RESET}`);
  console.log(`  2. Select ${BOLD}Squad${RESET} from the /agents list`);
  console.log(`  3. Tell it what you're building`);
  console.log();
}
