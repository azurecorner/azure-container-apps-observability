
$acrName="bgcontainerregistry"
az acr login --name $acrName

cd .\src\OtelReferenceApp\
docker build -t "$acrName.azurecr.io/web-api:latest" -f .\WebApi\Dockerfile . --no-cache


docker push "$acrName.azurecr.io/web-api:latest"

