namespace Pipeline.Worker.Jobs;

public sealed class FileJob
{
    public long Id { get; set; }

    public required string DatasetName { get; set; }
    public required string FilePath { get; set; }
    public string FileName { get; set; } = string.Empty;
    public FileJobStatus Status { get; set; }
    public DateTime DiscoveredAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}