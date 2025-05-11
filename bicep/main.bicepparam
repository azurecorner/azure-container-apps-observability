using './main.bicep'
param appName  = 'datasynchro'
param deployApps  = false

param adminUserObjectId  = '7abf4c5b-9638-4ec4-b830-ede0a8031b25'

param sqlserverAdminPassword  = 'StrongP@ssw0rd'

param tags  = {
  ApplicationName: 'DataSynchro'
  Environment: 'Development'
  DeployedBy: 'Bicep'
}
