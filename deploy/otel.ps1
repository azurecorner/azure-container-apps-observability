$RESOURCE_GROUP="RG-CONTAINER-APPS-OBSERVABILITY"
$ENVIRONMENT_NAME="envdatasynchroaca"
$LOCATION="France Central"
$STORAGE_ACCOUNT_NAME="otelstdatasynchroaca"
$STORAGE_SHARE_NAME="collector-config"
$STORAGE_MOUNT_NAME="configmount"
$CONTAINER_APP_NAME="collector"
$COLLECTOR_IMAGE="otel/opentelemetry-collector"
$HONEYCOMB_API_KEY="HQYf2deaIecXtxmDaYjs9c"

$STORAGE_ACCOUNT_KEY=(az storage account keys list -n $STORAGE_ACCOUNT_NAME --query "[0].value" -o tsv)


az containerapp env storage set `
  --access-mode ReadWrite `
  --azure-file-account-name $STORAGE_ACCOUNT_NAME `
  --azure-file-account-key $STORAGE_ACCOUNT_KEY `
  --azure-file-share-name $STORAGE_SHARE_NAME `
  --storage-name $STORAGE_MOUNT_NAME `
  --name $ENVIRONMENT_NAME `
  --resource-group $RESOURCE_GROUP


  az containerapp create `
  --name $CONTAINER_APP_NAME `
  --resource-group $RESOURCE_GROUP `
  --environment $ENVIRONMENT_NAME `
  --image $COLLECTOR_IMAGE `
  --min-replicas 1 `
  --max-replicas 1 `
  --target-port 4318 `
  --ingress external `
  --secrets "honeycomb-api-key=$HONEYCOMB_API_KEY" `
  --env-vars "HONEYCOMB_API_KEY=secretref:honeycomb-api-key" "HONEYCOMB_LOGS_DATASET=azure-logs"



  az containerapp show --name $CONTAINER_APP_NAME  --resource-group $RESOURCE_GROUP   --output yaml > app.yaml



  az containerapp update  --name $CONTAINER_APP_NAME  --resource-group $RESOURCE_GROUP   --yaml app.yaml



  az containerapp ingress show  --name $CONTAINER_APP_NAME  --resource-group $RESOURCE_GROUP   --query "fqdn" -o tsv