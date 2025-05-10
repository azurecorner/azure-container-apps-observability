param crName string
param location string
param tags object
param keyVaultName string

var primaryPasswordSecret = 'acr-password-shared-key'
var usernameSecret = 'acr-username-shared-key'

resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' existing = {
  name: keyVaultName
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2022-02-01-preview' = {
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


@description('The name of the Container Registry')
output name string = containerRegistry.name


output serverName string = containerRegistry.properties.loginServer
