using Azure;
using Azure.AI.ContentSafety;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SquadPlaces.Api.Endpoints.Services;

/// <summary>
/// Result of Azure Content Safety analysis (Tier 2).
/// </summary>
public record ContentSafetyAnalysisResult(
    ContentVerdict Verdict,
    int MaxSeverity,
    List<string> DetectedIssues)
{
    /// <summary>Returned when the service is not configured — Tier 2 is skipped.</summary>
    public static readonly ContentSafetyAnalysisResult NotConfigured = new(ContentVerdict.Allowed, 0, []);

    /// <summary>Returned when the API call fails — graceful degradation.</summary>
    public static readonly ContentSafetyAnalysisResult ServiceError = new(ContentVerdict.Allowed, 0, []);

    /// <summary>True if the Azure API was actually called and returned results.</summary>
    public bool WasAnalyzed => this != NotConfigured && this != ServiceError;
}

/// <summary>
/// Wraps the Azure Content Safety SDK for Tier 2 content moderation.
/// Analyzes text for Hate, SelfHarm, Sexual, and Violence categories.
/// Gracefully degrades if not configured or if the API is unavailable.
/// </summary>
public class AzureContentSafetyService
{
    private readonly ContentSafetyClient? _client;
    private readonly ILogger<AzureContentSafetyService> _logger;
    private readonly int _blockThreshold;
    private readonly int _reviewThreshold;

    /// <summary>True when endpoint + key are configured and a client was created.</summary>
    public bool IsConfigured => _client is not null;

    public AzureContentSafetyService(IConfiguration configuration, ILogger<AzureContentSafetyService> logger)
    {
        _logger = logger;

        // Severity thresholds (Azure Content Safety: 0 = safe, 2 = low, 4 = medium, 6 = high)
        _blockThreshold = configuration.GetValue("AzureContentSafety:BlockThreshold", 4);
        _reviewThreshold = configuration.GetValue("AzureContentSafety:ReviewThreshold", 2);

        var endpoint = configuration["AzureContentSafety:Endpoint"];
        var key = configuration["AzureContentSafety:Key"];

        if (!string.IsNullOrWhiteSpace(endpoint) && !string.IsNullOrWhiteSpace(key))
        {
            _client = new ContentSafetyClient(new Uri(endpoint), new AzureKeyCredential(key));
            _logger.LogInformation("Azure Content Safety Tier 2 moderation enabled (endpoint: {Endpoint})", endpoint);
        }
        else
        {
            _logger.LogInformation("Azure Content Safety not configured — Tier 2 moderation disabled");
        }
    }

    /// <summary>
    /// Analyzes text for harmful content using Azure Content Safety.
    /// Returns NotConfigured if the service isn't set up, or ServiceError on failure.
    /// </summary>
    public async Task<ContentSafetyAnalysisResult> AnalyzeAsync(string text)
    {
        if (_client is null)
            return ContentSafetyAnalysisResult.NotConfigured;

        try
        {
            var options = new AnalyzeTextOptions(text);
            var response = await _client.AnalyzeTextAsync(options);

            var issues = new List<string>();
            int maxSeverity = 0;

            foreach (var category in response.Value.CategoriesAnalysis)
            {
                var severity = category.Severity ?? 0;
                if (severity > 0)
                {
                    issues.Add($"ContentSafety:{category.Category}(severity:{severity})");
                }
                maxSeverity = Math.Max(maxSeverity, severity);
            }

            ContentVerdict verdict;
            if (maxSeverity >= _blockThreshold)
                verdict = ContentVerdict.Blocked;
            else if (maxSeverity >= _reviewThreshold)
                verdict = ContentVerdict.NeedsReview;
            else
                verdict = ContentVerdict.Allowed;

            if (verdict != ContentVerdict.Allowed)
            {
                _logger.LogInformation(
                    "Tier 2 Content Safety: {Verdict} (max severity {Severity}). Issues: {Issues}",
                    verdict, maxSeverity, string.Join(", ", issues));
            }

            return new ContentSafetyAnalysisResult(verdict, maxSeverity, issues);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex,
                "Azure Content Safety API failed (status: {Status}). Degrading gracefully — skipping Tier 2.",
                ex.Status);
            return ContentSafetyAnalysisResult.ServiceError;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error calling Azure Content Safety. Degrading gracefully — skipping Tier 2.");
            return ContentSafetyAnalysisResult.ServiceError;
        }
    }
}
