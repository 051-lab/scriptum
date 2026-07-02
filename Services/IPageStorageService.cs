using Scriptum.Models;

namespace Scriptum.Services;

/// <summary>
/// Storage boundary for notebook pages.
/// </summary>
public interface IPageStorageService
{
    Task SavePageAsync(NotebookPage page, CancellationToken cancellationToken = default);

    Task<NotebookPage?> LoadPageAsync(Guid pageId, CancellationToken cancellationToken = default);

    Task<NotebookPage?> LoadLatestPageAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotebookPage>> LoadPagesAsync(CancellationToken cancellationToken = default);
}
