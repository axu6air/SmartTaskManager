using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SmartTaskManager.Infrastructure.Persistence.Initialization;

// Creates the database and runs the embedded database/rbac-schema.sql. The script stays the source of truth.
internal sealed class DatabaseCreator(ApplicationDbContext context, ILogger<DatabaseCreator> logger)
{
    private const string SchemaResourceName = "rbac-schema.sql";
    private const int CommandTimeoutSeconds = 120;

    public async Task EnsureCreatedAsync(CancellationToken ct)
    {
        var connectionString = context.Database.GetConnectionString()
            ?? throw new InvalidOperationException("The DbContext has no connection string.");

        await CreateDatabaseIfMissingAsync(connectionString, ct);

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);

        if (await UsersTableExistsAsync(connection, ct))
        {
            return;
        }

        foreach (var batch in SqlBatchSplitter.Split(await ReadSchemaScriptAsync(ct)))
        {
            await using var command = new SqlCommand(batch, connection) { CommandTimeout = CommandTimeoutSeconds };
            await command.ExecuteNonQueryAsync(ct);
        }

        logger.LogInformation("Created the database tables and reference data from {Script}.", SchemaResourceName);
    }

    private async Task CreateDatabaseIfMissingAsync(string connectionString, CancellationToken ct)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("The connection string must name a database (Initial Catalog).");
        }

        // The target database may not exist yet, so connect to master to create it.
        builder.InitialCatalog = "master";

        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync(ct);

        await using var command = new SqlCommand(
            """
            IF DB_ID(@name) IS NULL
            BEGIN
                DECLARE @sql nvarchar(max) = N'CREATE DATABASE ' + QUOTENAME(@name);
                EXEC (@sql);
                SELECT 1;
            END
            ELSE SELECT 0;
            """,
            connection) { CommandTimeout = CommandTimeoutSeconds };
        command.Parameters.AddWithValue("@name", databaseName);

        var created = (int)(await command.ExecuteScalarAsync(ct) ?? 0) == 1;
        if (created)
        {
            logger.LogInformation("Created the database {Database}.", databaseName);
        }
    }

    private static async Task<bool> UsersTableExistsAsync(SqlConnection connection, CancellationToken ct)
    {
        await using var command = new SqlCommand("SELECT OBJECT_ID(N'dbo.Users', N'U');", connection);

        return await command.ExecuteScalarAsync(ct) is not (null or DBNull);
    }

    private static async Task<string> ReadSchemaScriptAsync(CancellationToken ct)
    {
        await using var stream = typeof(DatabaseCreator).Assembly.GetManifestResourceStream(SchemaResourceName)
            ?? throw new InvalidOperationException($"The embedded resource '{SchemaResourceName}' was not found.");
        using var reader = new StreamReader(stream);

        return await reader.ReadToEndAsync(ct);
    }
}
