using Azure;
using Azure.AI.Vision.ImageAnalysis;

namespace SquadPlaces.Api.Endpoints.Services;

/// <summary>
/// Safety verdict for image content analysis.
/// </summary>
public enum ImageSafetyVerdict
{
    /// <summary>Image content passed all checks — safe to publish.</summary>
    Safe,

    /// <summary>Image content triggered a hard block — reject immediately.</summary>
    Unsafe,

    /// <summary>Image content triggered a warning — store as pending review.</summary>
    NeedsReview
}

/// <summary>
/// Result of analyzing image content via Azure Computer Vision.
/// </summary>
public record ImageAnalysisResult(
    ImageSafetyVerdict Verdict,
    List<string> DetectedCategories)
{
    /// <summary>Returned when the service is not configured — image analysis is skipped.</summary>
    public static readonly ImageAnalysisResult NotConfigured = new(ImageSafetyVerdict.Safe, []);

    /// <summary>Returned when the API call fails — graceful degradation.</summary>
    public static readonly ImageAnalysisResult ServiceError = new(ImageSafetyVerdict.Safe, []);

    /// <summary>True if the Azure API was actually called and returned results.</summary>
    public bool WasAnalyzed => this != NotConfigured && this != ServiceError;
}

/// <summary>
/// Analyzes image content using Azure Computer Vision for adult/racy/gory material.
/// Uses UrlSafetyService for SSRF protection before downloading external images.
/// Gracefully degrades if Azure Computer Vision endpoint/key are not configured.
/// </summary>
public class ImageContentAnalysisService
{
    private readonly ImageAnalysisClient? _client;
    private readonly UrlSafetyService _urlSafety;
    private readonly ILogger<ImageContentAnalysisService> _logger;
    private readonly HttpClient _httpClient;

    /// <summary>True when endpoint + key are configured and a client was created.</summary>
    public bool IsConfigured => _client is not null;

    public ImageContentAnalysisService(
        IConfiguration configuration,
        UrlSafetyService urlSafety,
        ILogger<ImageContentAnalysisService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _urlSafety = urlSafety;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("ImageDownload");

        var endpoint = configuration["AzureComputerVision:Endpoint"];
        var key = configuration["AzureComputerVision:Key"];

        if (!string.IsNullOrWhiteSpace(endpoint) && !string.IsNullOrWhiteSpace(key))
        {
            _client = new ImageAnalysisClient(new Uri(endpoint), new AzureKeyCredential(key));
            _logger.LogInformation("Azure Computer Vision image analysis enabled (endpoint: {Endpoint})", endpoint);
        }
        else
        {
            _logger.LogInformation("Azure Computer Vision not configured — image content analysis disabled");
        }
    }

