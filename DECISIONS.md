# Some decisions are explained here

Some technical decisions and implementation details are included.

Some usability considerations are included.

Mostly behind-the-scenese stuff that is not README-worthy.

This is not intending to rise to the level of [Architectural Decision Records](https://adr.github.io). More ADR resources can be found [here](https://resources.sei.cmu.edu/asset_files/Presentation/2017_017_001_497746.pdf), [here](https://openpracticelibrary.com/practice/architectural-decision-records-adr/), [here](https://thinkrelevance.com/blog/2011/11/15/documenting-architecture-decisions), and [here](https://www.redhat.com/architect/architecture-decision-records).

## .NET 6 Programming Model for Console App

Using .NET 6 and the new console template supporting top-level statements <https://aka.ms/new-console-template>.

## Command Line Parameter parser

Parsing of the command line happens is facilitated using a [beta version](https://www.nuget.org/packages/System.CommandLine) of the open source [System.CommandLine library](<https://github.com/dotnet/command-line-api>).

Added to project via:

`dotnet add package System.CommandLine --version 2.0.0-beta3.22114.1`

Writing a decent command line parser is actually pretty hard to do well, and this library - even though in beta - does a really good job.

Also considered some required and optional environment variables, but those can be a nuisance to access from .NET in Mac OS command line in certain circumstances.

## .NET 6 Depoyment Model

Ease of deployment matters. The .NET 6 [single file apps feature](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-publish#arguments) allows creation of a single-file distributable executable that does not have any dependency on the .NET runtime being installed. One way to use this feature is to build the application accordingly:

`dotnet publish -p:PublishSingleFile=true -r win-x64 -c Release --self-contained true -p:PublishTrimmed=true`

More examples can be found [here](https://github.com/dotnet/designs/blob/main/accepted/2020/single-file/design.md).

Some of the commandline arguments apply to `dotnet publish` directly, while others are passed thru to MSBuild.

There is an [RID catalog](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog) where this value was found (for Big Sur): `osx.11.0-x64`. There are also ARM values for the new Macbooks with M1 chipset.

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
