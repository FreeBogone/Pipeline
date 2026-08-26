namespace Pipeline.Worker.Configuration;

public sealed class DatasetOptions
{
    public String Name { get; init; } = string.Empty;
    public String SourcePath { get; init; } = string.Empty;
    public String FilePattern { get; init; } = "*.csv";
    public int StabilityCheckSeconds { get; init; } = 10;
}

public sealed class ProcessingOptions
{
    public int MaxWorkers { get; init; } = 10;
    public int PollIntervalSeconds { get; init; } = 2;
}