var builder = DistributedApplication.CreateBuilder(args);

var githubToken = builder.AddParameter(
    name: "github-token",
    valueGetter: () => builder.Configuration["Parameters:github-token"]
        ?? builder.Configuration["GH_TOKEN"]
        ?? builder.Configuration["GITHUB_TOKEN"]
        ?? Environment.GetEnvironmentVariable("GH_TOKEN")
        ?? Environment.GetEnvironmentVariable("GITHUB_TOKEN")
        ?? string.Empty,
    secret: true);

var adminKey = builder.AddParameter(
    name: "admin-key",
    valueGetter: () => builder.Configuration["Parameters:admin-key"]
        ?? builder.Configuration["ADMIN_KEY"]
        ?? Environment.GetEnvironmentVariable("ADMIN_KEY")
        ?? string.Empty,
    secret: true);

var storage = builder.AddAzureStorage("storage").RunAsEmulator(conf =>
{
    conf
    .WithApiVersionCheck(true);
    conf.WithAnnotation(new ContainerImageAnnotation
    {
        Registry = "mcr.microsoft.com",
        Image = "azure-storage/azurite",
        Tag = "3.35.0-arm64"
    });
});

var blobs = storage.AddBlobs("BlobStorage");
var useWebDockerfile = string.Equals(builder.Configuration["USE_WEB_DOCKERFILE"], "true", StringComparison.OrdinalIgnoreCase);

if (useWebDockerfile)
{
    var webContainer = builder.AddDockerfile("web", "..\\..\\", "src/SquadPlaces.Web/Dockerfile.single")
        .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "http")
        .WithExternalHttpEndpoints()
        .WithReference(blobs)
        .WithEnvironment("NODE_OPTIONS", "--no-warnings")
        .WithEnvironment("NODE_NO_WARNINGS", "1")
        .WithEnvironment("GH_TOKEN", githubToken)
        .WithEnvironment("GITHUB_TOKEN", githubToken)
        .WaitFor(blobs);
}
else
{
    builder.AddProject<Projects.SquadPlaces_Web>("web")
        .WithExternalHttpEndpoints()
        .WithReference(blobs)
        .WithEnvironment("NODE_OPTIONS", "--no-warnings")
        .WithEnvironment("NODE_NO_WARNINGS", "1")
        .WithEnvironment("GH_TOKEN", githubToken)
        .WithEnvironment("GITHUB_TOKEN", githubToken)
        .WaitFor(blobs);
}

builder.Build().Run();

