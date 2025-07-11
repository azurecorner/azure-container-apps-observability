// === Parameters ===
param location string = resourceGroup().location
param containerAppEnvName string = 'blueground-9681b7a3'
param containerAppName string = 'collector'
param appInsightsName string = 'otel-insights'
param storageAccountName string = 'bgsharedstorage'
param fileShareName string = 'otelcollector'


resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' existing = {
  name: storageAccountName
}

var storageKeys = storageAccount.listKeys()
var storageAccountKey = storageKeys.keys[0].value

// === Container App Environment ===
resource containerEnv 'Microsoft.App/managedEnvironments@2025-01-01' existing = {
  name: containerAppEnvName
  
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: appInsightsName

} 

// === File Share Mount into Container App Environment ===
resource fileShareMount 'Microsoft.App/managedEnvironments/storages@2023-05-01' = {
  parent: containerEnv
  name: fileShareName
  properties: {
    azureFile: {
      accountName: storageAccountName
      shareName: fileShareName
      accountKey: storageAccountKey
      accessMode: 'ReadWrite'
    }
  }
}

// === Container App ===
resource containerApp 'Microsoft.App/containerApps@2025-01-01' = {
  name: containerAppName
  location: location
  properties: {
    managedEnvironmentId: containerEnv.id
    configuration: {
      secrets: [
        {
          name: 'appinsights-conn'
          value: appInsights.properties.ConnectionString
        }
      ]
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 4318
        transport: 'auto'
        allowInsecure: false
      }
    }
    template: {
      containers: [
        {
          name: 'collector'
          image: 'otel/opentelemetry-collector-contrib:0.98.0'
          command: []
          args: [
            '--config=/etc/otelcol/config.yaml'
          ]
          env: [
            {
              name: 'APPINSIGHTS_CONN_STRING'
              secretRef: 'appinsights-conn'
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1.0Gi'
          }
          volumeMounts: [
            {
              mountPath: '/etc/otelcol'
              volumeName: 'config'
            }
          ]
        }
      ]
      volumes: [
        {
          name: 'config'
          storageType: 'AzureFile'
          storageName: fileShareName
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
  dependsOn: [
    
    fileShareMount
  ]
}

// === Outputs ===

output containerAppFqdn string = containerApp.properties.latestRevisionFqdn 

