@description('The location where the Frontend UI will be deployed to')
param location string

@description('The Container App environment that the Container App will be deployed to')
param containerAppEnvName string

@description('The name of the Container Registry that this Container App pull images')
param containerRegistryName string

@description('The container image that this Container App will use')
param imageName string

@secure()
param oltp_endpoind string 

param userAssignedIdentityName string 

var containerAppName = 'weatherforecast-api'

resource env 'Microsoft.App/managedEnvironments@2025-01-01' existing = {
  name: containerAppEnvName
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2025-04-01' existing = {
  name: containerRegistryName
}

resource userAssignedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' existing = {
  name: userAssignedIdentityName
}

resource containerApp 'Microsoft.App/containerApps@2025-01-01' = {
  name: containerAppName
  location: location

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
          name: 'otlp-endpoint'
          value: oltp_endpoind
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
              name: 'OLTP_ENDPOINT'
              secretRef: 'otlp-endpoint'
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
        maxReplicas: 3
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


@description('The FQDN for the Backend API')
output fqdn string = containerApp.properties.configuration.ingress.fqdn
