using Pipeline.Worker.Data;
using Pipeline.Worker.Discovery;
using Microsoft.Data.SqlClient;

namespace Pipeline.Worker.Configuration;

public sealed class ConfigurationRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public ConfigurationRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ICollection<Dataset>> GetAllDatasetsAsync
    (
        CancellationToken cancellationToken
    )
    {
        const string sql = """
            SELECT 
                Id, 
                DatasetKey,
                DisplayName,
                SourcePath,
                FilePattern,
                StabilityCheckSeconds,
                IsEnabled,
                CreatedAt,
                UpdatedAt
            FROM dbo.Dataset
            WHERE IsEnabled = 1;
            """;
        
        var datasets = new List<Dataset>();

        await using var connection =
            await _connectionFactory.OpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            datasets.Add(MapDataset(reader));
        }

        return datasets;  
    }

    private static Dataset MapDataset(SqlDataReader reader)
    {
        return new Dataset
        {
            Id = reader.GetInt64(reader.GetOrdinal("Id")),

            DatasetKey =
                reader.GetString(reader.GetOrdinal("DatasetKey")),

            DisplayName =
                reader.GetString(reader.GetOrdinal("DisplayName")),

            SourcePath =
                reader.GetString(reader.GetOrdinal("SourcePath")),

            FilePattern =
                reader.GetString(reader.GetOrdinal("FilePattern")),

            StabilityCheckSeconds =
                reader.GetInt32(reader.GetOrdinal("StabilityCheckSeconds")),

            IsEnabled =
                reader.GetBoolean(reader.GetOrdinal("IsEnabled")),

            CreatedAt =
                reader.GetDateTime(reader.GetOrdinal("CreatedAt")),

            UpdatedAt =
                reader.IsDBNull(reader.GetOrdinal("UpdatedAt"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
        };
    }

    public async Task<ICollection<DatasetColumnMapping>> GetDatasetColumnMappingById
    (
        long datasetId,
        CancellationToken cancellationToken
    )
    {
        const string sql = """
            SELECT 
                Id, 
                DatasetId,
                SourceColumnIndex,
                DestinationColumnName,
                TargetType,
                IsRequired
            FROM dbo.DatasetColumnMappings
            WHERE DatasetId = @DatasetId;
            """;
        
        var ColumnMappings = new List<DatasetColumnMapping>();

        await using var connection =
            await _connectionFactory.OpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        
        command.Parameters.AddWithValue("@DatasetId", datasetId);
        
        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            ColumnMappings.Add(MapDatasetColumnMappings(reader));
        }

        return ColumnMappings;  
    }

    private static DatasetColumnMapping MapDatasetColumnMappings(SqlDataReader reader)
    {
        return new DatasetColumnMapping
        {
            Id = reader.GetInt64(reader.GetOrdinal("Id")),

            DatasetId = reader.GetInt64(reader.GetOrdinal("DatasetId")),

            SourceColumnIndex = reader.GetInt32(reader.GetOrdinal("SourceColumnIndex")),

            DestinationColumnName = reader.GetString(reader.GetOrdinal("DestinationColumnName")),

            TargetType = reader.GetString(reader.GetOrdinal("TargetType")),

            IsRequired = reader.GetBoolean(reader.GetOrdinal("IsRequired")),
        };
    }
}
