using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Serilog;

public sealed class IoMetricsEnricher : ILogEventEnricher, IDisposable
{
    private readonly bool _emitDeltas;
    private readonly TimeSpan _minSampleInterval;
    private readonly object _gate = new();

    private Metrics _baseline;
    private Metrics _last;
    private DateTime _lastAt;

    public IoMetricsEnricher(bool emitDeltas = false, TimeSpan? minSampleInterval = null)
    {
        _emitDeltas = emitDeltas;
        _minSampleInterval = minSampleInterval ?? TimeSpan.Zero;

        _baseline = Sample();
        _last = _baseline;
        _lastAt = DateTime.UtcNow;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var now = DateTime.UtcNow;
        Metrics current;
        lock (_gate)
        {
            if (now - _lastAt >= _minSampleInterval)
            {
                _last = Sample();
                _lastAt = now;
            }
            current = _last;
        }

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("IoReadBytes", current.ReadBytes));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("IoWriteBytes", current.WriteBytes));

        if (_emitDeltas)
        {
            var d = current - _baseline;
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("DeltaIoReadBytes", d.ReadBytes));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("DeltaIoWriteBytes", d.WriteBytes));

            _baseline = current; 
        }
    }

    public void Dispose() { }

    public static IDisposable PushToLogContext(bool emitDeltas = false, TimeSpan? minSampleInterval = null)
        => Context.LogContext.Push(new IoMetricsEnricher(emitDeltas, minSampleInterval));

    private readonly record struct Metrics(long ReadBytes, long WriteBytes)
    {
        public static Metrics operator -(Metrics a, Metrics b)
            => new(a.ReadBytes - b.ReadBytes, a.WriteBytes - b.WriteBytes);
    }

    private static Metrics Sample()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return Win();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return Linux();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return Mac(); 

        return default;
    }

    private static Metrics Win()
    {
        using var p = Process.GetCurrentProcess();
        if (!GetProcessIoCounters(p.Handle, out var io)) return default;
        return new((long)io.ReadTransferCount, (long)io.WriteTransferCount);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetProcessIoCounters(IntPtr hProcess, out IO_COUNTERS counters);

    [StructLayout(LayoutKind.Sequential)]
    private struct IO_COUNTERS
    {
        public ulong ReadOperationCount;
        public ulong WriteOperationCount;
        public ulong OtherOperationCount;
        public ulong ReadTransferCount;
        public ulong WriteTransferCount;
        public ulong OtherTransferCount;
    }

    private static Metrics Linux()
    {
        string path = $"/proc/{Environment.ProcessId}/io";
        if (!File.Exists(path)) return default;

        long read = 0, write = 0;
        foreach (var line in File.ReadLines(path))
        {
            if (line.StartsWith("read_bytes:"))
                read = long.Parse(line.AsSpan("read_bytes:".Length).Trim());
            else if (line.StartsWith("write_bytes:"))
                write = long.Parse(line.AsSpan("write_bytes:".Length).Trim());
        }
        return new(read, write);
    }

    private static Metrics Mac()
    {

        return default;
    }
}
