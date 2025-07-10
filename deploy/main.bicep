param location string = resourceGroup().location

param runScript string = loadTextContent('./scripts/run.ps1')
var configBase64Raw = loadTextContent('./config/config.yaml')
var configBase64 = base64(configBase64Raw)

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' = {
  name: 'datasyncManagedIdentity'
  location: location
  
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
 
module containerApp 'modules/otel-collector.bicep' = {
  name: 'container-app'
  params: {
          location: location
          containerAppEnvName  : 'blueground-9681b7a3'
          containerAppName  : 'collector'
          logAnalyticsWorkspaceName  : 'bg-log-analytics'
          appInsightsName  : 'otel-insights'
          storageAccountName  : 'bgsharedstorage'
          fileShareName  : 'otelcollector'
   }
  
  dependsOn: [
    deploymentScript
  ]
}
