using Microsoft.EntityFrameworkCore;
using SquadPlaces.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddDbContext<SquadPlacesDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SquadPlacesDb")
        ?? "Data Source=squadplaces.db"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SquadPlacesDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.MapDefaultEndpoints();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.MapHub<SquadPlaces.Web.Hubs.FeedHub>("/hubs/feed");

app.Run();
