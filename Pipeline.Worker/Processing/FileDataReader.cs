using System.Data;
using Pipeline.Worker.Configuration;

namespace Pipeline.Worker.Processing;

public sealed class FileDataReaderFactory
{
    public IDataReader Create(
        string filePath,
        IReadOnlyList<DatasetColumnMapping> mappings
    )
    {
        return Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".csv" => new CsvFileDataReader(filePath, mappings),
            ".xlsx" => new ExcelFileDataReader(filePath, mappings),
            _ => throw new NotSupportedException(
                $"Unsupported file type: {Path.GetExtension(filePath)}"
            )
        };
    }
}