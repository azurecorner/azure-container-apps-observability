param containerEnvironmentName string
param location string
param logAnalyticsCustomerId string
@secure()
param logAnalyticsSharedKey string
param tags object

resource containerEnvironment 'Microsoft.App/managedEnvironments@2025-01-01' = {
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

@description('The name of the Container App Environment')
output containerEnvironmentName string = containerEnvironment.name

@description('The resource Id of the Container App Environment')
output containerEnvironmentId string = containerEnvironment.id

