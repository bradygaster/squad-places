#!/usr/bin/env node

const fs = require('fs');
const path = require('path');

const GREEN = '\x1b[32m';
const RED = '\x1b[31m';
const DIM = '\x1b[2m';
const BOLD = '\x1b[1m';
const RESET = '\x1b[0m';

function fatal(msg) {
  console.error(`${RED}✗${RESET} ${msg}`);
  process.exit(1);
}

process.on('uncaughtException', (err) => {
  fatal(`Unexpected error: ${err.message}`);
});

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
  try {
    if (fs.statSync(src).isDirectory()) {
      fs.mkdirSync(target, { recursive: true });
      for (const entry of fs.readdirSync(src)) {
        copyRecursive(path.join(src, entry), path.join(target, entry));
      }
    } else {
      fs.mkdirSync(path.dirname(target), { recursive: true });
      fs.copyFileSync(src, target);
    }
  } catch (err) {
    fatal(`Failed to copy ${path.relative(root, src)}: ${err.message}`);
  }
}

// Validate source files exist
const agentSrcCheck = path.join(root, '.github', 'agents', 'squad.agent.md');
const templatesSrcCheck = path.join(root, 'templates');
if (!fs.existsSync(agentSrcCheck)) {
  fatal(`Source file missing: .github/agents/squad.agent.md — installation may be corrupted`);
}
if (!fs.existsSync(templatesSrcCheck) || !fs.statSync(templatesSrcCheck).isDirectory()) {
  fatal(`Source directory missing or corrupted: templates/ — installation may be corrupted`);
}

// Validate destination is writable
try {
  fs.accessSync(dest, fs.constants.W_OK);
} catch {
  fatal(`Cannot write to ${dest} — check directory permissions`);
}

const isUpgrade = cmd === 'upgrade';

// Stamp version into squad.agent.md after copying
function stampVersion(filePath) {
  const content = fs.readFileSync(filePath, 'utf8');
  fs.writeFileSync(filePath, content.replace('version: "0.0.0-source"', `version: "${pkg.version}"`));
}

// Copy agent file (Squad-owned — overwrite on upgrade)
const agentSrc = path.join(root, '.github', 'agents', 'squad.agent.md');
const agentDest = path.join(dest, '.github', 'agents', 'squad.agent.md');

if (isUpgrade) {
  try {
    fs.mkdirSync(path.dirname(agentDest), { recursive: true });
    fs.copyFileSync(agentSrc, agentDest);
    stampVersion(agentDest);
  } catch (err) {
    fatal(`Failed to upgrade squad.agent.md: ${err.message}`);
  }
  console.log(`${GREEN}✓${RESET} ${BOLD}upgraded${RESET} .github/agents/squad.agent.md (v${pkg.version})`);
} else if (fs.existsSync(agentDest)) {
  console.log(`${DIM}squad.agent.md already exists — skipping (run 'upgrade' to update)${RESET}`);
} else {
  try {
    fs.mkdirSync(path.dirname(agentDest), { recursive: true });
    fs.copyFileSync(agentSrc, agentDest);
    stampVersion(agentDest);
  } catch (err) {
    fatal(`Failed to create squad.agent.md: ${err.message}`);
  }
  console.log(`${GREEN}✓${RESET} .github/agents/squad.agent.md (v${pkg.version})`);
}

// Pre-create drop-box, orchestration-log, and casting directories (additive-only)
const inboxDir = path.join(dest, '.ai-team', 'decisions', 'inbox');
const orchLogDir = path.join(dest, '.ai-team', 'orchestration-log');
const castingDir = path.join(dest, '.ai-team', 'casting');
try {
  fs.mkdirSync(inboxDir, { recursive: true });
  fs.mkdirSync(orchLogDir, { recursive: true });
  fs.mkdirSync(castingDir, { recursive: true });
} catch (err) {
  fatal(`Failed to create .ai-team/ directories: ${err.message}`);
}

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

// Copy workflow templates (Squad-owned — overwrite on upgrade)
const workflowsSrc = path.join(root, 'templates', 'workflows');
const workflowsDest = path.join(dest, '.github', 'workflows');

if (fs.existsSync(workflowsSrc) && fs.statSync(workflowsSrc).isDirectory()) {
  const workflowFiles = fs.readdirSync(workflowsSrc).filter(f => f.endsWith('.yml'));

  if (isUpgrade) {
    fs.mkdirSync(workflowsDest, { recursive: true });
    for (const file of workflowFiles) {
      fs.copyFileSync(path.join(workflowsSrc, file), path.join(workflowsDest, file));
    }
    console.log(`${GREEN}✓${RESET} ${BOLD}upgraded${RESET} squad workflow files (${workflowFiles.length} workflows)`);
  } else {
    fs.mkdirSync(workflowsDest, { recursive: true });
    let copied = 0;
    for (const file of workflowFiles) {
      const destFile = path.join(workflowsDest, file);
      if (fs.existsSync(destFile)) {
        console.log(`${DIM}${file} already exists — skipping (run 'upgrade' to update)${RESET}`);
      } else {
        fs.copyFileSync(path.join(workflowsSrc, file), destFile);
        console.log(`${GREEN}✓${RESET} .github/workflows/${file}`);
        copied++;
      }
    }
    if (copied === 0 && workflowFiles.length > 0) {
      console.log(`${DIM}all squad workflows already exist — skipping${RESET}`);
    }
  }
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
