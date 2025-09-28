using Microsoft.Extensions.Logging;
using System.Diagnostics;

internal class Main(ILogger<Main> logger)
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var logwatch = Stopwatch.StartNew();

        var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        await LogDirectoriesRecursiveAsync(path, logwatch, cancellationToken);

        logger.LogInformation("Completed in {Elapsed}", stopwatch.Elapsed);
    }

    private async Task LogDirectoriesRecursiveAsync(string path, Stopwatch logwatch, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return;        

        if (logwatch.Elapsed > TimeSpan.FromSeconds(1))
        {
            logger.LogInformation("Directory: {Directory:l}", Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar)));
            logwatch.Restart();
        }

        
        try
        {
            foreach (var file in Directory.EnumerateFiles(path))
            {
                if (cancellationToken.IsCancellationRequested) 
                    return;

                if (file.Length < 128 * 1024)
                {
                    try
                    {
                        var bytes = await File.ReadAllBytesAsync(file, cancellationToken);

                        if (logwatch.Elapsed > TimeSpan.FromSeconds(1))
                        {
                            logger.LogInformation("File: {Directory:l}", Path.GetFileName(file));
                            logwatch.Restart();
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Ignore
                    }
                    catch (Exception)
                    {
                        logger.LogError("Error reading file: {File}", file);
                    }
                }

            }

            foreach (var dir in Directory.EnumerateDirectories(path))
            {
                await LogDirectoriesRecursiveAsync(dir, logwatch, cancellationToken);
            }
        }
        catch (UnauthorizedAccessException)
        {
            logger.LogWarning("Access denied to directory: {Directory}", path);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error accessing directory: {Directory}", path);
        }
    }
}