using Microsoft.Data.Sqlite;
using SQLitePCL;

namespace Scriptum.Data;

/// <summary>
/// Initializes and manages the SQLCipher-encrypted SQLite database connection.
/// </summary>
public sealed class DatabaseContext : IDisposable
{
    private const string DevelopmentDatabaseKey = "scriptum-dev-key";

    private readonly string _databasePath;
    private readonly string _encryptionKey;
    private SqliteConnection? _connection;
    private bool _disposed;

    public DatabaseContext(string databasePath, string? encryptionKey = null)
    {
        _databasePath = databasePath;
        _encryptionKey = ResolveEncryptionKey(encryptionKey);

        var databaseDirectory = Path.GetDirectoryName(_databasePath);
        if (!string.IsNullOrWhiteSpace(databaseDirectory))
        {
            Directory.CreateDirectory(databaseDirectory);
        }

        Batteries_V2.Init();
    }

    public SqliteConnection Connection
    {
        get
        {
            ThrowIfDisposed();

            if (_connection is null)
            {
                var connectionString = new SqliteConnectionStringBuilder
                {
                    DataSource = _databasePath,
                    Mode = SqliteOpenMode.ReadWriteCreate
                }.ToString();

                _connection = new SqliteConnection(connectionString);
                _connection.Open();

                using var command = _connection.CreateCommand();

                command.CommandText = $"PRAGMA key = '{EscapeSqlLiteral(_encryptionKey)}';";
                command.ExecuteNonQuery();

                command.CommandText = "PRAGMA page_size = 4096;";
                command.ExecuteNonQuery();

                command.CommandText = "PRAGMA journal_mode = WAL;";
                command.ExecuteNonQuery();
            }

            return _connection;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _connection?.Dispose();
        _disposed = true;
    }

    private static string ResolveEncryptionKey(string? encryptionKey)
    {
        if (!string.IsNullOrWhiteSpace(encryptionKey))
        {
            return encryptionKey;
        }

        var environmentKey = Environment.GetEnvironmentVariable("SCRIPTUM_DATABASE_KEY");
        return !string.IsNullOrWhiteSpace(environmentKey)
            ? environmentKey
            : DevelopmentDatabaseKey;
    }

    private static string EscapeSqlLiteral(string value) => value.Replace("'", "''", StringComparison.Ordinal);

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(DatabaseContext));
        }
    }
}
