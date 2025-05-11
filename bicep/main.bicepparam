using './main.bicep'
param appName  = 'datasynchro'
param deployApps  = true

param adminUserObjectId  = 'XXXXXXXXXXXXXXXXXXXXXXXXXXXXX'

param sqlserverAdminPassword  = 'StrongP@ssw0rd'

param tags  = {
  ApplicationName: 'DataSynchro'
  Environment: 'Development'
  DeployedBy: 'Bicep'
}
