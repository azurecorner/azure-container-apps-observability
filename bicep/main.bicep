@description('Main Bicep file for deploying Azure resources for the DataSync application.')
param location string = resourceGroup().location

@description('The name of the resource group that will be used to deploy the resources.')
param appName string 

@description('The name of log  logAnalyticsWorkspaceName  deploy the resources to.')
param logAnalyticsWorkspaceName string = 'law${appName}'

param keyVaultName string = 'kv${appName}'

param deployApps bool 

param backendApiImage string = 'acrdatasynchro.azurecr.io/weatherforecast-web-api:latest'
param frontendUIImage string = 'acrdatasynchro.azurecr.io/weatherforecast-web-app:latest'

param containerRegistryName string = 'acr${appName}'

param containerEnvironmentName string = 'env${appName}'

param appInsightsName string = 'ai${appName}'

param sqlserverName string = 'sqlserver-${appName}'

param sqlserverAdminLogin string = 'logcorner'

@secure()
param sqlserverAdminPassword string 

param databaseName string = 'WeatherForecastDb'

param tags object 

param adminUserObjectId string 
var keyVaultSecretUserRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6')


module userAssignedIdentity 'modules/user-assigned-managed-identity.bicep' = {
  name: 'user-assigned-managed-identity'
  params: {
    userAssignedIdentityName: 'uami-${appName}'
    location: location
    tags: tags
  }
}

#disable-next-line BCP081
resource keyVault 'Microsoft.KeyVault/vaults@2024-11-01' = {
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
 
  }
}

resource keyVaultSecretUserRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, adminUserObjectId, keyVaultSecretUserRoleId)
  scope: keyVault
  properties: {
    principalId: adminUserObjectId
    roleDefinitionId: keyVaultSecretUserRoleId
    principalType: 'User'
  }
}


resource keyVaultSecretPrincipalRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, userAssignedIdentity.name, keyVaultSecretUserRoleId)
  scope: keyVault
  properties: {
    principalId: userAssignedIdentity.outputs.userAssignedIdentityPrincipalId
    roleDefinitionId: keyVaultSecretUserRoleId
    principalType: 'ServicePrincipal'
  }
}

module logAnalytics 'modules/log-analytics.bicep' = {
  name: 'log-analytics-workspaces'
  params: {
    tags: tags
    keyVaultName: keyVault.name
    location: location
    logAnalyticsWorkspaceName: logAnalyticsWorkspaceName
  }
}

module containerEnvironment 'modules/container-app-env.bicep' = {
  name: 'container-app-managed-environment'
  params: {
    containerEnvironmentName: containerEnvironmentName
    location: location
    logAnalyticsCustomerId: logAnalytics.outputs.customerId 
    logAnalyticsSharedKey: keyVault.getSecret('law-shared-key')
    tags: tags
  }
}

module containerRegistry 'modules/container-registry.bicep' = {
  name: 'container-registry'
  params: {
    tags: tags
    crName: containerRegistryName
    keyVaultName: keyVault.name
    location: location
    userAssignedIdentityPrincipalId: userAssignedIdentity.outputs.userAssignedIdentityPrincipalId
  }
}


module appInsights 'modules/app-insights.bicep' = {
  name: 'app-insights'
  params: {
    appInsightsName: appInsightsName
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


module backend 'modules/backend-api.bicep' = if (deployApps) {
  name: 'web-api'
  params: {
    containerAppEnvName: containerEnvironment.outputs.containerEnvironmentName
    containerRegistryName: containerRegistry.outputs.name
    keyVaultName: keyVault.name
    location: location
    tags: tags
    imageName: backendApiImage
    userAssignedIdentityName: userAssignedIdentity.outputs.userAssignedIdentityName
  }
  dependsOn: [

    appInsights
  ]
}


module frontend 'modules/frontend-ui.bicep' = if (deployApps) {
  name: 'web-app'
  params: {
    containerAppEnvName: containerEnvironment.outputs.containerEnvironmentName
    containerRegistryName: containerRegistry.outputs.name
    keyVaultName: keyVault.name
    location: location
    userAssignedIdentityName: userAssignedIdentity.outputs.userAssignedIdentityName
    tags: tags
    imageName: frontendUIImage
    backendFqdn: backend.outputs.fqdn
  }
}

module sqlserver 'modules/sql-server.bicep' = {
  name: 'sqlserver'
  params: {
    sqlServerName: sqlserverName
    adminLogin: sqlserverAdminLogin
    adminPassword: sqlserverAdminPassword
    databaseName: databaseName
    serverLocation: location
    keyVaultName: keyVaultName
  }
  dependsOn: [
    keyVault
  ]
}
