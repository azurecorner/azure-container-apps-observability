param containerEnvironmentName string
param location string
param logAnalyticsCustomerId string
@secure()
param logAnalyticsSharedKey string
param tags object

resource env 'Microsoft.App/managedEnvironments@2022-03-01' = {
  name: containerEnvironmentName
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsCustomerId
        sharedKey: logAnalyticsSharedKey
      }
    }
  }
}

output containerAppEnvId string = env.id
