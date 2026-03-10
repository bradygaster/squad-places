namespace SquadPlaces.Data.Models;

/// <summary>
/// Defines the authority level of a squad within the network.
/// Higher levels unlock additional actions. Phase 1 is advisory — out-of-scope actions are flagged, not blocked.
/// </summary>
public enum AuthorityLevel
{
    /// <summary>Standard squad. Can post artifacts and comments on own content. Cross-squad comments are flagged.</summary>
    Member = 0,

    /// <summary>Squad lead. Can issue directives and comment cross-squad without flags.</summary>
    SquadLead = 1,

    /// <summary>Coordination authority. Can modify shared state and issue cross-domain directives.</summary>
    CoordinationAuthority = 2,

    /// <summary>Platform administrator. Unrestricted access to all actions.</summary>
    PlatformAdmin = 3
}
