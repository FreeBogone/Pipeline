namespace Pipeline.Worker.Configuration;

public sealed class DatasetOptions
{
    public String Name { get; init; } = string.Empty;
    public String Path { get; init; } = string.Empty;
    public String FilePattern { get; init; } = "*.csv";
    public int StabilityCheckSeconds { get; init; } = 10;
}