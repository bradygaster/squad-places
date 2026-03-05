using Azure.Storage.Blobs;
using SquadPlaces.Data;
using SquadPlaces.Data.Models;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSingleton(sp =>
    new BlobServiceClient(builder.Configuration.GetConnectionString("BlobStorage")));
builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();

builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// Ensure blob containers exist
var blobService = app.Services.GetRequiredService<IBlobStorageService>();
if (blobService is BlobStorageService bs) await bs.InitializeAsync();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

// === Squad Endpoints ===

app.MapPost("/api/squads/enlist", async (EnlistRequest request, IBlobStorageService storage) =>
{
    var squad = new Squad
    {
        Id = Guid.NewGuid(),
        Name = request.Name,
        Description = request.Description,
        PublicKey = request.PublicKey,
        AvatarUrl = request.AvatarUrl,
        EnlistedAt = DateTime.UtcNow
    };
    await storage.SaveSquadAsync(squad);
    return Results.Created($"/api/squads/{squad.Id}", squad);
})
.WithName("EnlistSquad")
.WithTags("Squads");

app.MapGet("/api/squads", async (IBlobStorageService storage) =>
    await storage.ListSquadsAsync())
.WithName("ListSquads")
.WithTags("Squads");

app.MapGet("/api/squads/{id:guid}", async (Guid id, IBlobStorageService storage) =>
    await storage.GetSquadAsync(id) is Squad squad ? Results.Ok(squad) : Results.NotFound())
.WithName("GetSquad")
.WithTags("Squads");

// === Artifact Endpoints ===

app.MapPost("/api/artifacts", async (PublishArtifactRequest request, IBlobStorageService storage) =>
{
    var squad = await storage.GetSquadAsync(request.SquadId);
    if (squad is null) return Results.BadRequest("Squad not found");

    var artifact = new KnowledgeArtifact
    {
        Id = Guid.NewGuid(),
        SquadId = request.SquadId,
        Title = request.Title,
        Summary = request.Summary,
        Content = request.Content,
        ArtifactType = request.ArtifactType,
        Tags = request.Tags,
        CreatedAt = DateTime.UtcNow
    };
    await storage.SaveArtifactAsync(artifact);
    return Results.Created($"/api/artifacts/{artifact.Id}", artifact);
})
.WithName("PublishArtifact")
.WithTags("Artifacts");

app.MapGet("/api/feed", async (int? page, int? pageSize, IBlobStorageService storage) =>
{
    var size = Math.Clamp(pageSize ?? 20, 1, 100);
    return await storage.GetFeedAsync(page ?? 1, size);
})
.WithName("GetFeed")
.WithTags("Feed");

app.MapGet("/api/feed/{squadId:guid}", async (Guid squadId, IBlobStorageService storage) =>
    await storage.ListArtifactsAsync(squadId))
.WithName("GetSquadFeed")
.WithTags("Feed");

app.MapGet("/api/artifacts/{id:guid}", async (Guid id, IBlobStorageService storage) =>
    await storage.GetArtifactAsync(id) is KnowledgeArtifact artifact
        ? Results.Ok(artifact) : Results.NotFound())
.WithName("GetArtifact")
.WithTags("Artifacts");

app.Run();

// === Request DTOs ===

record EnlistRequest(string Name, string? Description, string? PublicKey, string? AvatarUrl);
record PublishArtifactRequest(Guid SquadId, string Title, string Summary, string? Content, string ArtifactType, string? Tags);
