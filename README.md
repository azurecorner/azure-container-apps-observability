
# 

$subscriptionId = (Get-AzContext).Subscription.Id
az account set --subscription $subscriptionId

$resourceGroupName = "RG-ACA-OTEL-COLLECTOR"
New-AzResourceGroup -Name $resourceGroupName -Location "francecentral"

az deployment group create  --resource-group $resourceGroupName --template-file main.bicep 

#

$acrName="datasyncotelcr"
az acr login --name $acrName

cd .\src\OtelReferenceApp\
docker build -t "$acrName.azurecr.io/web-api:latest" -f .\WebApi\Dockerfile . --no-cache

docker push "$acrName.azurecr.io/web-api:latest"

docker build -t "$acrName.azurecr.io/web-app:latest" -f .\WebApp\Dockerfile . --no-cache

docker push "$acrName.azurecr.io/web-app:latest"

#

az deployment group create  --resource-group $resourceGroupName --template-file main.bicep --parameters deployApps=true



# logs

traces
| where  severityLevel >= 1              // filter Warning/Error
| where cloud_RoleName == "WeatherForecast.WebApp"
| project LogTime = timestamp,
          Message = message,
          Logger = customDimensions.CategoryName
| order by LogTime desc


traces
| where  severityLevel >= 1              // filter Warning/Error
| where cloud_RoleName == "WeatherForecast.WebApi"
| project LogTime = timestamp,
          Message = message,
          Logger = customDimensions.CategoryName
| order by LogTime desc

# traces

requests
| where cloud_RoleName == "WeatherForecast.WebApi"
| project RequestTime = timestamp,
          Name = name,
          Duration = duration,
          Success = success
| order by RequestTime desc


requests
| where cloud_RoleName == "WeatherForecast.WebApp"
| project RequestTime = timestamp,
          Name = name,
          Duration = duration,
          Success = success
| order by RequestTime desc

// External dependencies
dependencies
| where cloud_RoleName == "WeatherForecast.WebApp"
| project DepTime = timestamp,
          Target = target,
          Type = type,
          ResultCode = resultCode
| order by DepTime desc

# metrics

customMetrics
 | where cloud_RoleName == "WeatherForecast.WebApi"
 | project Time = timestamp,
            Type = "metric",
            Name = name, value


customMetrics
 | where cloud_RoleName == "WeatherForecast.WebApp"
 | project Time = timestamp,
            Type = "metric",
            Name = name, value