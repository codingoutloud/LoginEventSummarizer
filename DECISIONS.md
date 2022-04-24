# Some decisions are explained here

Some technical decisions and implementation details are included.

Some usability considerations are included.

Mostly behind-the-scenes stuff that is not README-worthy.

This is not intending to rise to the level of [Architectural Decision Records](https://adr.github.io). More ADR resources can be found [here](https://resources.sei.cmu.edu/asset_files/Presentation/2017_017_001_497746.pdf), [here](https://openpracticelibrary.com/practice/architectural-decision-records-adr/), [here](https://thinkrelevance.com/blog/2011/11/15/documenting-architecture-decisions), and [here](https://www.redhat.com/architect/architecture-decision-records).

## Took Dependecy on .NET 6

Assumed this would run in context of .NET 6 which opens up some library features and (I think) some C# features. There are no contraints to use any earlier versions.

## .NET 6 Programming Model for Console App

Using .NET 6 and the new console template supporting top-level statements <https://aka.ms/new-console-template>.

## Command Line Parameter parser

Parsing of the command line happens is facilitated using a [beta version](https://www.nuget.org/packages/System.CommandLine) of the open source [System.CommandLine library](<https://github.com/dotnet/command-line-api>).

Added to project via:

`dotnet add package System.CommandLine --version 2.0.0-beta3.22114.1`

Writing a decent command line parser is actually pretty hard to do well, and this library - even though in beta - does a really good job. Lists support for auto-complete on the command line, but have not explored that feature.

Also considered some required and optional environment variables, but those can be a nuisance to access from .NET in Mac OS command line in certain circumstances.

## .NET 6 Depoyment Model

Ease of deployment matters. The .NET 6 [single file apps feature](https://docs.microsoft.com/dotnet/core/tools/dotnet-publish#arguments) allows creation of a single-file distributable executable that does not have any dependency on the .NET runtime being installed. One way to use this feature is to build the application accordingly:

`dotnet publish -p:PublishSingleFile=true -r win-x64 -c Release --self-contained true -p:PublishTrimmed=true`

More examples can be found [here](https://github.com/dotnet/designs/blob/main/accepted/2020/single-file/design.md).

Some of the commandline arguments apply to `dotnet publish` directly, while others are passed thru to MSBuild.

There is an [RID catalog](https://docs.microsoft.com/dotnet/core/rid-catalog) where this value was found (for Big Sur): `osx.11.0-x64`. There are also ARM values for the new Macbooks with M1 chipset.

### Create Single-file Deployment for Windows

Simplest command line:

   `dotnet publish -p:PublishSingleFile=true -r win-x64 -c Release --sc true -p:PublishTrimmed=true`

   `dotnet publish -r win-x64`
   `dotnet publish -r osx.10.11-x64`

### Create Single-file Deployment for MacOS

dotnet publish -p:PublishSingleFile=true -r osx.10.11-x64 -c Release --self-contained true -p:PublishTrimmed=true ; bin/Release/net6.0/osx.10.11-x64/publish/AttackCruncher -- --t:xxxx

--os linux

Most compact (smallest download):

   `dotnet publish -p:PublishSingleFile=true -r win-x64 -c Release --sc true -p:PublishTrimmed=true`

For Windows:

   `dotnet publish -p:PublishSingleFile=true -r win-x64 -c Release --sc true -p:PublishTrimmed=true`

   `dotnet publish -p:PublishSingleFile=true -r win-x64 -c Release --self-contained true -p:PublishTrimmed=true`

## Azure Table Storage Programmatic Interface

These days there is a unified programming model between "traditional" Azure Table Storage and a similar feature available from CosmosDB. The Cosmos variant has a bit more functionality, but both are supported by the .NET `Azure.Data.Tables` library.

Added to project via:

   `dotnet add package Azure.Data.Tables`

## Generated Default .gitignore

Did not use one of the canned .gitignore files out on the web. Instead generated one via:

   `dotnet new gitignore`

and then deleted most of its content (except the section marked for VS Code). No good reason for this path.

## Structured Code Comments

For C# code, taking advantage of standardized XML tags, guided by [recommended tags](https://docs.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags) and [details about individual tags](https://docs.microsoft.com/dotnet/csharp/language-reference/language-specification/documentation-comments).

## Unit Tests using xUnit

   `dotnet add package xunit`

   `dotnet run --project LoginEventSummarizer.csproj`

But need to first move existing code to a parallel directory. So going to structure as /src and /test (or /src and /tests (pl)).

Also need to make the class under test referenced from the unit test, such as by:

   `dotnet add reference ../../src/obj/Debug/TableQueryFilterBuilder.dll`

## Parse Hostname from Azure Storage Connection String

   `pip3 install azure-storage-blob`

   `pip3 install azure-data-tables`

Write short python app to load the connection string using the Azure Storage SDK and use SDK features to return the hostname value.

## Use of Record type

Using C# Record type as return value from parsed `FailedLoginEvent`.

Not doing anything fancy, like no baseclass. Records are handy: concise and clean.

<ht tps://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record>
<https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/records>

## Enabling `bcp` into Azure SQL Database

Using [bcp](https://docs.microsoft.com/azure/azure-sql/load-from-csv-with-bcp), we should be able to import a CSV file into an Azure SQL Database. [Per official documentation](https://docs.microsoft.com/sql/t-sql/statements/bulk-insert-transact-sql?view=sql-server-ver15#format--csv), Azure SQL Database only supports importing files from Azure Blob Storage.

Uploading the full details to Blob is manageable for now since the file size for first three months of 2022 is 286146811 bytes (286MB). So if we round that up to 100 MB per month, over time that could add up.

## Date Range Comparisons

[Edsger Dijkstra](https://en.wikipedia.org/wiki/Edsger_W._Dijkstra) remarks in an [essay](https://www.cs.utexas.edu/users/EWD/transcriptions/EWD08xx/EWD831.html) that based on experience with the Mesa programming language, there is evidence that `2 ≤ i < 13` is the most effective way to express the range `2..12`.

I've adopted this convention for date ranges. The date range that includes all of the days in Jan, Feb, and Mar would look like this: `Jan 1 <= dates < Apr 1`, and the general case like so: `StartAt <= dates < EndBy` where `EndBy` is the first date not in the range. Mathematicians know this sort of interval as [half open](https://en.wikipedia.org/wiki/Interval_(mathematics)) which can also be expressed in more concise mathematical notation as `[2,13)` in the Dijkstra example, or `[Jan 1, Apr 1)` for our example date range.

Some [GNU project documentation on Iterators](https://gcc.gnu.org/onlinedocs/libstdc++/manual/iterators.html#iterators.predefined.end) encourages thinking in terms of boundary markers, not start/end, which is also consistent with this approach. The justification includes that it is less error-prone, which aligns with what we are going for here.

And just to pile on, it is not unheard of to [complain about the inflexibility of SQL BETWEEN](https://softwareengineering.stackexchange.com/questions/160191/why-is-sqls-between-inclusive-rather-than-half-open). Some respond with [detailed analysis of intervals and related concepts](https://www.sqlservercentral.com/articles/intervals-part-1-definitions-and-terms) in the SQL context.

There have also been some attempts at [time interval libraries for .NET](https://www.codeproject.com/Articles/168662/Time-Period-Library-for-NET). I am not using any such library, although the idea is reasonable since the range is a valid concept in the domain.

## Redecorating Directory Hierarchy

The code started simply as a single directory. But once I wanted to add unit tests that led to wanting to have a library, which pushed me towards have a library in its own directory. I decided to use `git mv` to reorganize. For example:

   `mkdir src`

   `git mv *.cs *.sh *.py src`

   `git mv LoginEventSummarizer.csproj src`

   `git mv .vscode src`

Another option would have been to just start a new repo; I didn't go with that since it isn't really a strategy that works long-term.
