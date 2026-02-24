/**
 * ThinkingIndicator — clean feedback during agent operations.
 *
 * Shows spinner + activity context + elapsed time.
 * Default label: "Routing to agent..." (covers SDK connection, initial routing).
 * Activity hints from SDK events or @Agent mentions override the default.
 *
 * Owned by Cheritto (TUI Engineer). Design approved by Marquez.
 */

import React, { useState, useEffect } from 'react';
import { Box, Text } from 'ink';
import { isNoColor } from '../terminal.js';

export interface ThinkingIndicatorProps {
  isThinking: boolean;
  elapsedMs: number;
  activityHint?: string;
}

/** Default thinking label — exported for backward compat with tests. */
export const THINKING_PHRASES = ['Routing to agent'];

const SPINNER_FRAMES = ['⠋', '⠙', '⠹', '⠸', '⠼', '⠴', '⠦', '⠧', '⠇', '⠏'];

/** Color cycles through as time passes — feels alive. */
function indicatorColor(elapsedSec: number): string {
  if (elapsedSec < 5) return 'cyan';
  if (elapsedSec < 15) return 'yellow';
  return 'magenta';
}

function formatElapsed(ms: number): string {
  const sec = Math.floor(ms / 1000);
  if (sec < 1) return '';
  return `${sec}s`;
}

/** Static dots for NO_COLOR mode (no animation). */
const STATIC_SPINNER = '...';

export const ThinkingIndicator: React.FC<ThinkingIndicatorProps> = ({
  isThinking,
  elapsedMs,
  activityHint,
}) => {
  const noColor = isNoColor();
  const [frame, setFrame] = useState(0);

  // Spinner animation — 80ms per frame (disabled in NO_COLOR)
  useEffect(() => {
    if (!isThinking || noColor) return;
    const timer = setInterval(() => {
      setFrame(f => (f + 1) % SPINNER_FRAMES.length);
    }, 80);
    return () => clearInterval(timer);
  }, [isThinking, noColor]);

  // Reset frame when thinking starts
  useEffect(() => {
    if (isThinking) setFrame(0);
  }, [isThinking]);

  if (!isThinking) return null;

  const elapsedSec = Math.floor(elapsedMs / 1000);
  const elapsedStr = formatElapsed(elapsedMs);
  const spinnerChar = noColor ? STATIC_SPINNER : (SPINNER_FRAMES[frame] ?? '⠋');

  // NO_COLOR: no color props, use text labels
  if (noColor) {
    return (
      <Box gap={1}>
        <Text>{spinnerChar}</Text>
        <Text>{activityHint ?? 'Routing to agent...'}</Text>
        {elapsedStr ? <Text>({elapsedStr})</Text> : null}
      </Box>
    );
  }

  const color = indicatorColor(elapsedSec);

  // Activity hint takes priority when available
  if (activityHint) {
    return (
      <Box gap={1}>
        <Text color={color}>{spinnerChar}</Text>
        <Text color={color} italic>{activityHint}</Text>
        {elapsedStr ? <Text dimColor>({elapsedStr})</Text> : null}
      </Box>
    );
  }

  // Default: "Routing to agent..." label
  return (
    <Box gap={1}>
      <Text color={color}>{spinnerChar}</Text>
      <Text color={color} italic>Routing to agent...</Text>
      {elapsedStr ? <Text dimColor>({elapsedStr})</Text> : null}
    </Box>
  );
};
