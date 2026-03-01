/**
 * Team casting engine — parses coordinator team proposals and scaffolds agent files.
 * @module cli/core/cast
 */

import { mkdir, writeFile, readFile } from 'node:fs/promises';
import { existsSync } from 'node:fs';
import { join } from 'node:path';

// ── Types ──────────────────────────────────────────────────────────

export interface CastMember {
  name: string;
  role: string;
  scope: string;
  emoji: string;
}

export interface CastProposal {
  members: CastMember[];
  universe: string;
  projectDescription: string;
}

export interface CastResult {
  teamRoot: string;
  membersCreated: string[];
  filesCreated: string[];
}

// ── Emoji mapping ──────────────────────────────────────────────────

const ROLE_EMOJI_MAP: [RegExp, string][] = [
  [/lead|architect|tech\s*lead/i, '🏗️'],
  [/frontend|ui|design/i, '⚛️'],
  [/backend|api|server/i, '🔧'],
  [/test|qa|quality/i, '🧪'],
  [/devops|infra|platform/i, '⚙️'],
  [/docs|devrel|writer/i, '📝'],
  [/data|database|analytics/i, '📊'],
  [/security|auth/i, '🔒'],
];

/** Map a role string to its emoji. Exported for reuse. */
export function roleToEmoji(role: string): string {
  for (const [pattern, emoji] of ROLE_EMOJI_MAP) {
    if (pattern.test(role)) return emoji;
  }
  return '👤';
}

// ── Parser ─────────────────────────────────────────────────────────

/**
 * Parse an INIT_TEAM block from the coordinator's response.
 * Returns null if the response doesn't contain the expected format.
 */
export function parseCastResponse(response: string): CastProposal | null {
  const initIdx = response.indexOf('INIT_TEAM:');
  if (initIdx === -1) return null;

  const block = response.slice(initIdx);
  const lines = block.split('\n').map(l => l.trim()).filter(Boolean);

  const members: CastMember[] = [];
  let universe = '';
  let projectDescription = '';

  for (const line of lines) {
    // Member line: - Name | Role | Scope
    if (line.startsWith('-') && line.includes('|')) {
      const parts = line.slice(1).split('|').map(s => s.trim());
      if (parts.length >= 3) {
        const name = parts[0]!;
        const role = parts[1]!;
        const scope = parts[2]!;
        members.push({ name, role, scope, emoji: roleToEmoji(role) });
      }
    }

    // UNIVERSE: line
    const universeMatch = line.match(/^UNIVERSE:\s*(.+)/i);
    if (universeMatch) {
      universe = universeMatch[1]!.trim();
    }

    // PROJECT: line
    const projectMatch = line.match(/^PROJECT:\s*(.+)/i);
    if (projectMatch) {
      projectDescription = projectMatch[1]!.trim();
    }
  }

  if (members.length === 0) return null;

  return { members, universe, projectDescription };
}

// ── Charter / history generators ───────────────────────────────────

function personalityForRole(role: string): string {
  const lower = role.toLowerCase();
  if (/lead|architect|tech\s*lead/.test(lower))
    return 'Sees the big picture without losing sight of the details. Decides fast, revisits when the data says so.';
  if (/frontend|ui|design/.test(lower))
    return 'Pixel-aware and user-obsessed. If it looks off by one, it is off by one.';
  if (/backend|api|server/.test(lower))
    return 'Data flows in, answers flow out. Keeps the plumbing tight and the contracts clear.';
  if (/test|qa|quality/.test(lower))
    return 'Breaks things on purpose so users never break them by accident.';
  if (/devops|infra|platform/.test(lower))
    return 'If it ships, it ships reliably. Automates everything twice.';
  if (/docs|devrel|writer/.test(lower))
    return 'Turns complexity into clarity. If the docs are wrong, the product is wrong.';
  if (/data|database|analytics/.test(lower))
    return 'Thinks in tables and queries. Normalizes first, denormalizes when the numbers demand it.';
  if (/security|auth/.test(lower))
    return 'Paranoid by design. Assumes every input is hostile until proven otherwise.';
  if (/session|scribe|log/.test(lower))
    return 'Silent observer. Keeps the record straight so the team never loses context.';
  if (/monitor|queue|work/.test(lower))
    return 'Watches the board, keeps the queue honest, nudges when things stall.';
  return 'Focused and reliable. Gets the job done without fanfare.';
}

function ownershipFromRole(role: string, scope: string): string {
  const items = scope.split(',').map(s => s.trim()).filter(Boolean);
  if (items.length >= 3) return items.slice(0, 3).map(i => `- ${i}`).join('\n');
  if (items.length > 0) return items.map(i => `- ${i}`).join('\n');
  return `- ${role} domain tasks`;
}

