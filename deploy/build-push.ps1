
$acrName="acrdatasync001"
az acr login --name $acrName

docker build -t "$acrName.azurecr.io/weatherforecast-web-api:latest" . -f .\WeatherForecast.WebApi\Dockerfile --no-cache

docker push "$acrName.azurecr.io/weatherforecast-web-api:latest"


docker build -t "$acrName.azurecr.io/weatherforecast-web-app:latest" . -f .\WeatherForecast.WebApp\Dockerfile --no-cache

docker push "$acrName.azurecr.io/weatherforecast-web-app:latest"
