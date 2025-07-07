#!/bin/bash

set -euo pipefail

# === 1. Configuration ===
RESOURCE_GROUP="honeycomb-collectordatasync"
LOCATION="uksouth"
ENVIRONMENT_NAME="blueground-9681b7a3"
CONTAINER_APP_NAME="collector"
APP_INSIGHTS_NAME="otel-insights"

STORAGE_ACCOUNT_NAME="bgsharedstorage"
FILE_SHARE_NAME="otelcollector"
MOUNT_PATH="/etc/otelcol"
CONFIG_FILE_LOCAL="./otelcol-config.yaml"
CONFIG_FILE_REMOTE="config.yaml"

# === 2. Create Resource Group ===
az group create --name "$RESOURCE_GROUP" --location "$LOCATION"

# === 3. Create App Insights (if not exists) ===
az monitor app-insights component create \
  --app "$APP_INSIGHTS_NAME" \
  --location "$LOCATION" \
  --resource-group "$RESOURCE_GROUP" \
  --application-type web || true

APPINSIGHTS_CONN_STRING=$(az monitor app-insights component show \
  --app "$APP_INSIGHTS_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --query connectionString -o tsv)

# === 4. Create Azure Storage Account & File Share ===
az storage account create \
  --name "$STORAGE_ACCOUNT_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --location "$LOCATION" \
  --sku Standard_LRS || true

STORAGE_KEY=$(az storage account keys list \
  --resource-group "$RESOURCE_GROUP" \
  --account-name "$STORAGE_ACCOUNT_NAME" \
  --query '[0].value' -o tsv)

az storage share-rm create \
  --resource-group "$RESOURCE_GROUP" \
  --storage-account "$STORAGE_ACCOUNT_NAME" \
  --name "$FILE_SHARE_NAME" || true

# === 5. Upload OpenTelemetry config ===
az storage file upload \
  --account-name "$STORAGE_ACCOUNT_NAME" \
  --account-key "$STORAGE_KEY" \
  --share-name "$FILE_SHARE_NAME" \
  --source "$CONFIG_FILE_LOCAL" \
  --path "$CONFIG_FILE_REMOTE"

# === 6. Create Container Apps environment ===
az containerapp env create \
  --name "$ENVIRONMENT_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --location "$LOCATION" || true

# === 7. Create Container App with Azure File volume ===

az containerapp create \
  --name "$CONTAINER_APP_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --environment "$ENVIRONMENT_NAME" \
  --image otel/opentelemetry-collector-contrib:0.98.0 \
  --cpu 0.5 \
  --memory 1.0Gi \
  --ingress external \
  --target-port 4317 \
  --min-replicas 1 \
  --max-replicas 1 \
  --env-vars APPINSIGHTS_CONN_STRING="$APPINSIGHTS_CONN_STRING" \
  --volume-mount name=config,mount-path="$MOUNT_PATH" \
  --storage-mount name=config,storage-type=AzureFile,account-name="$STORAGE_ACCOUNT_NAME",share-name="$FILE_SHARE_NAME",access-key="$STORAGE_KEY",mount-path="$MOUNT_PATH" \
  --args "--config=${MOUNT_PATH}/$CONFIG_FILE_REMOTE"
