using Scriptum.Models;

namespace Scriptum.Services;

public sealed class NoOpPageImagePreprocessingService : IPageImagePreprocessingService
{
    private readonly string _preparedDirectory;

    public NoOpPageImagePreprocessingService(string? appDataRoot = null)
    {
        var root = appDataRoot ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Scriptum");

        _preparedDirectory = Path.Combine(root, "PreparedPages");
    }

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
        var preparedPath = GetPreparedImagePath(page);

        Directory.CreateDirectory(_preparedDirectory);
        File.Copy(page.SourceImagePath, preparedPath, overwrite: true);

        return Task.FromResult(new PageImagePreprocessingResult
        {
            SourceImagePath = page.SourceImagePath,
            PreparedImagePath = preparedPath,
            Width = page.ImagePixelWidth,
            Height = page.ImagePixelHeight,
            RotationDegrees = page.RotationDegrees,
            CropX = page.CropX,
            CropY = page.CropY,
            CropWidth = page.CropWidth,
            CropHeight = page.CropHeight,
            DeskewApplied = page.DeskewApplied,
            Summary = $"No transforms applied; copied original imported image into prepared pipeline storage ({dimensions})."
        });
    }

    private string GetPreparedImagePath(NotebookPage page)
    {
        var extension = Path.GetExtension(page.SourceImagePath);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".png";
        }

        return Path.Combine(_preparedDirectory, $"{page.Id:N}_prepared{extension.ToLowerInvariant()}");
    }
}
