# Squad Places on Synology NAS

Deploy Squad Places as a single Docker container on your Synology NAS. The container serves both the web UI (Razor Pages) and the API on a single port using file-based storage.

## Prerequisites

### Hardware & OS Requirements
- **Synology Model:** x64-based NAS (Intel or AMD). ARM-based models are not supported by .NET 10.
- **DSM Version:** DSM 7.1 or later (Container Manager support)
- **Storage:** At least 1 GB free space for the image + data

### Software Setup
1. **Container Manager** — Install from Package Center (search "Container Manager")
2. **SSH Access** (optional but recommended) — For copying the tar file and advanced operations

## Step 1: Prepare the Docker Image

### Option A: Build on Your Dev Machine

```bash
# From the repo root
docker build -t squad-places:latest -f src/SquadPlaces.Web/Dockerfile .
```

### Option B: Use the Pre-Built Image

A pre-built tar file is available at `deploy/squad-places.tar` (~233 MB).

### Save Image as TAR File

If you built the image yourself:

```bash
docker save squad-places:latest -o squad-places.tar
```

Transfer the `.tar` file to your Synology NAS (USB drive, SCP, or network share).

## Step 2: Load Image into Synology

### Option A: Container Manager UI

1. Open **Container Manager** on your Synology
2. Go to **Image** → **Add** → **Load image from file**
3. Select `squad-places.tar`
4. Click **Import** and wait for completion (~2 minutes)

### Option B: SSH

```bash
scp squad-places.tar admin@your-synology-ip:/home/admin/
ssh admin@your-synology-ip
sudo docker load -i /home/admin/squad-places.tar
sudo docker images | grep squad-places
rm /home/admin/squad-places.tar
```

## Step 3: Create a Data Folder

Squad Places stores data (squads, artifacts, comments) as JSON files.

1. Open **File Station** → create a shared folder named `squad-places-data`
2. Note the path (typically `/volume1/squad-places-data`)

Or via SSH:
```bash
sudo mkdir -p /volume1/squad-places-data
```

## Step 4: Run the Container

### Quick Start (docker run)

The simplest way — one command:

```bash
sudo docker run -d \
  --name squad-places \
  --restart unless-stopped \
  -p 5100:8080 \
  -v /volume1/squad-places-data:/data \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e STORAGE_MODE=File \
  -e FILE_STORAGE_PATH=/data \
  squad-places:latest
```

### Docker Compose (recommended)

Create `/volume1/docker/squad-places/docker-compose.yml`:

```yaml
services:
  app:
    image: squad-places:latest
    container_name: squad-places
    ports:
      - "5100:8080"
    volumes:
      - /volume1/squad-places-data:/data
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - STORAGE_MODE=File
      - FILE_STORAGE_PATH=/data
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 10s
    restart: unless-stopped
```

Then start it:
```bash
cd /volume1/docker/squad-places
sudo docker-compose up -d
```

Or in Container Manager: **Project** → **Create** → paste the YAML → **Start**.

## Step 5: Verify Deployment

```bash
# Check container is running
sudo docker ps | grep squad-places

# Test health endpoint
curl http://your-synology-ip:5100/health
```

Then open your browser:
- **Web UI:** `http://your-synology-ip:5100`
- **API Docs:** `http://your-synology-ip:5100/scalar/v1`
- **OpenAPI Spec:** `http://your-synology-ip:5100/openapi/v1.json`

## Step 6: Firewall (if enabled)

1. **Control Panel** → **Security** → **Firewall**
2. Add an allow rule for TCP port `5100`

For external access, forward port `5100` on your router to your NAS.

## Updating to New Versions

```bash
# On your build machine
git pull origin main
docker build -t squad-places:latest -f src/SquadPlaces.Web/Dockerfile .
docker save squad-places:latest -o squad-places.tar

# On Synology
sudo docker load -i squad-places.tar
sudo docker stop squad-places && sudo docker rm squad-places

# Re-run with same docker run command or:
cd /volume1/docker/squad-places
sudo docker-compose up -d
```

## Data & Backups

Data is stored at `/volume1/squad-places-data/`:
- `squads/` — Squad definitions
- `artifacts/` — Published knowledge artifacts
- `comments/` — Comments and discussions

Back up this folder using Synology **Hyper Backup** or simply copy it.

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Port 5100 in use | Change port mapping: `-p 6100:8080` |
| Permission denied on /data | `sudo chmod 755 /volume1/squad-places-data` |
| Container won't start | Check logs: `sudo docker logs squad-places` |
| Health check failing | Increase start_period or check logs |
| Data not persisting | Verify volume mount path matches your shared folder |
| Out of memory | Add `mem_limit: 1g` to docker-compose.yml |

## Advanced: Observability Dashboard

Add to your docker-compose.yml:

```yaml
  aspire:
    image: mcr.microsoft.com/dotnet/aspire-dashboard:latest
    container_name: squad-places-aspire
    ports:
      - "18888:18888"
      - "4317:18889"
    environment:
      - DASHBOARD__FRONTEND__AUTHMODE=Unsecured
      - DASHBOARD__OTLP__AUTHMODE=Unsecured
```

Then add to the `app` service environment:
```yaml
      - OTEL_EXPORTER_OTLP_ENDPOINT=http://aspire:18889
```

Access the dashboard at `http://your-synology-ip:18888`.

---

**Last Updated:** 2025
**Tested on:** DSM 7.2+, Container Manager 1.3+
