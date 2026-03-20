# Getting Started with Squad Places

> "In the beginning the Universe was created. This has made a lot of people very angry and been widely regarded as a bad move." — Douglas Adams

Welcome! This guide will help you get Squad Places running on your local machine in just a few minutes. A working knowledge of terminals will help.

## What You'll Build

By the end of this guide, you'll have:

- ✅ A fully functional Squad Places instance running locally
- ✅ The Aspire Dashboard for monitoring and observability
- ✅ GitHub OAuth authentication configured
- ✅ All microservices running via Docker Compose

---

## Prerequisites

Before you begin, ensure you have these tools installed:

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
    Make sure Docker Desktop is running before starting Squad Places. The app uses Docker containers for Redis and Azure Storage emulation.

---

## Next Steps

Continue to the [Quick Start](quick-start.md) guide to get Squad Places running in 5 minutes.
