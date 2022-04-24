#define VERBOSE

using System;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Azure.Core.Pipeline;
using Azure.Core;

///using AzureEventSourceListener listener = AzureEventSourceListener.CreateConsoleLogger();

namespace EventDownloader
{
   public class RetryLogger : HttpPipelinePolicy
   {
      /// <inheritdoc />
      public override void Process(HttpMessage message, ReadOnlyMemory<HttpPipelinePolicy> pipeline)
      {
        ///??? ProcessAsync(message, pipeline, false).EnsureCompleted();
         //ProcessNext(message, pipeline).EnsureCompleted();
      }

      /// <inheritdoc />
      public override ValueTask ProcessAsync(HttpMessage message, ReadOnlyMemory<HttpPipelinePolicy> pipeline)
      {
         return ProcessAsync(message, pipeline, true);

         // await ProcessNextAsync(message, pipeline).ConfigureAwait(false);
      }

      static int retryCount = 0;

      private async ValueTask ProcessAsync(HttpMessage message, ReadOnlyMemory<HttpPipelinePolicy> pipeline, bool async)
      {
         await Console.Error.WriteLineAsync($"WE HAVE ANOTHER RETRY (#{++retryCount}): {message.Request.ToString()}");
         throw new ArgumentException("TOAST!");

         ///if (message.Request.Uri.Scheme != Uri.UriSchemeHttps)
         {
            ///throw new InvalidOperationException("Bearer token authentication is not permitted for non TLS protected (https) endpoints.");
         }

/*
         if (async)
         {
            //await AuthorizeRequestAsync(message).ConfigureAwait(false);
            await ProcessNextAsync(message, pipeline).ConfigureAwait(false);
         }
         else
         {
            //AuthorizeRequest(message);
            ProcessNext(message, pipeline);
         }
*/
      }
   }




   public class TableStorageEventExporter
   {
      public readonly record struct FailedLoginEvent(DateTimeOffset timestamp, string ts, string loginName, string attackerIp, string countrycode);

      private string TableConnectionString;
      private Dictionary<string, string> IpCountryDictionary;
      private HttpClient HttpClient;
      private string AzureMapApiKey;

      public TableStorageEventExporter(string tableConnectionString, string azureMapApiKey)
      {
         TableConnectionString = tableConnectionString;
         AzureMapApiKey = azureMapApiKey;
         IpCountryDictionary = new Dictionary<string, string>();
         HttpClient = new HttpClient();

         LogToConsole($"table cs = {TableConnectionString}", ConsoleColor.Red); // 992223
      }

      public async Task<string?> IpToCountry(string ip)
      {
         if (IpAddressCategorizer.Routing.IsPrivate(ip))
         {
            return ip;
         }

         var url = $"https://atlas.microsoft.com/geolocation/ip/json?subscription-key={AzureMapApiKey}&api-version=1.0&ip={ip}";

         AzureMapLocation? loc = await HttpClient.GetFromJsonAsync<AzureMapLocation>(url);

         if (loc == null)
         {
            LogToConsole($"Ruh roh! {ip}", ConsoleColor.Red); // 992223
         }

         return loc?.CountryRegion?.IsoCode;
      }

      static int x = 0;

