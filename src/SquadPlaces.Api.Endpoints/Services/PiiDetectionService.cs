using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SquadPlaces.Api.Endpoints.Services;

/// <summary>
/// Types of PII that can be detected.
/// </summary>
public enum PiiType
{
    Email,
    PhoneNumber,
    SSN,
    CreditCard,
    ApiKey,
    AwsAccessKey,
    GitHubToken,
    ConnectionString
}

/// <summary>
/// A character range indicating where PII was found (without revealing the actual content).
/// </summary>
public record PiiPosition(PiiType Type, int StartIndex, int Length);

/// <summary>
/// Result of a PII detection scan.
/// </summary>
public record PiiDetectionResult(bool ContainsPii, List<PiiType> DetectedTypes, List<PiiPosition> Positions)
{
    public static readonly PiiDetectionResult Clean = new(false, [], []);
}

/// <summary>
/// Detects PII (emails, phone numbers, SSNs, credit cards, API keys, tokens)
/// in user-generated content using high-confidence regex patterns.
/// No external API calls — fast and deterministic.
/// Configurable via IConfiguration ("PiiDetection" section).
/// </summary>
public class PiiDetectionService
{
    private readonly bool _enabled;
    private readonly HashSet<PiiType> _blockedTypes;
    private readonly List<(PiiType Type, Regex Pattern)> _detectors;
    private readonly ILogger<PiiDetectionService> _logger;

    // Patterns designed for high confidence — false positives are worse than false negatives
    private static readonly (PiiType Type, string Pattern)[] BuiltInDetectors =
    [
        // Email — standard RFC-5322 simplified (high confidence)
        (PiiType.Email, @"[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}"),

        // US phone numbers: (xxx) xxx-xxxx, xxx-xxx-xxxx, xxx.xxx.xxxx, +1xxxxxxxxxx
        (PiiType.PhoneNumber, @"(?<!\d)(?:\+?1[\s.\-]?)?(?:\(\d{3}\)|\d{3})[\s.\-]?\d{3}[\s.\-]?\d{4}(?!\d)"),

        // SSN: XXX-XX-XXXX (require dashes for high confidence — bare 9-digit numbers have too many false positives)
        (PiiType.SSN, @"(?<!\d)\d{3}-\d{2}-\d{4}(?!\d)"),

        // Credit cards: 13-19 digit sequences with optional separators (Visa, MC, Amex, Discover patterns)
        (PiiType.CreditCard, @"(?<!\d)(?:4\d{3}|5[1-5]\d{2}|3[47]\d{2}|6(?:011|5\d{2}))[\s\-]?\d{4}[\s\-]?\d{4}[\s\-]?\d{1,7}(?!\d)"),

        // API keys / secrets — key=value, secret=value, password=value patterns
        (PiiType.ApiKey, @"(?i)(?:api[_\-]?key|secret|password|token|access[_\-]?key|private[_\-]?key)\s*[=:]\s*[""']?\S{8,}"),

        // Azure / generic connection strings
        (PiiType.ConnectionString, @"(?i)(?:AccountKey|SharedAccessKey|DefaultEndpointsProtocol)\s*=\s*\S+"),

        // AWS access keys: always start with AKIA (20 chars)
        (PiiType.AwsAccessKey, @"(?<![A-Z0-9])AKIA[0-9A-Z]{16}(?![A-Z0-9])"),

        // GitHub tokens: ghp_, gho_, ghs_, github_pat_ prefixes
        (PiiType.GitHubToken, @"(?:ghp_[A-Za-z0-9]{36,}|gho_[A-Za-z0-9]{36,}|ghs_[A-Za-z0-9]{36,}|github_pat_[A-Za-z0-9_]{22,})")
    ];

    public PiiDetectionService(IConfiguration configuration, ILogger<PiiDetectionService> logger)
    {
        _logger = logger;

        var section = configuration.GetSection("PiiDetection");
        _enabled = section.GetValue("Enabled", true);

        // Parse blocked types from config, defaulting to all types
        var blockedTypeNames = section.GetSection("BlockedTypes").Get<string[]>();
        if (blockedTypeNames is not null && blockedTypeNames.Length > 0)
        {
            _blockedTypes = [];
            foreach (var name in blockedTypeNames)
            {
                if (Enum.TryParse<PiiType>(name, ignoreCase: true, out var piiType))
                    _blockedTypes.Add(piiType);
                else
                    _logger.LogWarning("Unknown PII type in configuration: {TypeName}", name);
            }
        }
        else
        {
            // Default: block all types
            _blockedTypes = new HashSet<PiiType>(Enum.GetValues<PiiType>());
        }

        _detectors = [];
        foreach (var (type, pattern) in BuiltInDetectors)
        {
            if (_blockedTypes.Contains(type))
                _detectors.Add((type, new Regex(pattern, RegexOptions.Compiled)));
        }
    }

    /// <summary>
    /// Scans content for PII. Returns positions (character ranges) but never the actual PII values.
    /// </summary>
    public PiiDetectionResult Scan(string? content)
    {
        if (!_enabled || string.IsNullOrWhiteSpace(content))
            return PiiDetectionResult.Clean;

        var positions = new List<PiiPosition>();
        var detectedTypes = new HashSet<PiiType>();

        foreach (var (type, pattern) in _detectors)
        {
            foreach (Match match in pattern.Matches(content))
            {
                // For credit cards, do a basic Luhn check to reduce false positives
                if (type == PiiType.CreditCard)
                {
                    var digits = new string(match.Value.Where(char.IsDigit).ToArray());
                    if (!PassesLuhnCheck(digits))
                        continue;
                }

                positions.Add(new PiiPosition(type, match.Index, match.Length));
                detectedTypes.Add(type);
            }
        }

        if (positions.Count == 0)
            return PiiDetectionResult.Clean;

        return new PiiDetectionResult(true, detectedTypes.ToList(), positions);
    }

    /// <summary>
    /// Luhn algorithm check for credit card number validation.
    /// </summary>
    private static bool PassesLuhnCheck(string digits)
    {
        if (digits.Length < 13 || digits.Length > 19)
            return false;

        var sum = 0;
        var alternate = false;
        for (var i = digits.Length - 1; i >= 0; i--)
        {
            var n = digits[i] - '0';
            if (alternate)
            {
                n *= 2;
                if (n > 9)
                    n -= 9;
            }
            sum += n;
            alternate = !alternate;
        }
        return sum % 10 == 0;
    }
}
