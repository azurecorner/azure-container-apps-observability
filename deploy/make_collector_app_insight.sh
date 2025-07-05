#!/bin/bash
set -euo pipefail

SUFFIX="datasync"

# Base names & image
export RESOURCE_GROUP="honeycomb-collector$SUFFIX"
export ENVIRONMENT_NAME="collector-env$SUFFIX"
export LOCATION="uksouth"
export STORAGE_ACCOUNT_NAME="otelappst$SUFFIX"
export STORAGE_SHARE_NAME="collector-config"
export STORAGE_MOUNT_NAME="configmount"
export CONTAINER_APP_NAME="collector"
export COLLECTOR_IMAGE="otel/opentelemetry-collector-contrib"

# App Insights
export APPINSIGHTS_NAME="collector-ai$SUFFIX"

echo "1/ Creating Resource Group"
az group create \
  --name "$RESOURCE_GROUP" \
  --location "$LOCATION" \
  --output none

echo "2/ Creating Application Insights"
az monitor app-insights component create \
  --app "$APPINSIGHTS_NAME" \
  --location "$LOCATION" \
  --resource-group "$RESOURCE_GROUP" \
  --application-type web \
  --kind web \
  --output none

echo "3/ Retrieving App Insights connection string"
export APPINSIGHTS_CONN_STRING=$(az monitor app-insights component show \
  --app "$APPINSIGHTS_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --query connectionString -o tsv)
echo "   ConnString starts with: ${APPINSIGHTS_CONN_STRING:0:20}..."

echo "4/ Generating config.yaml (preserve runtime env-vars)"
cat > config.yaml << 'EOF'
receivers:
  otlp:
    protocols:
      grpc:
      http:

processors:
  batch:
  attributes/collector_info:
    actions:
      - key: collector.hostname
        value: $HOSTNAME
        action: insert
      - key: azure.container_app.revision
        value: $CONTAINER_APP_REVISION
        action: insert
      - key: azure.container_app.name
        value: $CONTAINER_APP_NAME
        action: insert
  filter/healthcheck:
    spans:
      exclude:
        match_type: strict
        attributes:
          - key: http.target
            value: /health

exporters:
  debug:
    verbosity: detailed
  azuremonitor:
    connection_string: "${APPINSIGHTS_CONN_STRING}"
  logging:
    loglevel: debug

service:
  telemetry:
    logs:
      level: debug
  pipelines:
    traces:
      receivers: [otlp]
      processors: [batch, attributes/collector_info, filter/healthcheck]
      exporters: [debug, azuremonitor, logging]
    metrics:
      receivers: [otlp]
      processors: [batch, attributes/collector_info]
      exporters: [azuremonitor, logging]
    logs:
      receivers: [otlp]
      processors: [batch, attributes/collector_info]
      exporters: [azuremonitor, logging]
EOF
echo "   config.yaml generated."

echo "5/ Creating Storage Account"
az storage account create \
  --resource-group "$RESOURCE_GROUP" \
  --name "$STORAGE_ACCOUNT_NAME" \
  --location "$LOCATION" \
  --kind StorageV2 \
  --sku Standard_LRS \
  --enable-large-file-share \
  --output none

echo "6/ Creating File Share"
az storage share-rm create \
  --resource-group "$RESOURCE_GROUP" \
  --storage-account "$STORAGE_ACCOUNT_NAME" \
  --name "$STORAGE_SHARE_NAME" \
  --quota 1024 \
  --enabled-protocols SMB \
  --output none

echo "7/ Fetching Storage Account key"
export STORAGE_ACCOUNT_KEY=$(az storage account keys list \
  --resource-group "$RESOURCE_GROUP" \
  --account-name "$STORAGE_ACCOUNT_NAME" \
  --query "[0].value" -o tsv)

echo "8/ Uploading config.yaml"
az storage file upload \
  --share-name "$STORAGE_SHARE_NAME" \
  --source config.yaml \
  --account-name "$STORAGE_ACCOUNT_NAME" \
  --account-key "$STORAGE_ACCOUNT_KEY" \
  --output none

echo "9/ Creating Container Apps environment"
az containerapp env create \
  --name "$ENVIRONMENT_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --location "$LOCATION" \
  --output none

echo "10/ Mounting Azure File share"
az containerapp env storage set \
  --name "$ENVIRONMENT_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --storage-name "$STORAGE_MOUNT_NAME" \
  --azure-file-account-name "$STORAGE_ACCOUNT_NAME" \
  --azure-file-account-key "$STORAGE_ACCOUNT_KEY" \
  --azure-file-share-name "$STORAGE_SHARE_NAME" \
  --access-mode ReadWrite \
  --output none

echo "11/ Deploying Container App (no volumes yet)"
az containerapp create \
  --name "$CONTAINER_APP_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --environment "$ENVIRONMENT_NAME" \
  --image "$COLLECTOR_IMAGE" \
  --ingress external \
  --target-port 4318 \
  --min-replicas 1 \
  --max-replicas 1 \
  --env-vars "CONTAINER_APP_NAME=$CONTAINER_APP_NAME" "CONTAINER_APP_REVISION=rev1" \
  --output none

echo "12/ Fetching existing Container App YAML"
az containerapp show \
  --name "$CONTAINER_APP_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --output yaml > app.yaml

echo "13/ Patching volume, mount, command & args in app.yaml"
yq -i '
  .properties.template.volumes[0].name = "config" |
  .properties.template.volumes[0].storageName = strenv(STORAGE_MOUNT_NAME) |
  .properties.template.volumes[0].storageType = "AzureFile" |
  .properties.template.containers[0].volumeMounts[0].volumeName = "config" |
  .properties.template.containers[0].volumeMounts[0].mountPath = "/etc/otelcol" |
  del(.properties.configuration.secrets)
' app.yaml

echo "14/ Applying updated configuration"
az containerapp update \
  --name "$CONTAINER_APP_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --yaml app.yaml \
  --output none

echo "15/ Extracting FQDN for OTLP endpoint"
FQDN=$(yq '.properties.configuration.ingress.fqdn' app.yaml -r)
export OTEL_EXPORTER_OTLP_ENDPOINT="https://${FQDN}/v1/traces"

cat << EOF

====== DEPLOYMENT COMPLETE ======

Collector OTLP endpoint: ${OTEL_EXPORTER_OTLP_ENDPOINT}

To test from your CLI:
  export OTEL_EXPORTER_OTLP_ENDPOINT=${OTEL_EXPORTER_OTLP_ENDPOINT}
  otel-cli span --service "CLI" \\
    --name "CollectorTest" \\
    --start \$(date +%s.%N) \\
    --end   \$(date +%s.%N) \\
    --verbose

HAPPY TRACING!

EOF

# Cleanup
rm -f config.yaml app.yaml

echo "Done."
