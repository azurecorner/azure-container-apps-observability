param location string = resourceGroup().location
param logAnalyticsWorkspaceName string = 'datasync-otel-law'
param appInsightsName string = 'datasync-otel-insights'
param storageAccountName string = 'datasyncotelstorage'
param fileShareName string = 'otelcollector'
param containerAppEnvName string = 'datasync-otel-env'
param containerRegistryName string = 'datasyncotelcr'
param userAssignedIdentityName string = 'datasync-otel-uami'
param deployApps bool = false
param runScript string = loadTextContent('./scripts/run.ps1')
var configBase64Raw = loadTextContent('./config/config.yaml')
var configBase64 = base64(configBase64Raw)

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' = {
  name: userAssignedIdentityName
  location: location
  
}

// === Log Analytics Workspace ===
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

// === Application Insights ===
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

// === Container App Environment ===
resource containerEnv 'Microsoft.App/managedEnvironments@2025-01-01' = {
  name: containerAppEnvName
  location: location
  properties: {
    daprAIConnectionString: appInsights.properties.ConnectionString
     appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

// === Managed Identity for Container Registry ===
module storageAccount 'modules/storage_account.bicep' = {
  name: 'storage-account'
  params: {
    location: location
    storageAccountName: storageAccountName
    fileShareName: fileShareName
    managedIdentityPrincipalId: managedIdentity.properties.principalId
  }
}

// === Deployment Script for Configurations ===
module deploymentScript 'modules/deployment-script.bicep' =  {
  name: 'deployment-script'
  params: {
    location: location
    storageAccountName: storageAccountName
    storageShareName: fileShareName
    configBase64: configBase64
    managedIdentityId: managedIdentity.id
    runScript: runScript
  }
  
  dependsOn: [
    storageAccount
  ]
}

// === OpenTelemetry Collector Container App ===
// This module deploys the OpenTelemetry Collector as a Container App
module otelcollector 'modules/otel-collector.bicep' = {
  name: 'container-app'
  params: {
          location: location
          containerAppEnvName  : containerAppEnvName
          containerAppName  : 'datasync-otel-collector'
          appInsightsName  : appInsightsName
          storageAccountName  : storageAccountName
          fileShareName  : fileShareName
   }
  
  dependsOn: [
    deploymentScript
    containerEnv
  ]
}

// === Container Registry ===
// This module sets up a Container Registry for storing application images
module containerRegistry 'modules/container-registry.bicep' =   {
  name: 'container-registry'
  params: {
    crName: containerRegistryName
    location: location
    userAssignedIdentityPrincipalId: managedIdentity.properties.principalId
  }
}



module backend 'modules/web-api.bicep' = if (deployApps) {
  name: 'web-api'
  params: {
    containerAppEnvName: containerAppEnvName
    containerRegistryName: containerRegistry.outputs.name
    location: location
    userAssignedIdentityName: managedIdentity.name
    appInsightsName: appInsights.name
    imageName: '${containerRegistry.outputs.serverName}/web-api:latest'
    oltp_endpoind: 'https://${otelcollector.outputs.containerAppFqdn}'
   }
}
 
module frontend 'modules/web-app.bicep' = if (deployApps) {
  name: 'web-app'
  params: {
    containerAppEnvName: containerAppEnvName
    containerRegistryName: containerRegistry.outputs.name
    appInsightsName: appInsights.name
    location: location
    userAssignedIdentityName: managedIdentity.name
    imageName: '${containerRegistry.outputs.serverName}/web-app:latest'
    oltp_endpoind: 'https://${otelcollector.outputs.containerAppFqdn}'
    backendFqdn: backend.outputs.fqdn
   }
}
 
