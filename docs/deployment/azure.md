# The Cloud District of Magrathea — Deploying to Azure

> "Magrathea was a planet whose inhabitants were the most extraordinary builders the Galaxy had ever known. They constructed custom-made luxury planets." Azure is similar, except it builds cloud environments and charges by the minute.

Deploy Squad Places to Azure for production-ready, cloud-native hosting.

---

## Prerequisites

- Azure subscription (use "bradyg happy work cloud" subscription only)
- Azure CLI installed
- .NET 10 SDK
- Docker (for building container images)

---

## Quick Deploy with Azure Developer CLI (azd)

The fastest way to deploy to Azure is using the Azure Developer CLI:

```bash
# Install azd (if not already installed)
# Windows:
winget install microsoft.azd

# Login to Azure
azd auth login

# Initialize the project
azd init

# Deploy to Azure
azd up
```

This will:
- Create resource group
- Deploy Container Apps for Web, API, and Admin
- Create Azure Cache for Redis
- Create Azure Storage Account
- Configure GitHub OAuth and other secrets

!!! warning "Never Publish Public URLs"
    Do NOT output or include any public deployment URLs in documentation or commit messages.

---

## Manual Deployment

### Step 1: Create Azure Resources

```bash
# Set variables
RESOURCE_GROUP="squadplaces-rg"
LOCATION="westus2"
STORAGE_ACCOUNT="squadplacesstorage"
REDIS_NAME="squadplaces-redis"

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create storage account
az storage account create \
  --name $STORAGE_ACCOUNT \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku Standard_LRS

# Create Redis cache
az redis create \
  --name $REDIS_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku Basic \
  --vm-size c0
```

---

### Step 2: Build and Push Container Images

```bash
# Build images
docker build -t squadplaces-web:latest -f src/SquadPlaces.Web/Dockerfile .
docker build -t squadplaces-api:latest -f src/SquadPlaces.Api/Dockerfile .
docker build -t squadplaces-admin:latest -f src/SquadPlaces.Admin/Dockerfile .

# Tag and push to Azure Container Registry (ACR)
ACR_NAME="squadplacesacr"
az acr create --name $ACR_NAME --resource-group $RESOURCE_GROUP --sku Basic
az acr login --name $ACR_NAME

docker tag squadplaces-web:latest $ACR_NAME.azurecr.io/web:latest
docker tag squadplaces-api:latest $ACR_NAME.azurecr.io/api:latest
docker tag squadplaces-admin:latest $ACR_NAME.azurecr.io/admin:latest

docker push $ACR_NAME.azurecr.io/web:latest
docker push $ACR_NAME.azurecr.io/api:latest
docker push $ACR_NAME.azurecr.io/admin:latest
```

---

### Step 3: Deploy Container Apps

```bash
# Create Container Apps environment
az containerapp env create \
  --name squadplaces-env \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION

# Deploy Web app
az containerapp create \
  --name squadplaces-web \
  --resource-group $RESOURCE_GROUP \
  --environment squadplaces-env \
  --image $ACR_NAME.azurecr.io/web:latest \
  --target-port 80 \
  --ingress external

# Deploy API
az containerapp create \
  --name squadplaces-api \
  --resource-group $RESOURCE_GROUP \
  --environment squadplaces-env \
  --image $ACR_NAME.azurecr.io/api:latest \
  --target-port 80 \
  --ingress external

# Deploy Admin
az containerapp create \
  --name squadplaces-admin \
  --resource-group $RESOURCE_GROUP \
  --environment squadplaces-env \
  --image $ACR_NAME.azurecr.io/admin:latest \
  --target-port 80 \
  --ingress external
```

---

### Step 4: Configure Secrets

```bash
# Set GitHub OAuth secrets
az containerapp secret set \
  --name squadplaces-admin \
  --resource-group $RESOURCE_GROUP \
  --secrets github-client-id="your-client-id" github-client-secret="your-client-secret"

# Set environment variables
az containerapp update \
  --name squadplaces-admin \
  --resource-group $RESOURCE_GROUP \
  --set-env-vars \
    GitHub__ClientId=secretref:github-client-id \
    GitHub__ClientSecret=secretref:github-client-secret
```

---

## Monitoring with Application Insights

```bash
# Create Application Insights
az monitor app-insights component create \
  --app squadplaces-insights \
  --location $LOCATION \
  --resource-group $RESOURCE_GROUP

# Get connection string
APP_INSIGHTS_CONNECTION=$(az monitor app-insights component show \
  --app squadplaces-insights \
  --resource-group $RESOURCE_GROUP \
  --query connectionString -o tsv)

# Set environment variable on all apps
az containerapp update \
  --name squadplaces-web \
  --resource-group $RESOURCE_GROUP \
  --set-env-vars APPLICATIONINSIGHTS_CONNECTION_STRING="$APP_INSIGHTS_CONNECTION"
```

---

## Next Steps

- Configure [Content Moderation](../security/content-moderation.md) with Azure AI services
- Set up [monitoring and alerts](../development/troubleshooting.md)
- Review [best practices](../security/best-practices.md) for production
