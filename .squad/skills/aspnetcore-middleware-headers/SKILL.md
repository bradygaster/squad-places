---
name: "aspnetcore-middleware-headers"
description: "How to safely set response headers in ASP.NET Core middleware"
domain: "aspnetcore, middleware, http"
confidence: "high"
source: "earned (bug fix 2026-02-24)"
---

## Context

When writing custom ASP.NET Core middleware that needs to set response headers, the timing of when you set headers matters. Kestrel begins streaming response bodies once the initial buffer (~16KB) is filled, at which point response headers become read-only.

## Patterns

### ✅ Correct: Use `OnStarting` callback

Set response headers using the `context.Response.OnStarting()` callback **before** calling `await next()`:

```csharp
app.Use(async (context, next) =>
{
    // Register header-setting logic before response starts
    context.Response.OnStarting(() =>
    {
        if (!context.Response.Headers.ContainsKey("X-Custom-Header"))
        {
            context.Response.Headers["X-Custom-Header"] = "value";
        }
        return Task.CompletedTask;
    });

    await next();
});
```

**Why this works:** The `OnStarting` callback is guaranteed to execute before the first byte of the response body is written, regardless of response size or buffering behavior.

### ✅ Alternative: Check `HasStarted` flag

If you must set headers after `next()`, check `Response.HasStarted` first:

```csharp
app.Use(async (context, next) =>
{
    await next();

    if (!context.Response.HasStarted && !context.Response.Headers.ContainsKey("X-Custom-Header"))
    {
        context.Response.Headers["X-Custom-Header"] = "value";
    }
});
```

**Caveat:** This approach means headers won't be present on responses that started streaming early. Use `OnStarting` if you need guaranteed header presence.

## Anti-Patterns

### ❌ Setting headers after `next()` without checking

```csharp
app.Use(async (context, next) =>
{
    await next();

    // ❌ CRASHES if response body > initial buffer size
    context.Response.Headers["X-Custom-Header"] = "value";
});
```

**Why this fails:**
- Kestrel starts streaming once the response buffer is full (~16KB)
- Once streaming starts, `Response.HasStarted` is `true` and headers are read-only
- Attempting to set headers throws `InvalidOperationException: Headers are read-only, response has already started`

**Symptoms:**
- Small responses work (fit in buffer)
- Large responses crash (OpenAPI docs, HTML pages, large JSON)
- Intermittent failures depending on response size

## Examples

Real-world example from SquadPlaces (2026-02-24):

**Before (broken):**
```csharp
app.Use(async (context, next) =>
{
    var blocklist = context.RequestServices.GetRequiredService<IpBlocklistService>();
    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    if (blocklist.IsBlocked(ip))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(new { error = "Blocked" });
        return;
    }

    await next();

    // ❌ Crashes on large responses
    if (!context.Response.Headers.ContainsKey("X-RateLimit-Limit"))
    {
        var isWrite = HttpMethods.IsPost(context.Request.Method);
        context.Response.Headers["X-RateLimit-Limit"] = isWrite ? "30" : "60";
    }
});
```

**After (fixed):**
```csharp
app.Use(async (context, next) =>
{
    var blocklist = context.RequestServices.GetRequiredService<IpBlocklistService>();
    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    if (blocklist.IsBlocked(ip))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(new { error = "Blocked" });
        return;
    }

    // ✅ Headers set before response starts
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
```

## References

- ASP.NET Core Middleware: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/
- HttpResponse.OnStarting: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpresponse.onstarting
