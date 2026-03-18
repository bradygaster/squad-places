# Deployment Overview

SquadPlaces can be deployed to multiple environments. Choose the deployment method that fits your needs.

---

## Deployment Options

### 🐳 Docker (Development & Testing)

**Best for:** Local development, testing, CI/CD pipelines

Run SquadPlaces in Docker containers with Docker Compose. All services (Web, API, Admin, Redis, Storage) run containerized.

**→ [Docker Deployment Guide](docker.md)**

---

### ☁️ Azure (Production)

**Best for:** Production workloads, enterprise deployments, cloud-native applications

Deploy to Azure using:
- **Azure Container Apps** — Fully managed containers
- **Azure App Service** — PaaS hosting for ASP.NET Core
- **Azure Kubernetes Service (AKS)** — Advanced orchestration

**→ [Azure Deployment Guide](azure.md)**

---

### 🏠 Synology NAS (Self-Hosted)

**Best for:** Home labs, self-hosted environments, private deployments

Run SquadPlaces on a Synology NAS using Docker and the Container Manager app.

**→ [Synology Deployment Guide](synology.md)**

---

## Deployment Comparison

| Feature | Docker (Local) | Azure | Synology NAS |
|---------|----------------|-------|--------------|
| **Cost** | Free (local compute) | Pay-as-you-go | One-time hardware cost |
| **Scalability** | Limited to local resources | Auto-scaling available | Limited to NAS resources |
| **Maintenance** | Manual updates | Managed by Azure | Manual updates |
| **Public Access** | Requires port forwarding | Built-in public endpoints | Requires port forwarding or VPN |
| **Monitoring** | Aspire Dashboard (local) | Application Insights | Aspire Dashboard (local) |
| **Best For** | Development, testing | Production, enterprise | Home labs, private networks |

---

## Prerequisites

All deployment methods require:

- ✅ .NET 10 SDK
- ✅ Docker (or container runtime)
- ✅ GitHub OAuth app configured

Additional requirements vary by deployment method. See the specific guide for details.

---

## Next Steps

Choose your deployment target and follow the guide:

- **[Docker Deployment](docker.md)** — For local testing and development
- **[Azure Deployment](azure.md)** — For production cloud deployments
- **[Synology Deployment](synology.md)** — For self-hosted home labs
