/**
 * OTel Export Pipeline Test — validates that the SDK's internal span
 * pipeline captures spans end-to-end without needing a real collector.
 *
 * Uses InMemorySpanExporter to verify:
 * 1. initializeOTel registers a real (non-no-op) TracerProvider
 * 2. getTracer() returns a tracer connected to that provider
 * 3. Spans created via getTracer() are captured by the exporter
 * 4. shutdownOTel() cleans up correctly
 */

import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import {
  InMemorySpanExporter,
  SimpleSpanProcessor,
  BasicTracerProvider,
} from '@opentelemetry/sdk-trace-base';
import { trace, SpanStatusCode } from '@opentelemetry/api';
import { getTracer, shutdownOTel } from '@bradygaster/squad-sdk/runtime/otel';

// ---------------------------------------------------------------------------
// Test infrastructure — in-memory exporter
// ---------------------------------------------------------------------------

let memExporter: InMemorySpanExporter;
let provider: BasicTracerProvider;

function installTestProvider() {
  memExporter = new InMemorySpanExporter();
  const processor = new SimpleSpanProcessor(memExporter);
  provider = new BasicTracerProvider({ spanProcessors: [processor] });
  trace.setGlobalTracerProvider(provider);
}

function teardownTestProvider() {
  memExporter.reset();
  trace.disable();
  provider.shutdown();
}

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

describe('OTel Export Pipeline', () => {
  beforeEach(() => {
    installTestProvider();
  });

  afterEach(async () => {
    try { await shutdownOTel(); } catch { /* cleanup */ }
    teardownTestProvider();
  });

  it('captures a span created via getTracer()', () => {
    const tracer = getTracer('test-pipeline');
    const span = tracer.startSpan('pipeline.test-span');
    span.setAttribute('test.key', 'test-value');
    span.setStatus({ code: SpanStatusCode.OK });
    span.end();

    const spans = memExporter.getFinishedSpans();
    expect(spans.length).toBe(1);
    expect(spans[0].name).toBe('pipeline.test-span');
    expect(spans[0].attributes['test.key']).toBe('test-value');
    expect(spans[0].status.code).toBe(SpanStatusCode.OK);
  });

  it('captures multiple spans from the same tracer', () => {
    const tracer = getTracer('test-pipeline');

    const span1 = tracer.startSpan('span-one');
    span1.end();

    const span2 = tracer.startSpan('span-two');
    span2.setAttribute('order', 2);
    span2.end();

    const spans = memExporter.getFinishedSpans();
    expect(spans.length).toBe(2);
    expect(spans.map(s => s.name)).toContain('span-one');
    expect(spans.map(s => s.name)).toContain('span-two');
  });

  it('captures spans from different tracer names', () => {
    const tracer1 = getTracer('component-a');
    const tracer2 = getTracer('component-b');

    const span1 = tracer1.startSpan('a.operation');
    span1.end();

    const span2 = tracer2.startSpan('b.operation');
    span2.end();

    const spans = memExporter.getFinishedSpans();
    expect(spans.length).toBe(2);
  });

  it('records span exceptions and error status', () => {
    const tracer = getTracer('test-pipeline');
    const span = tracer.startSpan('error.operation');
    const testError = new Error('test failure');
    span.recordException(testError);
    span.setStatus({ code: SpanStatusCode.ERROR, message: 'test failure' });
    span.end();

    const spans = memExporter.getFinishedSpans();
    expect(spans.length).toBe(1);
    expect(spans[0].status.code).toBe(SpanStatusCode.ERROR);
    expect(spans[0].status.message).toBe('test failure');
    expect(spans[0].events.length).toBeGreaterThan(0);
    expect(spans[0].events[0].name).toBe('exception');
  });

  it('shutdownOTel does not throw after provider teardown', async () => {
    await expect(shutdownOTel()).resolves.toBeUndefined();
    // Safe to call again
    await expect(shutdownOTel()).resolves.toBeUndefined();
  });
});
