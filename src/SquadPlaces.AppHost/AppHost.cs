var builder = DistributedApplication.CreateBuilder(args);
var repoRoot = Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "..", ".."));

// Azure Resources
var storage = builder.AddAzureStorage("storage").RunAsEmulator();
var blobs = storage.AddBlobs("BlobStorage");

var redis = builder.AddRedis("cache")
    .WithLifetime(ContainerLifetime.Persistent);

// Application Insights — optional, only provisioned when deploying to Azure
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
// Each squad is a first-class Aspire resource visible in the dashboard.
// The SquadRunner worker service handles waking them up and prompting engagement.

var placesSquad = builder.AddSquad("places-squad",
    teamRoot: Path.Combine(repoRoot, ".squad"));

var friendlyNeighbors = builder.AddSquad("friendly-neighbors",
    teamRoot: Path.Combine(repoRoot, "squads", "friendly-neighbors"));

var redTeam = builder.AddSquad("red-team",
    teamRoot: Path.Combine(repoRoot, "squads", "red-team"));

var noiseMachine = builder.AddSquad("noise-machine",
    teamRoot: Path.Combine(repoRoot, "squads", "noise-machine"));

var lurkersAnonymous = builder.AddSquad("lurkers-anonymous",
    teamRoot: Path.Combine(repoRoot, "squads", "lurkers-anonymous"));

var complianceAuditors = builder.AddSquad("compliance-auditors",
    teamRoot: Path.Combine(repoRoot, "squads", "compliance-auditors"));

// Squad Runner — background worker that prompts each squad to participate.
// It receives squad connection strings (via WithReference) and sends periodic
// engagement prompts so squads stay active on the network.
builder.AddProject<Projects.SquadPlaces_SquadRunner>("squad-runner")
    .WithReference(api)
    .WithReference(friendlyNeighbors)
    .WithReference(redTeam)
    .WithReference(noiseMachine)
    .WithReference(lurkersAnonymous)
    .WithReference(complianceAuditors)
    .WaitFor(api);

builder.Build().Run();
