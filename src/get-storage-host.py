## pip3 install azure-data-tables
import sys
from azure.data.tables import TableServiceClient
connection_string = sys.argv[1]
table_client = TableServiceClient.from_connection_string(conn_str=connection_string)
print(table_client.url)