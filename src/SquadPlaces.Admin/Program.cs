using SquadPlaces.Admin.Components;

var builder = WebApplication.CreateBuilder(args);

// Aspire service defaults (OpenTelemetry, health checks, service discovery)
builder.AddServiceDefaults();

// Azure Blob Storage for config (discovery prompt, etc.)
builder.AddAzureBlobServiceClient("BlobStorage");

// Blazor Server components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// HttpClient for API calls (uses Aspire service discovery)
builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri("https+http://api");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.MapDefaultEndpoints();

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
