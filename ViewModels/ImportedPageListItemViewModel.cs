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

    public Guid? NotebookId { get; private set; }

    public string SearchText { get; private set; } = string.Empty;

    public DateTimeOffset? ImportedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public string TranscriptionStatus { get; private set; } = "Waiting for transcription";

    public bool IsImageMissing { get; private set; }

    public string Metadata => $"Updated {UpdatedAt.ToLocalTime():MMM d, h:mm tt} | {GetStatusMetadata()}";

    public void UpdateFrom(NotebookPage page)
    {
        Title = page.Title;
        SourceFileName = page.OriginalFileName ?? "Imported page";
        NotebookId = page.NotebookId;
        SearchText = string.Join(
            ' ',
            page.Title,
            page.OriginalFileName,
            page.NotebookTitle,
            page.RawTranscriptionText,
            page.CorrectedTranscriptionText,
            page.TranscriptionText);
        ImportedAt = page.ImportedAt;
        UpdatedAt = page.UpdatedAt;
        IsImageMissing = !string.IsNullOrWhiteSpace(page.SourceImagePath) && !File.Exists(page.SourceImagePath);
        TranscriptionStatus = string.IsNullOrWhiteSpace(page.CorrectedTranscriptionText ?? page.RawTranscriptionText ?? page.TranscriptionText)
            ? "Waiting for transcription"
            : "Draft ready";

        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(SourceFileName));
        OnPropertyChanged(nameof(NotebookId));
        OnPropertyChanged(nameof(SearchText));
        OnPropertyChanged(nameof(ImportedAt));
        OnPropertyChanged(nameof(UpdatedAt));
        OnPropertyChanged(nameof(IsImageMissing));
        OnPropertyChanged(nameof(TranscriptionStatus));
        OnPropertyChanged(nameof(Metadata));
    }

    private string GetStatusMetadata() => IsImageMissing
        ? "Image missing"
        : TranscriptionStatus;
}
