using System.Data;
using Microsoft.Data.SqlClient;
using Pipeline.Worker.Configuration;
using Pipeline.Worker.Data;

namespace Pipeline.Worker.Processing;

public sealed class BulkDataWriter
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<BulkDataWriter> _logger;

    public BulkDataWriter(
        ISqlConnectionFactory connectionFactory,
        ILogger<BulkDataWriter> logger
    )
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task WriteAsync(
        IDataReader reader,
        string destinationTableName,
        IReadOnlyList<DatasetColumnMapping> mappings,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationTableName);
        ArgumentNullException.ThrowIfNull(mappings);

        if (mappings.Count == 0)
        {
            throw new ArgumentException(
                "At least one column mapping is required.",
                nameof(mappings)
            );
        }

        await using var connection =
            await _connectionFactory.OpenConnectionAsync(
                cancellationToken
            );

        using var bulkCopy = new SqlBulkCopy(
            connection,
            SqlBulkCopyOptions.TableLock,
            null
        );

        bulkCopy.DestinationTableName = destinationTableName;

        bulkCopy.EnableStreaming = true;

        bulkCopy.BatchSize = 10_000;

        bulkCopy.BulkCopyTimeout = 0;

        for (var i = 0; i < mappings.Count; i++)
        {
            bulkCopy.ColumnMappings.Add(
                i,
                mappings[i].DestinationColumnName
            );
        }

        _logger.LogInformation(
            "Starting bulk copy. DestinationTable={DestinationTable}, ColumnCount={ColumnCount}",
            destinationTableName,
            mappings.Count
        );

        await bulkCopy.WriteToServerAsync(
            reader,
            cancellationToken
        );

        _logger.LogInformation(
            "Bulk copy completed. DestinationTable={DestinationTable}",
            destinationTableName
        );
    }
}
