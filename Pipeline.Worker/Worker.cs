using Pipeline.Worker.Discovery;

namespace Pipeline.Worker;

public sealed class Worker : BackgroundService
{
    private readonly FileDiscoveryService _fileDiscoveryService;
    private readonly ILogger<Worker> _logger;

    public Worker(
        FileDiscoveryService fileDiscoveryService,
        ILogger<Worker> logger
    )
    {
        _fileDiscoveryService = fileDiscoveryService;
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
                // 2. Check whether the file has already been processed
                //    - Look up the file in ETL metadata/history
                //    - Skip completed files
                //    - Decide how to handle failed/retryable files


                // 3. Register the file as an ETL import
                //    - Create a FileImport record
                //    - Assign an ImportId / LoadId
                //    - Set status = Pending


                // 4. Queue the file for processing
                //    - Create a FileJob
                //    - Add it to the bounded job queue
                //    - Worker threads will process jobs separately

                _logger.LogInformation(
                    "Discovered file. Dataset={Dataset}, File={File}",
                    file.DatasetName,
                    file.FilePath
                );
            }


            // 5. Wait before scanning source folders again
            await Task.Delay(
                TimeSpan.FromSeconds(30),
                stoppingToken
            );
        }
    }
}