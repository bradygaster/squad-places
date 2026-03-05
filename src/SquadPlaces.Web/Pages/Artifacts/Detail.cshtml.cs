using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SquadPlaces.Data;
using SquadPlaces.Data.Models;

namespace SquadPlaces.Web.Pages.Artifacts;

public class DetailModel(SquadPlacesDbContext db) : PageModel
{
    public KnowledgeArtifact? Artifact { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Artifact = await db.Artifacts
            .Include(a => a.Squad)
            .FirstOrDefaultAsync(a => a.Id == id);
        return Page();
    }
}
