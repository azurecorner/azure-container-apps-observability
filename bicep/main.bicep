param location string = resourceGroup().location
param appName string = uniqueString(resourceGroup().id)
param logAnalyticsWorkspaceName string = 'law${appName}'
param keyVaultName string = 'kv${appName}'
param lastDeployed string = utcNow('d')
//this is used as a conditional when deploying the container app


//container registry
param containerRegistryName string = 'acr${appName}'

//container environment
param containerEnvironmentName string = 'env${appName}'

//container app 
param containerAppName string = 'aca${appName}'
var containerAppEnvVariables = [
  {
    name: 'ASPNETCORE_ENVIRONMENT'
    value: 'Development'
  }
]

var tags = {
  ApplicationName: 'EpicApp'
  Environment: 'Development'
  LastDeployed: lastDeployed
}

resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' = {
  name: keyVaultName
  location: location
  tags: tags
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: tenant().tenantId
    enabledForDeployment: true
    enabledForTemplateDeployment: true
    enableSoftDelete: false
    accessPolicies: [
    ]
  }
}
//module invocations:

module logAnalytics 'modules/log-analytics.bicep' = {
  name: 'log-analytics'
  params: {
    tags: tags
    keyVaultName: keyVault.name
    location: location
    logAnalyticsWorkspaceName: logAnalyticsWorkspaceName
  }
}

module containerEnv 'modules/container-app-env.bicep' = {
  name: 'container-app-env'
  params: {
    containerEnvironmentName: containerEnvironmentName
    location: location
    logAnalyticsCustomerId: logAnalytics.outputs.customerId 
    logAnalyticsSharedKey: keyVault.getSecret('law-shared-key')
    tags: tags
  }
}

module containerRegistry 'modules/container-registry.bicep' = {
  name: 'acr'
  params: {
    tags: tags
    crName: containerRegistryName
    keyVaultName: keyVault.name
    location: location
  }
}

module containerApp 'modules/containerapp-otel.bicep' = {
  name: 'container-app'
  params: {
    tags: tags
    location: location
    containerAppName: containerAppName
    envVariables: containerAppEnvVariables
    containerAppEnvId: containerEnv.outputs.containerAppEnvId
    acrServerName: containerRegistry.outputs.serverName
    acrUsername: keyVault.getSecret('acr-username-shared-key')
    acrPasswordSecret: keyVault.getSecret('acr-password-shared-key')  
  }
}

module appInsights 'modules/app-insights.bicep' = {
  name: 'appins'
  params: {
    appInsightsName: 'ai-datasynaca-obs'
    keyVaultName: keyVaultName
    location: location
    logAnalyticsName: logAnalyticsWorkspaceName
    tags: tags
  }
}
