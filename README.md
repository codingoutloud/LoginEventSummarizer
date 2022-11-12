# LoginEventSummarizer

The LoginEventSummarizer command line tool analyzes Azure Table Storage-hosted login-related Windows Event Log events and outputs a CSV summary.

## Input

This tool expects to crunch (read, parse, analyze) data in the format written to Azure Storage Tables by Azure VMs when Event Logs are written via Azure Diagnostics. Though the format differs, these logs contain the same core data that Windows Server writes to the Windows Event Log when failed RDP login attempts are detected.

The focus is on IP addresses associated with failed RDP login attempts. You aim this tool at an Azure Storage account containing Azure Diagnostics logs in the expected format and it will parse out key fields (e.g., source IP data) and organize and enrich it by providing for each IP address the ISO country codes for its country of registration/origin. 

## Output

This tool outputs two CSV files. You aim this tool at an Azure Storage account containing Azure Diagnostics logs in the expected format and it will parse out key fields (e.g., source IP data) and organize and enrich it by providing for each IP address the ISO country codes for its country of registration/origin. The first file output (default name: ipcc.csv) is a two-column CSV with the IP address followed by the ISO country code. The second file output (default name: details.csv) contains more details, including one row for each failed RDP login security event.

The ISO country codes are sourced using an Azure Maps API that returns the ISO country code for a given IP address; note that this data may not be 100% reliable and may not definitively correlate with the country from which any given security event or attack originated.

## Optional Upload to a Hosting location

As an optional final step, this tool can be configured to upload the generated CSV files to a blob container in Azure Storage. The CSV file hosted as an Azure Storage Blob can be configured to be accessible without any authentication which makes it especially handy to import the data (provided the data is not too large) within a Kusto query.

## Testing Notes

Tested on MacOS
