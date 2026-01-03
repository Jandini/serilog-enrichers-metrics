using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Serilog;

public sealed class IoMetricsEnricher : ILogEventEnricher, IDisposable
{
    private readonly TimeSpan _minSampleInterval;
    private readonly object _gate = new();

    private Metrics _last;
    private DateTime _lastAt;

    public IoMetricsEnricher(TimeSpan? minSampleInterval = null)
    {
        _minSampleInterval = minSampleInterval ?? TimeSpan.FromSeconds(1);
        _last = Sample();
        _lastAt = DateTime.UtcNow;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        Metrics current;
        lock (_gate)
        {
            var now = DateTime.UtcNow;
            if (now - _lastAt >= _minSampleInterval)
            {
                _last = Sample();
                _lastAt = now;
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
        try
        {
            string path = $"/proc/{Environment.ProcessId}/io";
            if (!File.Exists(path)) return default;

            long rchar = 0, wchar = 0;
            long readBytes = 0, writeBytes = 0;
            
            foreach (var line in File.ReadLines(path))
            {
                if (line.StartsWith("rchar:"))
                    rchar = long.Parse(line.AsSpan("rchar:".Length).Trim());
                else if (line.StartsWith("wchar:"))
                    wchar = long.Parse(line.AsSpan("wchar:".Length).Trim());
                else if (line.StartsWith("read_bytes:"))
                    readBytes = long.Parse(line.AsSpan("read_bytes:".Length).Trim());
                else if (line.StartsWith("write_bytes:"))
                    writeBytes = long.Parse(line.AsSpan("write_bytes:".Length).Trim());
            }
            
            // Use rchar/wchar (includes all I/O including console) instead of read_bytes/write_bytes (disk only)
            // This ensures console writes are properly tracked in WSL/Linux environments
            return new(rchar, wchar);
        }
        catch (UnauthorizedAccessException)
        {
            return default;
        }
        catch (IOException)
        {
            return default;
        }
    }

    private static Metrics Mac()
    {
        return default;
    }
}
