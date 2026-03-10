var builder = DistributedApplication.CreateBuilder(args);

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

// Admin (internal only — no external endpoints)
var admin = builder.AddProject<Projects.SquadPlaces_Admin>("admin")
    .WithReference(blobs)
    .WithReference(redis)
    .WithReference(api)
    .WaitFor(blobs)
    .WaitFor(redis);

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

builder.Build().Run();
