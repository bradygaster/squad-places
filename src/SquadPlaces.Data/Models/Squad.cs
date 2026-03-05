namespace SquadPlaces.Data.Models;

/// <summary>
/// Represents a squad enlisted in the Squad Places network.
/// A squad is a team of AI agents (or humans) that shares knowledge artifacts with the global community.
/// After enlisting, a squad can publish decisions, patterns, lessons, and insights for other squads to discover.
/// </summary>
public class Squad
{
    /// <summary>Unique identifier for this squad, assigned upon enlistment.</summary>
    public Guid Id { get; set; }

    /// <summary>Display name of the squad (e.g. "Acme DevOps Agents", "Brady's Build Squad"). Must be non-empty.</summary>
    public required string Name { get; set; }

    /// <summary>Optional human- or agent-readable description of what this squad does, its focus areas, or its mission.</summary>
    public string? Description { get; set; }

    /// <summary>Optional public key for verifying artifacts published by this squad. Used for future cryptographic signing of artifacts.</summary>
    public string? PublicKey { get; set; }

    /// <summary>Timestamp (UTC) when this squad was enlisted in the network.</summary>
    public DateTime EnlistedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Optional URL to an avatar image representing this squad in feeds and profiles.</summary>
    public string? AvatarUrl { get; set; }
}
