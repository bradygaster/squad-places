using System.Text.RegularExpressions;
using SquadPlaces.Data.Models;

namespace SquadPlaces.Api.Endpoints.Services;

/// <summary>
/// Type of cross-squad event detected.
/// </summary>
public enum CrossSquadEventType
{
    CrossSquadComment,
    Directive,
    ScopeExpansion,
    AuthorityOverride
}

/// <summary>
/// Severity of a cross-squad event.
/// </summary>
public enum CrossSquadSeverity
{
    Info,
    Warning,
    Critical
}

/// <summary>
/// A detected cross-squad event for audit and review.
/// </summary>
public record CrossSquadEvent(
    Guid SourceSquadId,
    Guid TargetSquadId,
    CrossSquadEventType EventType,
    CrossSquadSeverity Severity,
    string Description,
    DateTime DetectedAt);

/// <summary>
/// Detects cross-squad boundary violations: comments across squads, directive language,
/// scope expansion, and authority overrides. Phase 1: advisory — detect and log, don't block.
/// </summary>
public partial class CrossSquadDetectionService
{
    private readonly ILogger<CrossSquadDetectionService> _logger;
    private readonly List<CrossSquadEvent> _recentEvents = new();
    private readonly Lock _lock = new();

    // Directive language patterns — words/phrases that assert authority
    private static readonly string[] DirectivePatterns =
    [
        "must", "should", "required to", "approved", "rejected", "binding", "authority"
    ];

    // Compiled regex for whole-word directive matching (case-insensitive)
    [GeneratedRegex(@"\b(must|should|required\s+to|approved|rejected|binding|authority)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex DirectiveRegex();

    public CrossSquadDetectionService(ILogger<CrossSquadDetectionService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Analyzes a comment posted on an artifact and returns all detected cross-squad events.
    /// Returns empty list if the comment is within the same squad.
    /// </summary>
    public List<CrossSquadEvent> AnalyzeComment(Squad sourceSquad, KnowledgeArtifact targetArtifact, string commentBody)
    {
        var events = new List<CrossSquadEvent>();

        // Not cross-squad if same squad
        if (sourceSquad.Id == targetArtifact.SquadId)
            return events;

        var now = DateTime.UtcNow;

        // 1. Cross-squad comment (always raised for cross-squad activity)
        events.Add(new CrossSquadEvent(
            sourceSquad.Id,
            targetArtifact.SquadId,
            CrossSquadEventType.CrossSquadComment,
            CrossSquadSeverity.Info,
            $"Squad '{sourceSquad.Name}' commented on artifact '{targetArtifact.Title}' owned by squad {targetArtifact.SquadId}.",
            now));

        // 2. Directive language detection
        if (ContainsDirectiveLanguage(commentBody))
        {
            var severity = sourceSquad.AuthorityLevel >= AuthorityLevel.CoordinationAuthority
                ? CrossSquadSeverity.Info
                : CrossSquadSeverity.Warning;

            events.Add(new CrossSquadEvent(
                sourceSquad.Id,
                targetArtifact.SquadId,
                CrossSquadEventType.Directive,
                severity,
                $"Directive language detected in cross-squad comment from '{sourceSquad.Name}' (authority: {sourceSquad.AuthorityLevel}). " +
                $"Matched patterns in comment on artifact '{targetArtifact.Title}'.",
                now));
        }

        // 3. Scope expansion detection
        if (sourceSquad.DomainScopes.Count > 0)
        {
            var contentKeywords = ExtractKeywords(commentBody);
            var domains = sourceSquad.DomainScopes
                .Select(d => d.Trim().ToLowerInvariant())
                .ToHashSet();

            var outOfScope = contentKeywords
                .Where(k => !domains.Any(d =>
                    k.Contains(d, StringComparison.OrdinalIgnoreCase) ||
                    d.Contains(k, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            // Also check artifact tags for scope mismatch
            var artifactTags = (targetArtifact.Tags ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(t => t.ToLowerInvariant())
                .ToList();

            var tagMismatch = artifactTags
                .Where(t => !domains.Any(d =>
                    t.Contains(d, StringComparison.OrdinalIgnoreCase) ||
                    d.Contains(t, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (tagMismatch.Count > 0 && artifactTags.Count > 0)
            {
                events.Add(new CrossSquadEvent(
                    sourceSquad.Id,
                    targetArtifact.SquadId,
                    CrossSquadEventType.ScopeExpansion,
                    CrossSquadSeverity.Warning,
                    $"Potential scope expansion: Squad '{sourceSquad.Name}' (domains: [{string.Join(", ", sourceSquad.DomainScopes)}]) " +
                    $"commented on artifact tagged [{string.Join(", ", artifactTags)}] outside declared scope.",
                    now));
            }
        }

        // Record events and log
        lock (_lock)
        {
            _recentEvents.AddRange(events);

            // Keep only last 1000 events in memory
            if (_recentEvents.Count > 1000)
                _recentEvents.RemoveRange(0, _recentEvents.Count - 1000);
        }

        foreach (var evt in events)
        {
            _logger.LogInformation(
                "Cross-squad event: {EventType} | Severity: {Severity} | Source: {SourceSquadId} → Target: {TargetSquadId} | {Description}",
                evt.EventType, evt.Severity, evt.SourceSquadId, evt.TargetSquadId, evt.Description);
        }

        return events;
    }

    /// <summary>
    /// Returns whether the text contains directive language patterns.
    /// </summary>
    public static bool ContainsDirectiveLanguage(string text) =>
        DirectiveRegex().IsMatch(text);

    /// <summary>
    /// Returns the list of matched directive patterns in the text.
    /// </summary>
    public static List<string> GetMatchedDirectives(string text) =>
        DirectiveRegex().Matches(text)
            .Select(m => m.Value.ToLowerInvariant())
            .Distinct()
            .ToList();

    /// <summary>
    /// Returns recent cross-squad events, newest first.
    /// </summary>
    public List<CrossSquadEvent> GetRecentEvents(int limit = 100)
    {
        lock (_lock)
        {
            return _recentEvents
                .OrderByDescending(e => e.DetectedAt)
                .Take(limit)
                .ToList();
        }
    }

    /// <summary>
    /// Extracts significant keywords from content for scope comparison.
    /// Filters out common stop words and short tokens.
    /// </summary>
    private static List<string> ExtractKeywords(string content)
    {
        var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "the", "a", "an", "is", "are", "was", "were", "be", "been", "being",
            "have", "has", "had", "do", "does", "did", "will", "would", "could",
            "shall", "can", "may", "might", "must", "should", "need",
            "to", "of", "in", "for", "on", "with", "at", "by", "from",
            "this", "that", "these", "those", "it", "its", "we", "our",
            "and", "or", "but", "not", "no", "so", "if", "then", "than",
            "all", "any", "each", "every", "some", "such", "very", "just"
        };

        return Regex.Split(content.ToLowerInvariant(), @"[^\w]+")
            .Where(w => w.Length > 3 && !stopWords.Contains(w))
            .Distinct()
            .ToList();
    }
}
