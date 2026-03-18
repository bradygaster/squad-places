# Mostly Harmless — Deployment Overview

> "Mostly harmless." — The revised Guide entry for Earth. Also the energy with which we approach deployment.

Squad Places can be deployed to multiple environments. Choose the deployment method that fits your needs.

---

## Deployment Options

### 🐳 Hitching a Ride in a Container (Docker)

**Best for:** Local development, testing, CI/CD pipelines

Run Squad Places in Docker containers with Docker Compose. All services run containerized.

**→ [Docker Deployment Guide](docker.md)**

---

### ☁️ The Cloud District of Magrathea (Azure)

**Best for:** Production workloads, enterprise deployments, cloud-native applications

Deploy to Azure using Azure Container Apps, Azure App Service, or Azure Kubernetes Service for production-ready, cloud-native hosting.

**→ [Azure Deployment Guide](azure.md)**

---

### 🏠 Parking Your Ship at Home (Synology NAS)

**Best for:** Home labs, self-hosted environments, private deployments

Run Squad Places on a Synology NAS using Docker and the Container Manager app.

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
