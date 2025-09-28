using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Serilog;

public sealed class IoMetricsEnricher(TimeSpan? minSampleInterval = null) : ILogEventEnricher, IDisposable
{
    private readonly TimeSpan _minSampleInterval = minSampleInterval ?? TimeSpan.Zero;
    private readonly object _gate = new();
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private Metrics _last = Sample();

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        Metrics current;
        lock (_gate)
        {
            if (_stopwatch.Elapsed >= _minSampleInterval)
            {
                _last = Sample();
                _stopwatch.Restart();
            }
            current = _last;
        }

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("IoReadBytes", current.ReadBytes));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("IoWriteBytes", current.WriteBytes));
    }

    public void Dispose() { }

    public static IDisposable PushToLogContext(TimeSpan? minSampleInterval = null)
        => Context.LogContext.Push(new IoMetricsEnricher(minSampleInterval));

    private readonly record struct Metrics(long ReadBytes, long WriteBytes);

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
