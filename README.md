# azure-container-apps-observability

# https://github.com/denniszielke/serverless-job-engine

# https://denniszielke.medium.com/understanding-your-application-performance-with-custom-metrics-in-azure-container-apps-aee0ccc2bb85

#  Exploring OpenTelemetry Agent support in Azure Container Apps

  https://youtu.be/CjU-HxmsGBk?si=Ky6lcBOxNscL0qQn

  https://github.com/willvelida/lets-build-aca

# How to monitor applications by using OpenTelemetry on Azure Container Apps

  https://techcommunity.microsoft.com/blog/fasttrackforazureblog/how-to-monitor-applications-by-using-opentelemetry-on-azure-container-apps/4235035#community-4235035-4-forwarding-diag

# Running the OpenTelemetry Collector in Azure Container Apps  ==> GOOD

  https://www.honeycomb.io/blog/opentelemetry-collector-azure-container-apps

  https://medium.com/ingeniouslysimple/deploy-and-configure-an-opentelemetry-collector-in-azure-via-terraform-0c4941962f1c

# OpenTelemetry on Azure Container Apps Revisited - Managed OpenTelemetry Agent
  https://blog.depechie.com/posts/2024-05-05-opentelemetry-on-azure-container-apps-revisited/

# deploy

$subscriptionId= (Get-AzContext).Subscription.id 

az account set --subscription $subscriptionId 

$resourceGroupName="RG-CONTAINER-APPS-OBSERVABILITY"

New-AzResourceGroup -Name $resourceGroupName -Location "francecentral" 

New-AzResourceGroupDeployment -Name "container-apps-observability-001" -ResourceGroupName $resourceGroupName -TemplateFile main.bicep  -DeploymentDebugLogLevel All



 
New-AzResourceGroupDeployment -Name "container-apps-observability-001" -ResourceGroupName $resourceGroupName -TemplateFile main.bicep -TemplateParameterFile bicepparam.json -DeploymentDebugLogLevel All




az containerapp show --name dayasync-weatherforecast-api  --resource-group RG-CONTAINER-APPS-OBSERVABILITY


az containerapp logs show --name dayasync-weatherforecast-api  --resource-group RG-CONTAINER-APPS-OBSERVABILITY --follow

az containerapp show \
  --name dayasync-weatherforecast-api \
  --resource-group RG-CONTAINER-APPS-OBSERVABILITY \
  --query "properties.template.containers[0].image"


az containerapp revision list \
  --name dayasync-weatherforecast-api \
  --resource-group RG-CONTAINER-APPS-OBSERVABILITY \
  --query "[].{Name:name, State:properties.active, Reason:properties.healthState, Conditions:properties.conditions}" \
  --output json

 az containerapp show \
  --name dayasync-weatherforecast-api \
  --resource-group RG-CONTAINER-APPS-OBSERVABILITY \
  --query "properties.provisioningState"
"Failed"

