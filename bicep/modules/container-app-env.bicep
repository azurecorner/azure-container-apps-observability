@description('The name of the Container App Environment that will be deployed')
param containerAppEnvironmentName string

@description('The name of the Log Analytics workspace that this Container App environment sends logs to')
param logAnalyticsName string

@description('The name of the App Insights that this Container App Environment will send Dapr logs to')
param appInsightsName string

@description('The location that the Container App Environment will be deployed')
param location string

@description('The tags that will be applied to the Container App Environment')
param tags object

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' existing = {
  name: logAnalyticsName
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: appInsightsName
}

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2025-01-01' = {
  name: containerAppEnvironmentName
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
   
  }
}

@description('The name of the Container App Environment')
output containerAppEnvName string = containerAppEnvironment.name

@description('The resource Id of the Container App Environment')
output containerAppEnvId string = containerAppEnvironment.id
