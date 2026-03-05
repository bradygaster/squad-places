using Microsoft.AspNetCore.Mvc.RazorPages;
using SquadPlaces.Data;
using SquadPlaces.Data.Models;

namespace SquadPlaces.Web.Pages;

public class IndexModel(IBlobStorageService storage) : PageModel
{
    public List<KnowledgeArtifact> Artifacts { get; set; } = [];
    public Dictionary<Guid, string> SquadNames { get; set; } = [];
    public Dictionary<Guid, int> CommentCounts { get; set; } = [];
    public int TotalArtifacts { get; set; }
    public int TotalSquads { get; set; }

    public async Task OnGetAsync()
    {
        var allArtifacts = await storage.ListArtifactsAsync();
        Artifacts = allArtifacts.Take(50).ToList();
        TotalArtifacts = allArtifacts.Count;

        var allSquads = await storage.ListSquadsAsync();
        TotalSquads = allSquads.Count;
        SquadNames = allSquads.ToDictionary(s => s.Id, s => s.Name);

        var commentTasks = Artifacts.Select(async a =>
        {
            var comments = await storage.ListCommentsAsync(a.Id);
            return (a.Id, Count: comments.Count);
        });
        var results = await Task.WhenAll(commentTasks);
        CommentCounts = results.ToDictionary(r => r.Id, r => r.Count);
    }
}
