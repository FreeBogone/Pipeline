using Pipeline.Worker.Discovery;
using Pipeline.Worker.Jobs;

namespace Pipeline.Worker;

public sealed class FileDiscoveryWorker : BackgroundService
{
    private readonly FileDiscoveryService _fileDiscoveryService;
    private readonly FileJobService _fileJobService;
    private readonly ILogger<FileDiscoveryWorker> _logger;

    public FileDiscoveryWorker(
        FileDiscoveryService fileDiscoveryService,
        FileJobService fileJobService,
        ILogger<FileDiscoveryWorker> logger
    )
    {
        _fileDiscoveryService = fileDiscoveryService;
        _fileJobService = fileJobService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // 1. Discover available files
            var files = await _fileDiscoveryService.DiscoverAsync(stoppingToken);

            foreach (var file in files)
            {
                if (await _fileJobService.IsProcessedAsync(file, stoppingToken))
                {
                    _logger.LogDebug("Skipping already processed file: {File}", file);
                    continue;
                }

                // 3. Queue the file
                await _fileJobService.QueueAsync(file, stoppingToken);
            }

            // 5. Wait before scanning source folders again
            await Task.Delay(
                TimeSpan.FromSeconds(10),
                stoppingToken
            );
        }
    }
}
