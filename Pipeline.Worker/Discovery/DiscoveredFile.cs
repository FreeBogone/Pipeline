namespace Pipeline.Worker.Discovery;

public sealed record DiscoveredFile(
    long DatasetId,
    string FilePath
);