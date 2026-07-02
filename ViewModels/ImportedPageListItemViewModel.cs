using Scriptum.Models;

namespace Scriptum.ViewModels;

public sealed class ImportedPageListItemViewModel
{
    public ImportedPageListItemViewModel(NotebookPage page)
    {
        Id = page.Id;
        Title = page.Title;
        SourceFileName = page.OriginalFileName ?? "Imported page";
        ImportedAt = page.ImportedAt;
        TranscriptionStatus = string.IsNullOrWhiteSpace(page.CorrectedTranscriptionText ?? page.RawTranscriptionText ?? page.TranscriptionText)
            ? "Waiting for transcription"
            : "Draft ready";
    }

    public Guid Id { get; }

    public string Title { get; }

    public string SourceFileName { get; }

    public DateTimeOffset? ImportedAt { get; }

    public string TranscriptionStatus { get; }

    public string Metadata => ImportedAt is null
        ? TranscriptionStatus
        : $"{ImportedAt.Value.ToLocalTime():MMM d, h:mm tt} | {TranscriptionStatus}";
}
