
// param location string = resourceGroup().location
// param logAnalyticsWorkspaceName string = 'bg-log-analytics'
// param appInsightsName string = 'otel-insights'
// param containerAppEnvName string = 'blueground-9681b7a3'
// param containerRegistryName string = 'datasyncotelcr'
// param deployApps bool =false
// param runScript string = loadTextContent('./scripts/run.ps1')
// var configBase64Raw = loadTextContent('./config/config.yaml')
// var configBase64 = base64(configBase64Raw)

// module frontend 'modules/web-api.bicep' = if (deployApps) {
//   name: 'web-app'
//   params: {
//     containerAppEnvName: containerAppEnvName
//     containerRegistryName: containerRegistry.outputs.name
//     location: location
//     userAssignedIdentityName: managedIdentity.name
//     imageName: '${containerRegistry.outputs.serverName}/web-api:latest'
//     oltp_endpoind: otelcollector.outputs.containerAppFqdn
//    }
// }
 
