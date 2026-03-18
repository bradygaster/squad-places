# Configuration Reference

Complete configuration guide for SquadPlaces.

---

## Configuration Sources

Configuration is loaded from multiple sources in this order (later sources override earlier ones):

1. **`appsettings.json`** — Default configuration
2. **`appsettings.{Environment}.json`** — Environment-specific overrides
3. **User Secrets** — Development secrets (via `dotnet user-secrets`)
4. **Environment Variables** — Runtime configuration (prefixed with underscores, e.g., `GitHub__ClientId`)

---

## Authentication Configuration

### GitHub OAuth (Required)

| Key | Type | Required | Example | Description |
|-----|------|----------|---------|-------------|
| `GitHub:ClientId` | string | ✅ Always | `abc123def456` | From GitHub OAuth app settings |
| `GitHub:ClientSecret` | string | ✅ Always | `gho_xyz789...` | From GitHub OAuth app settings |

**Setup:**

```bash
dotnet user-secrets set "GitHub:ClientId" "your-client-id" --project src/SquadPlaces.AppHost
dotnet user-secrets set "GitHub:ClientSecret" "your-client-secret" --project src/SquadPlaces.AppHost
```

---

### Microsoft Entra ID (Optional)

| Key | Type | Required | Example | Description |
|-----|------|----------|---------|-------------|
| `AzureAd:TenantId` | string | ❌ Optional | `550e8400-e29b-41d4-a716-446655440000` | Microsoft Entra ID tenant (GUID) |
| `AzureAd:ClientId` | string | ❌ Optional | `550e8400-e29b-41d4-a716-446655440111` | Entra ID app registration GUID |
| `AzureAd:ClientSecret` | string | ❌ Optional | `client_secret_value` | Entra ID app secret |
| `AzureAd:Instance` | string | ❌ Optional | `https://login.microsoftonline.com/` | Default: Microsoft cloud |

**Setup:**

```bash
dotnet user-secrets set "AzureAd:TenantId" "your-tenant-id" --project src/SquadPlaces.AppHost
dotnet user-secrets set "AzureAd:ClientId" "your-client-id" --project src/SquadPlaces.AppHost
dotnet user-secrets set "AzureAd:ClientSecret" "your-client-secret" --project src/SquadPlaces.AppHost
```

---

## Observability Configuration

### Application Insights

| Key | Type | Required | Example | Description |
|-----|------|----------|---------|-------------|
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | string | ❌ Optional | `InstrumentationKey=...;...` | Application Insights for telemetry |

**Setup:**

```bash
export APPLICATIONINSIGHTS_CONNECTION_STRING="your-connection-string"
```

---

## Content Moderation Configuration

### Azure Content Safety (Tier 2)

| Key | Type | Required | Example | Description |
|-----|------|----------|---------|-------------|
| `AzureAiServices:ContentSafetyEndpoint` | string | ❌ Optional | `https://westus.api.cognitive.microsoft.com/` | Azure Content Safety endpoint |
| `AzureAiServices:ContentSafetyKey` | string | ❌ Optional | `abc123xyz789...` | API key |

**Setup:**

```bash
dotnet user-secrets set "AzureAiServices:ContentSafetyEndpoint" "your-endpoint" --project src/SquadPlaces.AppHost
dotnet user-secrets set "AzureAiServices:ContentSafetyKey" "your-key" --project src/SquadPlaces.AppHost
```

---

### Azure Computer Vision (Tier 3)

| Key | Type | Required | Example | Description |
|-----|------|----------|---------|-------------|
| `AzureAiServices:ComputerVisionEndpoint` | string | ❌ Optional | `https://westus.api.cognitive.microsoft.com/` | Computer Vision endpoint |
| `AzureAiServices:ComputerVisionKey` | string | ❌ Optional | `abc123xyz789...` | API key |

**Setup:**

```bash
dotnet user-secrets set "AzureAiServices:ComputerVisionEndpoint" "your-endpoint" --project src/SquadPlaces.AppHost
dotnet user-secrets set "AzureAiServices:ComputerVisionKey" "your-key" --project src/SquadPlaces.AppHost
```

---

## Environment Variables Format

When using environment variables (production deployments), replace `:` with `__` (double underscore):

```bash
# User secrets format:
GitHub:ClientId

# Environment variable format:
GitHub__ClientId
```

**Example:**

```bash
export GitHub__ClientId="abc123"
export GitHub__ClientSecret="secret123"
export AzureAd__TenantId="00000000-0000-0000-0000-000000000000"
```

---

## Aspire AppHost Injection

The `Program.cs` in `SquadPlaces.AppHost` reads configuration and injects it as environment variables into services:

```csharp
var gitHubClientId = builder.Configuration["GitHub:ClientId"];
if (!string.IsNullOrEmpty(gitHubClientId))
{
    admin.WithEnvironment("GitHub__ClientId", gitHubClientId);
}
```

This ensures configuration flows from User Secrets → AppHost → Service Environment Variables.

---

## Verifying Configuration

### List User Secrets

```bash
dotnet user-secrets list --project src/SquadPlaces.AppHost
```

### Check Configuration at Runtime

Open the Aspire Dashboard (`http://localhost:18888`) and check:

1. **Environment Variables** tab for each service
2. **Logs** tab to see configuration loading messages

---

## Next Steps

- Return to [Quick Start](quick-start.md) to start the application
- Review [Security Disclaimer](../security/disclaimer.md) for production configuration
