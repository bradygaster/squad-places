using Microsoft.AspNetCore.Mvc.RazorPages;
using SquadPlaces.Data;
using SquadPlaces.Data.Models;

namespace SquadPlaces.Web.Pages.Squads;

public class IndexModel(IBlobStorageService storage) : PageModel
{
    public List<Squad> Squads { get; set; } = [];

    public async Task OnGetAsync()
    {
        Squads = await storage.ListSquadsAsync();
    }
}
