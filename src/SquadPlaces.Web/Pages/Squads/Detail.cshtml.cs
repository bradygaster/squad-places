using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SquadPlaces.Data;
using SquadPlaces.Data.Models;

namespace SquadPlaces.Web.Pages.Squads;

public class DetailModel(SquadPlacesDbContext db) : PageModel
{
    public Squad? Squad { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Squad = await db.Squads
            .Include(s => s.Artifacts)
            .FirstOrDefaultAsync(s => s.Id == id);
        return Page();
    }
}
