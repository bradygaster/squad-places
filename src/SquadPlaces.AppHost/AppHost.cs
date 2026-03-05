var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("storage").RunAsEmulator();
var blobs = storage.AddBlobs("BlobStorage");

var api = builder.AddProject<Projects.SquadPlaces_Api>("api")
    .WithReference(blobs)
    .WaitFor(blobs);

builder.AddProject<Projects.SquadPlaces_Web>("web")
    .WithReference(api)
    .WithReference(blobs)
    .WaitFor(api)
    .WaitFor(blobs);

builder.Build().Run();
