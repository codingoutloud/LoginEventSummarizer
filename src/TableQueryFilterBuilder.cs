
using System;

namespace TableFilterBuilder;

/// Very limited OData filter builder - only handles creating filters that query Azure Data Tables (Storage and CosmosDb)
/// holding Windows Event Logs formatted by Azure Monitor agent as Resource logs (fka Azure Diagnostics logs).
public class TableQueryFilterBuilder
{
   ///
   /// Format is compatible with PreciseTimeStamp in Azure Table Storage for Windows Event Logs as written by
   /// Azure Diagnostic Logs (old handle) / Azure Resource Logs for Windows VMs (new handle)
   ///
   public static string GetPreciseTimeStampString(DateTimeOffset timestamp)
   {
      var preciseString = $"{timestamp.UtcDateTime:0}";
      return preciseString;
   }

   /// ***??*** DateTime instead of DateTimeOffset because this method only deals with dates, not times - assumes midnight
   public static string BuildRdpFailedLoginFilter(DateTimeOffset startAt, DateTimeOffset endBy)
   {
      const int RdpFailedLoginEventId = 4625;

      //var nullSpan = new TimeSpan(0);
      var startAtString = $"{startAt.UtcDateTime:O}"; // <== this matches PreciseTimeStamp in table storage //new DateTimeOffset(2022, 1, 1, 0, 0, 0, 0, nullSpan);
      var endByString = $"{endBy.UtcDateTime:O}"; // <== this matches PreciseTimeStamp in table storage   //new DateTimeOffset(2022, 1, 2, 0, 0, 0, 0, nullSpan);

      //var dtostr = $"{dto.UtcDateTime:O}"; // <== this matches PreciseTimeStamp in table storage

      //string startAtString = ""; //  2022-01-01T00:00:00.0000000Z
      //string endByString = "";   // "2022-01-02T00:00:00.0000000Z";

      string filter = $"(EventId eq {RdpFailedLoginEventId}) and (PreciseTimeStamp ge datetime'{startAtString}') and (PreciseTimeStamp lt datetime'{endByString}')";
      //filter = $"(EventId eq {failedLoginAttemptEventId}) and (PreciseTimeStamp ge datetime'{year}-{month:D2}-01T00:00:00.0000000Z') and (PreciseTimeStamp lt datetime'{yearUpperBound}-{(monthUpperBound):D2}-{day:D2}T00:00:00.0000000Z')";
      //LogToConsole($"FILTER ➜ {filter}");

      /*
FILTER ➜ (EventId eq 4625) and (PreciseTimeStamp ge datetime'2022-01-01T00:00:00.0000000Z') and (PreciseTimeStamp lt datetime'2022-01-02T00:00:00.0000000Z')
FILTER ➜ (EventId eq 4625) and (PreciseTimeStamp ge datetime'2022-01-01T00:00:00.0000000Z') and (PreciseTimeStamp lt datetime'2022-04-01T00:00:00.0000000Z')
      */

      return filter;
   }
}


/*

      public async Task GetAttackerDataAsCsvIpToCountryMap(int year, int month)
      {
         int monthUpperBound = month == 12 ? 1 : month + 1;
         int yearUpperBound = monthUpperBound == 1 ? year + 1 : year;

         var serviceClient = new TableServiceClient(TableConnectionString);
         var tableName = "WADWindowsEventLogsTable";
         var tableClient = serviceClient.GetTableClient(tableName);

         Pageable<TableItem> queryTableResults = serviceClient.Query(filter: $"TableName eq '{tableName}'");

         foreach (TableItem table in queryTableResults) /// DOESN'T REALLY ITERATE -- there's just one, but I guess could be ZERO of them?
         {
            // Date ge datetime'31-8-2013T14:15:14Z' and Date lt datetime'31-8-2013T14:19:10Z'";
            // Timestamp gt datetime'2012-12-10T15:00:00'

            // https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.table.tablequery.generatefilterconditionfordate?view=azure-dotnet
            const int failedLoginAttemptEventId = 4625;
            var nullSpan = new TimeSpan(0);
            var startDate = new DateTimeOffset(2022, 1, 1, 0, 0, 0, 0, nullSpan);
            var endDate = new DateTimeOffset(2022, 1, 2, 0, 0, 0, 0, nullSpan);
            ////var endDate = new DateTimeOffset(2022, 4, 1, 0, 0, 0, 0, nullSpan);


            month = 1;
            monthUpperBound = month;
            var day = 2;

            var filter = $"(EventId eq {failedLoginAttemptEventId}) and (PreciseTimeStamp ge datetime'{year}-{month:D2}-01T00:00:00.0000000Z') and (PreciseTimeStamp lt datetime'{yearUpperBound}-{(monthUpperBound):D2}-{day:D2}T00:00:00.0000000Z')";
            ////var filter = $"(EventId eq {failedLoginAttemptEventId}) and (PreciseTimeStamp ge datetime'{year}-{month:D2}-01T00:00:00.0000000Z') and (PreciseTimeStamp lt datetime'{yearUpperBound}-{(monthUpperBound):D2}-01T00:00:00.0000000Z')";

            LogToConsole($"FILTER ➜ {filter}");

            monthUpperBound = 4;
            day = 1;

            filter = $"(EventId eq {failedLoginAttemptEventId}) and (PreciseTimeStamp ge datetime'{year}-{month:D2}-01T00:00:00.0000000Z') and (PreciseTimeStamp lt datetime'{yearUpperBound}-{(monthUpperBound):D2}-{day:D2}T00:00:00.0000000Z')";
            LogToConsole($"FILTER ➜ {filter}");

            Pageable<Azure.Data.Tables.TableEntity> entities = tableClient.Query<Azure.Data.Tables.TableEntity>(filter: filter);



*/