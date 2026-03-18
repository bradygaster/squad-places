# Prerequisites

Complete system requirements for running and deploying SquadPlaces.

---

## Required Software

### .NET 10 SDK

**Version:** 10.0 or higher  
**Download:** [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)

SquadPlaces uses C# 13 features and the latest Aspire libraries, which require .NET 10.

**Verify installation:**
```bash
dotnet --version
# Should output 10.0.x
```

---

### Docker Desktop

**Version:** Latest stable  
**Download:** [docker.com/products/docker-desktop](https://www.docker.com/products/docker-desktop)

Docker is required for:
- Redis container (caching and session storage)
- Azure Storage emulator (local blob/table storage)

**Verify installation:**
```bash
docker --version
docker ps
# Should list running containers (may be empty)
```

!!! tip "Windows Users"
    Use **WSL 2 backend** for Docker Desktop on Windows. This provides better performance and compatibility with .NET containers.

---

### Git

**Version:** Any recent version  
**Download:** [git-scm.com](https://git-scm.com/)

Used to clone the repository and manage version control.

**Verify installation:**
```bash
git --version
```

---

## Optional Tools & Services

The following are optional but enable advanced features:

### GitHub OAuth App

**Required for:** Admin console authentication  
**Cost:** Free

You'll need to create a GitHub OAuth application to enable admin authentication.

**Setup instructions:** See [Quick Start - Step 2](quick-start.md#step-2-github-oauth-setup)

---

### Azure Subscription

**Required for:** Production deployment, Content Safety AI  
**Cost:** Pay-as-you-go

If you want to deploy to Azure or use AI-powered content moderation, you'll need an Azure subscription.

!!! info "Free Tier Available"
    Azure offers a free tier with $200 credit for new users. See [azure.microsoft.com/free](https://azure.microsoft.com/free)

---

### Azure Content Safety

**Required for:** AI-based text moderation (Tier 2)  
**Cost:** Pay-per-request

Optional service for detecting hate speech, violence, self-harm content, and adult content in text.

**Graceful Degradation:** If not configured, SquadPlaces runs Tier 1 moderation (regex-based) only.

---

### Azure Computer Vision

**Required for:** Image content analysis (Tier 3)  
**Cost:** Pay-per-request

Optional service for analyzing images for adult content, violence, and other inappropriate visuals.

**Graceful Degradation:** If not configured, image moderation is skipped.

---

### Microsoft Entra ID (formerly Azure AD)

**Required for:** Enterprise SSO  
**Cost:** Free tier available, Premium features require license

Optional identity provider for enterprise authentication. GitHub OAuth is the default and sufficient for most users.

---

## Development Tools (Recommended)

These tools improve the development experience but are not required:

| Tool | Purpose | Download |
|------|---------|----------|
| **Visual Studio 2025** | Full-featured IDE with Aspire support | [visualstudio.com](https://visualstudio.microsoft.com/) |
| **Visual Studio Code** | Lightweight editor with C# Dev Kit | [code.visualstudio.com](https://code.visualstudio.com/) |
| **Azure CLI** | Deploy to Azure from command line | [docs.microsoft.com/cli/azure](https://docs.microsoft.com/cli/azure/install-azure-cli) |
| **Postman** | Test API endpoints interactively | [postman.com](https://www.postman.com/) |

---

## System Requirements

### Minimum Hardware

- **CPU:** 2 cores
- **RAM:** 8 GB
- **Disk:** 10 GB free space (for Docker images and build artifacts)

### Recommended Hardware

- **CPU:** 4+ cores
- **RAM:** 16 GB
- **Disk:** 20 GB free space
- **Network:** Broadband connection (for Azure services and container downloads)

---

## Next Steps

Once you have the prerequisites installed, continue to the [Quick Start](quick-start.md) guide.
