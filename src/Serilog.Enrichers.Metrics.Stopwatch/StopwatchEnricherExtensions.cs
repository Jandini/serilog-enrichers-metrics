using Serilog.Configuration;

namespace Serilog;

public static class StopwatchEnricherExtensions
{
    public static LoggerConfiguration WithStopwatch(this LoggerEnrichmentConfiguration enrich, string format = @"hh\:mm\:ss")
        => enrich.With(new StopwatchEnricher(format));
}
