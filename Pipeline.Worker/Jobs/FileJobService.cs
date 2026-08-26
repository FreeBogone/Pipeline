using System.Net.Http.Headers;
using Pipeline.Worker.Discovery;

namespace Pipeline.Worker.Jobs;

public sealed class FileJobService
{
    private readonly FileJobRepository _repository;
    private readonly ILogger<FileJobService> _logger;
    public FileJobService(FileJobRepository repository, ILogger<FileJobService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task QueueAsync(
        DiscoveredFile file,
        CancellationToken cancellationToken
    )
    {
        if (await _repository.ExistsAsync(
            file.DatasetName,
            file.FilePath,
            cancellationToken
        ))
        {
            return;
        }

        await _repository.CreateAsync(file, cancellationToken);
    }

    public async Task<IReadOnlyList<FileJob>> ClaimPendingAsync(
        int count,
        CancellationToken cancellationToken
    )
    {
        var jobs = await _repository.ClaimPendingAsync(
            count, cancellationToken
        );

        return jobs;
    }

    public async Task CompleteAsync (
        long jobId,
        CancellationToken cancellationToken
    )
    {
        await _repository.CompleteAsync(jobId, cancellationToken);
    }

    public async Task FailAsync (
        long jobId,
        string errorMessage,
        CancellationToken cancellationToken
    )
    {
        await _repository.FailAsync(jobId, errorMessage, cancellationToken);
    }
}