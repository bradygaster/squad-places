import React, { useState, useEffect } from 'react';
import { Box, Text } from 'ink';
import { getRoleEmoji } from '../lifecycle.js';
import type { ShellMessage, AgentSession } from '../types.js';

interface MessageStreamProps {
  messages: ShellMessage[];
  agents?: AgentSession[];
  streamingContent?: { agentName: string; content: string } | null;
  processing?: boolean;
  maxVisible?: number;
}

/** Animated spinner shown while waiting for agent response. */
const ThinkingIndicator: React.FC<{ label: string }> = ({ label }) => {
  const [frame, setFrame] = useState(0);
  const frames = ['⠋', '⠙', '⠹', '⠸', '⠼', '⠴', '⠦', '⠧', '⠇', '⠏'];

  useEffect(() => {
    const timer = setInterval(() => {
      setFrame(f => (f + 1) % frames.length);
    }, 80);
    return () => clearInterval(timer);
  }, []);

  return (
    <Box gap={1}>
      <Text color="yellow">{frames[frame] ?? '⠋'}</Text>
      <Text dimColor>💭 {label}</Text>
    </Box>
  );
};

export const MessageStream: React.FC<MessageStreamProps> = ({
  messages,
  agents,
  streamingContent,
  processing = false,
  maxVisible = 50,
}) => {
  const visible = messages.slice(-maxVisible);
  const roleMap = new Map((agents ?? []).map(a => [a.name, a.role]));

  // Determine thinking label from last user message when waiting
  const getThinkingLabel = (): string => {
    const lastUser = [...messages].reverse().find(m => m.role === 'user');
    if (lastUser) {
      const atMatch = lastUser.content.match(/^@(\w+)/);
      if (atMatch?.[1]) return `${atMatch[1]} is thinking...`;
    }
    return 'Routing your request...';
  };

  return (
    <Box flexDirection="column" flexGrow={1} marginTop={1}>
      {visible.map((msg, i) => {
        const isNewTurn = msg.role === 'user' && i > 0;
        const agentRole = msg.agentName ? roleMap.get(msg.agentName) : undefined;
        const emoji = agentRole ? getRoleEmoji(agentRole) : '';

        return (
          <React.Fragment key={i}>
            {isNewTurn && <Text dimColor>{'─'.repeat(40)}</Text>}
            <Box gap={1}>
              {msg.role === 'user' ? (
                <>
                  <Text color="cyan" bold>❯ you:</Text>
                  <Text bold wrap="wrap">{msg.content}</Text>
                </>
              ) : msg.role === 'system' ? (
                <>
                  <Text dimColor>⚙ system:</Text>
                  <Text dimColor wrap="wrap">{msg.content}</Text>
                </>
              ) : (
                <>
                  <Text color="green" bold>{emoji ? `${emoji} ` : ''}{msg.agentName ?? 'agent'}:</Text>
                  <Text wrap="wrap">{msg.content}</Text>
                </>
              )}
            </Box>
          </React.Fragment>
        );
      })}

      {/* Streaming content with live cursor */}
      {streamingContent && streamingContent.content && (
        <Box gap={1}>
          <Text color="green" bold>
            {roleMap.has(streamingContent.agentName)
              ? `${getRoleEmoji(roleMap.get(streamingContent.agentName)!)} `
              : ''}
            {streamingContent.agentName}:
          </Text>
          <Text wrap="wrap">{streamingContent.content}</Text>
          <Text color="gray">▌</Text>
        </Box>
      )}

      {/* Thinking indicator — shown when processing but no content yet */}
      {processing && !streamingContent?.content && (
        <ThinkingIndicator label={getThinkingLabel()} />
      )}
    </Box>
  );
};
