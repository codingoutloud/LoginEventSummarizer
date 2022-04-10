# BACKLOG items

- ✅ &nbsp; Parameterize the required Azure Map API access key
- ✅ &nbsp; Parameterize the required source Azure Table Storage connection string
- ⬜️ &nbsp; Make the destination Azure Blob Storage connection string optional (currently is required)
- ⬜️ &nbsp; Parameterize the optional output CSV filename (currently is hard-coded)
- ⬜️ &nbsp; Parameterize the optional time range considered for summarization (currently is hard-coded)
- ⬜️ &nbsp; Enhance stats.sh to not hard-code the CSV filename (other scripts use environment variable)
- ⬜️ &nbsp; Enhance to support CSV file generation that sums up the number of security events by IP
- ⬜️ &nbsp; Enhance to support CSV file generation that sums up the number of security events by country
- ⬜️ &nbsp; Enhance to support CSV file generation that outputs the individual security events
- ⬜️ &nbsp; Consider if Record or Struct datatype might be more natural for AzureMapLocation (currently uses 'class')
- ⬜️ &nbsp; Design a more proper IpAddressCategorizer: current iteration is messy, uneven, perhaps unclear goal
- ⬜️ &nbsp; Consider retiring this BACKLOG file and switching to Github Issues
- ✅ &nbsp; Solve the "Link ... contains a language reference: undefined" markdown warning (currently 2 instances) ➜ Work-around was to either change the .../us-en/... to .../us-EN/... or remove it from the URL. Seems like a bug in the VS Code markdown parser/linter?
- ⬜️ &nbsp; Reconsider client-side secret approach (currently config-secrets.sh stores secrets in the clear - but of course is not checked in with code)
