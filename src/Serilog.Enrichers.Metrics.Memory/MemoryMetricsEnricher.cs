using System.Diagnostics;

namespace Serilog;

public sealed class MemoryMetricsEnricher : Core.ILogEventEnricher, IDisposable
{
    private readonly TimeSpan _minSampleInterval;
    private readonly object _gate = new();

    private Snapshot _last;
    private DateTime _lastAt;

    public MemoryMetricsEnricher(TimeSpan? minSampleInterval = null)
    {
        _minSampleInterval = minSampleInterval ?? TimeSpan.Zero;
        _last = Sample();
        _lastAt = DateTime.UtcNow;
    }

    public void Enrich(Events.LogEvent logEvent, Core.ILogEventPropertyFactory pf)
    {
        var now = DateTime.UtcNow;
        Snapshot current;
        lock (_gate)
        {
            if (now - _lastAt >= _minSampleInterval)
            {
                _last = Sample();
                _lastAt = now;
            }
            current = _last;
        }

        logEvent.AddPropertyIfAbsent(pf.CreateProperty("WorkingSetBytes", current.WorkingSet));
        logEvent.AddPropertyIfAbsent(pf.CreateProperty("ManagedMemoryBytes", current.ManagedLive));
    }

    public void Dispose() { }

    public static IDisposable PushToLogContext(TimeSpan? minSampleInterval = null)
        => Context.LogContext.Push(new MemoryMetricsEnricher(minSampleInterval));

    private readonly record struct Snapshot(long WorkingSet, long ManagedLive);

    private static Snapshot Sample()
    {
        using var p = Process.GetCurrentProcess();
        long workingSet = p.WorkingSet64;
        long managedLive = GC.GetTotalMemory(false);
        return new(workingSet, managedLive);
    }
}
