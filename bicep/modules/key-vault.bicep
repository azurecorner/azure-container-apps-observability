@description('The name of the Key Vault that will be deployed')
param keyVaultName string

@description('The location that the Key Vault will be deployed to')
param location string

@description('The tags that will be applied to the Key Vault')
param tags object

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  tags: tags
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: tenant().tenantId
    accessPolicies: []
    enableRbacAuthorization: true
    enabledForTemplateDeployment: true
    enableSoftDelete: false
  }
}

@description('The name of the Key Vault')
output name string = keyVault.name

@description('The URI of the Key Vault')
output uri string = keyVault.properties.vaultUri
