namespace Scriptum.Models;

/// <summary>
/// A single page captured from a physical notebook.
/// </summary>
public sealed class NotebookPage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = "Untitled Page";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ImportedAt { get; set; }

    public string? SourceImagePath { get; set; }

    public string? OriginalFileName { get; set; }

    public long? SourceImageBytes { get; set; }

    public int? ImagePixelWidth { get; set; }

    public int? ImagePixelHeight { get; set; }

    public string? TranscriptionText { get; set; }

    public List<InkStroke> Strokes { get; set; } = new();
}
