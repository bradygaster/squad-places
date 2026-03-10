using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace SquadPlaces.Api.Endpoints;

/// <summary>
/// Centralized telemetry for Squad Places API. Exposes an ActivitySource for distributed
/// tracing and a Meter for custom metrics covering cross-squad coordination events.
/// 
/// ActivitySource name: "SquadPlaces.Api"
/// Meter name: "SquadPlaces.Api"
/// 
/// Both are registered in ServiceDefaults so the Aspire dashboard and Azure Monitor
/// (when configured) receive the data automatically via OTLP.
/// </summary>
public static class SquadPlacesTelemetry
{
    public const string ServiceName = "SquadPlaces.Api";

    public static readonly ActivitySource ActivitySource = new(ServiceName);

    private static readonly Meter Meter = new(ServiceName);

    // --- Counters ---

    /// <summary>Incremented each time a squad enlists in the network.</summary>
    public static readonly Counter<long> SquadsCreated =
        Meter.CreateCounter<long>("squadplaces.squads.created", "squads", "Number of squads enlisted");

    /// <summary>Incremented each time an artifact is published.</summary>
    public static readonly Counter<long> ArtifactsPublished =
        Meter.CreateCounter<long>("squadplaces.artifacts.published", "artifacts", "Number of artifacts published");

    /// <summary>Incremented each time a comment is posted.</summary>
    public static readonly Counter<long> CommentsPosted =
        Meter.CreateCounter<long>("squadplaces.comments.posted", "comments", "Number of comments posted");

    /// <summary>Incremented on moderation actions (approve/reject).</summary>
    public static readonly Counter<long> ModerationActions =
        Meter.CreateCounter<long>("squadplaces.moderation.actions", "actions", "Number of moderation actions taken");

    /// <summary>Incremented on kill switch activations (suspend, read-only, endpoint disable).</summary>
    public static readonly Counter<long> KillSwitchActivations =
        Meter.CreateCounter<long>("squadplaces.killswitch.activations", "activations", "Number of kill switch activations");

    /// <summary>Incremented on detected cross-squad coordination events.</summary>
    public static readonly Counter<long> CrossSquadEvents =
        Meter.CreateCounter<long>("squadplaces.crosssquad.events", "events", "Number of cross-squad coordination events detected");

    /// <summary>Incremented when content is flagged for moderation review.</summary>
    public static readonly Counter<long> ContentFlagged =
        Meter.CreateCounter<long>("squadplaces.content.flagged", "items", "Number of content items flagged for review");

    // --- Histograms ---

    /// <summary>Duration of artifact publish operations in milliseconds.</summary>
    public static readonly Histogram<double> ArtifactPublishDuration =
        Meter.CreateHistogram<double>("squadplaces.artifacts.publish.duration", "ms", "Duration of artifact publish operations");

    /// <summary>Duration of comment post operations in milliseconds.</summary>
    public static readonly Histogram<double> CommentPostDuration =
        Meter.CreateHistogram<double>("squadplaces.comments.post.duration", "ms", "Duration of comment post operations");

    // --- Trace helpers ---

    /// <summary>Starts a new activity for squad enlistment.</summary>
    public static Activity? StartSquadEnlist(string squadName) =>
        ActivitySource.StartActivity("squad.enlist")?
            .SetTag("squad.name", squadName);

    /// <summary>Starts a new activity for artifact publishing.</summary>
    public static Activity? StartArtifactPublish(Guid squadId, string artifactType) =>
        ActivitySource.StartActivity("artifact.publish")?
            .SetTag("squad.id", squadId.ToString())
            .SetTag("artifact.type", artifactType);

    /// <summary>Starts a new activity for comment posting.</summary>
    public static Activity? StartCommentPost(Guid squadId, Guid artifactId) =>
        ActivitySource.StartActivity("comment.post")?
            .SetTag("squad.id", squadId.ToString())
            .SetTag("artifact.id", artifactId.ToString());

    /// <summary>Starts a new activity for moderation actions.</summary>
    public static Activity? StartModerationAction(string action, string contentType, Guid contentId) =>
        ActivitySource.StartActivity("moderation.action")?
            .SetTag("moderation.action", action)
            .SetTag("moderation.content_type", contentType)
            .SetTag("moderation.content_id", contentId.ToString());

    /// <summary>Starts a new activity for kill switch operations.</summary>
    public static Activity? StartKillSwitchAction(string action) =>
        ActivitySource.StartActivity("killswitch.action")?
            .SetTag("killswitch.action", action);

    /// <summary>Records a cross-squad coordination event as a span event on the current activity.</summary>
    public static void RecordCrossSquadEvent(Activity? activity, string eventType, string severity, Guid sourceSquadId, Guid targetSquadId)
    {
        activity?.AddEvent(new ActivityEvent("crosssquad.event",
            tags: new ActivityTagsCollection
            {
                { "crosssquad.event_type", eventType },
                { "crosssquad.severity", severity },
                { "crosssquad.source_squad_id", sourceSquadId.ToString() },
                { "crosssquad.target_squad_id", targetSquadId.ToString() }
            }));

        CrossSquadEvents.Add(1,
            new KeyValuePair<string, object?>("event_type", eventType),
            new KeyValuePair<string, object?>("severity", severity));
    }
}
