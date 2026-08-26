using System.Diagnostics;
using Pipeline.Worker.Configuration;
using Pipeline.Worker.Discovery;
using Pipeline.Worker.Jobs;
using Microsoft.Extensions.Options;
using System.Security;

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
            try
            {
                // 1. Claim Pending Jobs up to Max Workers
                var jobs = await _fileJobService.ClaimPendingAsync(_options.MaxWorkers, stoppingToken);

                if (jobs.Count == 0)
                {
                    await Task.Delay(
                        TimeSpan.FromSeconds(_options.PollIntervalSeconds),
                        stoppingToken
                    );

                    continue;
                }

                // 2. Process the jobs Concurrently (up to MaxWorkers)
                await Parallel.ForEachAsync(
                    jobs,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = _options.MaxWorkers,
                        CancellationToken = stoppingToken
                    },
                    ProcessJobAsync
                );
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Failed while polling the file-job queue"
                );

                await Task.Delay(
                    TimeSpan.FromSeconds(_options.PollIntervalSeconds),
                    stoppingToken
                );
            }
        }
    }

private async ValueTask ProcessJobAsync(
        FileJob job,
        CancellationToken cancellationToken
    )
    {
        try
        {
            _logger.LogInformation(
                "Processing file job {JobId}: {FilePath}",
                job.Id,
                job.FilePath
            );

            // Process the file
            // await _processor.ProcessAsync(job, cancellationToken);

            await _fileJobService.CompleteAsync(
                job.Id,
                cancellationToken
            );

            _logger.LogInformation(
                "Completed file job {JobId}",
                job.Id
            );
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "File job {JobId} failed",
                job.Id
            );

            await _fileJobService.FailAsync(
                job.Id,
                exception.Message,
                cancellationToken
            );
        }
    }
}
