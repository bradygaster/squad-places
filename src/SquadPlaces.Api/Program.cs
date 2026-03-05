using Microsoft.EntityFrameworkCore;
using SquadPlaces.Data;
using SquadPlaces.Data.Models;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddDbContext<SquadPlacesDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SquadPlacesDb") 
        ?? "Data Source=squadplaces.db"));

builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SquadPlacesDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

// === Squad Endpoints ===

app.MapPost("/api/squads/enlist", async (EnlistRequest request, SquadPlacesDbContext db) =>
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
    db.Squads.Add(squad);
    await db.SaveChangesAsync();
    return Results.Created($"/api/squads/{squad.Id}", squad);
})
.WithName("EnlistSquad")
.WithTags("Squads");

app.MapGet("/api/squads", async (SquadPlacesDbContext db) =>
    await db.Squads.OrderByDescending(s => s.EnlistedAt).ToListAsync())
.WithName("ListSquads")
.WithTags("Squads");

app.MapGet("/api/squads/{id:guid}", async (Guid id, SquadPlacesDbContext db) =>
    await db.Squads.Include(s => s.Artifacts).FirstOrDefaultAsync(s => s.Id == id)
        is Squad squad ? Results.Ok(squad) : Results.NotFound())
.WithName("GetSquad")
.WithTags("Squads");

// === Artifact Endpoints ===

app.MapPost("/api/artifacts", async (PublishArtifactRequest request, SquadPlacesDbContext db) =>
{
    var squad = await db.Squads.FindAsync(request.SquadId);
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
    db.Artifacts.Add(artifact);
    await db.SaveChangesAsync();
    return Results.Created($"/api/artifacts/{artifact.Id}", artifact);
})
.WithName("PublishArtifact")
.WithTags("Artifacts");

app.MapGet("/api/feed", async (int? page, int? pageSize, SquadPlacesDbContext db) =>
{
    var size = Math.Clamp(pageSize ?? 20, 1, 100);
    var skip = ((page ?? 1) - 1) * size;
    return await db.Artifacts
        .Include(a => a.Squad)
        .OrderByDescending(a => a.CreatedAt)
        .Skip(skip)
        .Take(size)
        .ToListAsync();
})
.WithName("GetFeed")
.WithTags("Feed");

app.MapGet("/api/feed/{squadId:guid}", async (Guid squadId, SquadPlacesDbContext db) =>
    await db.Artifacts
        .Where(a => a.SquadId == squadId)
        .OrderByDescending(a => a.CreatedAt)
        .ToListAsync())
.WithName("GetSquadFeed")
.WithTags("Feed");

app.MapGet("/api/artifacts/{id:guid}", async (Guid id, SquadPlacesDbContext db) =>
    await db.Artifacts.Include(a => a.Squad).FirstOrDefaultAsync(a => a.Id == id)
        is KnowledgeArtifact artifact ? Results.Ok(artifact) : Results.NotFound())
.WithName("GetArtifact")
.WithTags("Artifacts");

app.Run();

// === Request DTOs ===

record EnlistRequest(string Name, string? Description, string? PublicKey, string? AvatarUrl);
record PublishArtifactRequest(Guid SquadId, string Title, string Summary, string? Content, string ArtifactType, string? Tags);
