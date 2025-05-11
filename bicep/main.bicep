@description('Main Bicep file for deploying Azure resources for the DataSync application.')
param location string = resourceGroup().location

@description('The name of the resource group that will be used to deploy the resources.')
param appName string = 'datasync001'

@description('The name of log  logAnalyticsWorkspaceName  deploy the resources to.')
param logAnalyticsWorkspaceName string = 'law${appName}'

param keyVaultName string = 'kv${appName}'

param lastDeployed string = utcNow('d')

param deployapps bool = true

param backendApiImage string = 'acrdatasync001.azurecr.io/weatherforecast-web-api:latest'
param frontendUIImage string = 'acrdatasync001.azurecr.io/weatherforecast-web-app:latest'

param containerRegistryName string = 'acr${appName}'

param containerEnvironmentName string = 'env${appName}'

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

resource userAssignedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' = {
  name: 'aca-identity${appName}'
  location: location
  tags: tags
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
     enableRbacAuthorization: true
    enableSoftDelete: false
    accessPolicies: [
    ]
  }
}
var adminUserObjectId = '7abf4c5b-9638-4ec4-b830-ede0a8031b25'
var keyVaultSecretUserRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6')
resource keyVaultSecretUserRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, adminUserObjectId, keyVaultSecretUserRoleId)
  scope: keyVault
  properties: {
    principalId: adminUserObjectId
    roleDefinitionId: keyVaultSecretUserRoleId
    principalType: 'User'
  }
}



module logAnalytics 'modules/log-analytics.bicep' = {
  name: 'log-analytics'
  params: {
    tags: tags
    keyVaultName: keyVault.name
    location: location
    logAnalyticsWorkspaceName: logAnalyticsWorkspaceName
  }
}

module containerEnvironment 'modules/container-app-env.bicep' = {
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

// module containerApp 'modules/containerapp-otel.bicep' = {
//   name: 'container-app'
//   params: {
//     tags: tags
//     location: location
//     containerAppName: containerAppName
//     envVariables: containerAppEnvVariables
//     containerAppEnvId: containerEnvironment.outputs.containerEnvironmentId
//     acrServerName: containerRegistry.outputs.serverName
//     acrUsername: keyVault.getSecret('acr-username-shared-key')
//     acrPasswordSecret: keyVault.getSecret('acr-password-shared-key')  
//   }
// }

module appInsights 'modules/app-insights.bicep' = {
  name: 'appins'
  params: {
    appInsightsName: 'ai-datasynaca-obs'
    keyVaultName: keyVaultName
    location: location
    logAnalyticsName: logAnalyticsWorkspaceName
    tags: tags
  }
  dependsOn: [
    logAnalytics
    keyVault
  ]
}


module backend 'modules/backend-api.bicep' = if (deployapps) {
  name: 'web-api'
  params: {
    containerAppEnvName: containerEnvironment.outputs.containerEnvironmentName
    containerRegistryName: containerRegistry.outputs.name
    keyVaultName: keyVault.name
    location: location
    tags: tags
    imageName: backendApiImage
    userAssignedIdentityName: userAssignedIdentity.name
  }
  dependsOn: [

    appInsights
  ]
}


module frontend 'modules/frontend-ui.bicep' = {
  name: 'ui'
  params: {
    containerAppEnvName: containerEnvironment.outputs.containerEnvironmentName
    containerRegistryName: containerRegistry.outputs.name
    keyVaultName: keyVault.name
    location: location
    userAssignedIdentityName: userAssignedIdentity.name
    tags: tags
    imageName: frontendUIImage
    backendFqdn: backend.outputs.fqdn
  }
}

module sqlserver 'modules/sql-server.bicep' = {
  name: 'sqlserver'
  params: {
    sqlServerName: 'sqlserver-datasync-001'
    adminLogin: 'logcorner'
    adminPassword: 'StrongP@ssw0rd'
    databaseName: 'WeatherForecastDb'
    serverLocation: location
    keyVaultName: keyVaultName
  }
}
