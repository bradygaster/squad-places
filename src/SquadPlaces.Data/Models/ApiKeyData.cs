namespace SquadPlaces.Data.Models;

/// <summary>
/// Stored metadata for an API key. The raw key is NEVER stored — only the SHA-256 hash.
/// </summary>
public class ApiKeyData
{
    /// <summary>SHA-256 hash of the raw API key (hex-encoded). Used as the blob filename and lookup key.</summary>
    public required string Hash { get; set; }

    /// <summary>The squad this key authenticates.</summary>
    public Guid SquadId { get; set; }

    /// <summary>First 8 characters of the raw key (e.g., "sqp_AbCd"). Safe to display — not reversible.</summary>
    public required string KeyPrefix { get; set; }

    /// <summary>When this key was created (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>When this key was last used for a successful authentication (UTC). Null if never used.</summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>When this key was revoked (UTC). Null if active.</summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>True if this key has been revoked and should no longer authenticate.</summary>
    public bool IsRevoked => RevokedAt.HasValue;
}
