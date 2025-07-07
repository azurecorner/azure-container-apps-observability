

./make_collector_app_insight.sh

CONTAINER_APP_NAME=collector
RESOURCE_GROUP=honeycomb-collectordatasync

az containerapp exec \
  --name $CONTAINER_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --container collector \
  --command "ls -l /etc/otelcol && cat /etc/otelcol/config.yaml"
