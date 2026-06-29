namespace Scriptum.Models;

/// <summary>
/// Represents a single sampled point in a handwritten stroke.
/// </summary>
public sealed class InkPoint
{
    public double X { get; set; }

    public double Y { get; set; }

    public float Pressure { get; set; } = 0.5f;

    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