      ///<Summary>
      /// Azure Table Storage ("Tables") is a wide-column NoSQL database. The Tables data model
      /// (<see href="https://docs.microsoft.com/en-us/rest/api/storageservices/understanding-the-table-service-data-model">documented here</see>)
      /// refers to entries (or "rows" to use SQL analogy) as "Entities" and
      /// each entity is made up of "Properties" (3 standard ones + up to 252 custom ones).
      /// This method parses the properties for Event Log types it knows about and outputs findings as one CSV row.
      ///</Summary>
      public async Task ParseXmlProperty(DateTimeOffset precise, string xml)
      {
         XDocument xdoc = XDocument.Parse(xml);
         XNamespace ns = "http://schemas.microsoft.com/win/2004/08/events/event";
         XElement? eventElement = xdoc.Element(ns + "Event");
         if (eventElement == null)
         {
            LogToConsole($"Null EventElement for {xml}", ConsoleColor.Red);
            return;
         }
         XElement? systemElement = eventElement.Element(ns + "System");
         XElement? eventDataElement = eventElement.Element(ns + "EventData");

         var eventId = systemElement!.Element(ns + "EventID")!.Value;
         var timeCreated = systemElement.Element(ns + "TimeCreated")!.Attribute("SystemTime")!.Value;

         if (eventId == "4625") // probably will be handled in query/filter
         {
            var ipAddress = new XElement(ns + "IpAddress",
               from e in eventDataElement!.Elements()
               where (e.Attribute("Name")!.Value == "IpAddress")
               select e.Value
            );

            var targetUserName = new XElement(ns + "TargetUserName",
               from e in eventDataElement.Elements()
               where (e.Attribute("Name")!.Value == "TargetUserName")
               select e.Value
            );

            var status = new XElement(ns + "Status",
               from e in eventDataElement.Elements()
               where (e.Attribute("Name")!.Value == "Status")
               select e.Value
            );

            var failureReason = new XElement(ns + "FailureReason",
               from e in eventDataElement.Elements()
               where (e.Attribute("Name")!.Value == "FailureReason")
               select e.Value
            );

            var subStatus = new XElement(ns + "SubStatus",
               from e in eventDataElement.Elements()
               where (e.Attribute("Name")!.Value == "SubStatus")
               select e.Value
            );

            string? ipCountry;

            if (!IpCountryDictionary.TryGetValue(ipAddress.Value, out ipCountry))
            {
               // this is first time we've seen this IP address, so...

               // let's look up and track its associated country
               ipCountry = await IpToCountry(ipAddress.Value);
               if (ipCountry is not null)
               {
                  IpCountryDictionary.Add(ipAddress.Value, ipCountry);
                  // output first time in CSV format - this may be redirected from stdout
                  Console.WriteLine($"{ipAddress.Value}, {ipCountry}");
                  // do this only if stdout is being redirected
#if VERBOSE
                  if (Console.IsOutputRedirected)
                  {
                     LogToConsole($"IP/CC was added [IP FIRST ENCOUNTER] {ipAddress.Value}, {ipCountry}>");
                  }
#endif
               }
               else
               {
                  // should only happen if CC not available for this IP
                  LogToConsole($"IP/CC not added [IP CC LOOKUP FAILED] {ipAddress.Value} <{IpCountryDictionary[ipAddress.Value]}>      [{x++}/{IpCountryDictionary.Count}]", ConsoleColor.Red);
               }
            }
            else
            {
#if VERBOSE
               // happens A LOT - very normal
               LogToConsole($"IP/CC not added [IP ALREADY KNOWN] {ipAddress.Value} <{IpCountryDictionary[ipAddress.Value]}>      [{x++}/{IpCountryDictionary.Count}]");
#endif
            }
         }
      }

      //  var(first, middle, last) = LookupName(id1); // var outside
      // (string first, string middle, string last) LookupName(long id) // tuple elements have names
      //       public readonly record struct FailedLoginEvent(DateTimeOffset time, string loginName, string attackerIp);
      // public readonly record struct FailedLoginEvent(DateTimeOffset timestamp, string loginName, string attackerIp, string countrycode);


