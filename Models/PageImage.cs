namespace Scriptum.Models;

/// <summary>
/// Image metadata for a captured physical notebook page.
/// </summary>
public sealed class PageImage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string LocalPath { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public long? ByteLength { get; set; }

    public int? PixelWidth { get; set; }

    public int? PixelHeight { get; set; }

    public DateTimeOffset ImportedAt { get; set; } = DateTimeOffset.UtcNow;
}
