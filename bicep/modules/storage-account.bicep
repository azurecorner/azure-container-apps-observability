@description('Specifies the name of the Azure Storage account.')
param storageAccountName string = 'storage${uniqueString(resourceGroup().id)}'

@description('Specifies the name of the File Share. File share names must be between 3 and 63 characters in length and use numbers, lower-case letters and dash (-) only.')
@minLength(3)
@maxLength(63)
param fileShareName string

@description('Specifies the location in which the Azure Storage resources should be deployed.')
param location string = resourceGroup().location

param tags object

resource sa 'Microsoft.Storage/storageAccounts@2024-01-01' = {
  name: storageAccountName
  location: location
  tags: tags
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
    
  }
}

resource fileShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2024-01-01' = {
  name: '${sa.name}/default/${fileShareName}'

  properties: {
    shareQuota: 1024  // Quota in GB
    enabledProtocols: 'SMB'
    accessTier: 'Hot'
  }
}