      ///<Summary>
      /// Azure Table Storage ("Tables") is a wide-column NoSQL database. The Tables data model
      /// (<see href="https://docs.microsoft.com/en-us/rest/api/storageservices/understanding-the-table-service-data-model">documented here</see>)
      /// refers to entries (or "rows" to use SQL analogy) as "Entities" and
      /// each entity is made up of "Properties" (3 standard ones + up to 252 custom ones).
      /// This method parses the properties for Event Log types it knows about and outputs findings as one CSV row.
      ///</Summary>
      public async Task<FailedLoginEvent?> ParseFailedLoginEventEntityXml(DateTimeOffset precise, string xml)
      {
         XDocument xdoc = XDocument.Parse(xml);
         XNamespace ns = "http://schemas.microsoft.com/win/2004/08/events/event";
         XElement? eventElement = xdoc.Element(ns + "Event");
         if (eventElement == null)
         {
            LogToConsole($"Null EventElement for {xml}", ConsoleColor.Red);
            return null;
         }
         XElement? systemElement = eventElement.Element(ns + "System");
         XElement? eventDataElement = eventElement.Element(ns + "EventData");

         var eventId = systemElement!.Element(ns + "EventID")!.Value;
         var timeCreated = systemElement.Element(ns + "TimeCreated")!.Attribute("SystemTime")!.Value;

         if (eventId == "4625") // probably will be handled in query/filter
         {
            var ipAddress = new XElement(ns + "IpAddress",
               from e in eventDataElement!.Elements()
               where (e.Attribute("Name")!.Value == "IpAddress")
               select e.Value
            );

            var targetUserName = new XElement(ns + "TargetUserName",
               from e in eventDataElement.Elements()
               where (e.Attribute("Name")!.Value == "TargetUserName")
               select e.Value
            );

            var status = new XElement(ns + "Status",
               from e in eventDataElement.Elements()
               where (e.Attribute("Name")!.Value == "Status")
               select e.Value
            );

            var failureReason = new XElement(ns + "FailureReason",
               from e in eventDataElement.Elements()
               where (e.Attribute("Name")!.Value == "FailureReason")
               select e.Value
            );

            var subStatus = new XElement(ns + "SubStatus",
               from e in eventDataElement.Elements()
               where (e.Attribute("Name")!.Value == "SubStatus")
               select e.Value
            );

            string? ipCountry;

            if (!IpCountryDictionary.TryGetValue(ipAddress.Value, out ipCountry))
            {
               // this is first time we've seen this IP address, so...

               // let's look up and track its associated country
               ipCountry = await IpToCountry(ipAddress.Value);
               if (ipCountry is not null)
               {
                  IpCountryDictionary.Add(ipAddress.Value, ipCountry);
                  // output first time in CSV format - this may be redirected from stdout
                  Console.WriteLine($"{ipAddress.Value}, {ipCountry}");
                  // do this only if stdout is being redirected
#if VERBOSE
                  if (Console.IsOutputRedirected)
                  {
                     LogToConsole($"IP/CC was added [IP FIRST ENCOUNTER] {ipAddress.Value}, {ipCountry}>");
                  }
#endif
               }
               else
               {
                  ipCountry = "XX"; // ??? TODO: how to handle failed IP CC lookups?

                  // should only happen if CC not available for this IP
                  LogToConsole($"IP/CC not added [IP CC LOOKUP FAILED] {ipAddress.Value} <{IpCountryDictionary[ipAddress.Value]}>      [{x++}/{IpCountryDictionary.Count}]", ConsoleColor.Red);
               }
            }
            else
            {
#if VERBOSE
               // happens A LOT - very normal
               LogToConsole($"IP/CC not added [IP ALREADY KNOWN] {ipAddress.Value} <{IpCountryDictionary[ipAddress.Value]}>      [{x++}/{IpCountryDictionary.Count}]");
#endif
            }

            return new FailedLoginEvent(precise, timeCreated, targetUserName.Value, ipAddress.Value, ipCountry);
         }

         return null;
      }


      public void LogToConsole(string msg, ConsoleColor textColor = ConsoleColor.Blue)
      {
         var fg = Console.ForegroundColor;
         Console.ForegroundColor = ConsoleColor.Red;
         Console.Error.WriteLine(msg);
         Console.Error.Flush();
         Console.ForegroundColor = fg;
      }


      //          var date = new DateOnly(2020, 04, 20);

      public async Task<int> ExportToCsv(DateOnly startAtDate, DateOnly endByDate, string detailsPath)
      {
         // Date ge datetime'31-8-2013T14:15:14Z' and Date lt datetime'31-8-2013T14:19:10Z'";
         // Timestamp gt datetime'2012-12-10T15:00:00'

         var nullSpan = new TimeSpan(0);
         var startAtDateTimeOffset = new DateTimeOffset(startAtDate.Year, startAtDate.Month, startAtDate.Day, 0, 0, 0, 0, nullSpan);
         var endByDateTimeOffset = new DateTimeOffset(endByDate.Year, endByDate.Month, endByDate.Day, 0, 0, 0, 0, nullSpan);

         return await ExportToCsv(startAtDateTimeOffset, endByDateTimeOffset, detailsPath);
      }


