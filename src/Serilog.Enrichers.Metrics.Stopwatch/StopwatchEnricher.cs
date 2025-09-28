using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;

namespace Serilog;

public sealed class StopwatchEnricher : ILogEventEnricher, IDisposable
{
    private readonly bool _includeMilliseconds;
    private readonly Stopwatch _stopwatch;

    public StopwatchEnricher(bool includeMilliseconds = false)
    {
        _includeMilliseconds = includeMilliseconds;
        _stopwatch = Stopwatch.StartNew();
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        string value = _includeMilliseconds
            ? _stopwatch.Elapsed.ToString()
            : ((int)_stopwatch.Elapsed.TotalSeconds).ToString();
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Stopwatch", value));
    }

    public void Dispose() { }
}
