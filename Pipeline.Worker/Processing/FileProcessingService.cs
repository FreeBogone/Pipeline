using Pipeline.Worker.Configuration;
using Pipeline.Worker.Jobs;

namespace Pipeline.Worker.Processing;

public sealed class FileProcessingService
{
    private readonly ConfigurationRepository _configRepository;
    private readonly FileDataReaderFactory _fileDataReaderFactory;
    private readonly BulkDataWriter _bulkDataWriter;
    private readonly ILogger<FileProcessingService> _logger;

    public FileProcessingService(
        ConfigurationRepository configRepository,
        FileDataReaderFactory fileDataReaderFactory,
        BulkDataWriter bulkDataWriter,
        ILogger<FileProcessingService> logger
    )
    {
        _configRepository = configRepository;
        _fileDataReaderFactory = fileDataReaderFactory;
        _bulkDataWriter = bulkDataWriter;
        _logger = logger;
    }

    public async Task ProcessAsync(
        FileJob job,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(job);

        // 1. Load Dataset Configuration
        _logger.LogInformation(
            "Processing file job. JobId={JobId}, File={FilePath}, DatasetId={DatasetId}",
            job.Id,
            job.FilePath,
            job.DatasetId
        );

        var dataset = await _configRepository.GetDatasetByIdAsync(
            job.DatasetId,
            cancellationToken
        );

        if (dataset is null)
        {
            throw new InvalidOperationException(
                $"Dataset {job.DatasetId} was not found."
            );
        }

        // 2. Load column mappings
        var mappings = await _configRepository.GetDatasetColumnMappingByIdAsync(
            job.DatasetId,
            cancellationToken
        );

        if (mappings.Count == 0)
        {
            throw new InvalidOperationException(
                $"Dataset {job.DatasetId} has no column mappings configured."
            );
        }

        // 3. Determine appropriate file reader
        if (!File.Exists(job.FilePath))
        {
            throw new FileNotFoundException(
                "File job source file was not found.",
                job.FilePath
            );
        }

        using var reader = _fileDataReaderFactory.Create(
            job.FilePath,
            mappings
        );

        // 4. Open file as IDataReader, Bulk copy into destination table
        await _bulkDataWriter.WriteAsync(
            reader,
            dataset.DestinationTableName,
            mappings,
            cancellationToken
        );

        _logger.LogInformation(
            "Completed file job. JobId={JobId}, File={FilePath}",
            job.Id,
            job.FilePath
        );
    }
}