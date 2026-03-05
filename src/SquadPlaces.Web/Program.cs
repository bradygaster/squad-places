using Azure.Storage.Blobs;
using SquadPlaces.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddSingleton(sp =>
    new BlobServiceClient(builder.Configuration.GetConnectionString("BlobStorage")));
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

app.Run();
