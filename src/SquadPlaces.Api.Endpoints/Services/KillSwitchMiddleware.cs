using System.Text.Json;

namespace SquadPlaces.Api.Endpoints.Services;

/// <summary>
/// Middleware that enforces kill switch state early in the request pipeline.
/// Checks: network read-only mode, squad suspensions, disabled endpoints.
/// GET requests always pass through — the network is always readable.
/// </summary>
public class KillSwitchMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<KillSwitchMiddleware> _logger;

    // HTTP methods that are considered "writes"
    private static readonly HashSet<string> WriteMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "POST", "PUT", "PATCH", "DELETE"
    };

    public KillSwitchMiddleware(RequestDelegate next, ILogger<KillSwitchMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only enforce on /api routes
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await _next(context);
            return;
        }

        // Admin endpoints always pass through — admins need to manage kill switches even when active
        if (context.Request.Path.StartsWithSegments("/api/admin"))
        {
            await _next(context);
            return;
        }

        var killSwitch = context.RequestServices.GetRequiredService<KillSwitchService>();
        var isWrite = WriteMethods.Contains(context.Request.Method);

        // Check disabled endpoints (applies to both reads and writes)
        if (killSwitch.IsEndpointDisabled(context.Request.Path.Value ?? ""))
        {
            _logger.LogWarning("Request blocked — endpoint disabled: {Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                error = "This endpoint is temporarily disabled.",
            });
            return;
        }

        // GET requests always pass through for read-only and squad suspension checks
        if (!isWrite)
        {
            await _next(context);
            return;
        }

        // Network-level: read-only mode blocks all writes
        if (killSwitch.IsReadOnly())
        {
            var readOnlyState = killSwitch.GetReadOnlyMode();
            _logger.LogWarning("Write blocked — network is in read-only mode. Path: {Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Network is in read-only mode.",
                reason = readOnlyState?.Reason ?? "Unknown"
            });
            return;
        }

        // Squad-level: check if the request's squad is suspended
        var squadId = await ExtractSquadIdAsync(context);
        if (squadId.HasValue && killSwitch.IsSquadSuspended(squadId.Value))
        {
            var suspension = killSwitch.GetSquadSuspension(squadId.Value);
            _logger.LogWarning("Write blocked — squad {SquadId} is suspended. Path: {Path}",
                squadId.Value, context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Squad is suspended.",
                reason = suspension?.Reason ?? "Unknown",
                expiresAt = suspension?.ExpiresAt?.ToString("O")
            });
            return;
        }

        await _next(context);
    }

    /// <summary>
    /// Extracts the squad ID from the request — checks route values, query string, and request body.
    /// Enables request body buffering so downstream handlers can still read it.
    /// </summary>
    private static async Task<Guid?> ExtractSquadIdAsync(HttpContext context)
    {
        // 1. Route parameter: /api/squads/{squadId}/members
        if (context.Request.RouteValues.TryGetValue("squadId", out var routeVal) &&
            Guid.TryParse(routeVal?.ToString(), out var routeGuid))
        {
            return routeGuid;
        }

        // 2. Query string
        if (context.Request.Query.TryGetValue("squadId", out var queryVal) &&
            Guid.TryParse(queryVal.ToString(), out var queryGuid))
        {
            return queryGuid;
        }

        // 3. Request body — enable buffering so downstream can still read
        if (context.Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true)
        {
            context.Request.EnableBuffering();
            try
            {
                var body = await JsonDocument.ParseAsync(context.Request.Body);
                context.Request.Body.Position = 0; // Reset for downstream

                if (body.RootElement.TryGetProperty("SquadId", out var squadIdProp) ||
                    body.RootElement.TryGetProperty("squadId", out squadIdProp))
                {
                    if (squadIdProp.ValueKind == JsonValueKind.String &&
                        Guid.TryParse(squadIdProp.GetString(), out var bodyGuid))
                    {
                        return bodyGuid;
                    }
                }
            }
            catch
            {
                // Body parse failed — not a JSON request or malformed. Let downstream handle.
                context.Request.Body.Position = 0;
            }
        }

        return null;
    }
}

/// <summary>
/// Extension method for registering KillSwitchMiddleware in the ASP.NET Core pipeline.
/// </summary>
public static class KillSwitchMiddlewareExtensions
{
    public static IApplicationBuilder UseKillSwitch(this IApplicationBuilder app)
    {
        return app.UseMiddleware<KillSwitchMiddleware>();
    }
}
