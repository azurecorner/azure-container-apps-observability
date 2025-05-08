//containerApp.bicep
param containerAppName string
param location string
param containerAppEnvId string
param acrServerName string
@secure()
param acrUsername string
@secure()
param acrPasswordSecret string
param envVariables array = []
param tags object
resource containerApp 'Microsoft.App/containerApps@2022-03-01' = {
  name: containerAppName
  location: location
  tags: tags
  properties: {
   managedEnvironmentId: containerAppEnvId
   configuration: {
    activeRevisionsMode: 'Single'
    ingress: {
      external: true
      transport: 'http'
      targetPort: 3500
      allowInsecure: false
      traffic: [
        {
          latestRevision: true
          weight: 100
        }
      ]
    }
    secrets: [
      {
        name: 'container-registry-password'
        value: acrPasswordSecret
      }
    ]
    registries: [
      {
        server: acrServerName
        username: acrUsername
        passwordSecretRef: 'container-registry-password'
      }
    ]
   }
   template: {
    containers: [
      {
        name: containerAppName
        image: 'acrh7y345we4dao4.azurecr.io/otel-collector:latest' //'${acrServerName}/epic-app:latest'
        env: envVariables
        resources: {
          cpu: 1
          memory: '2.0Gi'
        }
      }
    ]
    scale: {
      minReplicas: 1
      maxReplicas: 10
    }
   } 
  }
  identity: {
    type: 'SystemAssigned'
  }
}
