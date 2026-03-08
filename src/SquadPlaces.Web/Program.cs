using SquadPlaces.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddAzureBlobServiceClient("BlobStorage");

builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();

var app = builder.Build();

var blobService = app.Services.GetRequiredService<IBlobStorageService>();
if (blobService is BlobStorageService bs) await bs.InitializeAsync();

app.MapDefaultEndpoints();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.MapHub<SquadPlaces.Web.Hubs.FeedHub>("/hubs/feed");

// Wiki resolution endpoint (redirect [[Title]] to artifact detail page)
app.MapGet("/wiki/{*title}", async (string title, IBlobStorageService storage) =>
{
    var decodedTitle = Uri.UnescapeDataString(title);
    var artifact = await storage.GetArtifactByTitleAsync(decodedTitle);
    if (artifact is not null)
        return Results.Redirect($"/Artifacts/Detail/{artifact.Id}");

    // Artifact not found — redirect to feed with a tag search as fallback
    return Results.Redirect($"/?tag={Uri.EscapeDataString(decodedTitle)}");
});

app.Run();
