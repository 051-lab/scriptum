using Scriptum.Models;

namespace Scriptum.ViewModels;

public sealed partial class ImportedPageListItemViewModel : ViewModelBase
{
    public ImportedPageListItemViewModel(NotebookPage page)
    {
        Id = page.Id;
        UpdateFrom(page);
    }

    public Guid Id { get; }

    public string Title { get; private set; } = "Untitled notebook page";

    public string SourceFileName { get; private set; } = "Imported page";

    public DateTimeOffset? ImportedAt { get; private set; }

    public string TranscriptionStatus { get; private set; } = "Waiting for transcription";

    public string Metadata => ImportedAt is null
        ? TranscriptionStatus
        : $"{ImportedAt.Value.ToLocalTime():MMM d, h:mm tt} | {TranscriptionStatus}";

    public void UpdateFrom(NotebookPage page)
    {
        Title = page.Title;
        SourceFileName = page.OriginalFileName ?? "Imported page";
        ImportedAt = page.ImportedAt;
        TranscriptionStatus = string.IsNullOrWhiteSpace(page.CorrectedTranscriptionText ?? page.RawTranscriptionText ?? page.TranscriptionText)
            ? "Waiting for transcription"
            : "Draft ready";

        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(SourceFileName));
        OnPropertyChanged(nameof(ImportedAt));
        OnPropertyChanged(nameof(TranscriptionStatus));
        OnPropertyChanged(nameof(Metadata));
    }
}
