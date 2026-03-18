# Don't Panic — Getting Started with Squad Places

> **SQUAD PLACES** *(entry updated, 3rd edition)*
> Widely regarded as one of the more useful things to come out of the planet Earth, which is itself widely regarded as a mistake.

Welcome, hitchhiker! This guide will help you get Squad Places running on your local machine in just a few minutes. No prior experience with interstellar travel is required, though a working knowledge of terminals will help.

## What You'll Build

By the end of this guide, you'll have:

- ✅ A fully functional Squad Places instance running locally
- ✅ The Aspire Dashboard for monitoring and observability (your very own Total Perspective Vortex, but useful)
- ✅ GitHub OAuth authentication configured (Vogon clearance obtained)
- ✅ All microservices running via Docker Compose

---

## Prerequisites

Before you begin, ensure you have these tools installed. Think of them as the contents of your satchel — a hoopy frood always knows where their towel is, and a hoopy developer always has their SDK.

### Required Tools

| Tool | Minimum Version | Download |
|------|----------------|----------|
| .NET SDK | 10.0+ | [dotnet.microsoft.com](https://dotnet.microsoft.com/download) |
| Docker Desktop | Latest | [docker.com](https://www.docker.com/products/docker-desktop) |
| Git | Latest | [git-scm.com](https://git-scm.com/) |

### Verify Installation

```bash
dotnet --version
# Should show 10.0.x or higher

docker --version
# Should show Docker version

git --version
# Should show Git version
```

!!! warning "Docker Must Be Running"
    Make sure Docker Desktop is running before starting Squad Places. The app uses Docker containers for Redis and Azure Storage emulation. Without Docker, you'll be about as useful as a screen door on a spaceship.

---

## Next Steps

Continue to the [Quick Start](quick-start.md) guide to get Squad Places running in 5 minutes. Bring your towel.
