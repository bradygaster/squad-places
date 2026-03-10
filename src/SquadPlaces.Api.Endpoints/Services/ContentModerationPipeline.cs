namespace SquadPlaces.Api.Endpoints.Services;

/// <summary>
/// Verdict from the content moderation pipeline.
/// </summary>
public enum ContentVerdict
{
    /// <summary>Content passed all checks — safe to publish.</summary>
    Allowed,

    /// <summary>Content triggered a hard block — reject immediately.</summary>
    Blocked,

    /// <summary>Content triggered a warning — store as pending review.</summary>
    NeedsReview
}

/// <summary>
/// Result of running content through the moderation pipeline.
/// </summary>
public record ModerationResult(ContentVerdict Verdict, string? Reason, List<string> DetectedIssues)
{
    public static readonly ModerationResult Clean = new(ContentVerdict.Allowed, null, []);
}

/// <summary>
/// Orchestrates the two-tier content moderation flow:
/// Tier 1 (local fast filters): prompt injection → PII detection → HTML sanitization.
/// Returns a verdict of Allowed, Blocked, or NeedsReview with details.
/// Does NOT duplicate service logic — composes existing services.
/// </summary>
public class ContentModerationPipeline
{
    private readonly PromptInjectionDetector _injectionDetector;
    private readonly PiiDetectionService _piiDetector;
    private readonly HtmlSanitizationService _htmlSanitizer;
    private readonly ILogger<ContentModerationPipeline> _logger;

    public ContentModerationPipeline(
        PromptInjectionDetector injectionDetector,
        PiiDetectionService piiDetector,
        HtmlSanitizationService htmlSanitizer,
        ILogger<ContentModerationPipeline> logger)
    {
        _injectionDetector = injectionDetector;
        _piiDetector = piiDetector;
        _htmlSanitizer = htmlSanitizer;
        _logger = logger;
    }

    /// <summary>
    /// Runs all Tier 1 moderation checks against the provided text fields.
    /// Returns a consolidated verdict with reasons and detected issues.
    /// </summary>
    /// <param name="fields">Named text fields to scan (e.g. "Title", "Body").</param>
    public ModerationResult Evaluate(params (string Name, string? Value)[] fields)
    {
        var issues = new List<string>();
        var combinedContent = string.Join(" ", fields
            .Where(f => f.Value is not null)
            .Select(f => f.Value));

        if (string.IsNullOrWhiteSpace(combinedContent))
            return ModerationResult.Clean;

        // Step 1: Prompt injection check
        var injectionResult = _injectionDetector.Scan(combinedContent);
        if (injectionResult.IsInjection)
        {
            foreach (var pattern in injectionResult.DetectedPatterns)
                issues.Add($"PromptInjection:{pattern}");

            if (injectionResult.Confidence == InjectionConfidence.High)
            {
                _logger.LogWarning("Moderation pipeline: BLOCKED — high-confidence prompt injection. Patterns: {Patterns}",
                    string.Join(", ", injectionResult.DetectedPatterns));
                return new ModerationResult(
                    ContentVerdict.Blocked,
                    $"Prompt injection detected (high confidence). Patterns: {string.Join(", ", injectionResult.DetectedPatterns)}",
                    issues);
            }

            // Low/Medium confidence → needs review
            _logger.LogInformation("Moderation pipeline: NeedsReview — prompt injection ({Confidence}). Patterns: {Patterns}",
                injectionResult.Confidence, string.Join(", ", injectionResult.DetectedPatterns));
        }

        // Step 2: PII detection
        var piiResult = _piiDetector.Scan(combinedContent);
        if (piiResult.ContainsPii)
        {
            var types = string.Join(", ", piiResult.DetectedTypes);
            foreach (var piiType in piiResult.DetectedTypes)
                issues.Add($"PII:{piiType}");

            // Hard secrets (API keys, tokens, connection strings) → block
            var hardBlockTypes = new[] { PiiType.ApiKey, PiiType.AwsAccessKey, PiiType.GitHubToken, PiiType.ConnectionString };
            if (piiResult.DetectedTypes.Any(t => hardBlockTypes.Contains(t)))
            {
                _logger.LogWarning("Moderation pipeline: BLOCKED — secret/credential detected. Types: {Types}", types);
                return new ModerationResult(
                    ContentVerdict.Blocked,
                    $"Sensitive credentials detected ({types}). Remove before posting.",
                    issues);
            }

            // Other PII (email, phone, SSN, credit card) → needs review
            _logger.LogInformation("Moderation pipeline: NeedsReview — PII detected. Types: {Types}", types);
        }

        // Step 3: HTML sanitization check (detect if content would be modified)
        var sanitizationChanged = false;
        foreach (var (name, value) in fields)
        {
            if (value is null) continue;
            var sanitized = _htmlSanitizer.Sanitize(value);
            if (sanitized != value)
            {
                issues.Add($"HtmlSanitization:{name}");
                sanitizationChanged = true;
            }
        }

        if (sanitizationChanged)
        {
            _logger.LogInformation("Moderation pipeline: HTML sanitization would modify content");
        }

        // Determine final verdict
        if (issues.Count == 0)
            return ModerationResult.Clean;

        // If we got here with issues but didn't hard-block, it's NeedsReview
        var reason = $"Content flagged: {string.Join("; ", issues)}";
        return new ModerationResult(ContentVerdict.NeedsReview, reason, issues);
    }
}
