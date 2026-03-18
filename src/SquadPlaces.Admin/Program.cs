using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using SquadPlaces.Admin.Components;

var builder = WebApplication.CreateBuilder(args);

// Aspire service defaults (OpenTelemetry, health checks, service discovery)
builder.AddServiceDefaults();

// Azure Blob Storage for config (discovery prompt, etc.)
builder.AddAzureBlobServiceClient("BlobStorage");

// --- Authentication: multi-scheme (GitHub OAuth + optional Entra ID) ---
var authBuilder = builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/access-denied";
        options.Cookie.Name = "SquadPlaces.Admin.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// GitHub OAuth — primary auth for human operators
var gitHubClientId = builder.Configuration["GitHub:ClientId"];
var gitHubClientSecret = builder.Configuration["GitHub:ClientSecret"];

if (!string.IsNullOrEmpty(gitHubClientId) && !string.IsNullOrEmpty(gitHubClientSecret))
{
    authBuilder.AddGitHub(options =>
    {
        options.ClientId = gitHubClientId;
        options.ClientSecret = gitHubClientSecret;
        options.CallbackPath = "/signin-github";
        options.Scope.Add("read:user");
        options.Scope.Add("user:email");
        options.SaveTokens = true;

        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");
        options.ClaimActions.MapJsonKey("urn:github:avatar", "avatar_url");
        options.ClaimActions.MapJsonKey("urn:github:name", "name");
    });
}

// Entra ID — optional enterprise SSO, only when configured
var entraIdTenantId = builder.Configuration["AzureAd:TenantId"];
var entraIdClientId = builder.Configuration["AzureAd:ClientId"];

if (!string.IsNullOrEmpty(entraIdTenantId) && !string.IsNullOrEmpty(entraIdClientId))
{
    authBuilder.AddMicrosoftIdentityWebApp(
        builder.Configuration.GetSection("AzureAd"),
        OpenIdConnectDefaults.AuthenticationScheme);
}

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

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

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// --- Login endpoints ---

// GitHub OAuth login
app.MapGet("/login/github", (HttpContext context) =>
{
    var properties = new AuthenticationProperties { RedirectUri = "/" };
    return Results.Challenge(properties, ["GitHub"]);
}).AllowAnonymous();

// Entra ID login (only available when configured)
if (!string.IsNullOrEmpty(entraIdTenantId) && !string.IsNullOrEmpty(entraIdClientId))
{
    app.MapGet("/login/entra", (HttpContext context) =>
    {
        var properties = new AuthenticationProperties { RedirectUri = "/" };
        return Results.Challenge(properties, [OpenIdConnectDefaults.AuthenticationScheme]);
    }).AllowAnonymous();
}

// Login page — redirect to Blazor login page
app.MapGet("/login", () =>
{
    return Results.Content(LoginPageHtml(), "text/html");
}).AllowAnonymous();

// Logout
app.MapPost("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
}).AllowAnonymous();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

// Minimal login page served outside Blazor (can't use Blazor components pre-auth)
static string LoginPageHtml()
{
    return """
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1.0" />
            <title>Sign In — Squad Places Admin</title>
            <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" />
        </head>
        <body class="bg-light">
            <div class="d-flex justify-content-center align-items-center" style="min-height:100vh">
                <div class="card shadow-sm" style="max-width:420px;width:100%">
                    <div class="card-body p-4 text-center">
                        <h3 class="mb-3">🏠 Squad Places Admin</h3>
                        <p class="text-muted mb-4">Sign in to access the admin console.</p>
                        <a href="/login/github" class="btn btn-dark btn-lg w-100 mb-3">
                            Sign in with GitHub
                        </a>
                        <hr class="my-4" />
                        <small class="text-muted">Authentication is required to access admin functions.</small>
                    </div>
                </div>
            </div>
        </body>
        </html>
        """;
}
