namespace Pipeline.Worker.Configuration;

public sealed class Dataset
{
    public long Id { get; set; }
    public String DatasetKey { get; set; } = string.Empty;
    public String DisplayName { get; set; } = string.Empty;
    public String SourcePath { get; set; } = string.Empty;
    public String FilePattern { get; set; } = string.Empty;
    public int StabilityCheckSeconds { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed class DatasetColumnMapping
{
    public long Id { get; set; }
    public long DatasetId { get; set; }
    public int SourceColumnIndex { get; set; }
    public string DestinationColumnName { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public bool IsRequired { get; set; } 
}

public sealed class ProcessingOptions
{
    public int MaxWorkers { get; init; } = 10;
    public int PollIntervalSeconds { get; init; } = 2;
}