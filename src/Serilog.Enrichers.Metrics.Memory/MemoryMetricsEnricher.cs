using System.Diagnostics;

namespace Serilog;

public sealed class MemoryMetricsEnricher : Core.ILogEventEnricher, IDisposable
{
    private readonly bool _emitDeltas;
    private readonly TimeSpan _minSampleInterval;
    private readonly object _gate = new();

    private Snapshot _baseline;
    private Snapshot _last;
    private DateTime _lastAt;

    public MemoryMetricsEnricher(bool emitDeltas = false, TimeSpan? minSampleInterval = null)
    {
        _emitDeltas = emitDeltas;
        _minSampleInterval = minSampleInterval ?? TimeSpan.Zero;

        _baseline = Sample();
        _last = _baseline;
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

        logEvent.AddPropertyIfAbsent(pf.CreateProperty("working_set_bytes", current.WorkingSet));
        logEvent.AddPropertyIfAbsent(pf.CreateProperty("managed_live_bytes", current.ManagedLive));
        logEvent.AddPropertyIfAbsent(pf.CreateProperty("gc_heap_committed_bytes", current.GcHeapCommitted));
        logEvent.AddPropertyIfAbsent(pf.CreateProperty("runtime_committed_bytes", current.RuntimeCommitted));
        logEvent.AddPropertyIfAbsent(pf.CreateProperty("total_available_memory_bytes", current.TotalAvailable));

        if (_emitDeltas)
        {
            var d = current - _baseline;
            logEvent.AddPropertyIfAbsent(pf.CreateProperty("working_set_bytes_delta", d.WorkingSet));
            logEvent.AddPropertyIfAbsent(pf.CreateProperty("managed_live_bytes_delta", d.ManagedLive));
            logEvent.AddPropertyIfAbsent(pf.CreateProperty("gc_heap_committed_bytes_delta", d.GcHeapCommitted));
            logEvent.AddPropertyIfAbsent(pf.CreateProperty("runtime_committed_bytes_delta", d.RuntimeCommitted));
        }
    }

    public void Dispose() { }

    public static IDisposable PushToLogContext(bool emitDeltas = false, TimeSpan? minSampleInterval = null)
        => Context.LogContext.Push(new MemoryMetricsEnricher(emitDeltas, minSampleInterval));

    private readonly record struct Snapshot(
        long WorkingSet, long ManagedLive, long GcHeapCommitted, long RuntimeCommitted, long TotalAvailable)
    {
        public static Snapshot operator - (Snapshot a, Snapshot b)
            => new(a.WorkingSet - b.WorkingSet,
                   a.ManagedLive - b.ManagedLive,
                   a.GcHeapCommitted - b.GcHeapCommitted,
                   a.RuntimeCommitted - b.RuntimeCommitted,
                   a.TotalAvailable - b.TotalAvailable);
    }

    private static Snapshot Sample()
    {
        using var p = Process.GetCurrentProcess();
        long workingSet = p.WorkingSet64;
        long managedLive = GC.GetTotalMemory(false);
        var info = GC.GetGCMemoryInfo();
        long gcCommitted = info.HeapSizeBytes;
        long runtimeComm = info.TotalCommittedBytes;
        long totalAvail = info.TotalAvailableMemoryBytes;
        return new(workingSet, managedLive, gcCommitted, runtimeComm, totalAvail);
    }
}
