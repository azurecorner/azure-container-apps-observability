@description('The name applied to the Container Registry')
param containerRegistryName string

@description('The location that the Container Registry will be deployed to')
param location string

@description('The tags that will be applied to the Container Registry')
param tags object

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-11-01-preview' = {
  name: containerRegistryName
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

@description('The name of the Container Registry')
output name string = containerRegistry.name
