# Azure Deployment Infrastructure

## Quick Start

```bash
# Login to Azure
azd auth login

# Initialize environment (first time only)
azd init

# Provision infrastructure and deploy
azd up

# Or separately:
azd provision   # Create Azure resources
azd deploy      # Deploy application code

# Tear down
azd down
```

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Resource Group                         │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  ┌──────────────────────────────────────────────────┐   │
│  │         Virtual Network (10.0.0.0/16)            │   │
│  │                                                   │   │
│  │  ┌────────────────────────────────────────────┐  │   │
│  │  │  Container Apps Environment (10.0.0.0/21)  │  │   │
│  │  │                                            │  │   │
│  │  │  ┌──────┐  ┌──────┐  ┌──────────┐        │  │   │
│  │  │  │ API  │  │ Web  │  │  Admin   │        │  │   │
│  │  │  └──┬───┘  └──┬───┘  └────┬─────┘        │  │   │
│  │  │     │         │           │               │  │   │
│  │  └─────┼─────────┼───────────┼───────────────┘  │   │
│  │        │         │           │                   │   │
│  └────────┼─────────┼───────────┼───────────────────┘   │
│           │         │           │                        │
│  ┌────────▼─────────▼───────────▼────────────────────┐  │
│  │              Shared Services                       │  │
│  │  ┌─────────┐ ┌───────┐ ┌──────────┐ ┌─────────┐ │  │
│  │  │ Storage │ │ Redis │ │ AppInsights│ │Key Vault│ │  │
│  │  └─────────┘ └───────┘ └──────────┘ └─────────┘ │  │
│  └────────────────────────────────────────────────────┘  │
│                                                          │
│  ┌────────────────────────────────────────────────────┐  │
│  │           Managed Identity (RBAC)                  │  │
│  └────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

## Services

| Service | Description | External |
|---------|-------------|----------|
| `api`   | SquadPlaces API - public REST endpoints | ✅ |
| `web`   | SquadPlaces Web - public web frontend | ✅ |
| `admin` | SquadPlaces Admin - admin dashboard | ✅ |

## Security

- **Managed Identity**: User-assigned identity for all service-to-service auth
- **Key Vault**: Centralized secrets management with RBAC
- **VNet**: Container Apps isolated in a dedicated subnet
- **Storage**: Shared key access disabled — identity-based auth only
- **TLS 1.2**: Enforced on all services (Redis, Storage)

## Environment Variables

Set these in your azd environment for optional features:

```bash
# GitHub OAuth (for Admin)
azd env set GITHUB_CLIENT_ID <value>
azd env set GITHUB_CLIENT_SECRET <value>

# Entra ID (for Admin, optional)
azd env set AZURE_AD_TENANT_ID <value>
azd env set AZURE_AD_CLIENT_ID <value>
azd env set AZURE_AD_CLIENT_SECRET <value>
```

## Modules

| Module | Purpose |
|--------|---------|
| `log-analytics.bicep` | Log Analytics workspace for diagnostics |
| `app-insights.bicep` | Application Insights telemetry |
| `managed-identity.bicep` | User-assigned managed identity |
| `vnet.bicep` | Virtual network with Container Apps & PE subnets |
| `key-vault.bicep` | Key Vault with RBAC for managed identity |
| `storage.bicep` | Blob storage with identity-based access |
| `redis.bicep` | Azure Cache for Redis |
| `container-apps-env.bicep` | Container Apps Environment with VNet |
| `container-app.bicep` | Individual Container App (reusable) |
