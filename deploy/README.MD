
scp -i ~/.ssh/id_rsa.pem make_collector_app_insight.sh logcorner@4.180.1.130:/home/logcorner/
chmod +x make_collector_app_insight.sh

sudo apt update && sudo apt install dos2unix


dos2unix make_collector_app_insight.sh
chmod +x make_collector_app_insight.sh
./make_collector_app_insight.sh


file make_collector_app_insight.sh


./make_collector_app_insight.sh


az containerapp logs show  --name collector  --resource-group honeycomb-collectordatasync --follow
