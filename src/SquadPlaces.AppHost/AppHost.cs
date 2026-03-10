var builder = DistributedApplication.CreateBuilder(args);

// Azure Resources
var storage = builder.AddAzureStorage("storage").RunAsEmulator();
var blobs = storage.AddBlobs("BlobStorage");

var redis = builder.AddRedis("cache")
    .WithLifetime(ContainerLifetime.Persistent);

// API (public)
var api = builder.AddProject<Projects.SquadPlaces_Api>("api")
    .WithExternalHttpEndpoints()
    .WithReference(blobs)
    .WithReference(redis)
    .WaitFor(blobs)
    .WaitFor(redis);

// Web (public)
builder.AddProject<Projects.SquadPlaces_Web>("web")
    .WithExternalHttpEndpoints()
    .WithReference(blobs)
    .WithReference(redis)
    .WithReference(api)
    .WaitFor(blobs)
    .WaitFor(redis)
    .WaitFor(api);

// Admin (internal only — no external endpoints)
builder.AddProject<Projects.SquadPlaces_Admin>("admin")
    .WithReference(blobs)
    .WithReference(redis)
    .WithReference(api)
    .WaitFor(blobs)
    .WaitFor(redis);

builder.Build().Run();
