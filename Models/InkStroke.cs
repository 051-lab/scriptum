namespace Scriptum.Models;

/// <summary>
/// Vector representation of one continuous handwritten stroke.
/// </summary>
public sealed class InkStroke
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string ColorHex { get; set; } = "#151515";

    public double Thickness { get; set; } = 3.0;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<InkPoint> Points { get; set; } = new();
}
