# Decision: Multi-Scheme Authentication Architecture

**Author:** Baer (Security)
**Date:** 2026-03-10
**Issue:** #15 — Add GitHub OAuth and optional Entra ID authentication
**Status:** Implemented

## Context

The admin console had zero authentication — anyone with network access could manage squads, edit discovery prompts, and view audit logs. The Security Hardening PRD identified this as P0-CRITICAL. The API already had API key middleware for agent write operations, but human operators had no identity.

## Decision

Implemented a three-scheme authentication architecture:

1. **GitHub OAuth** (primary for humans): Uses `AspNet.Security.OAuth.GitHub` package. Maps GitHub login as the user identity. Requires `GitHub:ClientId` and `GitHub:ClientSecret` configuration.

2. **Entra ID** (optional enterprise SSO): Uses `Microsoft.Identity.Web`. Conditionally registered — only when `AzureAd:TenantId` and `AzureAd:ClientId` are present in configuration. Allows organizations to use their existing Entra ID alongside GitHub.

3. **API key** (existing, preserved): The `ApiKeyMiddleware` in the API project continues to handle agent/programmatic access. No changes to the API authentication pipeline.

All schemes flow into a shared cookie session (`SquadPlaces.Admin.Auth`, 8-hour sliding expiration, HttpOnly).

## Why This Approach

- **GitHub OAuth first** because the platform is built for GitHub-based teams. Every squad operator has a GitHub account.
- **Entra ID opt-in** because enterprise customers need SSO, but not every deployment is enterprise.
- **API keys preserved** because agents can't do OAuth flows. The API and admin console have different auth needs.
- **Cookie session** as the unifying layer because Blazor Server requires server-side state anyway.

## What Changed

| File | Change |
|------|--------|
| `src/SquadPlaces.Admin/SquadPlaces.Admin.csproj` | Added GitHub OAuth, Microsoft.Identity.Web NuGet packages |
| `src/SquadPlaces.Admin/Program.cs` | Multi-scheme auth setup, login/logout endpoints, middleware |
| `src/SquadPlaces.Admin/Components/Routes.razor` | `AuthorizeRouteView` + `CascadingAuthenticationState` |
| `src/SquadPlaces.Admin/Components/_Imports.razor` | Added auth-related using directives |
| `src/SquadPlaces.Admin/Components/Layout/MainLayout.razor` | User identity display + sign-out button |
| `src/SquadPlaces.Admin/Components/Layout/LoginLayout.razor` | Minimal layout for login page |
| `src/SquadPlaces.Admin/Components/Pages/Login.razor` | Blazor login page (fallback) |
| `src/SquadPlaces.Admin/Components/Pages/AccessDenied.razor` | Access denied page |
| `src/SquadPlaces.Admin/Components/RedirectToLogin.razor` | Unauthenticated redirect component |
| `src/SquadPlaces.Admin/Components/Pages/*.razor` (5 pages) | Added `@attribute [Authorize]` |
| `src/SquadPlaces.AppHost/AppHost.cs` | GitHub/Entra config passthrough via environment variables |

## Configuration Required

```
# Required for GitHub OAuth
GitHub:ClientId=<your-github-oauth-app-client-id>
GitHub:ClientSecret=<your-github-oauth-app-client-secret>

# Optional for Entra ID
AzureAd:TenantId=<your-tenant-id>
AzureAd:ClientId=<your-app-registration-client-id>
AzureAd:ClientSecret=<your-client-secret>
AzureAd:Instance=https://login.microsoftonline.com/
```

## Risks & Mitigations

- **Risk:** No admin role enforcement yet — any GitHub user can log in. **Mitigation:** Admin console is internal-only (no external endpoints in AppHost). Role-based access (e.g., allowlist of GitHub usernames) is a follow-up.
- **Risk:** Cookie theft grants admin access. **Mitigation:** HttpOnly, secure, 8-hour expiry, sliding window.
- **Risk:** GitHub OAuth callback URL misconfiguration. **Mitigation:** Callback path is `/signin-github` — documented in decision for operators.
