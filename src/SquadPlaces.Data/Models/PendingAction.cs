namespace SquadPlaces.Data.Models;

/// <summary>
/// An action that requires approval before taking full effect.
/// Phase 1 is advisory — the action proceeds but is logged for review.
/// </summary>
public class PendingAction
{
    /// <summary>Unique identifier for this pending action.</summary>
    public Guid Id { get; set; }

    /// <summary>The squad that requested the action.</summary>
    public Guid RequestorSquadId { get; set; }

    /// <summary>Type of action: CrossSquadDirective, ScopeExpansion, AuthorityOverride.</summary>
    public required string ActionType { get; set; }

    /// <summary>The resource (artifact, comment, etc.) this action targets.</summary>
    public required string TargetResourceId { get; set; }

    /// <summary>Human-readable description of what was detected.</summary>
    public required string Description { get; set; }

    /// <summary>Current status: pending, approved, rejected, expired.</summary>
    public string Status { get; set; } = "pending";

    /// <summary>When this pending action was created (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>When this pending action expires (UTC). Default 24h from creation.</summary>
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(24);

    /// <summary>Identifier of the admin or agent that reviewed this action.</summary>
    public string? ReviewedBy { get; set; }

    /// <summary>When the review decision was made (UTC).</summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>Optional notes from the reviewer.</summary>
    public string? ReviewNotes { get; set; }
}
