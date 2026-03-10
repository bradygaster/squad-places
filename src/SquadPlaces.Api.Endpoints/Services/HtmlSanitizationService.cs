using Ganss.Xss;
using Microsoft.Extensions.Logging;

namespace SquadPlaces.Api.Endpoints.Services;

/// <summary>
/// Sanitizes user-supplied text by stripping dangerous HTML while preserving safe formatting tags.
/// Defense-in-depth layer — runs after ApiValidation.Sanitize() which handles control characters.
/// </summary>
public class HtmlSanitizationService
{
    private readonly HtmlSanitizer _sanitizer;
    private readonly ILogger<HtmlSanitizationService> _logger;

    public HtmlSanitizationService(ILogger<HtmlSanitizationService> logger)
    {
        _logger = logger;
        _sanitizer = new HtmlSanitizer();

        // Clear defaults and explicitly allow only safe formatting tags
        _sanitizer.AllowedTags.Clear();
        foreach (var tag in new[] { "b", "i", "em", "strong", "p", "br", "ul", "ol", "li", "a", "code", "pre", "blockquote" })
        {
            _sanitizer.AllowedTags.Add(tag);
        }

        // Only allow href on anchor tags, no javascript: URIs
        _sanitizer.AllowedAttributes.Clear();
        _sanitizer.AllowedAttributes.Add("href");

        _sanitizer.AllowedSchemes.Clear();
        _sanitizer.AllowedSchemes.Add("http");
        _sanitizer.AllowedSchemes.Add("https");
    }

    /// <summary>
    /// Sanitizes the input, stripping dangerous HTML tags (script, iframe, object, embed, form, etc.)
    /// while preserving safe formatting. Returns the sanitized string.
    /// </summary>
    public string Sanitize(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var sanitized = _sanitizer.Sanitize(input);

        if (sanitized.Length != input.Length || sanitized != input)
        {
            _logger.LogWarning("HTML sanitization modified content — dangerous markup was stripped. Original length: {OriginalLength}, Sanitized length: {SanitizedLength}",
                input.Length, sanitized.Length);
        }

        return sanitized;
    }

    /// <summary>
    /// Sanitizes the input if non-null; returns null for null input.
    /// </summary>
    public string? SanitizeNullable(string? input)
    {
        if (input is null) return null;
        return Sanitize(input);
    }
}
