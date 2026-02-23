import React, { useState, useEffect, useRef } from 'react';
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

type ThinkingPhase = 'connecting' | 'routing' | 'streaming';

const PHASE_LABELS: Record<ThinkingPhase, string> = {
  connecting: 'Connecting',
  routing: 'Routing',
  streaming: 'Streaming',
};

const SPINNER_FRAMES = ['⠋', '⠙', '⠹', '⠸', '⠼', '⠴', '⠦', '⠧', '⠇', '⠏'];

/** Color cycles through as time passes — feels alive. */
function spinnerColor(elapsed: number): string {
  if (elapsed < 3) return 'cyan';
  if (elapsed < 8) return 'yellow';
  return 'magenta';
}

/** Animated spinner with elapsed time, phase transitions, and color cycling. */
const ThinkingIndicator: React.FC<{ label: string; hasContent: boolean }> = ({ label, hasContent }) => {
  const [frame, setFrame] = useState(0);
  const [elapsed, setElapsed] = useState(0);
  const startRef = useRef(Date.now());

  // Phase transitions: connecting → routing → streaming
  const phase: ThinkingPhase = hasContent ? 'streaming' : elapsed < 1 ? 'connecting' : 'routing';

  useEffect(() => {
    startRef.current = Date.now();
    setElapsed(0);
    setFrame(0);
  }, [label]);

  useEffect(() => {
    const timer = setInterval(() => {
      setFrame(f => (f + 1) % SPINNER_FRAMES.length);
      setElapsed(Math.floor((Date.now() - startRef.current) / 1000));
    }, 80);
    return () => clearInterval(timer);
  }, []);

  const color = spinnerColor(elapsed);
  const elapsedStr = elapsed > 0 ? ` (${elapsed}s)` : '';

  return (
    <Box gap={1}>
      <Text color={color}>{SPINNER_FRAMES[frame] ?? '⠋'}</Text>
      <Text color={color}>{PHASE_LABELS[phase]}</Text>
      <Text dimColor>— {label}{elapsedStr}</Text>
    </Box>
  );
};

/** Format elapsed seconds for response timestamps. */
function formatDuration(start: Date, end: Date): string {
  const ms = end.getTime() - start.getTime();
  if (ms < 1000) return `${ms}ms`;
  return `${(ms / 1000).toFixed(1)}s`;
}

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

  // Compute response duration: time from previous user message to this agent message
  const getResponseDuration = (index: number): string | null => {
    const msg = visible[index];
    if (!msg || msg.role !== 'agent') return null;
    // Walk backward to find the preceding user message
    for (let j = index - 1; j >= 0; j--) {
      if (visible[j]?.role === 'user') {
        return formatDuration(visible[j]!.timestamp, msg.timestamp);
      }
    }
    return null;
  };

  return (
    <Box flexDirection="column" flexGrow={1} marginTop={1}>
      {visible.map((msg, i) => {
        const isNewTurn = msg.role === 'user' && i > 0;
        const agentRole = msg.agentName ? roleMap.get(msg.agentName) : undefined;
        const emoji = agentRole ? getRoleEmoji(agentRole) : '';
        const duration = getResponseDuration(i);

        return (
          <React.Fragment key={i}>
            {isNewTurn && <Text dimColor>{'─'.repeat(50)}</Text>}
            <Box gap={1}>
              {msg.role === 'user' ? (
                <>
                  <Text color="cyan" bold>❯ you:</Text>
                  <Text color="cyan" wrap="wrap">{msg.content}</Text>
                </>
              ) : msg.role === 'system' ? (
                <>
                  <Text dimColor>◇ system:</Text>
                  <Text dimColor wrap="wrap">{msg.content}</Text>
                </>
              ) : (
                <>
                  <Text color="green" bold>{emoji ? `${emoji} ` : ''}{msg.agentName ?? 'agent'}:</Text>
                  <Text wrap="wrap">{msg.content}</Text>
                  {duration && <Text dimColor>({duration})</Text>}
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
          <Text color="cyan">▌</Text>
        </Box>
      )}

      {/* Thinking indicator — shown when processing but no content yet */}
      {processing && !streamingContent?.content && (
        <ThinkingIndicator label={getThinkingLabel()} hasContent={false} />
      )}

      {/* Thinking indicator alongside streaming — shows elapsed while content flows */}
      {processing && streamingContent?.content && (
        <ThinkingIndicator label={`${streamingContent.agentName} streaming`} hasContent={true} />
      )}
    </Box>
  );
};
