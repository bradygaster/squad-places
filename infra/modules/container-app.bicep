@description('Name of the Container App')
param name string

@description('Azure region')
param location string

@description('Container Apps environment ID')
param containerAppsEnvironmentId string

@description('Container image to deploy')
param containerImage string = 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'

@description('User-assigned managed identity ID')
param identityId string

@description('Target port for the container')
param targetPort int = 8080

@description('Whether the app is externally accessible')
param external bool = true

@description('Environment variables')
param env array = []

@description('CPU cores (e.g., 0.5)')
param cpu string = '0.5'

@description('Memory (e.g., 1Gi)')
param memory string = '1Gi'

@description('Min replicas')
param minReplicas int = 0

@description('Max replicas')
param maxReplicas int = 3

@description('Tags to apply')
param tags object = {}

resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: name
  location: location
  tags: tags
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${identityId}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironmentId
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: external
        targetPort: targetPort
        transport: 'auto'
        allowInsecure: false
      }
    }
    template: {
      containers: [
        {
          name: name
          image: containerImage
          resources: {
            cpu: json(cpu)
            memory: memory
          }
          env: env
        }
      ]
      scale: {
        minReplicas: minReplicas
        maxReplicas: maxReplicas
        rules: [
          {
            name: 'http-scaling'
            http: {
              metadata: {
                concurrentRequests: '50'
              }
            }
          }
        ]
      }
    }
  }
}

output id string = containerApp.id
output name string = containerApp.name
output fqdn string = containerApp.properties.configuration.ingress.fqdn
output uri string = 'https://${containerApp.properties.configuration.ingress.fqdn}'
