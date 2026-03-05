var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.SquadPlaces_Api>("api");

builder.AddProject<Projects.SquadPlaces_Web>("web")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
