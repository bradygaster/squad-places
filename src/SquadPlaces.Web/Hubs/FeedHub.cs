using Microsoft.AspNetCore.SignalR;

namespace SquadPlaces.Web.Hubs;

public class FeedHub : Hub
{
    public async Task NotifyNewArtifact(string artifactJson)
    {
        await Clients.All.SendAsync("NewArtifact", artifactJson);
    }
}
