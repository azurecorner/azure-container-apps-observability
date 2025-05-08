@description('The location to deploy all my resources')
param location string = resourceGroup().location

@description('The name of the log analytics workspace')
param logAnalyticsWorkspaceName string

@description('The name of the Application Insights workspace')
param appInsightsName string 

@description('The name of the Container App Environment')
param containerAppEnvName string

@description('The name of the Container Registry')
param containerRegistryName string

@description('The name of the Key Vault')
param keyVaultName string

@description('The container image used by the Backend API')
param backendApiImage string ='xxxx'

@description('The container image used by the Frontend UI')
param frontendUIImage string ='xxxx'

var tags = {
  environment: 'production'
  owner: 'Will Velida'
  application: 'lets-build-aca'
}

module logAnalytics 'modules/log-analytics.bicep' = {
  name: 'law'
  params: {
    location: location 
    logAnalyticsWorkspaceName: logAnalyticsWorkspaceName
    tags: tags
  }
}

module keyVault 'modules/key-vault.bicep' = {
  name: 'kv'
  params: {
    keyVaultName: keyVaultName
    location: location
    tags: tags
  }
}

module appInsights 'modules/app-insights.bicep' = {
  name: 'appins'
  params: {
    appInsightsName: appInsightsName
    keyVaultName: keyVault.outputs.name
    location: location
    logAnalyticsName: logAnalytics.outputs.name
    tags: tags
  }
}

module containerRegistry 'modules/container-registry.bicep' = {
  name: 'acr'
  params: {
    containerRegistryName: containerRegistryName
    location: location
    tags: tags
  }
}

module env 'modules/container-app-env.bicep' = {
  name: 'env'
  params: {
    appInsightsName: appInsights.outputs.name
    containerAppEnvironmentName: containerAppEnvName
    location: location
    logAnalyticsName: logAnalytics.outputs.name
    tags: tags
  }
}

module storage 'modules/storage-account.bicep' = {
  name: 'storage'
  params: {
    location: location
    storageAccountName: 'stdatasynacaobs'
    fileShareName: 'otel-config'
   }
}

// module backend 'modules/backend-api.bicep' = {
//   name: 'backend'
//   params: {
//     containerAppEnvName: env.outputs.containerAppEnvName
//     containerRegistryName: containerRegistry.outputs.name
//     keyVaultName: keyVault.outputs.name
//     location: location
//     tags: tags
//     imageName: backendApiImage
//   }
// }

// module frontend 'modules/frontend-ui.bicep' = {
//   name: 'ui'
//   params: {
//     containerAppEnvName: env.outputs.containerAppEnvName
//     containerRegistryName: containerRegistry.outputs.name
//     keyVaultName: keyVault.outputs.name
//     location: location
//     tags: tags
//     imageName: frontendUIImage
//     backendFqdn: backend.outputs.fqdn
//   }
// }


