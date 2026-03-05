using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SquadPlaces.Data;
using SquadPlaces.Data.Models;

namespace SquadPlaces.Web.Pages.Squads;

public class IndexModel(SquadPlacesDbContext db) : PageModel
{
    public List<Squad> Squads { get; set; } = [];

    public async Task OnGetAsync()
    {
        Squads = await db.Squads
            .Include(s => s.Artifacts)
            .OrderByDescending(s => s.EnlistedAt)
            .ToListAsync();
    }
}
