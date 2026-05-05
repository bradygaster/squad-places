import {
  defineAgent,
  defineDefaults,
  defineRouting,
  defineSquad,
  defineTeam,
} from '@bradygaster/squad-sdk';

const repoAnalystCharter = `You are Repo Analyst, a specialist in uploaded Git repository analysis for Squad Places.

Your job:
- inspect the attached repository snapshot
- identify the stack, repo purpose, architecture, and notable constraints
- summarize Copilot instructions, agents, skills, docs, and existing .squad state when present
- propose tools, MCP servers, plugins, and directives that would help a hackathon team build quickly
- produce concise, factual JSON with no markdown fences`;

const briefWriterCharter = `You are Brief Writer, a specialist in turning analyzed repositories into hackathon briefs.

Your job:
- read the provided repository analysis objects
- generate a motivating but practical hackathon brief
- keep the brief grounded in the actual repository evidence
- suggest roles, tools, MCP servers, skills, plugins, presentation instructions, and winner criteria
- produce concise, factual JSON with no markdown fences`;

export const hackathonSquad = defineSquad({
  version: '1.0.0',
  team: defineTeam({
    name: 'Hackathon Analysis Squad',
    description: 'Analyzes uploaded repositories and drafts actionable hackathon briefs for Squad Places.',
    projectContext: 'Used by the Squad Places Razor Pages app to inspect uploaded Git repo snapshots and generate hackathon brief drafts.',
    members: ['@repo-analyst', '@brief-writer'],
  }),
  defaults: defineDefaults({
    model: {
      preferred: 'claude-sonnet-4.5',
      fallback: 'claude-haiku-4.5',
      rationale: 'Good balance of repo comprehension, synthesis quality, and cost.',
    },
  }),
  agents: [
    defineAgent({
      name: 'repo-analyst',
      role: 'Repository Analyst',
      description: 'Extracts architecture, guidance, and opportunities from uploaded repository snapshots.',
      charter: repoAnalystCharter,
      capabilities: [
        { name: 'repo-analysis', level: 'expert' },
        { name: 'architecture-summarization', level: 'proficient' },
        { name: 'developer-guidance-extraction', level: 'proficient' },
      ],
      status: 'active',
    }),
    defineAgent({
      name: 'brief-writer',
      role: 'Hackathon Brief Writer',
      description: 'Turns repository analysis into a focused hackathon brief with concrete execution guidance.',
      charter: briefWriterCharter,
      capabilities: [
        { name: 'brief-generation', level: 'expert' },
        { name: 'team-scoping', level: 'proficient' },
        { name: 'presentation-guidance', level: 'proficient' },
      ],
      status: 'active',
    }),
  ],
  routing: defineRouting({
    rules: [
      {
        pattern: 'analyze*',
        agents: ['@repo-analyst'],
        tier: 'standard',
        priority: 1,
        description: 'Analyze uploaded repositories and summarize their developer guidance.',
      },
      {
        pattern: 'generate-brief*',
        agents: ['@brief-writer'],
        tier: 'standard',
        priority: 1,
        description: 'Generate a hackathon brief from analyzed repositories.',
      },
      {
        pattern: '*',
        agents: ['@repo-analyst'],
        tier: 'lightweight',
        priority: 99,
        description: 'Fallback to repo analysis when the prompt does not match a more specific rule.',
      },
    ],
    defaultAgent: '@repo-analyst',
    fallback: 'default-agent',
  }),
});

export const runtimeAgents = hackathonSquad.agents.map(agent => ({
  name: agent.name,
  displayName: agent.role,
  description: agent.description,
  prompt: agent.charter ?? agent.description ?? agent.role,
  infer: true,
}));

export default hackathonSquad;
