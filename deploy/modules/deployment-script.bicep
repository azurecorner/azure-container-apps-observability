param location string
param storageAccountName string
param storageShareName string
param runScript string
param configBase64 string
param managedIdentityId string

#disable-next-line BCP081
resource runSqlDeployment 'Microsoft.Resources/deploymentScripts@2023-08-01' = {
  name: 'run-script-deployment'
  location: location

     identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityId}': {}
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
      containerGroupName: 'cn-run-otel-deployment'
    }
    arguments: '-RESOURCEGROUP_NAME "${resourceGroup().name}"  -STORAGE_ACCOUNT_NAME "${storageAccountName}" -STORAGE_SHARE_NAME "${storageShareName}" -otelScriptBase64 "${configBase64}" '
  }
}

output scriptStatus string = runSqlDeployment.properties.provisioningState
