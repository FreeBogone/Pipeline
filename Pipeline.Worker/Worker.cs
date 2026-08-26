using Pipeline.Worker.Discovery;
using Pipeline.Worker.Jobs;

namespace Pipeline.Worker;

public sealed class Worker : BackgroundService
{
    private readonly FileDiscoveryService _fileDiscoveryService;
    private readonly FileJobService _fileJobService;
    private readonly ILogger<Worker> _logger;

    public Worker(
        FileDiscoveryService fileDiscoveryService,
        FileJobService fileJobService,
        ILogger<Worker> logger
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
                // _logger.LogInformation(
                //     "Discovered file. Dataset={Dataset}, File={File}",
                //     file.DatasetName,
                //     file.FilePath
                // );

                // 2. Check whether the file has already been processed
                //    - Look up the file in ETL metadata/history
                //    - Skip completed files
                //    - Decide how to handle failed/retryable files

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
