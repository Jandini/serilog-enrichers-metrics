# Serilog.Enrichers.Metrics

[![Build](https://github.com/Jandini/serilog-enrichers-metrics/actions/workflows/build.yml/badge.svg)](https://github.com/Jandini/serilog-enrichers-metrics/actions/workflows/build.yml)
[![NuGet](https://github.com/Jandini/serilog-enrichers-metrics/actions/workflows/nuget.yml/badge.svg)](https://github.com/Jandini/serilog-enrichers-metrics/actions/workflows/nuget.yml)

## Overview

This repository provides Serilog enrichers for monitoring IO operations and memory usage of the current process. These enrichers are extremely helpful when writing applications where low memory consumption is key. IO operation metrics allow you to track how much data is being read or written by your application.

- **Serilog.Enrichers.Metrics.IO**: Adds metrics for IO read/write operations.
- **Serilog.Enrichers.Metrics.Memory**: Adds metrics for memory usage by the current process.

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
Add the enricher to your Serilog configuration:

```csharp
// Example for memory metrics
dotnet add package Serilog.Enrichers.Metrics.Memory
// Example for IO metrics
dotnet add package Serilog.Enrichers.Metrics.IO
```

---
Created from [JandaBox](https://github.com/Jandini/JandaBox) | Icon created by [Freepik - Flaticon](https://www.flaticon.com/free-icons/box)
