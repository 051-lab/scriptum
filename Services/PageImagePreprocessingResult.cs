namespace Scriptum.Services;

public sealed class PageImagePreprocessingResult
{
    public string SourceImagePath { get; init; } = string.Empty;

    public string PreparedImagePath { get; init; } = string.Empty;

    public int? Width { get; init; }

    public int? Height { get; init; }

    public string Summary { get; init; } = string.Empty;
}
