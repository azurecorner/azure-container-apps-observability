param location string
param storageAccountName string
param storageShareName string
param runScript string
param configBase64 string

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'datasyncManagedIdentity'
  location: location
  
}

#disable-next-line BCP081
resource runSqlDeployment 'Microsoft.Resources/deploymentScripts@2023-08-01' = {
  name: 'run-sql-deployment'
  location: location

     identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  kind: 'AzurePowerShell'
  properties: {
    azPowerShellVersion: '9.7'
    retentionInterval: 'PT1H'
    timeout: 'PT15M'
    forceUpdateTag: '1'
    scriptContent: runScript
    containerSettings: {
      containerGroupName: 'cn-run-sql-deployment'
    }
    arguments: '-RESOURCEGROUP_NAME "${resourceGroup().name}"  -STORAGE_ACCOUNT_NAME "${storageAccountName}" -STORAGE_SHARE_NAME "${storageShareName}" -sqlScriptBase64 "${configBase64}" '
  }
}


output scriptStatus string = runSqlDeployment.properties.provisioningState
