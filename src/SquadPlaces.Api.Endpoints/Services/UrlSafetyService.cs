using System.Net;
using System.Text.RegularExpressions;

namespace SquadPlaces.Api.Endpoints.Services;

/// <summary>
/// Result of a URL safety check.
/// </summary>
public record UrlSafetyResult(bool IsValid, string? BlockReason)
{
    public static readonly UrlSafetyResult Safe = new(true, null);
}

/// <summary>
/// Validates external URLs against SSRF targets and content-type expectations.
/// Blocks requests to internal/private networks, non-HTTP schemes, and unexpected file types.
/// </summary>
public class UrlSafetyService
{
    private readonly ILogger<UrlSafetyService> _logger;

    private static readonly HashSet<string> AllowedSchemes = new(StringComparer.OrdinalIgnoreCase) { "http", "https" };

    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".gif", ".png", ".jpg", ".jpeg", ".webp", ".svg"
    };

    private static readonly HashSet<string> BlockedHostSuffixes = new(StringComparer.OrdinalIgnoreCase)
    {
        ".local", ".internal"
    };

    private static readonly Regex PrivateIpv4Pattern = new(
        @"^(127\.\d+\.\d+\.\d+|10\.\d+\.\d+\.\d+|192\.168\.\d+\.\d+|169\.254\.\d+\.\d+|172\.(1[6-9]|2\d|3[01])\.\d+\.\d+|0\.0\.0\.0)$",
        RegexOptions.Compiled);

    public UrlSafetyService(ILogger<UrlSafetyService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Validates a URL for SSRF safety and allowed image extensions.
    /// Returns a structured result with IsValid and BlockReason.
    /// </summary>
    public UrlSafetyResult Validate(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return UrlSafetyResult.Safe;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return new UrlSafetyResult(false, "URL is not a valid absolute URI.");

        // Scheme check
        if (!AllowedSchemes.Contains(uri.Scheme))
            return new UrlSafetyResult(false, $"URL scheme '{uri.Scheme}' is not allowed. Only http and https are accepted.");

        // Host checks — SSRF prevention
        var host = uri.Host;

        if (string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase))
            return Block(url, "URL points to localhost — internal network access blocked.");

        // IPv6 loopback
        if (host == "[::1]" || string.Equals(host, "::1", StringComparison.OrdinalIgnoreCase))
            return Block(url, "URL points to IPv6 loopback — internal network access blocked.");

        // Try to parse as IP address for private range detection
        if (IPAddress.TryParse(host, out var ipAddress))
        {
            var ipStr = ipAddress.ToString();

            if (PrivateIpv4Pattern.IsMatch(ipStr))
                return Block(url, "URL points to a private/internal IP address — SSRF blocked.");

            // IPv6 loopback check
            if (IPAddress.IsLoopback(ipAddress))
                return Block(url, "URL points to a loopback address — SSRF blocked.");

            // IPv6 link-local (fe80::/10)
            if (ipAddress.IsIPv6LinkLocal)
                return Block(url, "URL points to an IPv6 link-local address — SSRF blocked.");

            // IPv6-mapped IPv4 private addresses (::ffff:10.x.x.x, etc.)
            if (ipAddress.IsIPv4MappedToIPv6)
            {
                var mapped = ipAddress.MapToIPv4().ToString();
                if (PrivateIpv4Pattern.IsMatch(mapped))
                    return Block(url, "URL points to an IPv4-mapped private address — SSRF blocked.");
            }
        }

        // Blocked hostname suffixes
        foreach (var suffix in BlockedHostSuffixes)
        {
            if (host.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                return Block(url, $"URL hostname ends with '{suffix}' — internal network access blocked.");
        }

        // File extension validation
        var path = uri.AbsolutePath;
        var lastSegment = path.Split('/').LastOrDefault() ?? "";
        var dotIndex = lastSegment.LastIndexOf('.');
        if (dotIndex >= 0)
        {
            var extension = lastSegment[dotIndex..].ToLowerInvariant();
            // Strip query params from extension if present (shouldn't be, but defense in depth)
            var qIndex = extension.IndexOf('?');
            if (qIndex >= 0)
                extension = extension[..qIndex];

            if (!AllowedImageExtensions.Contains(extension))
                return new UrlSafetyResult(false, $"File extension '{extension}' is not allowed. Expected one of: {string.Join(", ", AllowedImageExtensions)}");
        }
        // No extension is allowed (CDN URLs like tenor/giphy often have no extension)

        return UrlSafetyResult.Safe;
    }

    private UrlSafetyResult Block(string url, string reason)
    {
        _logger.LogWarning("SSRF blocked: {Reason} URL: {Url}", reason, url);
        return new UrlSafetyResult(false, reason);
    }
}
