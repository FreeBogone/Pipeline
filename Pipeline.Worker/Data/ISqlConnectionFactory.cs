using Microsoft.Data.SqlClient;

namespace Pipeline.Worker.Data;

public interface ISqlConnectionFactory
{
    Task<SqlConnection> OpenConnectionAsync(
        CancellationToken cancellationToken = default
    );
}
