import sys
from azure.data.tables import TableServiceClient  # pip3 install azure-data-tables

connection_string = sys.argv[1]
table_client = TableServiceClient.from_connection_string(
    conn_str=connection_string)

print(table_client._primary_hostname)
