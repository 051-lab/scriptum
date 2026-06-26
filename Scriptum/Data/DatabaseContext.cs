using Microsoft.Data.Sqlite;
using SQLitePCL;

namespace Scriptum.Data;

/// <summary>
/// Initializes and manages the SQLCipher-encrypted SQLite database connection.
/// </summary>
public sealed class DatabaseContext : IDisposable
{
    private SqliteConnection? _connection;
    private readonly string _databasePath;
    private bool _disposed;

    public DatabaseContext(string databasePath)
    {
        _databasePath = databasePath;
        Batteries_V2.Init(); // Initialize SQLCipher
    }

    public SqliteConnection Connection
    {
        get
        {
            if (_connection == null)
            {
                var connectionString = new SqliteConnectionStringBuilder
                {
                    DataSource = _databasePath,
                    Mode = SqliteOpenMode.ReadWriteCreate
                }.ToString();

                _connection = new SqliteConnection(connectionString);
                _connection.Open();

                // Set encryption key (this will be replaced by TPM-backed key in Phase 7)
                using var cmd = _connection.CreateCommand();
                cmd.CommandText = "PRAGMA key = 'scriptum-dev-key';"; // Placeholder for Phase 7
                cmd.ExecuteNonQuery();

                // Enable WAL mode for better performance
                cmd.CommandText = "PRAGMA journal_mode = WAL;";
                cmd.ExecuteNonQuery();

                // Set page size for optimal encryption
                cmd.CommandText = "PRAGMA page_size = 4096;";
                cmd.ExecuteNonQuery();
            }
            return _connection;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _connection?.Dispose();
            _disposed = true;
        }
    }
}
