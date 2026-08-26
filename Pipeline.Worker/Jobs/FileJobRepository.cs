using Pipeline.Worker.Data;
using Pipeline.Worker.Discovery;
using Microsoft.Data.SqlClient;

namespace Pipeline.Worker.Jobs;

public sealed class FileJobRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public FileJobRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> ExistsAsync(
        string datasetName,
        string filePath,
        CancellationToken cancellationToken
    )
    {
        const string sql = """
            SELECT TOP (1) 1
            FROM dbo.FileJob
            WHERE DatasetName = @DatasetName
              AND FilePath = @FilePath;
            """;

        await using var connection =
            await _connectionFactory.OpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@DatasetName", datasetName);
        command.Parameters.AddWithValue("@FilePath", filePath);

        var result = await command.ExecuteScalarAsync(cancellationToken);

        return result is not null;
    }

    public async Task CreateAsync(
        DiscoveredFile file,
        CancellationToken cancellationToken
    )
    {
        const string sql = """
            INSERT INTO dbo.FileJob
            (
                DatasetName,
                FilePath,
                Status
            )
            VALUES
            (
                @DatasetName,
                @FilePath,
                'Pending'
            );
            """;

        await using var connection =
            await _connectionFactory.OpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "@DatasetName",
            file.DatasetName);

        command.Parameters.AddWithValue(
            "@FilePath",
            file.FilePath);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FileJob>> ClaimPendingAsync(
        int count,
        CancellationToken cancellationToken
    )
    {
        const string sql = """
            ;WITH JobsToClaim AS
            (
                SELECT TOP (@Count) *
                FROM dbo.FileJob WITH (UPDLOCK, READPAST, ROWLOCK)
                WHERE Status = 'Pending'
                ORDER BY DiscoveredAt
            )
            UPDATE JobsToClaim
            SET
                Status = 'Processing',
                StartedAt = SYSUTCDATETIME(),
                AttemptCount = AttemptCount + 1
            OUTPUT
                inserted.Id,
                inserted.DatasetName,
                inserted.FilePath,
                inserted.Status,
                inserted.DiscoveredAt,
                inserted.StartedAt,
                inserted.CompletedAt,
                inserted.AttemptCount,
                inserted.ErrorMessage;
            """;

        var jobs = new List<FileJob>();

        await using var connection =
            await _connectionFactory.OpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@Count", count);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            jobs.Add(MapFileJob(reader));
        }

        return jobs;
    }

    public async Task CompleteAsync(
        long jobId,
        CancellationToken cancellationToken
    )
    {
        const string sql = """
            UPDATE dbo.FileJob
            SET
                Status = 'Complete',
                CompletedAt = SYSUTCDATETIME(),
                ErrorMessage = NULL
            WHERE Id = @Id;
            """;

        await using var connection =
            await _connectionFactory.OpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@Id", jobId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task FailAsync(
        long jobId,
        string errorMessage,
        CancellationToken cancellationToken
    )
    {
        const string sql = """
            UPDATE dbo.FileJob
            SET
                Status = 'Failed',
                CompletedAt = SYSUTCDATETIME(),
                ErrorMessage = @ErrorMessage
            WHERE Id = @Id;
            """;

        await using var connection =
            await _connectionFactory.OpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@Id", jobId);
        command.Parameters.AddWithValue("@ErrorMessage", errorMessage);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
    private static FileJob MapFileJob(SqlDataReader reader)
    {
        return new FileJob
        {
            Id = reader.GetInt64(reader.GetOrdinal("Id")),

            DatasetName =
                reader.GetString(reader.GetOrdinal("DatasetName")),

            FilePath =
                reader.GetString(reader.GetOrdinal("FilePath")),

            Status =
                Enum.Parse<FileJobStatus>(reader.GetString(reader.GetOrdinal("Status"))),

            DiscoveredAt =
                reader.GetDateTime(reader.GetOrdinal("DiscoveredAt")),

            StartedAt =
                reader.IsDBNull(reader.GetOrdinal("StartedAt"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("StartedAt")),

            CompletedAt =
                reader.IsDBNull(reader.GetOrdinal("CompletedAt"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("CompletedAt")),

            ErrorMessage =
                reader.IsDBNull(reader.GetOrdinal("ErrorMessage"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("ErrorMessage"))
        };
    }
}