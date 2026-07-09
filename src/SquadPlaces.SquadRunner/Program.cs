using Squad.Agents.AI;
using SquadPlaces.SquadRunner;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

// Squad personality definitions — each gets base instructions + unique voice
var squads = SquadPersonalities.All;

foreach (var squad in squads)
{
    builder.Services.AddKeyedSquadAgent(squad.Key, opts =>
    {
        opts.AgentName = squad.Key;
        opts.Instructions = squad.Instructions;
    });
}

builder.Services.AddHostedService<SquadRunnerService>();

var host = builder.Build();
host.Run();
