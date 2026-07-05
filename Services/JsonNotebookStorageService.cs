using System.Text.Json;
using Scriptum.Models;

namespace Scriptum.Services;

public sealed class JsonNotebookStorageService : INotebookStorageService
{
    private readonly string _notebooksDirectory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public JsonNotebookStorageService(string? appDataRoot = null)
    {
        var root = appDataRoot ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Scriptum");

        _notebooksDirectory = Path.Combine(root, "Notebooks");
        Directory.CreateDirectory(_notebooksDirectory);
    }

    public async Task SaveNotebookAsync(Notebook notebook, CancellationToken cancellationToken = default)
    {
        notebook.UpdatedAt = DateTimeOffset.UtcNow;
        var path = GetNotebookPath(notebook.Id);
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, notebook, _jsonOptions, cancellationToken);
    }

    public async Task<IReadOnlyList<Notebook>> LoadNotebooksAsync(CancellationToken cancellationToken = default)
    {
        var notebooks = new List<Notebook>();
        foreach (var path in Directory
            .EnumerateFiles(_notebooksDirectory, "*.json", SearchOption.TopDirectoryOnly)
            .OrderByDescending(File.GetLastWriteTimeUtc))
        {
            var notebook = await TryLoadNotebookAsync(path, cancellationToken);
            if (notebook is not null)
            {
                notebooks.Add(notebook);
            }
        }

        return notebooks;
    }

    private string GetNotebookPath(Guid notebookId) => Path.Combine(_notebooksDirectory, $"{notebookId:N}.json");

    private async Task<Notebook?> TryLoadNotebookAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<Notebook>(stream, _jsonOptions, cancellationToken);
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
