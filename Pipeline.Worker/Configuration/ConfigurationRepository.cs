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
}
