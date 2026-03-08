using Markdig;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax.Inlines;

namespace SquadPlaces.Web.Helpers;

/// <summary>
/// Custom Markdig extension for [[WikiLink]] syntax.
/// Supports: [[Title]], [[Title|display]], [[#comment:id]], [[Title#comment:id]]
/// </summary>
public class WikiLinkExtension : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        if (!pipeline.InlineParsers.Contains<WikiLinkInlineParser>())
        {
            pipeline.InlineParsers.Insert(0, new WikiLinkInlineParser());
        }
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is HtmlRenderer htmlRenderer)
        {
            if (!htmlRenderer.ObjectRenderers.Contains<WikiLinkRenderer>())
            {
                htmlRenderer.ObjectRenderers.Insert(0, new WikiLinkRenderer());
            }
        }
    }
}

/// <summary>
/// AST node representing a WikiLink.
/// </summary>
public class WikiLinkInline : Inline
{
    public string Title { get; set; } = string.Empty;
    public string? DisplayText { get; set; }
    public string? CommentId { get; set; }
    public bool IsCommentOnly { get; set; }
}

/// <summary>
/// Parses [[...]] syntax into WikiLinkInline nodes.
/// </summary>
public class WikiLinkInlineParser : InlineParser
{
    public WikiLinkInlineParser()
    {
        OpeningCharacters = new[] { '[' };
    }

    public override bool Match(InlineProcessor processor, ref StringSlice slice)
    {
        // Check for opening [[
        if (slice.CurrentChar != '[') return false;
        if (slice.PeekChar(1) != '[') return false;

        var start = slice.Start;
        var saved = slice;
        slice.Start += 2; // Skip [[

        // Read until we find ]]
        var content = new System.Text.StringBuilder();
        while (slice.CurrentChar != '\0')
        {
            if (slice.CurrentChar == ']' && slice.PeekChar(1) == ']')
            {
                // Found closing ]]
                var linkText = content.ToString();
                
                // Parse the link text
                var wikiLink = ParseWikiLink(linkText);
                if (wikiLink is not null)
                {
                    processor.Inline = wikiLink;
                    slice.Start += 2; // Skip ]]
                    return true;
                }
                
                // Invalid wikilink — restore and fail
                slice = saved;
                return false;
            }

            content.Append(slice.CurrentChar);
            slice.NextChar();
        }

        // No closing ]] found — restore and fail
        slice = saved;
        return false;
    }

    private WikiLinkInline? ParseWikiLink(string linkText)
    {
        if (string.IsNullOrWhiteSpace(linkText)) return null;

        var wikiLink = new WikiLinkInline();

        // Check for comment-only reference: [[#comment:id]]
        if (linkText.StartsWith("#comment:", StringComparison.Ordinal))
        {
            wikiLink.IsCommentOnly = true;
            wikiLink.CommentId = linkText.Substring(9).Trim();
            wikiLink.Title = string.Empty;
            wikiLink.DisplayText = "comment";
            return wikiLink;
        }

        // Check for display text separator: [[Title|display]]
        var pipeIndex = linkText.IndexOf('|');
        string titlePart;
        
        if (pipeIndex >= 0)
        {
            titlePart = linkText.Substring(0, pipeIndex).Trim();
            wikiLink.DisplayText = linkText.Substring(pipeIndex + 1).Trim();
        }
        else
        {
            titlePart = linkText.Trim();
        }

        // Check for comment reference: [[Title#comment:id]]
        var commentIndex = titlePart.IndexOf("#comment:", StringComparison.Ordinal);
        if (commentIndex >= 0)
        {
            wikiLink.Title = titlePart.Substring(0, commentIndex).Trim();
            wikiLink.CommentId = titlePart.Substring(commentIndex + 9).Trim();
        }
        else
        {
            wikiLink.Title = titlePart;
        }

        return wikiLink;
    }
}

/// <summary>
/// Renders WikiLinkInline nodes to HTML anchor tags.
/// </summary>
public class WikiLinkRenderer : HtmlObjectRenderer<WikiLinkInline>
{
    protected override void Write(HtmlRenderer renderer, WikiLinkInline obj)
    {
        renderer.Write("<a href=\"");
        
        if (obj.IsCommentOnly)
        {
            // [[#comment:id]] → <a href="#comment-{id}">
            renderer.Write("#comment-");
            renderer.WriteEscape(obj.CommentId ?? string.Empty);
        }
        else if (!string.IsNullOrEmpty(obj.CommentId))
        {
            // [[Title#comment:id]] → <a href="/wiki/{Title}#comment-{id}">
            renderer.Write("/wiki/");
            renderer.WriteEscape(Uri.EscapeDataString(obj.Title));
            renderer.Write("#comment-");
            renderer.WriteEscape(obj.CommentId);
        }
        else
        {
            // [[Title]] → <a href="/wiki/{Title}">
            renderer.Write("/wiki/");
            renderer.WriteEscape(Uri.EscapeDataString(obj.Title));
        }
        
        renderer.Write("\" class=\"wikilink\">");
        
        // Display text
        if (!string.IsNullOrEmpty(obj.DisplayText))
        {
            renderer.WriteEscape(obj.DisplayText);
        }
        else if (!string.IsNullOrEmpty(obj.CommentId) && !obj.IsCommentOnly)
        {
            renderer.WriteEscape(obj.Title);
            renderer.Write(" (comment)");
        }
        else
        {
            renderer.WriteEscape(obj.Title);
        }
        
        renderer.Write("</a>");
    }
}

/// <summary>
/// Extension method for easy registration.
/// </summary>
public static class WikiLinkExtensions
{
    public static MarkdownPipelineBuilder UseWikiLinks(this MarkdownPipelineBuilder pipeline)
    {
        pipeline.Extensions.Add(new WikiLinkExtension());
        return pipeline;
    }
}
