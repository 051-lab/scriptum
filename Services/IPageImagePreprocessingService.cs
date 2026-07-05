using Scriptum.Models;

namespace Scriptum.Services;

public interface IPageImagePreprocessingService
{
    Task<PageImagePreprocessingResult> PrepareAsync(NotebookPage page, CancellationToken cancellationToken = default);
}
