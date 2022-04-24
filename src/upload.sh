#!/bin/bash

STORAGE_NAME=$(echo $AZURE_BLOB_STORAGE_CONNECTION_STRING | sed 's/.*AccountName=//' | sed 's/;.*//')

az storage blob upload --container-name $AZURE_BLOB_STORAGE_CONTAINER_NAME --file $CSV_FILENAME --name $CSV_FILENAME --content-type text/csv --connection-string $AZURE_BLOB_STORAGE_CONNECTION_STRING
az storage blob list --connection-string $AZURE_BLOB_STORAGE_CONNECTION_STRING --container-name $AZURE_BLOB_STORAGE_CONTAINER_NAME # --query '[].name' -o tsv

echo "DOWNLOAD URL:"
echo "https://${STORAGE_NAME}.blob.core.windows.net/${AZURE_BLOB_STORAGE_CONTAINER_NAME}/${CSV_FILENAME}"
