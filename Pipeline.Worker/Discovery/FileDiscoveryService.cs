using Pipeline.Worker.Configuration;
using Microsoft.Extensions.Options;

namespace Pipeline.Worker.Discovery;

public sealed class FileDiscoveryService
{
    private readonly ConfigurationRepository _configRepository;
    private readonly FileStabilityChecker _stabilityChecker;
    private readonly ILogger<FileDiscoveryService> _logger;

    public FileDiscoveryService(
        ConfigurationRepository configRepository,
        FileStabilityChecker stabilityChecker,
        ILogger<FileDiscoveryService> logger
    )
    {
        _configRepository = configRepository;
        _stabilityChecker = stabilityChecker;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DiscoveredFile>> DiscoverAsync(CancellationToken cancellationToken)
    {
        var discoveredFiles = new List<DiscoveredFile>();

        // get all datasets from config
        var _datasets = await _configRepository.GetAllDatasetsAsync(cancellationToken);

        foreach (var dataset in _datasets)
        {
            // check if directory exists or not
            if (!Directory.Exists(dataset.SourcePath))
            {
                _logger.LogWarning(
                    "Dataset source path does not exist. Dataset={Dataset}, Path={SourcePath}",
                    dataset.DisplayName,
                    dataset.SourcePath
                );
                continue;
            }

            // get all files in directory (not recursive)
            // to change to recursive, use SearchOption.AllDirectories
            var files = Directory.EnumerateFiles(
                dataset.SourcePath,
                dataset.FilePattern,
                SearchOption.TopDirectoryOnly
                // SearchOption.AllDirectories
            );

            foreach (var filePath in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // check stability
                var isStable = await _stabilityChecker.IsStableAsync(
                    filePath,
                    TimeSpan.FromSeconds(dataset.StabilityCheckSeconds),
                    cancellationToken
                );

                if (!isStable)
                {
                    _logger.LogDebug(
                        "Skipping unstable file. Dataset={Dataset}, Path={Path}",
                        dataset.DisplayName,
                        filePath
                    );

                    continue;
                }

                // add to list of discovered files
                discoveredFiles.Add(new DiscoveredFile(dataset.Id, filePath));

                _logger.LogInformation(
                    "Discovered file. Dataset={Dataset}, File={File}",
                    dataset.DisplayName,
                    filePath
                );
            }
        }

        return discoveredFiles;
    }
}