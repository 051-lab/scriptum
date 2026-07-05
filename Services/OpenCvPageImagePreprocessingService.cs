using Scriptum.Models;

namespace Scriptum.Services;

/// <summary>
/// Placeholder adapter for the future OpenCVSharp preprocessing pipeline.
/// </summary>
public sealed class OpenCvPageImagePreprocessingService : IPageImagePreprocessingService
{
    private readonly OpenCvPageImagePreprocessingOptions _options;
    private readonly string _preparedDirectory;

    public OpenCvPageImagePreprocessingService(OpenCvPageImagePreprocessingOptions? options = null)
    {
        _options = options ?? new OpenCvPageImagePreprocessingOptions();

        var root = _options.AppDataRoot ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Scriptum");

        _preparedDirectory = Path.Combine(root, "PreparedPages");
    }

    public Task<PageImagePreprocessingResult> PrepareAsync(NotebookPage page, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_options.Enabled)
        {
            throw new NotSupportedException(
                "OpenCV preprocessing is not enabled yet. Use NoOpPageImagePreprocessingService until OpenCVSharp is added and validated.");
        }

        if (string.IsNullOrWhiteSpace(page.SourceImagePath))
        {
            throw new InvalidOperationException("The page does not have a source image.");
        }

        if (!File.Exists(page.SourceImagePath))
        {
            throw new FileNotFoundException("The imported page image could not be found.", page.SourceImagePath);
        }

        // Future implementation sequence:
        // 1. Load the imported page image.
        // 2. Optionally convert to grayscale.
        // 3. Normalize contrast for handwriting legibility.
        // 4. Detect page bounds and crop.
        // 5. Estimate skew angle and deskew.
        // 6. Write the prepared image to PreparedPages.
        // This shell intentionally avoids OpenCVSharp references until the package/runtime choice is made.
        throw new NotImplementedException(
            $"OpenCV preprocessing shell is configured but not implemented. Target output: {GetPreparedImagePath(page)}");
    }

    private string GetPreparedImagePath(NotebookPage page)
    {
        var extension = Path.GetExtension(page.SourceImagePath);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".png";
        }

        Directory.CreateDirectory(_preparedDirectory);
        return Path.Combine(_preparedDirectory, $"{page.Id:N}_opencv_prepared{extension.ToLowerInvariant()}");
    }
}
