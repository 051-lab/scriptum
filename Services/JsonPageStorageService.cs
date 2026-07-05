using System.Text.Json;
using Scriptum.Models;

namespace Scriptum.Services;

/// <summary>
/// Temporary local page storage used while the SQLCipher-backed repository is being completed.
/// </summary>
public sealed class JsonPageStorageService : IPageStorageService
{
    private readonly string _pagesDirectory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public JsonPageStorageService(string? appDataRoot = null)
    {
        var root = appDataRoot ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Scriptum");

        _pagesDirectory = Path.Combine(root, "Pages");
        Directory.CreateDirectory(_pagesDirectory);
    }

    public async Task SavePageAsync(NotebookPage page, CancellationToken cancellationToken = default)
    {
        page.UpdatedAt = DateTimeOffset.UtcNow;

        var path = GetPagePath(page.Id);
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, page, _jsonOptions, cancellationToken);
    }

    public async Task<NotebookPage?> LoadPageAsync(Guid pageId, CancellationToken cancellationToken = default)
    {
        var path = GetPagePath(pageId);
        if (!File.Exists(path))
        {
            return null;
        }

        return await TryLoadPageAsync(path, cancellationToken);
    }

    public async Task<NotebookPage?> LoadLatestPageAsync(CancellationToken cancellationToken = default)
    {
        var latestPath = Directory
            .EnumerateFiles(_pagesDirectory, "*.json", SearchOption.TopDirectoryOnly)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();

        if (latestPath is null)
        {
            return null;
        }

        return await TryLoadPageAsync(latestPath, cancellationToken);
    }

    public async Task<IReadOnlyList<NotebookPage>> LoadPagesAsync(CancellationToken cancellationToken = default)
    {
        var pages = new List<NotebookPage>();
        foreach (var path in Directory
            .EnumerateFiles(_pagesDirectory, "*.json", SearchOption.TopDirectoryOnly)
            .OrderByDescending(File.GetLastWriteTimeUtc))
        {
            var page = await TryLoadPageAsync(path, cancellationToken);
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
        var pages = await LoadPagesAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return pages;
        }

        return pages
            .Where(page => notebookId is null || page.NotebookId == notebookId)
            .Where(page => BuildSearchText(page).Contains(searchText.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public Task DeletePageAsync(Guid pageId, CancellationToken cancellationToken = default)
    {
        var path = GetPagePath(pageId);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }

    private string GetPagePath(Guid pageId) => Path.Combine(_pagesDirectory, $"{pageId:N}.json");

    private static string BuildSearchText(NotebookPage page) => string.Join(
        ' ',
        page.Title,
        page.OriginalFileName,
        page.NotebookTitle,
        page.RawTranscriptionText,
        page.CorrectedTranscriptionText,
        page.TranscriptionText);

    private async Task<NotebookPage?> TryLoadPageAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<NotebookPage>(stream, _jsonOptions, cancellationToken);
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
}
