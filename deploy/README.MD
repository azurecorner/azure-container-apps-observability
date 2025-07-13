
scp -i ~/.ssh/id_rsa.pem make_collector_app_insight.sh logcorner@4.180.1.130:/home/logcorner/
chmod +x make_collector_app_insight.sh

sudo apt update && sudo apt install dos2unix


dos2unix make_collector_app_insight.sh
chmod +x make_collector_app_insight.sh
./make_collector_app_insight.sh


file make_collector_app_insight.sh


./make_collector_app_insight.sh



$subscriptionId = (Get-AzContext).Subscription.Id
az account set --subscription $subscriptionId

$resourceGroupName = "RG-ACA-OTEL-COLLECTOR"
New-AzResourceGroup -Name $resourceGroupName -Location "francecentral"

az deployment group create  --resource-group $resourceGroupName --template-file main.bicep 


az deployment group create  --resource-group $resourceGroupName --template-file main.bicep --parameters deployApps=true

CONTAINER_APP_NAME=collector
RESOURCE_GROUP=honeycomb-collectordatasync

az containerapp exec \
  --name $CONTAINER_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --container collector \
  --command "ls -l /etc/otelcol && cat /etc/otelcol/config.yaml"


az containerapp logs show  --name collector  --resource-group ACA-OTEL-COLLECTOR --follow

# test

curl -v https://collector.delightfulmoss-8b2af8d8.westeurope.azurecontainerapps.io/v1/traces \
 -H "Content-Type: application/json" \
 -d '{"resourceSpans":[{"resource":{"attributes":[{"key":"service.name","value":{"stringValue":"curl-test"}}]},"scopeSpans":[{"scope":{},"spans":[]}]}]}'


# logs

traces
| where timestamp > ago(1h)
| where severityLevel >= 1              // filter Warning/Error
| project LogTime = timestamp,
          Message = message,
          Logger = customDimensions.CategoryName
| order by LogTime desc

# traces

// HTTP requests
requests
| where timestamp > ago(1h)
| project RequestTime = timestamp,
          Name = name,
          Duration = duration,
          Success = success
| order by RequestTime desc

// External dependencies
dependencies
| where timestamp > ago(1h)
| project DepTime = timestamp,
          Target = target,
          Type = type,
          ResultCode = resultCode
| order by DepTime desc

# metrics

customMetrics
  | where timestamp > ago(1h)
  | project Time = timestamp,
            Type = "metric",
            Name = name, value