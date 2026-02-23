import { describe, it, expect, vi } from 'vitest';

/**
 * Tests for CopilotSessionAdapter — the runtime bridge between
 * CopilotSession (send/on/destroy) and SquadSession (sendMessage/on/off/close).
 *
 * We can't import the adapter class directly (it's file-scoped in client.ts),
 * so we test it via SquadClient.createSession() with a mocked CopilotClient.
 */

// Build a minimal mock CopilotSession matching the real @github/copilot-sdk shape
function createMockCopilotSession(sessionId = 'test-session-42') {
  const typedHandlers = new Map<string, Set<(event: any) => void>>();

  return {
    sessionId,
    send: vi.fn().mockResolvedValue('msg-1'),
    sendAndWait: vi.fn().mockResolvedValue(undefined),
    destroy: vi.fn().mockResolvedValue(undefined),
    abort: vi.fn().mockResolvedValue(undefined),
    getMessages: vi.fn().mockResolvedValue([]),
    on: vi.fn((eventType: string, handler: (event: any) => void) => {
      if (!typedHandlers.has(eventType)) {
        typedHandlers.set(eventType, new Set());
      }
      typedHandlers.get(eventType)!.add(handler);
      return () => {
        typedHandlers.get(eventType)?.delete(handler);
      };
    }),
    _dispatchEvent: vi.fn(),
    registerTools: vi.fn(),
    getToolHandler: vi.fn(),
    registerPermissionHandler: vi.fn(),
    registerUserInputHandler: vi.fn(),
    registerHooks: vi.fn(),
    // expose for test assertions
    _typedHandlers: typedHandlers,
  };
}

// We test the adapter indirectly by importing SquadClient and stubbing internals.
// The adapter is constructed inside createSession(), so we mock the CopilotClient.
import { SquadClient } from '@bradygaster/squad-sdk/client';

describe('CopilotSessionAdapter (via SquadClient)', () => {
  /** Helper: create a SquadClient wired to our mock */
  async function createAdaptedSession() {
    const client = new SquadClient({ autoStart: false });

    // Force connected state
    (client as any).state = 'connected';

    // Inject mock CopilotSession via the inner CopilotClient
    const mockSession = createMockCopilotSession();
    (client as any).client.createSession = vi.fn().mockResolvedValue(mockSession);

    const session = await client.createSession();
    return { session, mockSession };
  }

  it('sessionId is accessible', async () => {
    const { session } = await createAdaptedSession();
    expect(session.sessionId).toBe('test-session-42');
  });

  it('sendMessage() delegates to CopilotSession.send()', async () => {
    const { session, mockSession } = await createAdaptedSession();

    await session.sendMessage({ prompt: 'Hello from adapter' });

    expect(mockSession.send).toHaveBeenCalledOnce();
    expect(mockSession.send).toHaveBeenCalledWith({ prompt: 'Hello from adapter' });
  });

  it('sendMessage() passes attachments and mode through', async () => {
    const { session, mockSession } = await createAdaptedSession();

    const opts = {
      prompt: 'Check this file',
      attachments: [{ type: 'file' as const, path: './src/index.ts' }],
      mode: 'immediate' as const,
    };
    await session.sendMessage(opts);

    expect(mockSession.send).toHaveBeenCalledWith(opts);
  });

  it('on() registers event handler via CopilotSession.on()', async () => {
    const { session, mockSession } = await createAdaptedSession();

    const handler = vi.fn();
    session.on('message_delta', handler);

    expect(mockSession.on).toHaveBeenCalledWith('message_delta', handler);
  });

  it('off() calls the unsubscribe function returned by CopilotSession.on()', async () => {
    const { session, mockSession } = await createAdaptedSession();

    const handler = vi.fn();
    session.on('message_delta', handler);

    // Handler should be registered
    expect(mockSession._typedHandlers.get('message_delta')?.has(handler)).toBe(true);

    session.off('message_delta', handler);

    // The unsubscribe should have removed it
    expect(mockSession._typedHandlers.get('message_delta')?.has(handler)).toBe(false);
  });

  it('off() is a no-op for unregistered handlers', async () => {
    const { session } = await createAdaptedSession();

    // Should not throw
    const unknownHandler = vi.fn();
    session.off('message_delta', unknownHandler);
  });

  it('close() delegates to CopilotSession.destroy()', async () => {
    const { session, mockSession } = await createAdaptedSession();

    await session.close();

    expect(mockSession.destroy).toHaveBeenCalledOnce();
  });

  it('close() clears tracked unsubscribers', async () => {
    const { session } = await createAdaptedSession();

    const handler = vi.fn();
    session.on('message_delta', handler);

    await session.close();

    // After close, off() should be a no-op (no throw)
    session.off('message_delta', handler);
  });
});

describe('CopilotSessionAdapter via resumeSession', () => {
  it('resumeSession also wraps in adapter', async () => {
    const client = new SquadClient({ autoStart: false });
    (client as any).state = 'connected';

    const mockSession = createMockCopilotSession('resumed-session-99');
    (client as any).client.resumeSession = vi.fn().mockResolvedValue(mockSession);

    const session = await client.resumeSession('resumed-session-99');

    expect(session.sessionId).toBe('resumed-session-99');
    await session.sendMessage({ prompt: 'resumed' });
    expect(mockSession.send).toHaveBeenCalledWith({ prompt: 'resumed' });
  });
});
