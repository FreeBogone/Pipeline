namespace Pipeline.Worker.Discovery;

public sealed record DiscoveredFile(
    string DatasetName,
    string FilePath
);