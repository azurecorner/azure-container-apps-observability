@description('The location where the Backend API will be deployed to')
param location string

@description('The Container App environment that the Container App will be deployed to')
param containerAppEnvName string

@description('The name of the Container Registry that this Container App pull images')
param containerRegistryName string

@description('The name of the Key Vault that this Container App will pull secrets from')
param keyVaultName string

@description('The container image that this Container App will use')
param imageName string

param userAssignedIdentityName string 

@description('The tags that will be applied to the Backend API')
param tags object

var containerAppName = 'dayasync-weatherforecast-api'
var acrPullRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
var keyVaultSecretUserRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6')

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
resource backendApi 'Microsoft.App/containerApps@2023-11-02-preview' = {
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
  // identity: {
  //   type: 'SystemAssigned'
  // }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedIdentity.id}' : {}
    }
  }
  dependsOn: [
    env
    acrPullRole
    keyVaultSecretUserRoleAssignment
  ]
}

// resource acrPullRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
//   name: guid(containerRegistry.id, backendApi.id, acrPullRoleId)
//   scope: containerRegistry
//   properties: {
//     principalId: backendApi.identity.principalId
//     roleDefinitionId: acrPullRoleId
//     principalType: 'ServicePrincipal'
//   }
// }

// resource keyVaultSecretUserRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
//   name: guid(keyVault.id, backendApi.id, keyVaultSecretUserRoleId)
//   scope: keyVault
//   properties: {
//     principalId: backendApi.identity.principalId
//     roleDefinitionId: keyVaultSecretUserRoleId
//     principalType: 'ServicePrincipal'
//   }
// }


resource acrPullRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(containerRegistry.id, userAssignedIdentity.id, acrPullRoleId)
  scope: containerRegistry
  properties: {
    principalId: userAssignedIdentity.properties.principalId
    roleDefinitionId: acrPullRoleId
    principalType: 'ServicePrincipal'
  }
}

resource keyVaultSecretUserRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, userAssignedIdentity.id, keyVaultSecretUserRoleId)
  scope: keyVault
  properties: {
    principalId: userAssignedIdentity.properties.principalId
    roleDefinitionId: keyVaultSecretUserRoleId
    principalType: 'ServicePrincipal'
  }
}

@description('The FQDN for the Backend API')
output fqdn string = backendApi.properties.configuration.ingress.fqdn
