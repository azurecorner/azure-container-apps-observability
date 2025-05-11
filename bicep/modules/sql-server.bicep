param sqlServerName string 
@description('Location for the SQL Server.')
param adminLogin string 
@description('SQL Server admin login.')
@secure()
param adminPassword string
param databaseName string 
@description('Name of the SQL Database.')
param serverLocation string = resourceGroup().location

resource sqlServer 'Microsoft.Sql/servers@2023-08-01' = {
  name: sqlServerName
  location: serverLocation
  properties: {
    administratorLogin: adminLogin
    administratorLoginPassword: adminPassword
  }

}

resource allowAllWindowsAzureIps 'Microsoft.Sql/servers/firewallRules@2023-08-01' = {
  parent: sqlServer
  name: 'AllowAllWindowsAzureIps'
  properties: {
    endIpAddress: '0.0.0.0'
    startIpAddress: '0.0.0.0'
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01' = {
  name: databaseName
  parent: sqlServer
  location: serverLocation
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  
}

@description('The Key Vault that this Application Insights workspace will use to store secrets in')
param keyVaultName string

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}


resource sqlserverConnectionstring 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  name: 'sqlserver-connectionstring'
  parent: keyVault
  properties: {
    value: 'Server=tcp:${sqlServerName}.database.windows.net,1433;Initial Catalog=${databaseName};Persist Security Info=False;User ID=logcorner;Password=${adminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
  }
}
