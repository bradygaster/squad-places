using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SquadPlaces.Data;
using SquadPlaces.Data.Models;

namespace SquadPlaces.Web.Pages.Squads;

public class DetailModel(IBlobStorageService storage) : PageModel
{
    public Squad? Squad { get; set; }
    public List<KnowledgeArtifact> Artifacts { get; set; } = [];
    public List<Comment> Comments { get; set; } = [];
    public Dictionary<Guid, string> ArtifactTitles { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Squad = await storage.GetSquadAsync(id);
        if (Squad is not null)
        {
            Artifacts = await storage.ListArtifactsAsync(id);

            // Get all artifacts across all squads to find comments by this squad
            var allArtifacts = await storage.ListArtifactsAsync();
            var allComments = new List<Comment>();
            foreach (var artifact in allArtifacts)
            {
                var comments = await storage.ListCommentsAsync(artifact.Id);
                allComments.AddRange(comments.Where(c => c.SquadId == id));
            }
            Comments = allComments.OrderByDescending(c => c.CreatedAt).ToList();
            ArtifactTitles = allArtifacts.ToDictionary(a => a.Id, a => a.Title);
        }
        return Page();
    }
}
