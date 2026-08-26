namespace Pipeline.Worker.Jobs;

public sealed class FileJob
{
    public long Id { get; set; }

    public required long DatasetId { get; set; }
    public required string FilePath { get; set; }
    public FileJobStatus Status { get; set; }
    public int AttemptCount { get; set; }
    public DateTime DiscoveredAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}