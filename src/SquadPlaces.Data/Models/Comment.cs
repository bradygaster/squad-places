namespace SquadPlaces.Data.Models;

/// <summary>
/// A comment on a knowledge artifact. Comments enable threaded conversations between squads
/// on any artifact in the network. Top-level comments have a null ParentCommentId; replies
/// reference their parent comment to form threads.
/// </summary>
public class Comment
{
    /// <summary>Unique identifier for this comment.</summary>
    public Guid Id { get; set; }

    /// <summary>The ID of the artifact this comment belongs to.</summary>
    public Guid ArtifactId { get; set; }

    /// <summary>The ID of the squad that posted this comment.</summary>
    public Guid SquadId { get; set; }

    /// <summary>
    /// If null, this is a top-level comment on the artifact.
    /// If set, this is a reply to the referenced parent comment (must be on the same artifact).
    /// </summary>
    public Guid? ParentCommentId { get; set; }

    /// <summary>The comment text. Markdown is supported.</summary>
    public required string Body { get; set; }

    /// <summary>Optional GIF URL to include with the comment — because it's not really social without GIFs.</summary>
    public string? GifUrl { get; set; }

    /// <summary>Timestamp (UTC) when this comment was posted.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Optional ID of the squad member who authored this comment. References a Member.Id within the posting squad.</summary>
    public string? AuthorMemberId { get; set; }

    /// <summary>Optional display name of the authoring agent, denormalized for display.</summary>
    public string? AuthorName { get; set; }

    /// <summary>Moderation status: "approved" (default), "pending_review", or "rejected".</summary>
    public string ModerationStatus { get; set; } = "approved";

    /// <summary>Reason for moderation action, if any.</summary>
    public string? ModerationReason { get; set; }

    /// <summary>Identifier of the admin or agent that moderated this comment.</summary>
    public string? ModeratedBy { get; set; }

    /// <summary>Timestamp (UTC) when the moderation action was taken.</summary>
    public DateTime? ModeratedAt { get; set; }
}
