using Scriptum.Models;

namespace Scriptum.Services;

public sealed class NoOpPageImagePreprocessingService : IPageImagePreprocessingService
{
    public Task<PageImagePreprocessingResult> PrepareAsync(NotebookPage page, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(page.SourceImagePath))
        {
            throw new InvalidOperationException("The page does not have a source image.");
        }

        if (!File.Exists(page.SourceImagePath))
        {
            throw new FileNotFoundException("The imported page image could not be found.", page.SourceImagePath);
        }

        var dimensions = page.ImagePixelWidth is not null && page.ImagePixelHeight is not null
            ? $"{page.ImagePixelWidth} x {page.ImagePixelHeight}px"
            : "dimensions unknown";

        return Task.FromResult(new PageImagePreprocessingResult
        {
            SourceImagePath = page.SourceImagePath,
            PreparedImagePath = page.SourceImagePath,
            Width = page.ImagePixelWidth,
            Height = page.ImagePixelHeight,
            Summary = $"No preprocessing applied; using original imported image ({dimensions})."
        });
    }
}
