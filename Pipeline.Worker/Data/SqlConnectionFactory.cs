using Microsoft.Data.SqlClient;

namespace Pipeline.Worker.Data;

public sealed class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        // Parse once at startup so malformed configuration fails fast.
        _connectionString = new SqlConnectionStringBuilder(connectionString).ConnectionString;
    }

    public async Task<SqlConnection> OpenConnectionAsync(
        CancellationToken cancellationToken = default
    )
    {
        var connection = new SqlConnection(_connectionString);

        try
        {
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }
}
