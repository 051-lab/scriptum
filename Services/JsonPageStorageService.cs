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

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<NotebookPage>(stream, _jsonOptions, cancellationToken);
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

        await using var stream = File.OpenRead(latestPath);
        return await JsonSerializer.DeserializeAsync<NotebookPage>(stream, _jsonOptions, cancellationToken);
    }

    private string GetPagePath(Guid pageId) => Path.Combine(_pagesDirectory, $"{pageId:N}.json");
}
