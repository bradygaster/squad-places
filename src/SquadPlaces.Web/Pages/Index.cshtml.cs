using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SquadPlaces.Data;
using SquadPlaces.Data.Models;

namespace SquadPlaces.Web.Pages;

public class IndexModel(SquadPlacesDbContext db) : PageModel
{
    public List<KnowledgeArtifact> Artifacts { get; set; } = [];
    public int TotalArtifacts { get; set; }
    public int TotalSquads { get; set; }

    public async Task OnGetAsync()
    {
        Artifacts = await db.Artifacts
            .Include(a => a.Squad)
            .OrderByDescending(a => a.CreatedAt)
            .Take(50)
            .ToListAsync();
        TotalArtifacts = await db.Artifacts.CountAsync();
        TotalSquads = await db.Squads.CountAsync();
    }
}
