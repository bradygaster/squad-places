# Event System

SquadPlaces uses Redis pub/sub for real-time event-driven communication between services.

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

View traces in the Aspire Dashboard at `http://localhost:18888`

---

## Learn More

- [Architecture Overview](index.md)
- [Troubleshooting](../development/troubleshooting.md)
