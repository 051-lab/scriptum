namespace Scriptum.Services;

public sealed class OpenCvPageImagePreprocessingOptions
{
    public bool Enabled { get; init; }

    public bool AutoDeskew { get; init; } = true;

    public bool AutoCropPageBounds { get; init; } = true;

    public bool NormalizeContrast { get; init; } = true;

    public bool ConvertToGrayscale { get; init; } = true;

    public string? AppDataRoot { get; init; }
}
