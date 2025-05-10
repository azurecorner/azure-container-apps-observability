param logAnalyticsWorkspaceName string
param location string
param tags object
param keyVaultName string

var sharedKeyName = 'law-shared-key'

resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' existing = {
  name: keyVaultName
}


resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2020-10-01' = {
  name: logAnalyticsWorkspaceName
  location: location
  tags: tags
  properties: {
   retentionInDays: 30
   features: {
    searchVersion: 1
   }
   sku: {
    name: 'PerGB2018'
   } 
  }
}


resource sharedKeySecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: sharedKeyName
  parent: keyVault
  properties: {
    value: logAnalytics.listKeys().primarySharedKey
  }
}

output logAnalyticsId string = logAnalytics.id
output customerId string = logAnalytics.properties.customerId
