namespace Pipeline.Worker.Data;

public static class SqlServerServiceCollectionExtensions
{
    public static IServiceCollection AddSqlServer(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString = configuration.GetConnectionString("SqlServer");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'ConnectionStrings:SqlServer' is required."
            );
        }

        services.AddSingleton<ISqlConnectionFactory>(
            new SqlConnectionFactory(connectionString)
        );

        return services;
    }
}
