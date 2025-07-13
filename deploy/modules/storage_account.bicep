param location string = resourceGroup().location
param storageAccountName string
param fileShareName string 

param managedIdentityPrincipalId string

var StorageFileDataSMBShareContributorId = '0c867c2a-1d8c-454a-a3db-ab2ea1bdc8bb'
var StorageAccountKeyOperatorServiceRoleId = '81a9662b-bebf-436f-a333-f67b29880f12'
// === Storage Account ===
resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
  }
}

// === Azure File Share ===
resource fileShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2024-01-01' = {
  name: '${storageAccount.name}/default/${fileShareName}'
  properties: {
    shareQuota: 1024
    enabledProtocols: 'SMB'
    accessTier: 'Hot'
  }
}

resource StorageFileDataSMBShareContributorRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: StorageFileDataSMBShareContributorId
  scope: subscription()
}

resource systemAgentPoolSubnetNetworkContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name:  guid(managedIdentityPrincipalId, storageAccount.id, StorageFileDataSMBShareContributorRole.id)
  scope: storageAccount
  properties: {
    roleDefinitionId: StorageFileDataSMBShareContributorRole.id
    principalId: managedIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}


resource StorageAccountKeyOperatorServiceRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  name: StorageAccountKeyOperatorServiceRoleId
  scope: subscription()
}

resource StorageAccountKeyOperatorServiceRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name:  guid(managedIdentityPrincipalId, storageAccount.id, StorageAccountKeyOperatorServiceRole.id)
  scope: storageAccount
  properties: {
    roleDefinitionId: StorageAccountKeyOperatorServiceRole.id
    principalId: managedIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

