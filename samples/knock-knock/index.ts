/**
 * knock-knock — Real LLM Knock-Knock Joke Exchange
 *
 * Two Copilot sessions trade knock-knock jokes forever.
 * Demonstrates: SquadClientWithPool, CastingEngine, StreamingPipeline,
 * and live LLM-generated comedy.
 *
 * GITHUB_TOKEN required.
 */

import { CastingEngine, StreamingPipeline } from '@bradygaster/squad-sdk';
import type { StreamDelta } from '@bradygaster/squad-sdk';
import { SquadClientWithPool } from '@bradygaster/squad-sdk/client';

// ── Agent Setup ──────────────────────────────────────────────────────

interface AgentInfo {
  name: string;
  role: string;
  systemPrompt: string;
  sessionId?: string;
}

const TELLER_PROMPT = `You are a comedian performing knock-knock jokes. When prompted, tell ONE knock-knock joke. Keep the format: "Knock knock!" then wait for the response, then deliver the setup and punchline. Be creative and funny. Keep responses short — just the joke, no commentary.`;

const RESPONDER_PROMPT = `You are the audience for a knock-knock joke. Respond naturally to each part. Say "Who's there?" after "Knock knock!" and "[setup] who?" after the setup line. After the punchline, react with a short genuine response (laugh, groan, or witty comeback). Keep responses to one line.`;

// ── Main Loop ────────────────────────────────────────────────────────

async function main(): Promise<void> {
  // Auth check
  if (!process.env.GITHUB_TOKEN) {
    console.error('\n❌ Missing GITHUB_TOKEN environment variable.\n');
    console.error('Setup instructions:');
    console.error('  1. Generate a token at https://github.com/settings/tokens');
    console.error('  2. Set GITHUB_TOKEN in your environment:');
    console.error('     export GITHUB_TOKEN=ghp_...\n');
    process.exit(1);
  }

  const casting = new CastingEngine();
  const team = casting.castTeam({
    universe: 'usual-suspects',
    requiredRoles: ['developer', 'tester'],
    teamSize: 2,
  });

  const [agentA, agentB] = team;

  const agents: AgentInfo[] = [
    {
      name: agentA.name,
      role: agentA.role,
      systemPrompt: TELLER_PROMPT,
    },
    {
      name: agentB.name,
      role: agentB.role,
      systemPrompt: RESPONDER_PROMPT,
    },
  ];

  console.log('\n🎭 Knock-Knock Comedy Hour (Live LLM Edition)\n');
  console.log(`   ${agents[0].name} (Teller) vs. ${agents[1].name} (Responder)\n`);
  console.log('   Connecting to Copilot...\n');

  // Connect to Copilot
  const client = new SquadClientWithPool({ githubToken: process.env.GITHUB_TOKEN });
  
  try {
    await client.connect();
  } catch (err) {
    const msg = err instanceof Error ? err.message : String(err);
    console.error(`\n❌ Connection failed: ${msg}\n`);
    console.error('Verify your GITHUB_TOKEN is valid and has Copilot access.\n');
    process.exit(1);
  }

  // Create sessions
  const pipeline = new StreamingPipeline();
  pipeline.onDelta((event) => {
    process.stdout.write(event.content);
  });

  for (const agent of agents) {
    const session = await client.createSession({
      streaming: true,
      systemMessage: { mode: 'append', content: agent.systemPrompt },
    });
    agent.sessionId = session.sessionId;
    pipeline.attachToSession(session.sessionId);
  }

  console.log('   ✓ Connected. Let the jokes begin!\n');

  // Infinite joke loop
  let jokeCount = 0;

  while (true) {
    const teller = agents[jokeCount % 2];
    const responder = agents[(jokeCount + 1) % 2];

    // Teller starts the joke
    process.stdout.write(`🎭 ${teller.name}: `);
    const tellerResponse = await sendAndCapture(
      client,
      pipeline,
      teller,
      'Tell me a knock-knock joke!',
    );
    console.log();

    await new Promise((r) => setTimeout(r, 1000));

    // Responder reacts to the joke
    process.stdout.write(`🎭 ${responder.name}: `);
    const responderReaction = await sendAndCapture(
      client,
      pipeline,
      responder,
      tellerResponse,
    );
    console.log();

    await new Promise((r) => setTimeout(r, 1000));

    // Teller acknowledges the reaction
    process.stdout.write(`🎭 ${teller.name}: `);
    await sendAndCapture(
      client,
      pipeline,
      teller,
      responderReaction,
    );
    console.log('\n');

    // Swap roles for next iteration
    agents.reverse();
    jokeCount++;

    await new Promise((r) => setTimeout(r, 3000));
  }
}

// ── Helper: Send message and capture full response ──────────────────

async function sendAndCapture(
  client: SquadClientWithPool,
  pipeline: StreamingPipeline,
  agent: AgentInfo,
  message: string,
): Promise<string> {
  const sessionId = agent.sessionId!;
  let captured = '';

  pipeline.markMessageStart(sessionId);

  const session = await client.resumeSession(sessionId);

  const handler = (event: { type: string; [key: string]: unknown }) => {
    if (event.type === 'message_delta') {
      const content =
        (event['deltaContent'] as string) ??
        (event['delta'] as string) ??
        (event['content'] as string) ??
        '';
      if (content) {
        captured += content;
        void pipeline.processEvent({
          type: 'message_delta',
          sessionId,
          agentName: agent.name,
          content,
          index: typeof event['index'] === 'number' ? event['index'] : 0,
          timestamp: new Date(),
        });
      }
    }
  };

  session.on('message_delta', handler);

  try {
    if (session.sendAndWait) {
      await session.sendAndWait({ prompt: message }, 30_000);
    } else {
      await session.sendMessage({ prompt: message });
    }
  } finally {
    session.off('message_delta', handler);
  }

  return captured.trim();
}

main().catch((err) => {
  console.error('❌ Fatal error:', err);
  process.exit(1);
});
