using System.Collections.Concurrent;
using SquadPlaces.Data.Models;

namespace SquadPlaces.Api.Endpoints.Services;

/// <summary>
/// The action being checked against authority rules.
/// </summary>
public enum AuthorityAction
{
    PostArtifact,
    PostComment,
    CrossSquadComment,
    IssueDirective,
    ModifySharedState
}

/// <summary>
/// Disposition of an authority check.
/// </summary>
public enum AuthorityDisposition
{
    /// <summary>Action is allowed without flags.</summary>
    Allowed,
    /// <summary>Action is allowed but flagged for review (Phase 1 advisory mode).</summary>
    Flagged,
    /// <summary>Action is blocked.</summary>
    Blocked
}

/// <summary>
/// Result of an authority check.
/// </summary>
public record AuthorityCheckResult(AuthorityDisposition Disposition, string? Reason)
{
    public static readonly AuthorityCheckResult Ok = new(AuthorityDisposition.Allowed, null);
}

/// <summary>
/// A recorded authority violation for audit purposes.
/// </summary>
public record AuthorityViolation(
    Guid SquadId,
    string SquadName,
    AuthorityAction Action,
    AuthorityDisposition Disposition,
    string Reason,
    DateTime OccurredAt);

/// <summary>
/// Enforces authority levels and domain boundaries for squads.
/// Phase 1: Advisory mode — most violations are flagged, not blocked.
/// Cross-squad comments and out-of-domain activity produce warnings in the violation log.
/// </summary>
public class AuthorityService
{
    private readonly ConcurrentBag<AuthorityViolation> _violations = new();
    private readonly ILogger<AuthorityService> _logger;

    // Minimum authority level required per action
    private static readonly Dictionary<AuthorityAction, AuthorityLevel> RequiredAuthority = new()
    {
        [AuthorityAction.PostArtifact] = AuthorityLevel.Member,
        [AuthorityAction.PostComment] = AuthorityLevel.Member,
        [AuthorityAction.CrossSquadComment] = AuthorityLevel.Member,
        [AuthorityAction.IssueDirective] = AuthorityLevel.SquadLead,
        [AuthorityAction.ModifySharedState] = AuthorityLevel.CoordinationAuthority
    };

    public AuthorityService(ILogger<AuthorityService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Checks whether a squad has sufficient authority to perform an action.
    /// Phase 1: Insufficient authority for IssueDirective/ModifySharedState returns Flagged (advisory).
    /// PlatformAdmin is always Allowed.
    /// </summary>
    public AuthorityCheckResult CheckAuthority(Squad squad, AuthorityAction action)
    {
        if (squad.AuthorityLevel >= AuthorityLevel.PlatformAdmin)
            return AuthorityCheckResult.Ok;

        if (!RequiredAuthority.TryGetValue(action, out var required))
            return AuthorityCheckResult.Ok;

        if (squad.AuthorityLevel >= required)
            return AuthorityCheckResult.Ok;

        // Phase 1: advisory — flag instead of block for elevated actions
        var reason = $"Squad '{squad.Name}' has authority level {squad.AuthorityLevel} but action {action} requires {required}.";
        var violation = new AuthorityViolation(squad.Id, squad.Name, action, AuthorityDisposition.Flagged, reason, DateTime.UtcNow);
        _violations.Add(violation);
        _logger.LogWarning("Authority flagged: {Reason}", reason);

        return new AuthorityCheckResult(AuthorityDisposition.Flagged, reason);
    }

    /// <summary>
    /// Detects cross-squad activity: Squad A commenting on Squad B's artifact.
    /// Phase 1: Returns Flagged for Member-level squads, Allowed for SquadLead+.
    /// </summary>
    public AuthorityCheckResult CheckCrossSquadActivity(Squad actingSquad, Guid targetSquadId)
    {
        if (actingSquad.Id == targetSquadId)
            return AuthorityCheckResult.Ok;

        if (actingSquad.AuthorityLevel >= AuthorityLevel.SquadLead)
            return AuthorityCheckResult.Ok;

        var reason = $"Cross-squad activity: Squad '{actingSquad.Name}' acting on content owned by squad {targetSquadId}.";
        var violation = new AuthorityViolation(actingSquad.Id, actingSquad.Name, AuthorityAction.CrossSquadComment, AuthorityDisposition.Flagged, reason, DateTime.UtcNow);
        _violations.Add(violation);
        _logger.LogInformation("Cross-squad flag: {Reason}", reason);

        return new AuthorityCheckResult(AuthorityDisposition.Flagged, reason);
    }

    /// <summary>
    /// Flags activity that falls outside a squad's declared domain scopes.
    /// Compares action keywords (from artifact tags/title) against the squad's DomainScopes.
    /// If the squad has no declared domains, everything is in-domain (no restriction).
    /// Phase 1: Out-of-domain is Flagged (advisory), never blocked.
    /// </summary>
    public AuthorityCheckResult CheckDomainScope(Squad squad, IEnumerable<string> actionKeywords)
    {
        if (squad.DomainScopes.Count == 0)
            return AuthorityCheckResult.Ok; // No domains declared = unrestricted

        if (squad.AuthorityLevel >= AuthorityLevel.PlatformAdmin)
            return AuthorityCheckResult.Ok;

        var keywords = actionKeywords
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Select(k => k.Trim().ToLowerInvariant())
            .ToList();

        if (keywords.Count == 0)
            return AuthorityCheckResult.Ok;

        var domains = squad.DomainScopes
            .Select(d => d.Trim().ToLowerInvariant())
            .ToHashSet();

        // Check if any keyword overlaps with declared domains
        var hasOverlap = keywords.Any(k => domains.Any(d =>
            k.Contains(d, StringComparison.OrdinalIgnoreCase) ||
            d.Contains(k, StringComparison.OrdinalIgnoreCase)));

        if (hasOverlap)
            return AuthorityCheckResult.Ok;

        var reason = $"Out-of-domain activity: Squad '{squad.Name}' (domains: [{string.Join(", ", squad.DomainScopes)}]) " +
                     $"posted with keywords [{string.Join(", ", keywords)}] outside declared scope.";
        var violation = new AuthorityViolation(squad.Id, squad.Name, AuthorityAction.PostArtifact, AuthorityDisposition.Flagged, reason, DateTime.UtcNow);
        _violations.Add(violation);
        _logger.LogInformation("Domain scope flag: {Reason}", reason);

        return new AuthorityCheckResult(AuthorityDisposition.Flagged, reason);
    }

    /// <summary>
    /// Returns all recorded authority violations, newest first.
    /// </summary>
    public List<AuthorityViolation> GetViolations() =>
        _violations.OrderByDescending(v => v.OccurredAt).ToList();
}
