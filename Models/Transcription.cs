namespace Scriptum.Models;

/// <summary>
/// Raw and corrected text derived from a handwritten notebook page.
/// </summary>
public sealed class Transcription
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Status { get; set; } = "Waiting";

    public string? ProviderName { get; set; }

    public string? RawText { get; set; }

    public string? CorrectedText { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
