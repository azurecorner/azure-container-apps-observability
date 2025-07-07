
scp -i ~/.ssh/id_rsa.pem make_collector_app_insight.sh logcorner@4.180.1.130:/home/logcorner/
chmod +x make_collector_app_insight.sh

sudo apt update && sudo apt install dos2unix


dos2unix make_collector_app_insight.sh
chmod +x make_collector_app_insight.sh
./make_collector_app_insight.sh


file make_collector_app_insight.sh


./make_collector_app_insight.sh




az deployment group create  --resource-group appinsight-collectordatasync --template-file main.bicep 




CONTAINER_APP_NAME=collector
RESOURCE_GROUP=honeycomb-collectordatasync

az containerapp exec \
  --name $CONTAINER_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --container collector \
  --command "ls -l /etc/otelcol && cat /etc/otelcol/config.yaml"


az containerapp logs show  --name collector  --resource-group honeycomb-collectordatasync --follow
