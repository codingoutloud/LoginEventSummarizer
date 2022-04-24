#define VERBOSE
using System.CommandLine;

var tableStorageCSOption = new Option<string>(
   "--tablecs",
   description: "Azure Table Storage connection string. This is where the raw data to be crunched.");
tableStorageCSOption.AddAlias("-t");

var blobStorageCSOption = new Option<string>(
   "--blobcs",
   // getDefaultValue: () => "42 no default 42",
   description: "Azure Blob Storage connection string. We upload the resultant file here and it can be referenced from here.");
blobStorageCSOption.AddAlias("-b");

var azureMapApiKeyOption = new Option<string>(
   "--mapkey",
   // getDefaultValue: () => "42 no default 42",
   description: "Azure Map API key. This is needed to enrich the data with country codes for associated IP addresses.");
blobStorageCSOption.AddAlias("-m");

var csvFilenameOption = new Option<string>(
   "--csvfilename",
   getDefaultValue: () => "ipcc.csv",
   description: "CSV filename. Both used to store locally (./csvfilename) and if [optionally] " +
   "uploaded to blob storage. Currently cannot contain a path. But maybe we can add that feature " +
   "(where we parse the filename out from the path ☺ 😂 🚀).");
csvFilenameOption.AddAlias("-f");
csvFilenameOption.AddAlias("--csv");
csvFilenameOption.AddAlias("-csv");

#if false
var fileOption = new Option<FileInfo>(
   "--file-option",
   "An option whose argument is parsed as a FileInfo");
blobStorageCSOption.AddAlias("-f");
#endif

// Add the options to a root command:
var rootCommand = new RootCommand
{
   tableStorageCSOption,
   blobStorageCSOption,
   azureMapApiKeyOption,
   csvFilenameOption
};

rootCommand.Description = "The LoginEventSummarizer reads login events from the specified Azure Table Storage location for the date range provided. After analysis and some enrichment, a summary is output as a CSV file.";

rootCommand.SetHandler(async (string tablecs, string blobcs, string mapkey, string csv) =>
{
#if VERBOSE
   var fg = Console.ForegroundColor;
   Console.ForegroundColor = ConsoleColor.DarkRed;
   Console.Error.WriteLine($"REQUIRED Azure Table Storage connection string = {tablecs ?? "<null>"}");
   Console.Error.WriteLine($"REQUIRED Azure Blob Storage connection string = {blobcs ?? "<null>"}");
   Console.Error.WriteLine($"REQUIRED Azure API Key = {mapkey ?? "<null>"}");
   //downloader.LogToConsole($"The value for --file-option = {f?.FullName ?? "<null>"}");
   Console.Error.WriteLine($"OPTIONAL The CSV filename = {csv} (defaults to 'ipcc.csv')");
   Console.Error.Flush();
   Console.ForegroundColor = fg;
#endif

   // --table-cs $AZURE_TABLE_STORAGE_CONNECTION_STRING --blob-cs $AZURE_BLOB_STORAGE_CONNECTION_STRING --map-key $AZURE_MAP_API_KEY > $CSV_FILENAME

   //var azureTableStorageConnectionString = Environment.GetEnvironmentVariable("AZURE_TABLE_STORAGE_CONNECTION_STRING", EnvironmentVariableTarget.User);
   //var azureMapApiKey = Environment.GetEnvironmentVariable("AZURE_MAP_API_KEY", EnvironmentVariableTarget.User);

   if (String.IsNullOrEmpty(tablecs) || String.IsNullOrEmpty(blobcs) || String.IsNullOrEmpty(mapkey))
   {
      Console.Error.WriteLine($"Missing at least one required parameter. Exiting.");
      return;
   }

   //var downloader = new EventDownloader.TableStorageEventDownloader(azureTableStorageConnectionString, azureMapApiKey);
   var downloader = new EventDownloader.TableStorageEventDownloader(tablecs, mapkey);

   downloader.LogToConsole($"xxxx-------------------xxxx", ConsoleColor.Red); // 992223
   downloader.LogToConsole($"atscs = {tablecs}", ConsoleColor.Red); // 992223
   downloader.LogToConsole($"amak = {mapkey}", ConsoleColor.Red); // 992223

   var sw = new System.Diagnostics.Stopwatch();
   sw.Start();

   if (Console.IsOutputRedirected)
   {
      downloader.LogToConsole($"Console output is redirected", ConsoleColor.DarkMagenta);
   }
   else
   {
      downloader.LogToConsole($"Console output is NOT redirected");
   }

#if VERBOSE
   downloader.LogToConsole($"Started the execution of {System.AppDomain.CurrentDomain.FriendlyName} at {DateTime.Now} - VERBOSE mode");
#else
   downloader.LogToConsole($"Started the execution of {System.AppDomain.CurrentDomain.FriendlyName} at {DateTime.Now}");
#endif

   // TODO: move date rangest to command line or elsewhere
   ////////await downloader.GetAttackerDataAsCsvIpToCountryMap(2022, 3);

   var exporter = new EventDownloader.TableStorageEventExporter(tablecs, mapkey);
   var startAtDate = new DateOnly(2022, 1, 1);
   var endByDate = new DateOnly(2022, 4, 1);
   var detailsPath = "./details.csv";
   await exporter.ExportToCsv(startAtDate, endByDate, detailsPath);

   exporter.LogToConsole($"Completed the execution of {System.AppDomain.CurrentDomain.FriendlyName} at {DateTime.Now}");
   exporter.LogToConsole($"Elapsed execution time for {System.AppDomain.CurrentDomain.FriendlyName}: {sw.Elapsed.TotalMinutes} minutes");

}, tableStorageCSOption, blobStorageCSOption, azureMapApiKeyOption, csvFilenameOption);

return rootCommand.Invoke(args);
