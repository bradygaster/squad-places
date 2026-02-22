import React from 'react';
import { Box, Text } from 'ink';
import { getRoleEmoji } from '../lifecycle.js';
import type { AgentSession } from '../types.js';

interface AgentPanelProps {
  agents: AgentSession[];
  streamingContent?: { agentName: string; content: string } | null;
}

export const AgentPanel: React.FC<AgentPanelProps> = ({ agents, streamingContent }) => {
  if (agents.length === 0) return null;

  const hasActive = agents.some(a => a.status === 'working' || a.status === 'streaming');

  return (
    <Box flexDirection="column" paddingX={1} marginTop={1}>
      <Box flexWrap="wrap" gap={1}>
        {agents.map((agent) => {
          const active = agent.status === 'streaming' || agent.status === 'working';
          const errored = agent.status === 'error';
          return (
            <Text
              key={agent.name}
              dimColor={!active && !errored}
              bold={active}
              color={active ? 'green' : errored ? 'red' : undefined}
            >
              {getRoleEmoji(agent.role)} {agent.name}{active ? ' ●' : errored ? ' ✖' : ''}
            </Text>
          );
        })}
      </Box>
      {streamingContent ? (
        <Text color="yellow">
          {'  '}💭 {streamingContent.agentName} is responding{streamingContent.content ? '...' : ''}
        </Text>
      ) : !hasActive ? (
        <Text dimColor>{'  '}Team idle — ready for work</Text>
      ) : null}
    </Box>
  );
};
