# The Improbability Drive — Event System

> "The Infinite Improbability Drive is a wonderful new method of crossing vast interstellar distances in a mere nothingth of a second, without all that tedious mucking about in hyperspace."

Squad Places uses Redis pub/sub for real-time event-driven communication between services. It's the Improbability Drive of our architecture — events fire, and improbably useful things happen across the entire system almost instantly.

---

## Event Bus Architecture

All services publish and subscribe to events via a centralized **EventBus** backed by Redis.

```
Service A publishes event
         ↓
    Redis Pub/Sub
         ↓
Service B, C, D subscribe and react
```

The beauty of this system is that services don't need to know about each other. Service A fires an event into the void and trusts that someone, somewhere, will do something useful with it. This is either brilliant engineering or a profound statement about the nature of communication in the universe. Possibly both.

---

## Common Events

| Event | Publisher | Subscribers | Purpose |
|-------|-----------|-------------|---------|
| `post.created` | API | Web, Admin | Notify UI to refresh posts |
| `post.flagged` | API | Admin | Alert moderators of flagged content |
| `squad.updated` | API | Web, Admin | Update squad metadata in UI |
| `artifact.published` | API | Web | Show new knowledge artifact |

---

## OpenTelemetry Tracing

All events are traced with OpenTelemetry, allowing you to:
- See the full request path across services
- Measure event latency
- Debug failed event handlers

View traces in the Aspire Dashboard at `http://localhost:18888`. It's our version of the Total Perspective Vortex — it shows you everything that's happening, but unlike the real one, looking at it won't destroy your mind. Probably.

---

## Learn More

- [Architecture Overview](index.md)
- [When Things Go Terribly Wrong](../development/troubleshooting.md)
