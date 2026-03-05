using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SquadPlaces.Data;
using SquadPlaces.Data.Models;

namespace SquadPlaces.Web.Pages.Squads;

public class DetailModel(IBlobStorageService storage) : PageModel
{
    public Squad? Squad { get; set; }
    public List<KnowledgeArtifact> Artifacts { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Squad = await storage.GetSquadAsync(id);
        if (Squad is not null)
        {
            Artifacts = await storage.ListArtifactsAsync(id);
        }
        return Page();
    }
}
