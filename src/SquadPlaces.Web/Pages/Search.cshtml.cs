using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SquadPlaces.Data;
using SquadPlaces.Data.Models;

namespace SquadPlaces.Web.Pages;

public class SearchModel(IBlobStorageService storage) : PageModel
{
    public List<KnowledgeArtifact> Results { get; set; } = [];
    public Dictionary<Guid, string> SquadNames { get; set; } = new();
    public Dictionary<Guid, int> CommentCounts { get; set; } = new();
    public string? Query { get; set; }
    public int ResultCount { get; set; }

    [BindProperty(SupportsGet = true)]
    [FromQuery(Name = "q")]
    public string? SearchQuery { get; set; }

    public async Task OnGetAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;

        Query = SearchQuery.Trim();
        var query = Query.ToLowerInvariant();

        var allArtifacts = await storage.ListArtifactsAsync();
        var allSquads = await storage.ListSquadsAsync();
        SquadNames = allSquads.ToDictionary(s => s.Id, s => s.Name);

        Results = allArtifacts.Where(a =>
            a.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            a.Summary.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            (!string.IsNullOrEmpty(a.Content) && a.Content.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
            (!string.IsNullOrEmpty(a.Tags) && a.Tags.Split(',', StringSplitOptions.TrimEntries)
                .Any(t => t.Contains(query, StringComparison.OrdinalIgnoreCase)))
        ).OrderByDescending(a => a.CreatedAt).ToList();

        ResultCount = Results.Count;

        foreach (var artifact in Results)
        {
            CommentCounts[artifact.Id] = await storage.CountCommentsAsync(artifact.Id);
        }
    }
}
