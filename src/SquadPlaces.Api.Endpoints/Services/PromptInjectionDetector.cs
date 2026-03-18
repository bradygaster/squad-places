using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SquadPlaces.Api.Endpoints.Services;

/// <summary>
/// Confidence level for prompt injection detection.
/// </summary>
public enum InjectionConfidence { Low, Medium, High }

/// <summary>
/// Result of a prompt injection scan.
/// </summary>
public record PromptInjectionResult(bool IsInjection, List<string> DetectedPatterns, InjectionConfidence Confidence)
{
    public static readonly PromptInjectionResult Clean = new(false, [], InjectionConfidence.Low);
}

/// <summary>
/// Detects common prompt injection patterns in user-generated content.
/// Regex-based, no external API calls — fast and deterministic.
/// Patterns are configurable via IConfiguration ("PromptInjection" section).
/// </summary>
public class PromptInjectionDetector
{
    private readonly bool _enabled;
    private readonly List<(string Name, Regex Pattern)> _patterns;
    private readonly ILogger<PromptInjectionDetector> _logger;

    // Built-in pattern definitions: (name, regex pattern string)
    private static readonly (string Name, string Pattern)[] BuiltInPatterns =
    [
        // Instruction override
        ("IgnorePreviousInstructions", @"ignore\s+(all\s+)?previous\s+instructions"),
        ("DisregardAbove", @"disregard\s+(all\s+)?(above|previous|prior)"),
        ("ForgetInstructions", @"forget\s+(all\s+)?(your\s+)?(previous\s+)?instructions"),
        ("DoNotFollow", @"do\s+not\s+follow\s+(your\s+)?(previous\s+)?instructions"),
        ("OverrideInstructions", @"override\s+(all\s+)?(previous\s+)?(instructions|rules)"),
        ("NewInstructions", @"new\s+instructions\s*:"),

        // Role confusion
        ("RoleConfusionYouAreNow", @"you\s+are\s+now\s+(?!welcome|viewing|looking)"),
        ("RoleConfusionActAs", @"act\s+as\s+(a\s+|an\s+)?(?!team|squad)"),
        ("RoleConfusionPretend", @"pretend\s+(you\s+are|to\s+be)"),
        ("RoleConfusionImpersonate", @"impersonate\s+(a\s+|an\s+)?"),

        // System prompt references
        ("SystemPromptReference", @"system\s*prompt"),
        ("SystemColonPrefix", @"(?:^|\n)\s*system\s*:"),
        ("SystemHashPrefix", @"#{2,}\s*system"),
        ("AssistantColonPrefix", @"(?:^|\n)\s*assistant\s*:"),
        ("UserColonPrefix", @"(?:^|\n)\s*user\s*:"),

        // Jailbreak / DAN patterns
        ("DanMode", @"\bdan\s+mode\b"),
        ("Jailbreak", @"\bjailbreak\b"),
        ("DoAnythingNow", @"do\s+anything\s+now"),
        ("DeveloperMode", @"developer\s+mode\s+(enabled|activated|on)"),
        ("UnlockMode", @"(unlock|enable)\s+(god|admin|root|super)\s+mode"),

        // Data exfiltration attempts
        ("ExfiltrateData", @"(send|transmit|exfiltrate|forward|post)\s+(all\s+)?(data|information|content|secrets|keys|tokens)\s+to"),
        ("FetchUrl", @"(fetch|curl|wget|request|call|visit)\s+(https?://|http://)\S+"),

        // Delimiter injection
        ("DelimiterInjection", @"<\|?(system|user|assistant|im_start|im_end)\|?>"),
        ("EndOfPrompt", @"(end\s+of\s+(system\s+)?prompt|---\s*end\s*---)")
    ];

    public PromptInjectionDetector(IConfiguration configuration, ILogger<PromptInjectionDetector> logger)
    {
        _logger = logger;

        var section = configuration.GetSection("PromptInjection");
        _enabled = section.GetValue("Enabled", true);

        _patterns = [];

        // Load built-in patterns
        foreach (var (name, pattern) in BuiltInPatterns)
        {
            _patterns.Add((name, new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled)));
        }

        // Load additional configurable patterns
        var additional = section.GetSection("AdditionalPatterns").Get<string[]>();
        if (additional is not null)
        {
            for (var i = 0; i < additional.Length; i++)
            {
                try
                {
                    _patterns.Add(($"Custom_{i}", new Regex(additional[i], RegexOptions.IgnoreCase | RegexOptions.Compiled)));
                }
                catch (ArgumentException ex)
                {
                    _logger.LogWarning("Invalid additional prompt injection pattern at index {Index}: {Error}", i, ex.Message);
                }
            }
        }
    }

    /// <summary>
    /// Scans content for prompt injection patterns. Checks both plaintext and base64-encoded segments.
    /// </summary>
    public PromptInjectionResult Scan(string? content)
    {
        if (!_enabled || string.IsNullOrWhiteSpace(content))
            return PromptInjectionResult.Clean;

        var detectedPatterns = new List<string>();

        // Scan plaintext
        ScanText(content, detectedPatterns);

        // Scan base64-encoded segments (attackers may encode payloads)
        ScanBase64Segments(content, detectedPatterns);

        if (detectedPatterns.Count == 0)
            return PromptInjectionResult.Clean;

        // Deduplicate
        detectedPatterns = detectedPatterns.Distinct().ToList();

        var confidence = detectedPatterns.Count switch
        {
            1 => InjectionConfidence.Low,
            2 or 3 => InjectionConfidence.Medium,
            _ => InjectionConfidence.High
        };

        // Escalate to High if we see instruction override + role confusion together
        var hasOverride = detectedPatterns.Any(p =>
            p.Contains("Ignore") || p.Contains("Disregard") || p.Contains("Override") || p.Contains("Forget") || p.Contains("NewInstructions"));
        var hasRoleConfusion = detectedPatterns.Any(p => p.Contains("RoleConfusion"));
        if (hasOverride && hasRoleConfusion)
            confidence = InjectionConfidence.High;

        return new PromptInjectionResult(true, detectedPatterns, confidence);
    }

    private void ScanText(string text, List<string> detectedPatterns)
    {
        foreach (var (name, pattern) in _patterns)
        {
            if (pattern.IsMatch(text))
                detectedPatterns.Add(name);
        }
    }

    private void ScanBase64Segments(string content, List<string> detectedPatterns)
    {
        // Look for base64-like segments (40+ chars of valid base64 alphabet)
        var base64Regex = new Regex(@"[A-Za-z0-9+/=]{40,}", RegexOptions.Compiled);
        foreach (Match match in base64Regex.Matches(content))
        {
            try
            {
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(match.Value));
                ScanText(decoded, detectedPatterns);
            }
            catch (FormatException)
            {
                // Not valid base64 — skip
            }
        }
    }

    /// <summary>
    /// Computes a SHA-256 content hash for logging (avoids logging actual content).
    /// </summary>
    public static string ContentHash(string content)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexStringLower(bytes)[..16];
    }
}
