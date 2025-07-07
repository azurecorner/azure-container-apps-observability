// === Parameters ===
param location string = resourceGroup().location
param containerAppEnvName string = 'blueground-9681b7a3'
param containerAppName string = 'collector'
param logAnalyticsWorkspaceName string = 'bg-log-analytics'
param appInsightsName string = 'otel-insights'
param storageAccountName string = 'bgsharedstorage'
param fileShareName string = 'otelcollector'
param otelImage string = 'otel/opentelemetry-collector-contrib:0.98.0'
param mountPath string = '/etc/otelcol'
param configFileName string = 'config.yaml'

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2020-10-01' = {
  name: logAnalyticsWorkspaceName
  location: location

  properties: {
   retentionInDays: 30
   features: {
    searchVersion: 1
   }
   sku: {
    name: 'PerGB2018'
   } 
  }
}
// === Resource Definitions ===
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
  }
}

var storageKeys = listKeys(storageAccount.name, storageAccount.apiVersion)
var storageAccountKey = storageKeys.keys[0].value

resource fileShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2024-01-01' = {
  name: '${storageAccount.name}/default/${fileShareName}'

  properties: {
    shareQuota: 1024  // Quota in GB
    enabledProtocols: 'SMB'
    accessTier: 'Hot'
  }
}


resource containerEnv 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: containerAppEnvName
  location: location
  properties: {
    daprAIConnectionString: appInsights.properties.ConnectionString
  }
}

// Container App
resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: containerAppName
  location: location
  properties: {
    managedEnvironmentId: containerEnv.id
    configuration: {
      secrets: [
        {
          name: 'storage-account-key'
          value: storageAccountKey
        }
        {
          name: 'appinsights-conn'
          value: appInsights.properties.ConnectionString
        }
      ]
      activeRevisionsMode: 'Single'
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
    containerEnv
    appInsights
    storageAccount
    fileShare
  ]
}
output appInsightsConnectionString string = appInsights.properties.ConnectionString
output containerAppUrl string = containerApp.properties.configuration.ingress.fqdn
output storageAccountId string = storageAccount.id
output fileShareId string = fileShare.id
output containerAppFqdn string = containerApp.properties.fqdn  
