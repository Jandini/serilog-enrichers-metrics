using Serilog.Configuration;

namespace Serilog;

public static class IoMetricsEnricherExtensions
{    
    public static LoggerConfiguration WithIoMetrics(this LoggerEnrichmentConfiguration enrich, TimeSpan? minSampleInterval = null)
        => enrich.With(new IoMetricsEnricher(minSampleInterval));
}
