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
            // main worker logic here
            var files = await _fileDiscoveryService.DiscoverAsync(stoppingToken);

            foreach (var file in files)
            {
                _logger.LogInformation(
                    "Discovered file. Dataset={Dataset}, File={File}",
                    file.DatasetName,
                    file.FilePath
                );

                await Task.Delay(
                    TimeSpan.FromSeconds(30),
                    stoppingToken
                );
            }
        }
    }
}
