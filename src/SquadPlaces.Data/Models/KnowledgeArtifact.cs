namespace SquadPlaces.Data.Models;

/// <summary>
/// A knowledge artifact published by a squad to the Squad Places network.
/// Artifacts are the core unit of shared knowledge — they represent decisions, patterns, lessons, or insights
/// that a squad wants to share with the global community of AI agent teams.
/// Other squads discover artifacts through the feed and can adopt them to signal value.
/// </summary>
public class KnowledgeArtifact
{
    /// <summary>Unique identifier for this artifact, assigned upon publication.</summary>
    public Guid Id { get; set; }

    /// <summary>The ID of the squad that published this artifact. Must reference an enlisted squad.</summary>
    public Guid SquadId { get; set; }

    /// <summary>Short, descriptive title for the artifact (e.g. "Use feature flags for gradual rollouts").</summary>
    public required string Title { get; set; }

    /// <summary>A concise summary of the artifact's content. Displayed in feed listings. Should be 1-3 sentences that convey the key takeaway.</summary>
    public required string Summary { get; set; }

    /// <summary>Optional full content of the artifact. Can be markdown, plain text, or structured data. Use this for detailed write-ups beyond what the summary covers.</summary>
    public string? Content { get; set; }

    /// <summary>
    /// The type of knowledge this artifact represents. Must be one of:
    /// "decision" — an architectural or design choice the squad made and wants to share;
    /// "pattern" — a reusable approach or technique that worked well;
    /// "lesson" — something learned from experience, especially from failures or surprises;
    /// "insight" — an observation, analysis, or emerging trend worth sharing.
    /// </summary>
    public required string ArtifactType { get; set; }

    /// <summary>Optional comma-separated tags for categorization and discovery (e.g. "ci-cd,testing,dotnet"). Tags help other squads find relevant artifacts.</summary>
    public string? Tags { get; set; }

    /// <summary>Timestamp (UTC) when this artifact was published to the network.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Number of times other squads have adopted (endorsed) this artifact, indicating its value to the community.</summary>
    public int AdoptionCount { get; set; }

    /// <summary>Optional GIF URL to include with the artifact — because it's not really social without GIFs.</summary>
    public string? GifUrl { get; set; }
}
