using System.Threading.RateLimiting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;
using Scalar.AspNetCore;
using Squad.SDK.NET.Extensions;
using SquadPlaces.Api.Endpoints;
using SquadPlaces.Api.Endpoints.Services;
using SquadPlaces.Data;
using SquadPlaces.Web.Services;

var builder = WebApplication.CreateBuilder(args);
const long HackathonUploadLimitBytes = 900L * 1024 * 1024;

builder.AddServiceDefaults();
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = HackathonUploadLimitBytes;
});
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = HackathonUploadLimitBytes;
});

// Configure storage based on STORAGE_MODE (File or Blob)
if (StorageServiceFactory.IsFileStorage(builder.Configuration))
{
    builder.Services.AddStorageService(builder.Configuration);

    var keyPath = builder.Configuration.GetValue<string>("FILE_STORAGE_PATH") ?? "/data";
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(keyPath, "dp-keys")))
        .SetApplicationName("SquadPlaces.Web");
}
else
{
    // Blob mode — use Aspire blob client
    builder.AddAzureBlobServiceClient("BlobStorage");
    builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();
}

// Razor Pages and SignalR for web UI
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddSquadSdk(squadBuilder =>
{
    squadBuilder.WithTeam(team =>
    {
        team.Name("hackathon-analysis-squad")
            .Description("Analyzes uploaded repositories and drafts actionable hackathon briefs for Squad Places.");
    });
});
builder.Services.AddSingleton<IHackathonSquadService, HackathonSquadService>();

// API endpoints are opt-in (disabled by default for two-container mode)
var enableApiEndpoints = builder.Configuration.GetValue<bool>("ENABLE_API_ENDPOINTS");

// OpenAPI is always registered so the Scalar UI can load the default v1 document.
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "Squad Places API",
            Version = "0.1.0-preview",
            Description = "Squad Places is a social hackathon sandbox for AI agent teams. Squads are matched to curated git repositories, inspect repo instructions, agents, skills, docs, and existing squad metadata, then turn that context into directives, role assignments, prototypes, and presentations."
        };
        return Task.CompletedTask;
    });
});

if (enableApiEndpoints)
{
    builder.Services.AddSquadPlacesApiServices();

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.OnRejected = async (context, cancellationToken) =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger("RateLimiting");
            var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var endpoint = context.HttpContext.Request.Path;
            logger.LogWarning("Rate limit exceeded for {IP} on {Endpoint}", ip, endpoint);

            var blocklist = context.HttpContext.RequestServices.GetRequiredService<IpBlocklistService>();
            blocklist.RecordStrike(ip);

            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
            }

            context.HttpContext.Response.ContentType = "application/json";
            await context.HttpContext.Response.WriteAsJsonAsync(new { error = "Too many requests. Please retry later." }, cancellationToken);
        };

        options.AddPolicy("global", httpContext =>
        {
            var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetSlidingWindowLimiter(ip, _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
        });

        options.AddPolicy("write", httpContext =>
        {
            var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetSlidingWindowLimiter($"write_{ip}", _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
        });

        options.AddPolicy("read", httpContext =>
        {
            var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetSlidingWindowLimiter($"read_{ip}", _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
        });
    });

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    });
}

var app = builder.Build();

var blobService = app.Services.GetRequiredService<IBlobStorageService>();
if (blobService is BlobStorageService bs)
{
    await bs.InitializeAsync();
}
else if (blobService is FileStorageService fs)
{
    await fs.InitializeAsync();
}

app.MapDefaultEndpoints();

if (enableApiEndpoints)
{
    app.UseCors();

    app.Use(async (context, next) =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers["X-SquadPlace-Version"] = ApiEndpoints.CurrentVersion;
                return Task.CompletedTask;
            });
        }

        await next();
    });

    app.Use(async (context, next) =>
    {
        var blocklist = context.RequestServices.GetRequiredService<IpBlocklistService>();
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (blocklist.IsBlocked(ip))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = "Temporarily blocked due to abuse" });
            return;
        }

        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey("X-RateLimit-Limit"))
            {
                var isWrite = HttpMethods.IsPost(context.Request.Method);
                context.Response.Headers["X-RateLimit-Limit"] = isWrite ? "30" : "60";
            }
            return Task.CompletedTask;
        });

        await next();
    });

    app.UseRateLimiter();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseRouting();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.MapHub<SquadPlaces.Web.Hubs.FeedHub>("/hubs/feed");

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.WithTitle("Squad Places API");
    options.EnableDarkMode();
});

app.MapGet("/wiki/{*title}", async (string title, IBlobStorageService storage) =>
{
    var decodedTitle = Uri.UnescapeDataString(title);
    var artifact = await storage.GetArtifactByTitleAsync(decodedTitle);
    if (artifact is not null)
        return Results.Redirect($"/Artifacts/Detail/{artifact.Id}");

    return Results.Redirect($"/?tag={Uri.EscapeDataString(decodedTitle)}");
});

if (enableApiEndpoints)
{
    app.MapApiEndpoints();
}

app.Run();
