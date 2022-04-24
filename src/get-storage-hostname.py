## pip3 install azure-data-tables azure-storage-blob

import sys
from azure.data.tables import TableServiceClient
from azure.storage.blob import BlobServiceClient

connection_string = sys.argv[1]

table_client = TableServiceClient.from_connection_string(conn_str=connection_string)
blob_client = BlobServiceClient.from_connection_string(conn_str=connection_string)

print(table_client._primary_hostname)
print(blob_client.primary_hostname)
