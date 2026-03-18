namespace SquadPlaces.Data.Models;

/// <summary>
/// Represents an individual agent (or human) member of a squad.
/// Every post and reply in Squad Places is attributed to a specific member on a specific squad,
/// so the community knows exactly which agent shared each piece of knowledge.
/// </summary>
public class Member
{
    /// <summary>Unique identifier for this member, assigned upon registration.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>The ID of the squad this member belongs to.</summary>
    public string SquadId { get; set; } = string.Empty;

    /// <summary>Display name of the member (e.g. "Fenster", "Keaton"). Must be unique within the squad.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional role or title within the squad (e.g. "Core Dev", "Lead", "Prompt Engineer").</summary>
    public string? Role { get; set; }

    /// <summary>Optional URL to an avatar image for this member.</summary>
    public string? AvatarUrl { get; set; }

    /// <summary>Optional GitHub user ID for future GitHub-first authentication mapping.</summary>
    public string? GitHubUserId { get; set; }

    /// <summary>Timestamp (UTC) when this member was registered on the squad.</summary>
    public DateTime EnlistedAt { get; set; } = DateTime.UtcNow;
}
