using Microsoft.Extensions.Logging;

internal class Main(ILogger<Main> logger)
{

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Hello, World!");
        await Task.CompletedTask;
    }
}