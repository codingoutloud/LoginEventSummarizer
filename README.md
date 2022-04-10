# LoginEventSummarizer

The LoginEventSummarizer command line tool analyzes Azure Table Storage-hosted login-related Windows Event Log events and outputs a CSV summary.

## Input

This tool expects to crunch (read, parse, analyze) data in the format written to Azure Storage Tables by Azure VMs when Event Logs are written via Azure Diagnostics. Though the format differs, these logs contain the same data that Windows Server writes to the Windows Event Log when failed RDP login attempts are detected.

The focus is on IP addresses associated with failed RDP login attempts. You aim this tool at an Azure Storage account containing Azure Diagnostics logs in the expected format and it will parse out key fields (e.g., source IP data) and organize and enrich it by providing for each IP address the ISO country codes for its country of registration/origin.

## Output

The output of this tool is a CSV file. You aim this tool at an Azure Storage account containing Azure Diagnostics logs in the expected format (see Input description above) and it will parse out key fields (e.g., source IP data) and organize and enrich it by providing for each IP address the ISO country codes for its country of registration/origin.   The output is a two-column CSV with the IP address followed by the ISO country code.

## Optional Upload to a Hosting location

As an optional final step, this tool can be configured to upload the generated CSV to a blob container in Azure Storage. The CSV file hosted as an Azure Storage Blob can be configured to be accessible without any authentication which makes it especially handy to import the data (provided the data is not too large) within a Kusto query.

/Users/bill/dev/github/AttackCruncher/bin/Debug/net6.0/osx.11.0-x64/publish/AttackCruncher -t $AZURE_TABLE_STORAGE_CONNECTION_STRING

LINUX / AZURE SHELL
