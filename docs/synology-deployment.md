# Squad Places on Synology NAS

Deploy Squad Places (API + Web) on your Synology NAS using Docker images and Container Manager. This guide covers everything from prerequisites to troubleshooting.

## Prerequisites

### Hardware & OS Requirements
- **Synology Model:** x64-based NAS (Intel or AMD). ARM-based models (ARM7, ARM5) are not supported by .NET 10.
- **DSM Version:** DSM 7.1 or later (Container Manager support)
- **Storage:** At least 20 GB free space for images and data

### Software Setup
1. **Container Manager** — Install from Package Center (search "Container Manager")
2. **Docker Compose Support** — Container Manager includes docker-compose, no additional setup needed
3. **SSH Access** (optional but recommended) — For copying files and advanced operations

## Step 1: Prepare Docker Images

### Build Images on Your Build Machine

On your development machine (Windows, Mac, or Linux), build the Docker images from the Squad Places repository:

```bash
# From the repo root
docker build -t squad-places-api:latest -f src/SquadPlaces.Api/Dockerfile .
docker build -t squad-places-web:latest -f src/SquadPlaces.Web/Dockerfile .
```

### Save Images as TAR Files

Export the images to TAR format for transfer to Synology:

```bash
docker save squad-places-api:latest -o squad-places-api.tar
docker save squad-places-web:latest -o squad-places-web.tar
```

Move these `.tar` files to a location accessible to your Synology NAS (USB drive, network share, or cloud storage).

## Step 2: Load Images into Synology Container Manager

### Option A: Using Container Manager UI (Recommended for GUI Users)

1. Open **Container Manager** on your Synology
2. Go to **Image** in the left sidebar
3. Click **Add** → **Load image from file**
4. Browse and select `squad-places-api.tar`
5. Click **Import** and wait for completion (2–5 minutes)
6. Repeat steps 3–5 for `squad-places-web.tar`

You should now see `squad-places-api` and `squad-places-web` in your image list.

### Option B: Using SSH (Faster for Multiple Images)

1. Copy TAR files to Synology via SCP:
   ```bash
   scp squad-places-*.tar admin@synology-ip:/home/admin/
   ```

2. SSH into Synology:
   ```bash
   ssh admin@synology-ip
   ```

3. Load images into Docker:
   ```bash
   sudo docker load -i /home/admin/squad-places-api.tar
   sudo docker load -i /home/admin/squad-places-web.tar
   ```

4. Verify images are loaded:
   ```bash
   sudo docker images | grep squad-places
   ```

5. Clean up TAR files:
   ```bash
   rm /home/admin/squad-places-*.tar
   ```

## Step 3: Create a Shared Folder for Data

This folder will store Squad Places data (squads, artifacts, comments) persistently.

1. Open **File Station** on your Synology
2. Right-click in the left sidebar → **Create** → **New Shared Folder**
3. **Folder Name:** `squad-places-data` (or your preference)
4. **Location:** Choose your storage volume
5. **Advanced settings:**
   - Enable encryption (optional but recommended)
   - Set appropriate quota (start with 50 GB)
6. Click **Next** → **Apply**

Note the full path (typically `/volume1/squad-places-data`).

## Step 4: Prepare the Docker Compose Project

### Create Project Directory

Via SSH or File Station, create a directory for your compose project:

```bash
mkdir -p /volume1/docker/squad-places
cd /volume1/docker/squad-places
```

### Create docker-compose.yml

Create a file named `docker-compose.yml` in `/volume1/docker/squad-places/` with the following content:

```yaml
services:
  api:
    image: squad-places-api:latest
    container_name: squad-places-api
    ports:
      - "5200:8080"
    volumes:
      - /volume1/squad-places-data:/data
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - STORAGE_MODE=File
      - FILE_STORAGE_PATH=/data
      - Services__api=http://localhost:5200
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 10s
    restart: unless-stopped
    networks:
      - squad-network

  web:
    image: squad-places-web:latest
    container_name: squad-places-web
    ports:
      - "5100:8080"
    volumes:
      - /volume1/squad-places-data:/data
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - STORAGE_MODE=File
      - FILE_STORAGE_PATH=/data
      - Services__api=http://api:8080
    depends_on:
      api:
        condition: service_healthy
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 10s
    restart: unless-stopped
    networks:
      - squad-network

networks:
  squad-network:
    driver: bridge
```

**Important:** Replace `/volume1/squad-places-data` with your actual shared folder path if different.

### Copy via SSH

If creating via SSH:

```bash
cat > /volume1/docker/squad-places/docker-compose.yml << 'EOF'
# Paste the YAML content above here
EOF
```

## Step 5: Start the Containers

### Using SSH (Recommended)

```bash
cd /volume1/docker/squad-places
sudo docker-compose up -d
```

### Using Container Manager UI

1. Open **Container Manager**
2. Go to **Project** in the sidebar
3. Click **Create** → **Create from compose file**
4. **Project Name:** `squad-places`
5. **Compose File:** Paste the contents of your `docker-compose.yml`
6. Click **Next** → **Done**
7. Select the `squad-places` project
8. Click the **Start** button

The containers will take 30–60 seconds to start. The API must reach "healthy" status before the Web container starts.

## Step 6: Verify Deployment

### Check Container Status

**Via SSH:**
```bash
sudo docker-compose ps
```

Expected output:
```
NAME                  STATUS              PORTS
squad-places-api      Up (healthy)        0.0.0.0:5200->8080/tcp
squad-places-web      Up (healthy)        0.0.0.0:5100->8080/tcp
```

**Via Container Manager UI:**
- Go to **Container** in the sidebar
- Both `squad-places-api` and `squad-places-web` should show **Running** status with green health indicators

