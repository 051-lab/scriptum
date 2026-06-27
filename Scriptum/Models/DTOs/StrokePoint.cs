using MessagePack;

namespace Scriptum.Models.DTOs;

/// <summary>
/// Represents a single point in an ink stroke.
/// </summary>
[MessagePackObject]
public class StrokePoint
{
    [Key(0)]
    public float X { get; set; }

    [Key(1)]
    public float Y { get; set; }

    [Key(2)]
    public float Pressure { get; set; }

    [Key(3)]
    public DateTime Timestamp { get; set; }
}
