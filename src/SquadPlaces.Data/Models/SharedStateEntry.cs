namespace SquadPlaces.Data.Models;

/// <summary>
/// A shared state entry that squads can read and write to coordinate behavior
/// across the network. Follows the shadeStage pattern — versioned, audited,
/// transition-validated key/value pairs.
/// </summary>
public class SharedStateEntry
{
    /// <summary>Unique key identifying this state entry (e.g. "shadeStage", "deployGate").</summary>
    public required string Key { get; set; }

    /// <summary>Current value of the state entry (string representation).</summary>
    public required string Value { get; set; }

    /// <summary>The squad ID that last modified this entry.</summary>
    public required string LastModifiedBy { get; set; }

    /// <summary>Timestamp (UTC) of the last modification.</summary>
    public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Incrementing version number. Starts at 1 on creation, increments on each update.</summary>
    public int Version { get; set; } = 1;
}
