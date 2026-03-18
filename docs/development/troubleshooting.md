# Troubleshooting

Common issues and solutions for SquadPlaces.

---

## Installation Issues

### Port Already in Use

**Error:** `Address already in use: http://localhost:5000`

**Solution:** Another application is using the port. Either:
- Stop the conflicting application
- Change ports in `src/SquadPlaces.AppHost/Program.cs`

---

### Docker Connection Error

**Error:** `Cannot connect to Docker daemon`

**Solution:**
1. Start Docker Desktop
2. Wait for it to fully initialize (whale icon in system tray should be steady)
3. Verify with `docker ps`
4. Retry

---

## Authentication Issues

### GitHub OAuth Error

**Error:** `Invalid OAuth configuration`

**Solution:**
1. Verify callback URL is exactly `http://localhost:5000/signin-github`
2. Check user secrets:
   ```bash
   dotnet user-secrets list --project src/SquadPlaces.AppHost
   ```
3. Ensure secrets are set correctly:
   ```bash
   dotnet user-secrets set "GitHub:ClientId" "your-id" --project src/SquadPlaces.AppHost
   ```

---

## Runtime Errors

### Redis Connection Failed

**Error:** `Could not connect to Redis`

**Solution:**
1. Ensure Docker is running
2. Check Redis container status:
   ```bash
   docker ps | grep redis
   ```
3. Restart the application to recreate containers

---

### Azure Storage Emulator Error

**Error:** `Azure Storage connection failed`

**Solution:**
1. Ensure Docker is running
2. The Aspire AppHost automatically starts the emulator
3. Check container logs in Aspire Dashboard

---

## Monitoring

### View Logs in Aspire Dashboard

1. Open `http://localhost:18888`
2. Click **Logs** in the left sidebar
3. Filter by service name
4. Search for errors

---

### View Distributed Traces

1. Open `http://localhost:18888`
2. Click **Traces** in the left sidebar
3. Find your request by timestamp or operation name
4. Click to see the full trace with all service calls

---

### Application Insights Queries

If using Application Insights, query logs with:

```kusto
traces
| where severityLevel >= 3  // Warnings and errors
| order by timestamp desc
| take 100
```

---

## Performance Issues

### Slow Startup

**Cause:** Docker pulling images on first run

**Solution:** Wait for initial download to complete (1-2 minutes). Subsequent starts are faster.

---

### High Memory Usage

**Cause:** Multiple services running locally

**Solution:**
- Close other applications
- Increase Docker memory limit in Docker Desktop settings
- Consider deploying to Azure for production workloads

---

## Need More Help?

- **GitHub Issues:** [github.com/bradygaster/squad-social-network/issues](https://github.com/bradygaster/squad-social-network/issues)
- **GitHub Discussions:** [github.com/bradygaster/squad-social-network/discussions](https://github.com/bradygaster/squad-social-network/discussions)