      /// <summary>
      /// start_at <= records < end_by
      /// summary_file (roll ups), details_file (every record)
      /// returns number of records processed
      /// </summary>
      public async Task<int> ExportToCsv(DateTimeOffset startAt, DateTimeOffset endBy, string detailsPath)
      {
         int recordsProcessed = 0;

         using (var details = new StreamWriter(detailsPath))
         {
#if true
            var options = new TableClientOptions();
            //options.AddPolicy(new RetryLogger(), HttpPipelinePosition.PerRetry);
            var serviceClient = new TableServiceClient(TableConnectionString, options);
#else
            var serviceClient = new TableServiceClient(TableConnectionString);
#endif
            var tableName = "WADWindowsEventLogsTable";
            var tableClient = serviceClient.GetTableClient(tableName);

            Pageable<TableItem> queryTableResults = serviceClient.Query(filter: $"TableName eq '{tableName}'");

            foreach (TableItem table in queryTableResults)
            {
               // Date ge datetime'31-8-2013T14:15:14Z' and Date lt datetime'31-8-2013T14:19:10Z'";
               // Timestamp gt datetime'2012-12-10T15:00:00'

               // https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.table.tablequery.generatefilterconditionfordate?view=azure-dotnet
               const int failedLoginAttemptEventId = 4625;
               var nullSpan = new TimeSpan(0);

               var filter = $"(EventId eq {failedLoginAttemptEventId}) and (PreciseTimeStamp ge datetime'{startAt.Year:D4}-{startAt.Month:D2}-{startAt.Day:D2}T00:00:00.0000000Z') and (PreciseTimeStamp lt datetime'{endBy.Year:D4}-{(endBy.Month):D2}-{endBy.Day:D2}T00:00:00.0000000Z')";
               LogToConsole($"FILTER ➜ {filter}");

               Pageable<Azure.Data.Tables.TableEntity> entities = tableClient.Query<Azure.Data.Tables.TableEntity>(filter: filter);

               foreach (var entity in entities)
               {
                  var rawXml = entity.GetString("RawXml");
                  var dto = (DateTimeOffset)entity["PreciseTimeStamp"];
                  var dtostr = $"{dto.UtcDateTime:O}"; // <== this matches PreciseTimeStamp in table storage
                  var failedLoginEvent = await ParseFailedLoginEventEntityXml(dto, rawXml);
                  if (failedLoginEvent != null)
                  {
                     var ev = failedLoginEvent.Value;
                     await details.WriteLineAsync($"{ev.ts}, {ev.attackerIp}, {ev.countrycode}, {ev.loginName}");
                  }
                  else
                  {
                     LogToConsole($"FAILED to PARSE (date, xml) ➜ ({dto}, {rawXml})");
                  }
               }

               Console.Out.Flush();

               // ErrorMessage("Also... ip of attacked VM (20.127.126.6), location of VM (eastus), service under attack (RDP), and server OS (Win2016)");
               LogToConsole("AttackedIp (20.127.126.6), AttackedLocation (eastus), AttackedService (RDP), AttackedServerOS (Win2016)");
            }
            return recordsProcessed;
         }
      }





      public async Task GetAttackerDataAsCsvIpToCountryMap(int year, int month)
      {
         int monthUpperBound = month == 12 ? 1 : month + 1;
         int yearUpperBound = monthUpperBound == 1 ? year + 1 : year;

         var serviceClient = new TableServiceClient(TableConnectionString);
         var tableName = "WADWindowsEventLogsTable";
         var tableClient = serviceClient.GetTableClient(tableName);

         Pageable<TableItem> queryTableResults = serviceClient.Query(filter: $"TableName eq '{tableName}'");

         foreach (TableItem table in queryTableResults)
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

            foreach (var entity in entities)
            {
               var rawXml = entity.GetString("RawXml");
               var dto = (DateTimeOffset)entity["PreciseTimeStamp"];
               var dtostr = $"{dto.UtcDateTime:O}"; // <== this matches PreciseTimeStamp in table storage
               await ParseXmlProperty(dto, rawXml);
            }

            Console.Out.Flush();

            // ErrorMessage("Also... ip of attacked VM (20.127.126.6), location of VM (eastus), service under attack (RDP), and server OS (Win2016)");
            LogToConsole("AttackedIp (20.127.126.6), AttackedLocation (eastus), AttackedService (RDP), AttackedServerOS (Win2016)");
         }
      }
   }
}
