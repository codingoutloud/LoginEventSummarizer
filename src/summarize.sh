#!/bin/bash

# https://stackoverflow.com/a/50065002/306430
GREEN=$'\e[0;32m'
RED=$'\e[0;31m'
NC=$'\e[0m'
#GREEN=$(tput setaf 2)
#RED=$(tput setaf 1)
#NC=$(tput sgr0)

# build before running to avoid compilation warning messages in CSV file
#dotnet clean
dotnet build

if [ -f "config-secrets.sh" ]; then
   echo "WE GOOD! config-secrets.sh exists" # load key values
else
   echo "config-secrets.sh does NOT exist"
   grep "##" config-secrets-template.sh
   exit 1
fi

if [ $? -eq 0 ]; then
   ./config-secrets.sh # load key values
   . config-secrets.sh # load key values

   env

   AZURE_TABLE_STORAGE_HOST=$(python3 get-table-hostname.py $AZURE_TABLE_STORAGE_CONNECTION_STRING)

   echo "Azure Table host name = $AZURE_TABLE_STORAGE_HOST"
   dig $AZURE_TABLE_STORAGE_HOST +short
   host $AZURE_TABLE_STORAGE_HOST

   # you can leave these as is or modify to your liking
   export CSV_FILENAME="ipcc.csv"
   export AZURE_BLOB_STORAGE_CONTAINER_NAME="downloads"

   # assumed to output CSV format to stdout (when successful)
###   dotnet run -- --table-cs $AZURE_TABLE_STORAGE_CONNECTION_STRING --blob-cs $AZURE_BLOB_STORAGE_CONNECTION_STRING --map-key $AZURE_MAP_API_KEY > $CSV_FILENAME
   echo "dotnet run -- --tablecs $AZURE_TABLE_STORAGE_CONNECTION_STRING --blobcs $AZURE_BLOB_STORAGE_CONNECTION_STRING --mapkey $AZURE_MAP_API_KEY"
   dotnet run -- --tablecs $AZURE_TABLE_STORAGE_CONNECTION_STRING --blobcs $AZURE_BLOB_STORAGE_CONNECTION_STRING --mapkey $AZURE_MAP_API_KEY > $CSV_FILENAME

   if [ $? -eq 0 ]; then
      echo "${GREEN}Run completed. Let's upload.${NC}"
      ./upload.sh
      echo "${GREEN}Upload completed. Here are some stats.${NC}"
      ./stats.sh
      echo "${GREEN}All done.${NC}"
   else
      echo "${RED}Generation of $CSV_FILENAME failed.${NC}"
   fi

else
   echo "${RED}The build failed.${NC}"
fi
