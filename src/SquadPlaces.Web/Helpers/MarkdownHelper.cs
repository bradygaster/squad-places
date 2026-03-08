using Ganss.Xss;
using Markdig;

namespace SquadPlaces.Web.Helpers;

/// <summary>
/// Converts Markdown to sanitized HTML using Markdig + HtmlSanitizer.
/// </summary>
public static class MarkdownHelper
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseWikiLinks()
        .Build();

    private static readonly HtmlSanitizer Sanitizer = CreateSanitizer();

    private static HtmlSanitizer CreateSanitizer()
    {
        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedTags.Add("code");
        sanitizer.AllowedTags.Add("pre");
        sanitizer.AllowedTags.Add("blockquote");
        sanitizer.AllowedTags.Add("hr");
        sanitizer.AllowedTags.Add("table");
        sanitizer.AllowedTags.Add("thead");
        sanitizer.AllowedTags.Add("tbody");
        sanitizer.AllowedTags.Add("tr");
        sanitizer.AllowedTags.Add("th");
        sanitizer.AllowedTags.Add("td");
        sanitizer.AllowedTags.Add("dl");
        sanitizer.AllowedTags.Add("dt");
        sanitizer.AllowedTags.Add("dd");
        sanitizer.AllowedTags.Add("img");
        sanitizer.AllowedTags.Add("a");
        sanitizer.AllowedAttributes.Add("class");
        sanitizer.AllowedAttributes.Add("src");
        sanitizer.AllowedAttributes.Add("alt");
        sanitizer.AllowedAttributes.Add("title");
        sanitizer.AllowedAttributes.Add("width");
        sanitizer.AllowedAttributes.Add("height");
        sanitizer.AllowedAttributes.Add("href");

        // Allow /api/images/ (for img src), /wiki/ (for wikilink href), and #comment- (for comment anchors)
        sanitizer.FilterUrl += (sender, args) =>
        {
            var url = args.SanitizedUrl;
            if (url?.StartsWith("/api/images/") == true) return;
            if (url?.StartsWith("/wiki/") == true) return;
            if (url?.StartsWith("#comment-") == true) return;
            args.SanitizedUrl = string.Empty;
        };

        return sanitizer;
    }

    /// <summary>
    /// Renders Markdown to sanitized HTML. Returns empty string for null/empty input.
    /// </summary>
    public static string ToHtml(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return string.Empty;

        var rawHtml = Markdown.ToHtml(markdown, Pipeline);
        return Sanitizer.Sanitize(rawHtml);
    }
}
