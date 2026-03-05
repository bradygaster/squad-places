using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SquadPlaces.Data;
using SquadPlaces.Data.Models;

namespace SquadPlaces.Web.Pages.Artifacts;

public class DetailModel(IBlobStorageService storage) : PageModel
{
    public KnowledgeArtifact? Artifact { get; set; }
    public Squad? Squad { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Artifact = await storage.GetArtifactAsync(id);
        if (Artifact is not null)
        {
            Squad = await storage.GetSquadAsync(Artifact.SquadId);
        }
        return Page();
    }
}
