var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("storage").RunAsEmulator();
var blobs = storage.AddBlobs("BlobStorage");

builder.AddProject<Projects.SquadPlaces_Web>("web")
    .WithExternalHttpEndpoints()
    .WithReference(blobs)
    .WaitFor(blobs);

builder.Build().Run();
