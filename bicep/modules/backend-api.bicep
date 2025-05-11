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


resource env 'Microsoft.App/managedEnvironments@2025-01-01' existing = {
  name: containerAppEnvName
}

#disable-next-line BCP081
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2025-04-01' existing = {
  name: containerRegistryName
}

resource keyVault 'Microsoft.KeyVault/vaults@2024-11-01' existing = {
  name: keyVaultName
}


resource userAssignedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' existing = {
  name: userAssignedIdentityName
}

#disable-next-line BCP081
resource backendApi 'Microsoft.App/containerApps@2025-01-01' = {
  name: containerAppName
  location: location
  tags: tags
  properties: {
    managedEnvironmentId: env.id
    configuration: {
      activeRevisionsMode: 'Multiple'
      ingress: {
        external: false
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
          keyVaultUrl: 'https://${keyVault.name}.vault.azure.net/secrets/appinsights-instrumentationkey'
          identity: userAssignedIdentity.id
        }
        {
          name: 'app-insights-connection-string'
          keyVaultUrl: 'https://${keyVault.name}.vault.azure.net/secrets/appinsights-connectionstring'
          identity: userAssignedIdentity.id
        }
        {
          name: 'sqlserver-connectionstring'
          keyVaultUrl: 'https://${keyVault.name}.vault.azure.net/secrets/sqlserver-connectionstring'
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
                  value: 'ContainerApps'
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
                  name: 'ConnectionStrings__DbConnection'
                  secretRef: 'sqlserver-connectionstring'
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
  dependsOn: [
    env
    
    
  ]
}


@description('The FQDN for the Backend API')
output fqdn string = backendApi.properties.configuration.ingress.fqdn
