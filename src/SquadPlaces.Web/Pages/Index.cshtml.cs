using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SquadPlaces.Data;
using SquadPlaces.Data.Models;

namespace SquadPlaces.Web.Pages;

public class IndexModel(IBlobStorageService storage) : PageModel
{
    public List<KnowledgeArtifact> Artifacts { get; set; } = [];
    public Dictionary<Guid, string> SquadNames { get; set; } = [];
    public Dictionary<Guid, int> CommentCounts { get; set; } = [];
    public List<Squad> AllSquads { get; set; } = [];
    public int TotalArtifacts { get; set; }
    public int TotalSquads { get; set; }
    public int TotalComments { get; set; }

    // Pagination
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public const int PageSize = 20;

    [BindProperty(SupportsGet = true)]
    [FromQuery(Name = "sort")]
    public string? Sort { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery(Name = "squad")]
    public Guid? SquadFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery(Name = "tag")]
    public string? Tag { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery(Name = "page")]
    public new int? Page { get; set; }

    public string ActiveSort => string.IsNullOrEmpty(Sort) ? "latest" : Sort;

    public async Task OnGetAsync()
    {
        CurrentPage = Page is > 0 ? Page.Value : 1;

        var allSquads = await storage.ListSquadsAsync();
        AllSquads = allSquads;
        TotalSquads = allSquads.Count;
        SquadNames = allSquads.ToDictionary(s => s.Id, s => s.Name);

        var allArtifacts = SquadFilter.HasValue
            ? await storage.ListArtifactsAsync(SquadFilter.Value)
            : await storage.ListArtifactsAsync();

        // Filter by tag
        if (!string.IsNullOrWhiteSpace(Tag))
        {
            allArtifacts = allArtifacts
                .Where(a => !string.IsNullOrEmpty(a.Tags) &&
                    a.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                           .Any(t => t.Equals(Tag, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        TotalArtifacts = allArtifacts.Count;

        // Compute comment counts for visible artifacts
        var commentTasks = allArtifacts.Select(async a =>
        {
            var comments = await storage.ListCommentsAsync(a.Id);
            return (a.Id, Count: comments.Count);
        });
        var results = await Task.WhenAll(commentTasks);
        CommentCounts = results.ToDictionary(r => r.Id, r => r.Count);
        TotalComments = CommentCounts.Values.Sum();

        // Apply sorting
        var sorted = ActiveSort switch
        {
            "comments" => allArtifacts.OrderByDescending(a => CommentCounts.GetValueOrDefault(a.Id, 0))
                                      .ThenByDescending(a => a.CreatedAt),
            _ => allArtifacts.OrderByDescending(a => a.CreatedAt),
        };

        // Pagination
        TotalPages = Math.Max(1, (int)Math.Ceiling(TotalArtifacts / (double)PageSize));
        CurrentPage = Math.Clamp(CurrentPage, 1, TotalPages);

        Artifacts = sorted
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();
    }
}
