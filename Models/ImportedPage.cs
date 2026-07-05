namespace Scriptum.Models;

/// <summary>
/// Archive-level record for a page imported from a physical notebook.
/// </summary>
public sealed class ImportedPage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = "Untitled Page";

    public string? NotebookName { get; set; }

    public string? ProjectName { get; set; }

    public PageImage? Image { get; set; }

    public Transcription? Transcription { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
