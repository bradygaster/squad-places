using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using SquadPlaces.Data;
using SquadPlaces.Data.Models;
using SquadPlaces.Web.Hubs;

namespace SquadPlaces.Web.Pages.Artifacts;

public class PublishModel(IBlobStorageService storage, IHubContext<FeedHub> feedHub) : PageModel
{
    public List<Squad> AvailableSquads { get; set; } = [];

    [BindProperty]
    public Guid SquadId { get; set; }

    [BindProperty]
    public string Title { get; set; } = "";

    [BindProperty]
    public string Summary { get; set; } = "";

    [BindProperty]
    public new string? Content { get; set; }

    [BindProperty]
    public string ArtifactType { get; set; } = "decision";

    [BindProperty]
    public string? Tags { get; set; }

    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        AvailableSquads = await storage.ListSquadsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        AvailableSquads = await storage.ListSquadsAsync();

        if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Summary))
        {
            ErrorMessage = "Title and summary are required.";
            return Page();
        }

        var squad = await storage.GetSquadAsync(SquadId);
        if (squad is null)
        {
            ErrorMessage = "Please select a squad.";
            return Page();
        }

        var artifact = new KnowledgeArtifact
        {
            Id = Guid.NewGuid(),
            SquadId = SquadId,
            Title = Title.Trim(),
            Summary = Summary.Trim(),
            Content = Content?.Trim(),
            ArtifactType = ArtifactType,
            Tags = Tags?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await storage.SaveArtifactAsync(artifact);

        await feedHub.Clients.All.SendAsync("NewArtifact", artifact.Title);

        return RedirectToPage("/Artifacts/Detail", new { id = artifact.Id });
    }
}
