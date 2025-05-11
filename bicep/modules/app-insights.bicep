@description('The name of the Application Insights workspace that will be deployed.')
param appInsightsName string

@description('The location that the Application Insights workspace will be deployed')
param location string

@description('The name of the Log Analytics workspace that will be linked to this Applciation Insights workspace.')
param logAnalyticsName string

@description('The Key Vault that this Application Insights workspace will use to store secrets in')
param keyVaultName string

@description('The tags that will be applied to the Application Insights workspace')
param tags object

#disable-next-line BCP081
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2025-02-01' existing = {
  name: logAnalyticsName
}

#disable-next-line BCP081
resource keyVault 'Microsoft.KeyVault/vaults@2024-11-01' existing = {
  name: keyVaultName
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  tags: tags
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}


#disable-next-line BCP081
resource appInsightsConnectionString 'Microsoft.KeyVault/vaults/secrets@2024-11-01' = {
  parent: keyVault
  name: 'appinsightsconnectionstring'
  properties: {
    value:  appInsights.properties.ConnectionString
  }
}


#disable-next-line BCP081
resource appInsightsInstrumentationKey 'Microsoft.KeyVault/vaults/secrets@2024-11-01' = {
  parent: keyVault
  name: 'appinsightsinstrumentationkey'
  properties: {
    value: appInsights.properties.InstrumentationKey
  }
}

@description('The name of the Application Insights workspace')
output name string = appInsights.name
