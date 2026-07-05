using Scriptum.Models;

namespace Scriptum.Services;

/// <summary>
/// Storage boundary for notebook containers.
/// </summary>
public interface INotebookStorageService
{
    Task SaveNotebookAsync(Notebook notebook, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Notebook>> LoadNotebooksAsync(CancellationToken cancellationToken = default);
}
