namespace Scriptum.Models;

/// <summary>
/// A single notebook page made of vector ink strokes and future transcription metadata.
/// </summary>
public sealed class NotebookPage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = "Untitled Page";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<InkStroke> Strokes { get; set; } = new();
}
