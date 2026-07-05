using System.Text.Json;
using Microsoft.Data.Sqlite;
using Scriptum.Data;
using Scriptum.Models;

namespace Scriptum.Services;

public sealed class SqliteNotebookStorageService : INotebookStorageService, IDisposable
{
    private readonly DatabaseContext _databaseContext;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };
    private bool _initialized;
    private bool _disposed;

    public SqliteNotebookStorageService()
        : this(CreateDefaultDatabaseContext())
    {
    }

    public SqliteNotebookStorageService(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public async Task SaveNotebookAsync(Notebook notebook, CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        notebook.UpdatedAt = DateTimeOffset.UtcNow;
        var payload = JsonSerializer.SerializeToUtf8Bytes(notebook, _jsonOptions);

        using var command = _databaseContext.Connection.CreateCommand();
        command.CommandText = """
            INSERT INTO notebooks (id, title, created_at, updated_at, payload)
            VALUES ($id, $title, $createdAt, $updatedAt, $payload)
            ON CONFLICT(id) DO UPDATE SET
                title = excluded.title,
                updated_at = excluded.updated_at,
                payload = excluded.payload;
            """;

        command.Parameters.AddWithValue("$id", notebook.Id.ToString("N"));
        command.Parameters.AddWithValue("$title", notebook.Title);
        command.Parameters.AddWithValue("$createdAt", notebook.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("$updatedAt", notebook.UpdatedAt.ToString("O"));
        command.Parameters.Add("$payload", SqliteType.Blob).Value = payload;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Notebook>> LoadNotebooksAsync(CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        using var command = _databaseContext.Connection.CreateCommand();
        command.CommandText = """
            SELECT payload
            FROM notebooks
            ORDER BY updated_at DESC;
            """;

        var notebooks = new List<Notebook>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var notebook = DeserializeNotebook(reader);
            if (notebook is not null)
            {
                notebooks.Add(notebook);
            }
        }

        return notebooks;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _databaseContext.Dispose();
        _disposed = true;
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_initialized)
        {
            return;
        }

        using var command = _databaseContext.Connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS notebooks (
                id TEXT PRIMARY KEY NOT NULL,
                title TEXT NOT NULL,
                created_at TEXT NOT NULL,
                updated_at TEXT NOT NULL,
                payload BLOB NOT NULL
            );

            CREATE INDEX IF NOT EXISTS idx_notebooks_updated_at
            ON notebooks(updated_at DESC);
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
        _initialized = true;
    }

    private Notebook? DeserializeNotebook(SqliteDataReader reader)
    {
        try
        {
            var payload = (byte[])reader["payload"];
            return JsonSerializer.Deserialize<Notebook>(payload, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
        catch (NotSupportedException)
        {
            return null;
        }
    }

    private static DatabaseContext CreateDefaultDatabaseContext()
    {
        var appDataRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Scriptum");

        Directory.CreateDirectory(appDataRoot);
        return new DatabaseContext(Path.Combine(appDataRoot, "scriptum.db"));
    }
}
