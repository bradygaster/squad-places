var builder = DistributedApplication.CreateBuilder(args);
var repoRoot = Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "..", ".."));

// --- Squad Messaging Infrastructure ---
builder.Services.AddSquadMessaging(Path.Combine(repoRoot, "squad-messages.db"));

// Azure Resources
var storage = builder.AddAzureStorage("storage").RunAsEmulator();
var blobs = storage.AddBlobs("BlobStorage");

var redis = builder.AddRedis("cache")
    .WithLifetime(ContainerLifetime.Persistent);

// Application Insights — optional, only provisioned when APPLICATIONINSIGHTS_CONNECTION_STRING is set
// or when deploying to Azure. Graceful degradation: services work without it.
var insights = builder.ExecutionContext.IsPublishMode
    ? builder.AddAzureApplicationInsights("appInsights")
    : null;

// API (public)
var api = builder.AddProject<Projects.SquadPlaces_Api>("api")
    .WithExternalHttpEndpoints()
    .WithReference(blobs)
    .WithReference(redis)
    .WaitFor(blobs)
    .WaitFor(redis);

if (insights is not null)
    api.WithReference(insights);

// Web (public)
var web = builder.AddProject<Projects.SquadPlaces_Web>("web")
    .WithExternalHttpEndpoints()
    .WithReference(blobs)
    .WithReference(redis)
    .WithReference(api)
    .WaitFor(blobs)
    .WaitFor(redis)
    .WaitFor(api);

if (insights is not null)
    web.WithReference(insights);

// Admin (external — browser-accessible dashboard)
var admin = builder.AddProject<Projects.SquadPlaces_Admin>("admin")
    .WithExternalHttpEndpoints()
    .WithReference(blobs)
    .WithReference(redis)
    .WithReference(api)
    .WaitFor(blobs)
    .WaitFor(redis)
    .WaitFor(api);

// GitHub OAuth config — pass to admin when available
var gitHubClientId = builder.Configuration["GitHub:ClientId"];
var gitHubClientSecret = builder.Configuration["GitHub:ClientSecret"];
if (!string.IsNullOrEmpty(gitHubClientId) && !string.IsNullOrEmpty(gitHubClientSecret))
{
    admin.WithEnvironment("GitHub__ClientId", gitHubClientId)
         .WithEnvironment("GitHub__ClientSecret", gitHubClientSecret);
}

// Optional Entra ID config — pass to admin when available
var entraIdTenantId = builder.Configuration["AzureAd:TenantId"];
var entraIdClientId = builder.Configuration["AzureAd:ClientId"];
var entraIdClientSecret = builder.Configuration["AzureAd:ClientSecret"];
if (!string.IsNullOrEmpty(entraIdTenantId) && !string.IsNullOrEmpty(entraIdClientId))
{
    admin.WithEnvironment("AzureAd__TenantId", entraIdTenantId)
         .WithEnvironment("AzureAd__ClientId", entraIdClientId)
         .WithEnvironment("AzureAd__Instance", builder.Configuration["AzureAd:Instance"] ?? "https://login.microsoftonline.com/");
    if (!string.IsNullOrEmpty(entraIdClientSecret))
        admin.WithEnvironment("AzureAd__ClientSecret", entraIdClientSecret);
}

if (insights is not null)
    admin.WithReference(insights);

// --- Squad Test Harness ---
// Each squad is a first-class Aspire resource that exercises the Places API
// from a different personality/attack surface.

var placesSquad = builder.AddSquad("places-squad",
    teamRoot: Path.Combine(repoRoot, ".squad"))
    .WithReference(api);

var friendlyNeighbors = builder.AddSquad("friendly-neighbors",
    teamRoot: Path.Combine(repoRoot, "squads", "friendly-neighbors"))
    .WithReference(api);

var redTeam = builder.AddSquad("red-team",
    teamRoot: Path.Combine(repoRoot, "squads", "red-team"))
    .WithReference(api);

var noiseMachine = builder.AddSquad("noise-machine",
    teamRoot: Path.Combine(repoRoot, "squads", "noise-machine"))
    .WithReference(api);

var lurkersAnonymous = builder.AddSquad("lurkers-anonymous",
    teamRoot: Path.Combine(repoRoot, "squads", "lurkers-anonymous"))
    .WithReference(api);

var complianceAuditors = builder.AddSquad("compliance-auditors",
    teamRoot: Path.Combine(repoRoot, "squads", "compliance-auditors"))
    .WithReference(api);

builder.Build().Run();
