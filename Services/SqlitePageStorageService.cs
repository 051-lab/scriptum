using System.Text.Json;
using Microsoft.Data.Sqlite;
using Scriptum.Data;
using Scriptum.Models;

namespace Scriptum.Services;

/// <summary>
/// SQLCipher-backed storage for Scriptum notebook pages.
/// </summary>
public sealed class SqlitePageStorageService : IPageStorageService, IDisposable
{
    private readonly DatabaseContext _databaseContext;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };
    private bool _initialized;
    private bool _disposed;

    public SqlitePageStorageService()
        : this(CreateDefaultDatabaseContext())
    {
    }

    public SqlitePageStorageService(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public async Task SavePageAsync(NotebookPage page, CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        page.UpdatedAt = DateTimeOffset.UtcNow;
        var payload = JsonSerializer.SerializeToUtf8Bytes(page, _jsonOptions);

        using var command = _databaseContext.Connection.CreateCommand();
        command.CommandText = """
            INSERT INTO notebook_pages (id, title, created_at, updated_at, payload)
            VALUES ($id, $title, $createdAt, $updatedAt, $payload)
            ON CONFLICT(id) DO UPDATE SET
                title = excluded.title,
                updated_at = excluded.updated_at,
                payload = excluded.payload;
            """;

        command.Parameters.AddWithValue("$id", page.Id.ToString("N"));
        command.Parameters.AddWithValue("$title", page.Title);
        command.Parameters.AddWithValue("$createdAt", page.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("$updatedAt", page.UpdatedAt.ToString("O"));
        command.Parameters.Add("$payload", SqliteType.Blob).Value = payload;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<NotebookPage?> LoadPageAsync(Guid pageId, CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        using var command = _databaseContext.Connection.CreateCommand();
        command.CommandText = """
            SELECT payload
            FROM notebook_pages
            WHERE id = $id
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$id", pageId.ToString("N"));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return DeserializePage(reader);
    }

    public async Task<NotebookPage?> LoadLatestPageAsync(CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        using var command = _databaseContext.Connection.CreateCommand();
        command.CommandText = """
            SELECT payload
            FROM notebook_pages
            ORDER BY updated_at DESC
            LIMIT 1;
            """;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return DeserializePage(reader);
    }

    public async Task<IReadOnlyList<NotebookPage>> LoadPagesAsync(CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        using var command = _databaseContext.Connection.CreateCommand();
        command.CommandText = """
            SELECT payload
            FROM notebook_pages
            ORDER BY updated_at DESC;
            """;

        var pages = new List<NotebookPage>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var page = DeserializePage(reader);
            if (page is not null)
            {
                pages.Add(page);
            }
        }

        return pages;
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
            CREATE TABLE IF NOT EXISTS notebook_pages (
                id TEXT PRIMARY KEY NOT NULL,
                title TEXT NOT NULL,
                created_at TEXT NOT NULL,
                updated_at TEXT NOT NULL,
                payload BLOB NOT NULL
            );

            CREATE INDEX IF NOT EXISTS idx_notebook_pages_updated_at
            ON notebook_pages(updated_at DESC);
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
        _initialized = true;
    }

    private NotebookPage? DeserializePage(SqliteDataReader reader)
    {
        var payload = (byte[])reader["payload"];
        return JsonSerializer.Deserialize<NotebookPage>(payload, _jsonOptions);
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
