targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment (e.g., dev, staging, prod)')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string

@description('GitHub OAuth Client ID (optional)')
@secure()
param gitHubClientId string = ''

@description('GitHub OAuth Client Secret (optional)')
@secure()
param gitHubClientSecret string = ''

@description('Entra ID Tenant ID (optional)')
param entraIdTenantId string = ''

@description('Entra ID Client ID (optional)')
param entraIdClientId string = ''

@description('Entra ID Client Secret (optional)')
@secure()
param entraIdClientSecret string = ''

var abbrs = loadJsonContent('./abbreviations.json')
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))
var tags = {
  'azd-env-name': environmentName
  app: 'squad-places'
}

// Resource Group
resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: '${abbrs.resourcesResourceGroups}${environmentName}'
  location: location
  tags: tags
}

// Managed Identity
module identity 'modules/managed-identity.bicep' = {
  name: 'identity'
  scope: rg
  params: {
    name: '${abbrs.managedIdentityUserAssignedIdentities}${resourceToken}'
    location: location
    tags: tags
  }
}

// Virtual Network
module vnet 'modules/vnet.bicep' = {
  name: 'vnet'
  scope: rg
  params: {
    name: '${abbrs.networkVirtualNetworks}${resourceToken}'
    location: location
    tags: tags
  }
}

// Log Analytics
module logAnalytics 'modules/log-analytics.bicep' = {
  name: 'logAnalytics'
  scope: rg
  params: {
    name: '${abbrs.operationalInsightsWorkspaces}${resourceToken}'
    location: location
    tags: tags
  }
}

// Application Insights
module appInsights 'modules/app-insights.bicep' = {
  name: 'appInsights'
  scope: rg
  params: {
    name: '${abbrs.insightsComponents}${resourceToken}'
    location: location
    logAnalyticsWorkspaceId: logAnalytics.outputs.id
    tags: tags
  }
}

// Key Vault
module keyVault 'modules/key-vault.bicep' = {
  name: 'keyVault'
  scope: rg
  params: {
    name: '${abbrs.keyVaultVaults}${resourceToken}'
    location: location
    principalId: identity.outputs.principalId
    tags: tags
  }
}

// Storage Account
module storage 'modules/storage.bicep' = {
  name: 'storage'
  scope: rg
  params: {
    name: '${abbrs.storageStorageAccounts}${resourceToken}'
    location: location
    principalId: identity.outputs.principalId
    tags: tags
  }
}

// Redis Cache
module redis 'modules/redis.bicep' = {
  name: 'redis'
  scope: rg
  params: {
    name: '${abbrs.cacheRedis}${resourceToken}'
    location: location
    tags: tags
  }
}

// Container Apps Environment
module containerAppsEnv 'modules/container-apps-env.bicep' = {
  name: 'containerAppsEnv'
  scope: rg
  params: {
    name: '${abbrs.appManagedEnvironments}${resourceToken}'
    location: location
    logAnalyticsCustomerId: logAnalytics.outputs.customerId
    logAnalyticsSharedKey: logAnalytics.outputs.primarySharedKey
    infrastructureSubnetId: vnet.outputs.containerAppsSubnetId
    tags: tags
  }
}

// API Container App
module apiApp 'modules/container-app.bicep' = {
  name: 'apiApp'
  scope: rg
  params: {
    name: '${abbrs.appContainerApps}api-${resourceToken}'
    location: location
    containerAppsEnvironmentId: containerAppsEnv.outputs.id
    identityId: identity.outputs.id
    targetPort: 8080
    external: true
    cpu: '0.5'
    memory: '1Gi'
    minReplicas: 1
    maxReplicas: 5
    tags: union(tags, { 'azd-service-name': 'api' })
    env: [
      { name: 'AZURE_CLIENT_ID', value: identity.outputs.clientId }
      { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsights.outputs.connectionString }
      { name: 'BlobStorage__serviceUri', value: storage.outputs.blobEndpoint }
      { name: 'ConnectionStrings__cache', value: redis.outputs.connectionString }
    ]
  }
}

// Web Container App
module webApp 'modules/container-app.bicep' = {
  name: 'webApp'
  scope: rg
  params: {
    name: '${abbrs.appContainerApps}web-${resourceToken}'
    location: location
    containerAppsEnvironmentId: containerAppsEnv.outputs.id
    identityId: identity.outputs.id
    targetPort: 8080
    external: true
    cpu: '0.5'
    memory: '1Gi'
    minReplicas: 1
    maxReplicas: 5
    tags: union(tags, { 'azd-service-name': 'web' })
    env: [
      { name: 'AZURE_CLIENT_ID', value: identity.outputs.clientId }
      { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsights.outputs.connectionString }
      { name: 'BlobStorage__serviceUri', value: storage.outputs.blobEndpoint }
      { name: 'ConnectionStrings__cache', value: redis.outputs.connectionString }
      { name: 'services__api__https__0', value: apiApp.outputs.uri }
    ]
  }
}

// Admin Container App
module adminApp 'modules/container-app.bicep' = {
  name: 'adminApp'
  scope: rg
  params: {
    name: '${abbrs.appContainerApps}admin-${resourceToken}'
    location: location
    containerAppsEnvironmentId: containerAppsEnv.outputs.id
    identityId: identity.outputs.id
    targetPort: 8080
    external: true
    cpu: '0.5'
    memory: '1Gi'
    minReplicas: 0
    maxReplicas: 3
    tags: union(tags, { 'azd-service-name': 'admin' })
    env: concat(
      [
        { name: 'AZURE_CLIENT_ID', value: identity.outputs.clientId }
        { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsights.outputs.connectionString }
        { name: 'BlobStorage__serviceUri', value: storage.outputs.blobEndpoint }
        { name: 'ConnectionStrings__cache', value: redis.outputs.connectionString }
        { name: 'services__api__https__0', value: apiApp.outputs.uri }
      ],
      !empty(gitHubClientId) ? [
        { name: 'GitHub__ClientId', value: gitHubClientId }
        { name: 'GitHub__ClientSecret', value: gitHubClientSecret }
      ] : [],
      !empty(entraIdTenantId) ? [
        { name: 'AzureAd__TenantId', value: entraIdTenantId }
        { name: 'AzureAd__ClientId', value: entraIdClientId }
        { name: 'AzureAd__ClientSecret', value: entraIdClientSecret }
        { name: 'AzureAd__Instance', value: '${environment().authentication.loginEndpoint}/' }
      ] : []
    )
  }
}

// Outputs for azd
output AZURE_LOCATION string = location
output AZURE_RESOURCE_GROUP string = rg.name
output AZURE_CONTAINER_APPS_ENVIRONMENT_NAME string = containerAppsEnv.outputs.name
output AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN string = containerAppsEnv.outputs.defaultDomain
output SERVICE_API_URI string = apiApp.outputs.uri
output SERVICE_WEB_URI string = webApp.outputs.uri
output SERVICE_ADMIN_URI string = adminApp.outputs.uri
output APPLICATIONINSIGHTS_CONNECTION_STRING string = appInsights.outputs.connectionString
