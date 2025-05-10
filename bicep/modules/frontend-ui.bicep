@description('The location where the Frontend UI will be deployed to')
param location string

@description('The Container App environment that the Container App will be deployed to')
param containerAppEnvName string

@description('The name of the Container Registry that this Container App pull images')
param containerRegistryName string

@description('The name of the Key Vault that this Container App will pull secrets from')
param keyVaultName string

@description('The container image that this Container App will use')
param imageName string

@description('The Backend API FQDN that this Frontend will communicate with')
param backendFqdn string

@description('The tags that will be applied to the Frontend UI')
param tags object

param userAssignedIdentityName string 

var containerAppName = 'dayasync-weatherforecast-app'

resource env 'Microsoft.App/managedEnvironments@2023-11-02-preview' existing = {
  name: containerAppEnvName
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-11-01-preview' existing = {
  name: containerRegistryName
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

resource userAssignedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' existing = {
  name: userAssignedIdentityName
}

resource frontend 'Microsoft.App/containerApps@2023-11-02-preview' = {
  name: containerAppName
  location: location
  tags: tags
  properties: {
    managedEnvironmentId: env.id
    configuration: {
      activeRevisionsMode: 'Multiple'
      ingress: {
        external: true
        targetPort: 8080
        transport: 'http'
      }
      registries: [
        {
          server: containerRegistry.properties.loginServer
          username: containerRegistry.listCredentials().username
          identity: userAssignedIdentity.id
        }
      ]
      secrets: [
        {
          name: 'app-insights-key'
          keyVaultUrl: 'https://${keyVault.name}.vault.azure.net/secrets/appinsightsinstrumentationkey'
          identity: userAssignedIdentity.id
        }
        {
          name: 'app-insights-connection-string'
          keyVaultUrl: 'https://${keyVault.name}.vault.azure.net/secrets/appinsightsconnectionstring'
          identity: userAssignedIdentity.id
        }
      ]
    }
    template: {
      containers: [
        {
          name: containerAppName
          image: imageName
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Development'
            }
            {
              name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
              secretRef: 'app-insights-key'
            }
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              secretRef: 'app-insights-connection-string'
            }
            {
              name: 'WEBAPI_URL'
              value: 'https://${backendFqdn}'
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1.0Gi'
          }
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
      }
    }
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedIdentity.id}' : {}
    }
  }
}
// resource acrPullRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
//   name: guid(containerRegistry.id, frontend.id, acrPullRoleId)
//   scope: containerRegistry
//   properties: {
//     principalId: frontend.identity.principalId
//     roleDefinitionId: acrPullRoleId
//     principalType: 'ServicePrincipal'
//   }
// }

// resource keyVaultSecretUserRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
//   name: guid(keyVault.id, frontend.id, keyVaultSecretUserRoleId)
//   scope: keyVault
//   properties: {
//     principalId: frontend.identity.principalId
//     roleDefinitionId: keyVaultSecretUserRoleId
//     principalType: 'ServicePrincipal'
//   }
// }
