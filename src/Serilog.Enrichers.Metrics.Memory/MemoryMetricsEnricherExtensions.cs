using Serilog.Configuration;

namespace Serilog;

public static class MemoryMetricsEnricherExtensions
{
    public static LoggerConfiguration WithMemoryMetrics(this LoggerEnrichmentConfiguration enrich,
        TimeSpan? minSampleInterval = null)
        => enrich.With(new MemoryMetricsEnricher(minSampleInterval));
}