function generateCharter(member: CastMember): string {
  const nameLower = member.name.toLowerCase();
  return `# ${member.name} — ${member.role}

> ${personalityForRole(member.role)}

## Identity

- **Name:** ${member.name}
- **Role:** ${member.role}
- **Expertise:** ${member.scope}
- **Style:** Direct and focused.

## What I Own

${ownershipFromRole(member.role, member.scope)}

## How I Work

- Read decisions.md before starting
- Write decisions to inbox when making team-relevant choices
- Focused, practical, gets things done

## Boundaries

**I handle:** ${member.scope}

**I don't handle:** Work outside my domain — the coordinator routes that elsewhere.

**When I'm unsure:** I say so and suggest who might know.

**If I review others' work:** On rejection, I may require a different agent to revise (not the original author) or request a new specialist be spawned. The Coordinator enforces this.

## Model

- **Preferred:** auto
- **Rationale:** Coordinator selects the best model based on task type
- **Fallback:** Standard chain

## Collaboration

Before starting work, run \`git rev-parse --show-toplevel\` to find the repo root, or use the \`TEAM ROOT\` provided in the spawn prompt. All \`.squad/\` paths must be resolved relative to this root.

Before starting work, read \`.squad/decisions.md\` for team decisions that affect me.
After making a decision others should know, write it to \`.squad/decisions/inbox/${nameLower}-{brief-slug}.md\`.
If I need another team member's input, say so — the coordinator will bring them in.

## Voice

${personalityForRole(member.role)}
`;
}

function generateHistory(member: CastMember, projectDescription: string): string {
  return `# ${member.name} — History

## Core Context

- **Project:** ${projectDescription}
- **Role:** ${member.role}
- **Joined:** ${new Date().toISOString()}

## Learnings

<!-- Append learnings below -->
`;
}

// ── Built-in agents ────────────────────────────────────────────────

function scribeMember(): CastMember {
  return { name: 'Scribe', role: 'Session Logger', scope: 'Maintaining decisions.md, cross-agent context sharing, orchestration logging, session logging, git commits', emoji: '📋' };
}

function scribeCharter(): string {
  const m = scribeMember();
  return generateCharter(m);
}

function ralphMember(): CastMember {
  return { name: 'Ralph', role: 'Work Monitor', scope: 'Work queue tracking, backlog management, keep-alive', emoji: '🔄' };
}

function ralphCharter(): string {
  const m = ralphMember();
  return generateCharter(m);
}

// ── Team file updaters ─────────────────────────────────────────────

function buildMembersTable(allMembers: CastMember[]): string {
  let table = `## Members\n\n| Name | Role | Charter | Status |\n|------|------|---------|--------|\n`;
  for (const m of allMembers) {
    const nameLower = m.name.toLowerCase();
    let status = '✅ Active';
    if (m.role === 'Session Logger') status = '📋 Silent';
    if (m.role === 'Work Monitor') status = '🔄 Monitor';
    table += `| ${m.name} | ${m.role} | \`.squad/agents/${nameLower}/charter.md\` | ${status} |\n`;
  }
  return table;
}

function buildRoutingTable(members: CastMember[]): string {
  let table = `## Work Type → Agent\n\n| Work Type | Primary | Secondary |\n|-----------|---------|----------|\n`;
  for (const m of members) {
    if (m.role === 'Session Logger' || m.role === 'Work Monitor') continue;
    table += `| ${m.scope} | ${m.name} | — |\n`;
  }
  return table;
}

// ── Main cast function ─────────────────────────────────────────────

/**
 * Create all squad agent files for a cast proposal.
 * teamRoot is the project root (parent of .squad/).
 */
