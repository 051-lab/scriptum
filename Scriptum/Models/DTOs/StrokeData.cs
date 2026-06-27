using MessagePack;

namespace Scriptum.Models.DTOs;

/// <summary>
/// Represents a complete ink stroke with all its points.
/// </summary>
[MessagePackObject]
public class StrokeData
{
    [Key(0)]
    public List<StrokePoint> Points { get; set; } = new();

    [Key(1)]
    public string Color { get; set; } = "#000000";

    [Key(2)]
    public float Thickness { get; set; } = 2.0f;

    [Key(3)]
    public bool IsHighlighter { get; set; } = false;
}