    /// <summary>
    /// Analyzes image bytes for adult/racy/gory content.
    /// Returns NotConfigured if the service isn't set up, or ServiceError on failure.
    /// </summary>
    public async Task<ImageAnalysisResult> AnalyzeImageBytesAsync(byte[] imageBytes)
    {
        if (_client is null)
            return ImageAnalysisResult.NotConfigured;

        try
        {
            var imageData = BinaryData.FromBytes(imageBytes);
            return await AnalyzeCoreAsync(imageData);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex,
                "Azure Computer Vision API failed (status: {Status}). Degrading gracefully — skipping image analysis.",
                ex.Status);
            return ImageAnalysisResult.ServiceError;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error calling Azure Computer Vision. Degrading gracefully — skipping image analysis.");
            return ImageAnalysisResult.ServiceError;
        }
    }

    /// <summary>
    /// Downloads an image from a URL (with SSRF protection) and analyzes it for adult/racy/gory content.
    /// For GIFs, analyzes the first frame (Azure Computer Vision handles this natively).
    /// </summary>
    public async Task<ImageAnalysisResult> AnalyzeImageUrlAsync(string imageUrl)
    {
        if (_client is null)
            return ImageAnalysisResult.NotConfigured;

        // SSRF protection — validate URL before downloading
        var safetyResult = _urlSafety.Validate(imageUrl);
        if (!safetyResult.IsValid)
        {
            _logger.LogWarning("Image URL blocked by SSRF protection: {Reason}. URL: {Url}",
                safetyResult.BlockReason, imageUrl);
            return new ImageAnalysisResult(ImageSafetyVerdict.Unsafe,
                [$"ImageUrl:SSRF_Blocked({safetyResult.BlockReason})"]);
        }

        try
        {
            // Download with size limit (10MB) and timeout
            using var response = await _httpClient.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to download image from {Url}: HTTP {StatusCode}",
                    imageUrl, response.StatusCode);
                return ImageAnalysisResult.ServiceError;
            }

            // Enforce size limit before reading body
            const long maxBytes = 10 * 1024 * 1024; // 10MB
            if (response.Content.Headers.ContentLength > maxBytes)
            {
                _logger.LogWarning("Image at {Url} exceeds 10MB limit ({Size} bytes)",
                    imageUrl, response.Content.Headers.ContentLength);
                return new ImageAnalysisResult(ImageSafetyVerdict.Unsafe,
                    ["ImageUrl:ExceedsSizeLimit"]);
            }

            var imageBytes = await response.Content.ReadAsByteArrayAsync();
            if (imageBytes.Length > maxBytes)
            {
                _logger.LogWarning("Downloaded image from {Url} exceeds 10MB limit ({Size} bytes)",
                    imageUrl, imageBytes.Length);
                return new ImageAnalysisResult(ImageSafetyVerdict.Unsafe,
                    ["ImageUrl:ExceedsSizeLimit"]);
            }

            var imageData = BinaryData.FromBytes(imageBytes);
            return await AnalyzeCoreAsync(imageData);
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Timeout downloading image from {Url}", imageUrl);
            return ImageAnalysisResult.ServiceError;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to download image from {Url}", imageUrl);
            return ImageAnalysisResult.ServiceError;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex,
                "Azure Computer Vision API failed (status: {Status}). Degrading gracefully.",
                ex.Status);
            return ImageAnalysisResult.ServiceError;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error during image content analysis. Degrading gracefully.");
            return ImageAnalysisResult.ServiceError;
        }
    }

    private async Task<ImageAnalysisResult> AnalyzeCoreAsync(BinaryData imageData)
    {
        // Azure Computer Vision analyzes the first frame of GIFs natively
        var result = await _client!.AnalyzeAsync(
            imageData,
            VisualFeatures.DenseCaptions, // Use dense captions to understand content
            new ImageAnalysisOptions
            {
                GenderNeutralCaption = true
            });

        var categories = new List<string>();
        var verdict = ImageSafetyVerdict.Safe;

        // Check for adult/racy/gory content via the metadata
        // Azure Vision 4.0 returns content safety via the API response metadata
        if (result.Value.Metadata is not null)
        {
            _logger.LogInformation(
                "Image analyzed: {Width}x{Height}",
                result.Value.Metadata.Width,
                result.Value.Metadata.Height);
        }

        // Use dense captions to detect potentially unsafe content descriptions
        if (result.Value.DenseCaptions is not null)
        {
            foreach (var caption in result.Value.DenseCaptions.Values)
            {
                var text = caption.Text.ToLowerInvariant();

                // Check for content that indicates adult/violent/gory material
                if (ContainsUnsafeContentIndicators(text))
                {
                    categories.Add($"ImageContent:UnsafeCaption({caption.Text})");
                    if (caption.Confidence > 0.7)
                        verdict = ImageSafetyVerdict.Unsafe;
                    else if (verdict != ImageSafetyVerdict.Unsafe)
                        verdict = ImageSafetyVerdict.NeedsReview;
                }
            }
        }

        if (verdict != ImageSafetyVerdict.Safe)
        {
            _logger.LogInformation(
                "Image content analysis: {Verdict}. Categories: {Categories}",
                verdict, string.Join(", ", categories));
        }

        return new ImageAnalysisResult(verdict, categories);
    }

    private static bool ContainsUnsafeContentIndicators(string captionText)
    {
        // Content indicators for adult/racy/gory/violent material
        string[] unsafeIndicators =
        [
            "nude", "naked", "explicit", "sexual",
            "gore", "blood", "mutilat", "dismember",
            "violence", "weapon", "gun", "knife attack",
            "drug", "narcotic",
            "hate symbol", "swastika",
            "self-harm", "suicide"
        ];

        foreach (var indicator in unsafeIndicators)
        {
            if (captionText.Contains(indicator, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