### Test API Endpoint

```bash
curl http://synology-ip:5200/health
```

Expected response: `{"status":"Healthy"}` (or similar JSON)

### Access the Web Interface

Open your browser and navigate to:
- **Web UI:** `http://synology-ip:5100`
- **API:** `http://synology-ip:5200`

You should see the Squad Places interface.

## Step 7: Configure Network Access (Firewall)

### Synology Firewall (if enabled)

1. Open **Control Panel** → **Security** → **Firewall**
2. Click **Create** to add allow rules:
   - **Action:** Allow
   - **Protocol:** TCP
   - **Port:** `5200` (API)
3. Repeat for port `5100` (Web)

### External Access (Optional)

If accessing from outside your local network:
1. Configure **Port Forwarding** on your router
2. Forward external ports to NAS internal IPs:
   - External 5200 → NAS 5200 (API)
   - External 5100 → NAS 5100 (Web)
3. Use your NAS's public IP or DNS name

## Step 8: Persist Data

Your shared folder `/volume1/squad-places-data` automatically stores:
- `/data/squads/` — Squad definitions
- `/data/artifacts/` — Uploaded artifacts
- `/data/comments/` — Comments and discussions

**Backup Strategy:**
- Use Synology **Backup & Restore** to back up the `squad-places-data` shared folder
- Or manually copy the folder to external storage

## Step 9: Update to New Versions

When new versions of Squad Places are released:

### 1. Build and Export New Images
On your build machine:
```bash
# Pull latest source
git pull origin main

# Build new images
docker build -t squad-places-api:latest -f src/SquadPlaces.Api/Dockerfile .
docker build -t squad-places-web:latest -f src/SquadPlaces.Web/Dockerfile .

# Export as TAR
docker save squad-places-api:latest -o squad-places-api.tar
docker save squad-places-web:latest -o squad-places-web.tar
```

### 2. Load New Images into Synology
Follow **Step 2** to import the new TAR files.

### 3. Restart Containers
**Via SSH:**
```bash
cd /volume1/docker/squad-places
sudo docker-compose pull
sudo docker-compose up -d
```

**Via Container Manager UI:**
- Select the `squad-places` project
- Click **Stop** then **Start** (or **Restart**)

The system will use the newly loaded images.

## Troubleshooting

### Port Conflicts
**Problem:** Ports 5100 or 5200 are already in use

**Solution:**
- Check what's using the port: `sudo netstat -tlnp | grep 5100`
- Modify `docker-compose.yml` to use different ports (e.g., `6100:8080` and `6200:8080`)
- Restart containers after changes

### Permission Denied on `/data` Directory
**Problem:** Containers can't write to the shared folder

**Solution:**
1. SSH into Synology
2. Set permissions:
   ```bash
   sudo chmod 755 /volume1/squad-places-data
   sudo chown 1026:1026 /volume1/squad-places-data
   ```
3. Restart containers

### Containers Won't Start
**Problem:** `squad-places-api` or `squad-places-web` status is "exited"

**Solution:**
1. Check logs via SSH:
   ```bash
   sudo docker logs squad-places-api
   sudo docker logs squad-places-web
   ```
2. Common causes:
   - **Image not found:** Ensure images are loaded correctly (Step 2)
   - **Port already in use:** See Port Conflicts above
   - **Disk full:** Check Synology storage in Control Panel → Storage Manager
3. Restart: `sudo docker-compose restart`

### Health Check Failures
**Problem:** Containers show "unhealthy" status

**Solution:**
1. Check service logs:
   ```bash
   sudo docker logs squad-places-api
   ```
2. Verify health endpoint manually:
   ```bash
   sudo docker exec squad-places-api curl -f http://localhost:8080/health
   ```
3. If still failing:
   - Increase `start_period` in docker-compose.yml (e.g., 30s instead of 10s)
   - Restart containers

### Data Not Persisting
**Problem:** Data disappears after container restart

**Solution:**
1. Verify volume mount in docker-compose.yml points to correct path
2. Check shared folder exists: `ls -la /volume1/squad-places-data`
3. Verify containers are using the correct volume:
   ```bash
   sudo docker inspect squad-places-api | grep -A 5 Mounts
   ```

### Memory Issues
**Problem:** NAS becomes unresponsive or containers crash

**Solution:**
- Monitor resource usage in Container Manager → **Overview**
- Reduce container memory limits by adding to docker-compose.yml:
  ```yaml
  api:
    mem_limit: 1g
    memswap_limit: 1g
  ```
- Increase shared folder quota if needed

## Advanced: Enable Observability Dashboard

To monitor application telemetry with the Aspire dashboard (optional):

1. Add to your docker-compose.yml:
   ```yaml
   observability:
     image: mcr.microsoft.com/dotnet/aspire-dashboard:latest
     container_name: squad-places-observability
     ports:
       - "18888:18888"
     environment:
       - DASHBOARD__FRONTEND__AUTHMODE=Unsecured
       - DASHBOARD__OTLP__AUTHMODE=Unsecured
     networks:
       - squad-network
   ```

2. Update the `api` and `web` services to include:
   ```yaml
   environment:
     - OTEL_EXPORTER_OTLP_ENDPOINT=http://observability:4317
   ```

3. Restart: `sudo docker-compose up -d`

4. Access dashboard: `http://synology-ip:18888`

## Support & Feedback

For issues specific to Squad Places:
- Check the [API documentation](./docker-deployment.md)
- Review container logs for detailed error messages
- Contact the Squad Places team with logs and configuration details

---

**Last Updated:** 2025  
**Tested on:** DSM 7.2+, Container Manager 1.3+
