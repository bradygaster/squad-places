using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SquadPlaces.Api.Endpoints.Services;

/// <summary>
/// Middleware that enforces API key authentication on write endpoints (POST, PUT, DELETE).
/// Read endpoints (GET) remain open — the network is readable by anyone.
/// In Development environment, a well-known dev key bypasses validation.
/// </summary>
public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyMiddleware> _logger;
    private readonly bool _requireApiKey;
    private readonly bool _isDevelopment;

    private const string ApiKeyHeader = "X-Squad-Api-Key";
    private const string DevBypassKey = "sqp_dev_key_do_not_use_in_production";

    private static readonly HashSet<string> WriteMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "POST", "PUT", "DELETE"
    };

    public ApiKeyMiddleware(
        RequestDelegate next,
        ILogger<ApiKeyMiddleware> logger,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _isDevelopment = environment.IsDevelopment();

        // Default: require API keys in production, skip in development
        var configValue = configuration.GetValue<bool?>("Authentication:RequireApiKey");
        _requireApiKey = configValue ?? !_isDevelopment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only enforce on /api paths
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await _next(context);
            return;
        }

        // Read endpoints are always open
        if (!WriteMethods.Contains(context.Request.Method))
        {
            await _next(context);
            return;
        }

        // Key generation endpoints must remain open (chicken-and-egg)
        if (IsKeyManagementEndpoint(context.Request.Path, context.Request.Method))
        {
            await _next(context);
            return;
        }

        // If API key requirement is disabled, pass through
        if (!_requireApiKey)
        {
            await _next(context);
            return;
        }

        var apiKey = context.Request.Headers[ApiKeyHeader].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                error = "API key required for write operations. Include X-Squad-Api-Key header."
            });
            return;
        }

        // Dev bypass — ONLY in Development environment
        if (_isDevelopment && apiKey == DevBypassKey)
        {
            // Dev key doesn't associate with any specific squad
            await _next(context);
            return;
        }

        var apiKeyService = context.RequestServices.GetRequiredService<ApiKeyService>();
        var squadId = await apiKeyService.ValidateKeyAsync(apiKey);

        if (squadId is null)
        {
            _logger.LogWarning("Invalid API key attempt from {IP}",
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown");
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Invalid API key."
            });
            return;
        }

        // Store authenticated squad ID for downstream handlers
        context.Items["SquadId"] = squadId.Value;
        await _next(context);
    }

    /// <summary>
    /// Key generation and enlistment must remain unauthenticated (bootstrap).
    /// POST /api/squads/{id}/keys — generate key
    /// POST /api/squads/enlist — enlist squad (returns first key)
    /// </summary>
    private static bool IsKeyManagementEndpoint(PathString path, string method)
    {
        var pathValue = path.Value ?? string.Empty;

        // POST /api/squads/enlist — bootstrap, generates first key
        if (method.Equals("POST", StringComparison.OrdinalIgnoreCase)
            && pathValue.Equals("/api/squads/enlist", StringComparison.OrdinalIgnoreCase))
            return true;

        // POST /api/squads/{id}/keys — generate a new key
        if (method.Equals("POST", StringComparison.OrdinalIgnoreCase)
            && pathValue.EndsWith("/keys", StringComparison.OrdinalIgnoreCase)
            && pathValue.StartsWith("/api/squads/", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }
}

/// <summary>
/// Extension method to register the API key middleware in the pipeline.
/// </summary>
public static class ApiKeyMiddlewareExtensions
{
    public static IApplicationBuilder UseApiKeyAuthentication(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ApiKeyMiddleware>();
    }
}
