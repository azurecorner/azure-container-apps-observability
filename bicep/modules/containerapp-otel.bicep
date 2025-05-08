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

resource containerApp 'Microsoft.App/containerApps@2025-01-01' = {
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
        targetPort: 4317       // Updated to match one of the listening ports exposed by the image
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
          image: 'otel/opentelemetry-collector-contrib:latest'  // Replace with your image if needed
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Development'
            }
          ]
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
