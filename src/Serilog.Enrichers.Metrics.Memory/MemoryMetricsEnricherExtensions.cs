using Serilog.Configuration;

namespace Serilog;

public static class MemoryMetricsEnricherExtensions
{
    public static LoggerConfiguration WithMemoryMetrics(this LoggerEnrichmentConfiguration enrich,
        bool emitDeltas = false,
        TimeSpan? minSampleInterval = null)
        => enrich.With(new MemoryMetricsEnricher(emitDeltas, minSampleInterval));
}
