param userAssignedIdentityName string
param location string
param tags object
#disable-next-line BCP081
resource userAssignedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' = {
  name: userAssignedIdentityName
  location: location
  tags: tags
}

output userAssignedIdentityId string = userAssignedIdentity.id
output userAssignedIdentityPrincipalId string = userAssignedIdentity.properties.principalId
