using Pipeline.Worker.Configuration;
using Pipeline.Worker.Jobs;

namespace Pipeline.Worker.Processing;

public sealed class FileProcessingService
{
    private readonly ConfigurationRepository _configRepository;

    public FileProcessingService(
        ConfigurationRepository configRepository
    )
    {
        _configRepository = configRepository;
    }

    public async Task ProcessAsync(
        FileJob job,
        CancellationToken cancellationToken
    )
    {
        // 1. Load column mappings
        var mappings = await _configRepository.GetDatasetColumnMappingById(job.DatasetId, cancellationToken);

        // 2. Determine appropriate file reader
        

        // 3. Open file as IDataReader

        // 4. Bulk copy into destination table

        // 5. Return
    }
}