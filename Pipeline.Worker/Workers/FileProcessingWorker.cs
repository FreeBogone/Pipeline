using System.Diagnostics;
using Pipeline.Worker.Configuration;
using Pipeline.Worker.Discovery;
using Pipeline.Worker.Jobs;
using Microsoft.Extensions.Options;

namespace Pipeline.Worker;

public sealed class FileProcessingWorker : BackgroundService
{
    // private readonly FileDiscoveryService _fileDiscoveryService;
    private readonly ProcessingOptions _options;
    private readonly FileJobService _fileJobService;
    private readonly ILogger<FileProcessingWorker> _logger;

    public FileProcessingWorker(
        // FileDiscoveryService fileDiscoveryService,
        IOptions<ProcessingOptions> options,
        FileJobService fileJobService,
        ILogger<FileProcessingWorker> logger
    )
    {
        // _fileDiscoveryService = fileDiscoveryService;
        _options = options.Value;
        _fileJobService = fileJobService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Polling Queue for jobs"
            );
            // 1. Scan the FileJob queue

            // 2. Claim Jobs up to Max Workers

            // 3. Process then Concurrently (up to MaxWorkers)

            // 4. Wait before scanning queue again
            await Task.Delay(
                TimeSpan.FromSeconds(_options.PollIntervalSeconds),
                stoppingToken
            );
        }
    }
}
