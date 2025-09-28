# Serilog.Enrichers.Metrics

[![Build](https://github.com/Jandini/serilog-enrichers-metrics/actions/workflows/build.yml/badge.svg)](https://github.com/Jandini/serilog-enrichers-metrics/actions/workflows/build.yml)
[![NuGet](https://github.com/Jandini/serilog-enrichers-metrics/actions/workflows/nuget.yml/badge.svg)](https://github.com/Jandini/serilog-enrichers-metrics/actions/workflows/nuget.yml)

## Overview

This repository provides Serilog enrichers for monitoring key runtime metrics of the current process. These enrichers are extremely helpful when writing applications where low memory consumption, IO tracking, or elapsed time measurement is critical.

* **Serilog.Enrichers.Metrics.IO**: Adds metrics for IO read/write operations (`IoReadBytes`, `IoWriteBytes`).
* **Serilog.Enrichers.Metrics.Memory**: Adds metrics for memory usage by the current process (`WorkingSetBytes`, `ManagedMemoryBytes`).
* **Serilog.Enrichers.Metrics.Stopwatch**: Adds a running stopwatch value (`Stopwatch`) to each log event, formatted as a time span.

## Features

* Track memory usage in real time.
* Monitor IO throughput (read/write bytes).
* Measure elapsed application time with a running stopwatch.
* Simple integration with Serilog.

## Installation

Install the desired package(s) from NuGet:

```shell
# For IO metrics
dotnet add package Serilog.Enrichers.Metrics.IO

# For memory metrics
dotnet add package Serilog.Enrichers.Metrics.Memory

# For stopwatch elapsed time
dotnet add package Serilog.Enrichers.Metrics.Stopwatch
```

## Usage

Add the enrichers to your Serilog configuration in code:

```csharp
using Serilog;

var logger = new LoggerConfiguration()
    .Enrich.WithIoMetrics()
    .Enrich.WithMemoryMetrics()
    .Enrich.WithStopwatch() // default format is @"hh\:mm\:ss"
    .WriteTo.Console(
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u4}] {Message} | " +
                        "Read {IoReadBytes:N0} | Write {IoWriteBytes:N0} | " +
                        "WorkingSet {WorkingSetBytes:N0} | Managed {ManagedMemoryBytes:N0} | " +
                        "Elapsed {Stopwatch} | {Properties:j}{NewLine}{Exception}")
    .CreateLogger();
```

### Custom Stopwatch Format

The `Stopwatch` enricher accepts an optional `format` parameter (default: `@"hh\:mm\:ss"`) to control how the elapsed time is rendered.

```csharp
.Enrich.WithStopwatch(@"hh\:mm\:ss\.fff") // include milliseconds
```

## Example: appsettings.json

You can also configure the enrichers using `appsettings.json`:

```json
{
  "Serilog": {
    "Using": [
      "Serilog.Enrichers.Metrics.IO",
      "Serilog.Enrichers.Metrics.Memory",
      "Serilog.Enrichers.Metrics.Stopwatch"
    ],
    "Enrich": [
      "WithIoMetrics",
      "WithMemoryMetrics",
      "WithStopwatch"
    ],
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u4}] {Message} | " +
                            "Read {IoReadBytes:N0} | Write {IoWriteBytes:N0} | " +
                            "WorkingSet {WorkingSetBytes:N0} | Managed {ManagedMemoryBytes:N0} | " +
                            "Elapsed {Stopwatch} | {Properties:j}{NewLine}{Exception}"
        }
      }
    ]
  }
}
```

---

Created from [JandaBox](https://github.com/Jandini/JandaBox) | Icon created by [Freepik - Flaticon](https://www.flaticon.com/free-icons/box)
