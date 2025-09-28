# Serilog.Enrichers.Metrics

[![Build](https://github.com/Jandini/serilog-enrichers-metrics/actions/workflows/build.yml/badge.svg)](https://github.com/Jandini/serilog-enrichers-metrics/actions/workflows/build.yml)
[![NuGet](https://github.com/Jandini/serilog-enrichers-metrics/actions/workflows/nuget.yml/badge.svg)](https://github.com/Jandini/serilog-enrichers-metrics/actions/workflows/nuget.yml)

## Overview

This repository provides Serilog enrichers for monitoring IO operations and memory usage of the current process. These enrichers are extremely helpful when writing applications where low memory consumption is key. IO operation metrics allow you to track how much data is being read or written by your application.

- **Serilog.Enrichers.Metrics.IO**: Adds metrics for IO read/write operations (`IoReadBytes`, `IoWriteBytes`).
- **Serilog.Enrichers.Metrics.Memory**: Adds metrics for memory usage by the current process (`WorkingSetBytes`, `ManagedMemoryBytes`).

## Features
- Track memory usage in real time.
- Monitor IO throughput (read/write bytes).
- Simple integration with Serilog.

## Installation
Install the desired package from NuGet:

```shell
# For IO metrics
dotnet add package Serilog.Enrichers.Metrics.IO

# For memory metrics
dotnet add package Serilog.Enrichers.Metrics.Memory
```

## Usage
Add the enricher to your Serilog configuration in code:

```csharp
using Serilog;

var logger = new LoggerConfiguration()
    .Enrich.WithIoMetrics()
    .Enrich.WithMemoryMetrics()
    .WriteTo.Console(
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u4}] {Message} | Read Bytes {IoReadBytes:N0} | Write Bytes {IoWriteBytes:N0} | WorkingSet {WorkingSetBytes:N0} | Managed {ManagedMemoryBytes:N0} | {Properties:j} {NewLine}{Exception}")
    .CreateLogger();
```

## Example: appsettings.json
You can also configure the enrichers using `appsettings.json`:

```json
{
  "Serilog": {
    "Using": [
      "Serilog.Enrichers.Metrics.IO",
      "Serilog.Enrichers.Metrics.Memory"
    ],
    "Enrich": [
      "WithIoMetrics",
      "WithMemoryMetrics"
    ],
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u4}] {Message} | Read Bytes {IoReadBytes:N0} | Write Bytes {IoWriteBytes:N0} | WorkingSet {WorkingSetBytes:N0} | Managed {ManagedMemoryBytes:N0} | {Properties:j} {NewLine}{Exception}"
        }
      }
    ]
  }
}
```

---
Created from [JandaBox](https://github.com/Jandini/JandaBox) | Icon created by [Freepik - Flaticon](https://www.flaticon.com/free-icons/box)
