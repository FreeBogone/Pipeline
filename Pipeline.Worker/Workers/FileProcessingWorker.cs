using System.Diagnostics;
using Pipeline.Worker.Configuration;
using Pipeline.Worker.Discovery;
using Pipeline.Worker.Jobs;
using Microsoft.Extensions.Options;

namespace Pipeline.Worker;

public sealed class FileProcessingWorker : BackgroundService
{
    private readonly FileJobService _fileJobService;
    private readonly ProcessingOptions _options;
    private readonly ILogger<FileProcessingWorker> _logger;

    public FileProcessingWorker(
        FileJobService fileJobService,
        IOptions<ProcessingOptions> options,
        ILogger<FileProcessingWorker> logger
    )
    {
        _fileJobService = fileJobService;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Polling Queue for jobs"
            );

            // 1. Claim Pending Jobs up to Max Workers
            // var jobs = await _fileJobService.ClaimPendingAsync(_options.MaxWorkers, stoppingToken);

            // 2. Process them Concurrently (up to MaxWorkers)
            
            // 3. Wait before scanning queue again
            await Task.Delay(
                TimeSpan.FromSeconds(_options.PollIntervalSeconds),
                stoppingToken
            );
        }
    }
}
