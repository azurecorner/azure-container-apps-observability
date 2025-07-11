param location string = resourceGroup().location
param logAnalyticsWorkspaceName string = 'bg-log-analytics'
param appInsightsName string = 'otel-insights'
param containerAppEnvName string = 'blueground-9681b7a3'
param containerRegistryName string = 'bgcontainerregistry'
param deployApps bool =true
param runScript string = loadTextContent('./scripts/run.ps1')
var configBase64Raw = loadTextContent('./config/config.yaml')
var configBase64 = base64(configBase64Raw)

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' = {
  name: 'datasyncManagedIdentity'
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
  }
}


module storageAccount 'modules/storage_account.bicep' = {
  name: 'storage-account'
  params: {
    location: location
    storageAccountName: 'bgsharedstorage'
    fileShareName: 'otelcollector'
    managedIdentityPrincipalId: managedIdentity.properties.principalId
  }
}


module deploymentScript 'modules/deployment-script.bicep' =  {
  name: 'deployment-script'
  params: {
    location: location
    storageAccountName: 'bgsharedstorage'
    storageShareName: 'otelcollector'
    configBase64: configBase64
    managedIdentityId: managedIdentity.id
    runScript: runScript
  }
  
  dependsOn: [
    storageAccount
  ]
}
 
module otelcollector 'modules/otel-collector.bicep' = {
  name: 'container-app'
  params: {
          location: location
          containerAppEnvName  : containerAppEnvName
          containerAppName  : 'collector'
          appInsightsName  : 'otel-insights'
          storageAccountName  : 'bgsharedstorage'
          fileShareName  : 'otelcollector'
   }
  
  dependsOn: [
    deploymentScript
  ]
}


module containerRegistry 'modules/container-registry.bicep' = {
  name: 'container-registry'
  params: {
    crName: containerRegistryName
    location: location
    userAssignedIdentityPrincipalId: managedIdentity.properties.principalId
  }
}


module frontend 'modules/web-api.bicep' = if (deployApps) {
  name: 'web-app'
  params: {
    containerAppEnvName: containerAppEnvName
    containerRegistryName: containerRegistry.outputs.name
    location: location
    userAssignedIdentityName: managedIdentity.name
    imageName: '${containerRegistry.outputs.serverName}/web-api:latest'
    oltp_endpoind: 'https://${otelcollector.outputs.containerAppFqdn}'
   }
}
 
