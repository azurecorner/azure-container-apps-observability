# azure-container-apps-observability

# https://github.com/denniszielke/serverless-job-engine

# https://denniszielke.medium.com/understanding-your-application-performance-with-custom-metrics-in-azure-container-apps-aee0ccc2bb85

#  Exploring OpenTelemetry Agent support in Azure Container Apps

  https://youtu.be/CjU-HxmsGBk?si=Ky6lcBOxNscL0qQn

  https://github.com/willvelida/lets-build-aca

# deploy

$subscriptionId= (Get-AzContext).Subscription.id 

az account set --subscription $subscriptionId 

$resourceGroupName="RG-CONTAINER-APPS-OBSERVABILITY"

New-AzResourceGroup -Name $resourceGroupName -Location "francecentral" 
 
New-AzResourceGroupDeployment -Name "container-apps-observability-001" -ResourceGroupName $resourceGroupName -TemplateFile main.bicep -TemplateParameterFile bicepparam.json -DeploymentDebugLogLevel All