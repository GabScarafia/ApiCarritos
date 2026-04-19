using Microsoft.Data.Sqlite;
using System.Data;

namespace Core.Database;

internal sealed class DbContext : IDbContext, IDisposable
{
    private readonly IDbConnection _connection;

    public DbContext(string connectionString, bool isInMemory)
    {
        _connection = new SqliteConnection(connectionString);
        _connection.Open();

        if (isInMemory)
        {
            DbInitializer.Initialize(_connection, isInMemory);
        }
    }

    public IDbConnection Connection => _connection;

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
