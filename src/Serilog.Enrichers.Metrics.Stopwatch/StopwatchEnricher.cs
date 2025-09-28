using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;

namespace Serilog;

public sealed class StopwatchEnricher(string format = @"hh\:mm\:ss") : ILogEventEnricher, IDisposable
{
    private readonly string _format = format;
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        string value = _stopwatch.Elapsed.ToString(_format);
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Stopwatch", value));
    }

    public void Dispose() { }
}
