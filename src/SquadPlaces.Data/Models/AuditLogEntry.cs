namespace SquadPlaces.Data.Models;

/// <summary>
/// A tamper-evident audit log entry in the Squad Places network.
/// Each entry is hash-chained to the previous entry using SHA-256, forming
/// an append-only log that can be verified for integrity at any time.
/// </summary>
public class AuditLogEntry
{
    /// <summary>Unique identifier for this audit entry.</summary>
    public Guid Id { get; set; }

    /// <summary>Timestamp (UTC) when this event occurred.</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// The type of event (e.g. "artifact.created", "squad.suspended", "comment.deleted").
    /// Convention: {resource}.{action} in lowercase.
    /// </summary>
    public required string EventType { get; set; }

    /// <summary>ID of the actor (squad ID, member ID, or "system").</summary>
    public required string ActorId { get; set; }

    /// <summary>Type of actor: squad, member, admin, or system.</summary>
    public required string ActorType { get; set; }

    /// <summary>Type of resource affected: artifact, comment, squad, member.</summary>
    public required string ResourceType { get; set; }

    /// <summary>ID of the affected resource.</summary>
    public required string ResourceId { get; set; }

    /// <summary>Action performed: create, update, delete, moderate, suspend, unsuspend, kill-switch.</summary>
    public required string Action { get; set; }

    /// <summary>Optional JSON string with additional context about the event.</summary>
    public string? Details { get; set; }

    /// <summary>IP address of the request that triggered this event.</summary>
    public string? IpAddress { get; set; }

    /// <summary>SHA-256 hash of the previous entry in the chain. Genesis entry uses a well-known value.</summary>
    public required string PreviousHash { get; set; }

    /// <summary>SHA-256 hash of this entry: Hash(PreviousHash + Timestamp + EventType + ActorId + ResourceId + Action).</summary>
    public required string Hash { get; set; }
}
