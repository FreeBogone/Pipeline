namespace Pipeline.Worker.Job;

public sealed class FileJob
{
    public String Dataset { get; set; } = string.Empty;

    public String SourcePath { get; set; } = string.Empty;

    public String Status { get; set; } = string.Empty;

    public FileJob(String dataset, String sourcePath)
    {
        Dataset = dataset;
        SourcePath = sourcePath;
        Status = "Pending";
    }
}