export async function createTeam(teamRoot: string, proposal: CastProposal): Promise<CastResult> {
  const squadDir = join(teamRoot, '.squad');
  const agentsDir = join(squadDir, 'agents');
  const castingDir = join(squadDir, 'casting');
  const filesCreated: string[] = [];
  const membersCreated: string[] = [];
  const now = new Date().toISOString();

  // Ensure directories exist
  await mkdir(agentsDir, { recursive: true });
  await mkdir(castingDir, { recursive: true });

  // Collect all members (proposal + built-ins)
  const allMembers = [...proposal.members];

  const hasScribe = proposal.members.some(m => /scribe/i.test(m.name));
  if (!hasScribe) allMembers.push(scribeMember());

  const hasRalph = proposal.members.some(m => /ralph/i.test(m.name));
  if (!hasRalph) allMembers.push(ralphMember());

  // Create agent directories and files
  for (const member of allMembers) {
    const nameLower = member.name.toLowerCase();
    const agentDir = join(agentsDir, nameLower);
    await mkdir(agentDir, { recursive: true });

    const charterPath = join(agentDir, 'charter.md');
    let charter: string;
    if (member.name === 'Scribe' && !hasScribe) {
      charter = scribeCharter();
    } else if (member.name === 'Ralph' && !hasRalph) {
      charter = ralphCharter();
    } else {
      charter = generateCharter(member);
    }
    await writeFile(charterPath, charter);
    filesCreated.push(charterPath);

    const historyPath = join(agentDir, 'history.md');
    await writeFile(historyPath, generateHistory(member, proposal.projectDescription));
    filesCreated.push(historyPath);

    membersCreated.push(member.name);
  }

  // Update team.md — preserve content before and after ## Members
  const teamPath = join(squadDir, 'team.md');
  if (existsSync(teamPath)) {
    const content = await readFile(teamPath, 'utf8');
    const membersIdx = content.indexOf('## Members');
    if (membersIdx !== -1) {
      const before = content.slice(0, membersIdx);
      // Find next ## header after Members
      const afterMembers = content.slice(membersIdx + '## Members'.length);
      const nextHeaderMatch = afterMembers.match(/\n(## [^\n]+)/);
      const nextHeader = nextHeaderMatch?.[1];
      const after = nextHeader
        ? afterMembers.slice(afterMembers.indexOf(nextHeader))
        : '';
      const newContent = before + buildMembersTable(allMembers) + '\n' + after;
      await writeFile(teamPath, newContent);
      filesCreated.push(teamPath);
    }
  }

  // Update routing.md — append routing table
  const routingPath = join(squadDir, 'routing.md');
  if (existsSync(routingPath)) {
    const content = await readFile(routingPath, 'utf8');
    const table = buildRoutingTable(allMembers);
    await writeFile(routingPath, content.trimEnd() + '\n\n' + table + '\n');
    filesCreated.push(routingPath);
  }

  // Create casting state files
  const registryAgents: Record<string, object> = {};
  const snapshotAgents: string[] = [];
  for (const member of allMembers) {
    const nameLower = member.name.toLowerCase();
    registryAgents[nameLower] = {
      created_at: now,
      persistent_name: member.name,
      universe: proposal.universe,
      status: 'active',
    };
    snapshotAgents.push(nameLower);
  }

  const registry = { agents: registryAgents };
  await writeFile(join(castingDir, 'registry.json'), JSON.stringify(registry, null, 2) + '\n');
  filesCreated.push(join(castingDir, 'registry.json'));

  const history = {
    assignment_cast_snapshots: {
      [`repl-cast-${now}`]: {
        created_at: now,
        agents: snapshotAgents,
        universe: proposal.universe,
      },
    },
    universe_usage_history: [
      { universe: proposal.universe, used_at: now },
    ],
  };
  await writeFile(join(castingDir, 'history.json'), JSON.stringify(history, null, 2) + '\n');
  filesCreated.push(join(castingDir, 'history.json'));

  const policy = { universe_allowlist: ['*'], max_capacity: 25 };
  await writeFile(join(castingDir, 'policy.json'), JSON.stringify(policy, null, 2) + '\n');
  filesCreated.push(join(castingDir, 'policy.json'));

  return { teamRoot, membersCreated, filesCreated };
}

// ── Display helpers ────────────────────────────────────────────────

/** Format a cast proposal as a human-readable summary. */
export function formatCastSummary(proposal: CastProposal): string {
  const lines: string[] = [];

  for (const m of proposal.members) {
    const nameCol = m.name.padEnd(10);
    const roleCol = m.role.padEnd(15);
    lines.push(`${m.emoji}  ${nameCol} — ${roleCol} ${m.scope}`);
  }

  // Always show Scribe and Ralph in the summary
  const hasScribe = proposal.members.some(m => /scribe/i.test(m.name));
  if (!hasScribe) {
    lines.push(`📋  ${'Scribe'.padEnd(10)} — ${'(silent)'.padEnd(15)} Memory, decisions, session logs`);
  }

  const hasRalph = proposal.members.some(m => /ralph/i.test(m.name));
  if (!hasRalph) {
    lines.push(`🔄  ${'Ralph'.padEnd(10)} — ${'(monitor)'.padEnd(15)} Work queue, backlog, keep-alive`);
  }

  return lines.join('\n');
}
