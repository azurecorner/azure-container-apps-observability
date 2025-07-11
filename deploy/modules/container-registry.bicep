param crName string
param location string

param userAssignedIdentityPrincipalId string 

var acrPullRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')

#disable-next-line BCP081
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2025-04-01' = {
  name: crName
  location: location

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

resource acrPullRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(containerRegistry.id, userAssignedIdentityPrincipalId, acrPullRoleId)
  scope: containerRegistry
  properties: {
    principalId: userAssignedIdentityPrincipalId
    roleDefinitionId: acrPullRoleId
    principalType: 'ServicePrincipal'
  }
}

@description('The name of the Container Registry')
output name string = containerRegistry.name


output serverName string = containerRegistry.properties.loginServer
