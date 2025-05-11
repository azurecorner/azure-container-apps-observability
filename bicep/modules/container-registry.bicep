param crName string
param location string
param tags object
param keyVaultName string
param userAssignedIdentityPrincipalId string 

var primaryPasswordSecret = 'acr-password-shared-key'
var usernameSecret = 'acr-username-shared-key'
var acrPullRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')

resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' existing = {
  name: keyVaultName
}

#disable-next-line BCP081
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2025-04-01' = {
  name: crName
  location: location
  tags: tags
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
  }
  identity: {
    type: 'SystemAssigned'
  }
}


resource acrUsername 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: usernameSecret
  parent: keyVault
  properties: {
    value: containerRegistry.listCredentials().username
  }
}


resource acrPasswordSecret1 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: primaryPasswordSecret
  parent: keyVault
  properties: {
    value: containerRegistry.listCredentials().passwords[0].value
  }
}

resource acrPullRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(containerRegistry.id, userAssignedIdentityPrincipalId, acrPullRoleId)
  scope: containerRegistry
  properties: {
    principalId: userAssignedIdentityPrincipalId//userAssignedIdentity.properties.principalId
    roleDefinitionId: acrPullRoleId
    principalType: 'ServicePrincipal'
  }
}

@description('The name of the Container Registry')
output name string = containerRegistry.name


output serverName string = containerRegistry.properties.loginServer
