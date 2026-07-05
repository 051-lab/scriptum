using System.Text.Json;
using System.Text.RegularExpressions;
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
    private bool _searchIndexAvailable = true;

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
        await UpsertSearchIndexAsync(page, cancellationToken);
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

    public async Task<IReadOnlyList<NotebookPage>> SearchPagesAsync(
        string searchText,
        Guid? notebookId = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(searchText))
        {
            return await LoadPagesAsync(cancellationToken);
        }

        if (!_searchIndexAvailable)
        {
            return await SearchPagesInMemoryAsync(searchText, notebookId, cancellationToken);
        }

        var ftsQuery = BuildFtsQuery(searchText);
        if (string.IsNullOrWhiteSpace(ftsQuery))
        {
            return await SearchPagesInMemoryAsync(searchText, notebookId, cancellationToken);
        }

        try
        {
            using var command = _databaseContext.Connection.CreateCommand();
            command.CommandText = """
                SELECT p.payload
                FROM notebook_page_search AS s
                JOIN notebook_pages AS p ON p.id = s.page_id
                WHERE notebook_page_search MATCH $query
                    AND ($notebookId IS NULL OR s.notebook_id = $notebookId)
                ORDER BY p.updated_at DESC;
                """;

            command.Parameters.AddWithValue("$query", ftsQuery);
            command.Parameters.AddWithValue("$notebookId", notebookId?.ToString("N") ?? (object)DBNull.Value);

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
        catch (SqliteException)
        {
            _searchIndexAvailable = false;
            return await SearchPagesInMemoryAsync(searchText, notebookId, cancellationToken);
        }
    }

    public async Task DeletePageAsync(Guid pageId, CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        using var command = _databaseContext.Connection.CreateCommand();
        command.CommandText = """
            DELETE FROM notebook_pages
            WHERE id = $id;
            """;
        command.Parameters.AddWithValue("$id", pageId.ToString("N"));

        await command.ExecuteNonQueryAsync(cancellationToken);
        await DeleteSearchIndexEntryAsync(pageId, cancellationToken);
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
        await EnsureSearchIndexAsync(cancellationToken);
        _initialized = true;
    }

    private async Task EnsureSearchIndexAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var command = _databaseContext.Connection.CreateCommand();
            command.CommandText = """
                CREATE VIRTUAL TABLE IF NOT EXISTS notebook_page_search USING fts5(
                    page_id UNINDEXED,
                    notebook_id UNINDEXED,
                    title,
                    source_file_name,
                    notebook_title,
                    raw_transcription,
                    corrected_transcription,
                    transcription
                );
                """;

            await command.ExecuteNonQueryAsync(cancellationToken);
            _searchIndexAvailable = true;
        }
        catch (SqliteException)
        {
            _searchIndexAvailable = false;
        }
    }

    private async Task UpsertSearchIndexAsync(NotebookPage page, CancellationToken cancellationToken)
    {
        if (!_searchIndexAvailable)
        {
            return;
        }

        try
        {
            await DeleteSearchIndexEntryAsync(page.Id, cancellationToken);

            using var command = _databaseContext.Connection.CreateCommand();
            command.CommandText = """
                INSERT INTO notebook_page_search (
                    page_id,
                    notebook_id,
                    title,
                    source_file_name,
                    notebook_title,
                    raw_transcription,
                    corrected_transcription,
                    transcription
                )
                VALUES (
                    $pageId,
                    $notebookId,
                    $title,
                    $sourceFileName,
                    $notebookTitle,
                    $rawTranscription,
                    $correctedTranscription,
                    $transcription
                );
                """;

            command.Parameters.AddWithValue("$pageId", page.Id.ToString("N"));
            command.Parameters.AddWithValue("$notebookId", page.NotebookId?.ToString("N") ?? string.Empty);
            command.Parameters.AddWithValue("$title", page.Title);
            command.Parameters.AddWithValue("$sourceFileName", page.OriginalFileName ?? string.Empty);
            command.Parameters.AddWithValue("$notebookTitle", page.NotebookTitle ?? string.Empty);
            command.Parameters.AddWithValue("$rawTranscription", page.RawTranscriptionText ?? string.Empty);
            command.Parameters.AddWithValue("$correctedTranscription", page.CorrectedTranscriptionText ?? string.Empty);
            command.Parameters.AddWithValue("$transcription", page.TranscriptionText ?? string.Empty);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (SqliteException)
        {
            _searchIndexAvailable = false;
        }
    }

    private async Task DeleteSearchIndexEntryAsync(Guid pageId, CancellationToken cancellationToken)
    {
        if (!_searchIndexAvailable)
        {
            return;
        }

        try
        {
            using var command = _databaseContext.Connection.CreateCommand();
            command.CommandText = """
                DELETE FROM notebook_page_search
                WHERE page_id = $pageId;
                """;
            command.Parameters.AddWithValue("$pageId", pageId.ToString("N"));
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (SqliteException)
        {
            _searchIndexAvailable = false;
        }
    }

    private async Task<IReadOnlyList<NotebookPage>> SearchPagesInMemoryAsync(
        string searchText,
        Guid? notebookId,
        CancellationToken cancellationToken)
    {
        var pages = await LoadPagesAsync(cancellationToken);
        return pages
            .Where(page => notebookId is null || (page.NotebookId?.ToString("N") ?? string.Empty) == notebookId.Value.ToString("N"))
            .Where(page => BuildSearchText(page).Contains(searchText.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private NotebookPage? DeserializePage(SqliteDataReader reader)
    {
        try
        {
            var payload = (byte[])reader["payload"];
            return JsonSerializer.Deserialize<NotebookPage>(payload, _jsonOptions);
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

    private static string BuildSearchText(NotebookPage page) => string.Join(
        ' ',
        page.Title,
        page.OriginalFileName,
        page.NotebookTitle,
        page.RawTranscriptionText,
        page.CorrectedTranscriptionText,
        page.TranscriptionText);

    private static string BuildFtsQuery(string searchText)
    {
        var terms = Regex.Matches(searchText, @"[\p{L}\p{N}_]+")
            .Select(match => match.Value)
            .Where(term => !string.IsNullOrWhiteSpace(term))
            .Take(16)
            .ToList();

        return terms.Count == 0
            ? string.Empty
            : string.Join(' ', terms.Select(term => $"{term}*"));
    }
}
