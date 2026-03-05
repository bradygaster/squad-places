### 2026-03-05: Azure deployment via azd — Squad Places
**By:** Fenster
**What:** Deployed Squad Places to Azure Container Apps using `azd up`. Key decisions:
- **Resource group:** `rg-squad-places` in East US
- **Environment name:** `squad-places`
- **Subscription:** Bradyg's Happy Work Cloud (`e93e46f2-56c8-425d-bf31-90d2acdd26d5`)
- **Web endpoint (public):** `https://web.nicebeach-b92b0c14.eastus.azurecontainerapps.io/`
- **API endpoint (internal):** `https://api.internal.nicebeach-b92b0c14.eastus.azurecontainerapps.io/`
- **Aspire Dashboard:** `https://aspire-dashboard.ext.nicebeach-b92b0c14.eastus.azurecontainerapps.io`
- **Storage account:** `storagenkv6xgwigekle` (provisioned by Aspire via azd)
- **Container Registry:** `acrnkv6xgwigekle`
- Used `Aspire.Azure.Storage.Blobs` client integration (`AddAzureBlobServiceClient`) instead of manual `BlobServiceClient` construction — this handles both Azurite (local dev) and real Azure Storage (deployed) automatically via managed identity.
- Added `.WithExternalHttpEndpoints()` to the web project in AppHost to make it publicly accessible.
- API stays internal — only reachable within the Container Apps environment.
**Why:** Brady requested deployment. Aspire's `RunAsEmulator()` on `AddAzureStorage` handles the local/cloud duality — emulator locally, real storage when deployed.
**Redeploy command:** `cd C:\src\squad-social-network && azd up -e squad-places --no-prompt`
**Tear down:** `azd down -e squad-places --no-prompt`
