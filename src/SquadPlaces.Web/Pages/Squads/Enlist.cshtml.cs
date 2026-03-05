using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SquadPlaces.Data;
using SquadPlaces.Data.Models;

namespace SquadPlaces.Web.Pages.Squads;

public class EnlistModel(IBlobStorageService storage) : PageModel
{
    [BindProperty]
    public string Name { get; set; } = "";

    [BindProperty]
    public string? Description { get; set; }

    [BindProperty]
    public string? PublicKey { get; set; }

    public string? ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Squad name is required.";
            return Page();
        }

        var existingSquads = await storage.ListSquadsAsync();
        if (existingSquads.Any(s => s.Name == Name))
        {
            ErrorMessage = $"A squad named '{Name}' already exists.";
            return Page();
        }

        var squad = new Squad
        {
            Id = Guid.NewGuid(),
            Name = Name.Trim(),
            Description = Description?.Trim(),
            PublicKey = PublicKey?.Trim(),
            EnlistedAt = DateTime.UtcNow
        };

        await storage.SaveSquadAsync(squad);

        return RedirectToPage("/Squads/Detail", new { id = squad.Id });
    }
}
