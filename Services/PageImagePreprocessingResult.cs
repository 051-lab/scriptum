namespace Scriptum.Services;

public sealed class PageImagePreprocessingResult
{
    public string SourceImagePath { get; init; } = string.Empty;

    public string PreparedImagePath { get; init; } = string.Empty;

    public int? Width { get; init; }

    public int? Height { get; init; }

    public DateTimeOffset PreparedAt { get; init; } = DateTimeOffset.UtcNow;

    public double RotationDegrees { get; init; }

    public double? CropX { get; init; }

    public double? CropY { get; init; }

    public double? CropWidth { get; init; }

    public double? CropHeight { get; init; }

    public bool DeskewApplied { get; init; }

    public string Summary { get; init; } = string.Empty;
}
