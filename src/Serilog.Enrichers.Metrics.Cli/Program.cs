// Created with JandaBox 0.9.4 http://github.com/Jandini/JandaBox
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

using var serviceProvider = new ServiceCollection()
    .AddLogging(builder => builder.AddSerilog(new LoggerConfiguration()
        .Enrich.WithMachineName()
        .Enrich.WithIoMetrics(emitDeltas: true)
        .WriteTo.Console(
            theme: AnsiConsoleTheme.Code,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u4}] [{SourceContext}] {Message} | Read Bytes {IoReadBytes:N0} | Write Bytes {IoWriteBytes:N0} | {Properties:j} {NewLine}{Exception}")
        .CreateLogger()))
    .AddTransient<Main>()
    .BuildServiceProvider();

try
{
    using var cancellationTokenSource = new CancellationTokenSource();

    Console.CancelKeyPress += (sender, eventArgs) =>
    {
        if (!cancellationTokenSource.IsCancellationRequested) {
            serviceProvider.GetRequiredService<ILogger<Program>>()
                .LogWarning("User break (Ctrl+C) detected. Shutting down gracefully...");
        
            cancellationTokenSource.Cancel();
            eventArgs.Cancel = true; 
        }
    };

    await serviceProvider.GetRequiredService<Main>().RunAsync(cancellationTokenSource.Token);
}
catch (Exception ex)
{
    serviceProvider.GetService<ILogger<Program>>()?
        .LogCritical(ex, "Program failed.");
}