using Microsoft.UI.Xaml.Media.Imaging;
using Scriptum.Models;
using Scriptum.Services;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace Scriptum.ViewModels;

/// <summary>
/// ViewModel backing the first physical notebook page capture MVP.
/// </summary>
public sealed partial class NotebookPageViewModel : ViewModelBase
{
    private static readonly string[] SupportedImageExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".bmp",
        ".gif",
        ".tif",
        ".tiff"
    ];

    private readonly IPageStorageService _storageService;
    private readonly IPageImagePreprocessingService _preprocessingService;
    private readonly ITranscriptionProvider _transcriptionProvider;
    private readonly string _importDirectory;
    private string _editablePageTitle = "Untitled notebook page";
    private string _correctedTranscriptionDraft = string.Empty;

    public NotebookPageViewModel()
        : this(
            new SqlitePageStorageService(),
            PageImagePreprocessingServiceFactory.CreateDefault(),
            TranscriptionProviderFactory.CreateDefault())
    {
    }

    public NotebookPageViewModel(
        IPageStorageService storageService,
        IPageImagePreprocessingService? preprocessingService = null,
        ITranscriptionProvider? transcriptionProvider = null)
    {
        _storageService = storageService;
        _preprocessingService = preprocessingService ?? new NoOpPageImagePreprocessingService();
        _transcriptionProvider = transcriptionProvider ?? new MockTranscriptionProvider();
        _importDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Scriptum",
            "ImportedPages");

        CurrentPage = new NotebookPage
        {
            Title = "Untitled notebook page"
        };
        SyncEditableFieldsFromCurrentPage();
        StatusMessage = "Import a photo or scan from your physical notebook.";
    }

    public NotebookPage CurrentPage { get; private set; }

    public string StatusMessage { get; private set; }

    public BitmapImage? PageImage { get; private set; }

    public bool HasImportedImage => !string.IsNullOrWhiteSpace(CurrentPage.SourceImagePath);

    public bool HasReadablePageImage => HasImportedImage && File.Exists(CurrentPage.SourceImagePath);

    public bool IsImportedImageMissing => HasImportedImage && !HasReadablePageImage;

    public string PageTitle => CurrentPage.Title;

    public string EditablePageTitle
    {
        get => _editablePageTitle;
        set
        {
            if (_editablePageTitle == value)
            {
                return;
            }

            _editablePageTitle = value;
            OnPropertyChanged();
            NotifyTextEditStateChanged();
        }
    }

    public string SourceFileLabel => CurrentPage.OriginalFileName ?? "No page imported";

    public string ImageDetails => GetImageDetails();

    public string ImageSizeLabel => GetImageSizeLabel();

    public string PageStatusLabel => IsImportedImageMissing
        ? "Image file missing"
        : HasUnsavedTextEdits
            ? "Unsaved edits"
        : HasImportedImage
            ? "Preserved original"
            : "Ready to import";

    public double PageImageRotationDegrees => NormalizeRotation(CurrentPage.RotationDegrees);

    public string RotationLabel => HasImportedImage
        ? $"{PageImageRotationDegrees:0} deg rotation"
        : "No rotation";

    public string TranscriptionText => string.IsNullOrWhiteSpace(CurrentPage.CorrectedTranscriptionText ?? CurrentPage.TranscriptionText)
        ? "No transcription yet."
        : CurrentPage.CorrectedTranscriptionText ?? CurrentPage.TranscriptionText ?? string.Empty;

    public string ImportedDateLabel => CurrentPage.ImportedAt?.ToLocalTime().ToString("f") ?? "Not imported yet";

    public string UpdatedDateLabel => HasImportedImage
        ? CurrentPage.UpdatedAt.ToLocalTime().ToString("f")
        : "Not saved yet";

    public string TranscriptionStatus => string.IsNullOrWhiteSpace(CurrentPage.CorrectedTranscriptionText ?? CurrentPage.RawTranscriptionText ?? CurrentPage.TranscriptionText)
        ? "Waiting for transcription"
        : "Draft ready";

    public bool HasUnsavedTextEdits => HasImportedImage
        && (GetNormalizedTitle(EditablePageTitle) != GetNormalizedTitle(CurrentPage.Title)
            || GetNormalizedCorrectedText(CorrectedTranscriptionDraft) != GetNormalizedCorrectedText(CurrentPage.CorrectedTranscriptionText ?? CurrentPage.TranscriptionText));

    public string TextEditStateLabel => HasUnsavedTextEdits
        ? "Unsaved text edits"
        : HasImportedImage
            ? "Text saved"
            : "No page loaded";

    public string SaveButtonLabel => HasUnsavedTextEdits
        ? "Save Edits"
        : "Save";

    public string RawTranscriptionText => string.IsNullOrWhiteSpace(CurrentPage.RawTranscriptionText)
        ? "Raw handwriting transcription will appear here after a provider is connected."
        : CurrentPage.RawTranscriptionText;

    public string CorrectedTranscriptionText => string.IsNullOrWhiteSpace(CurrentPage.CorrectedTranscriptionText ?? CurrentPage.TranscriptionText)
        ? "Corrected, copy-ready notes will appear here after review."
        : CurrentPage.CorrectedTranscriptionText ?? CurrentPage.TranscriptionText ?? string.Empty;

    public string CorrectedTranscriptionDraft
    {
        get => _correctedTranscriptionDraft;
        set
        {
            if (_correctedTranscriptionDraft == value)
            {
                return;
            }

            _correctedTranscriptionDraft = value;
            OnPropertyChanged();
            NotifyTextEditStateChanged();
        }
    }

    public async Task ImportImageAsync(string sourceImagePath, CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            if (string.IsNullOrWhiteSpace(sourceImagePath) || !File.Exists(sourceImagePath))
            {
                StatusMessage = "No readable notebook image was selected.";
                OnPropertyChanged(nameof(StatusMessage));
                return;
            }

            var extension = Path.GetExtension(sourceImagePath);
            if (!SupportedImageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                StatusMessage = "Choose a notebook page image such as JPG, PNG, BMP, GIF, or TIFF.";
                OnPropertyChanged(nameof(StatusMessage));
                return;
            }

            Directory.CreateDirectory(_importDirectory);

            var importedAt = DateTimeOffset.UtcNow;
            var pageId = Guid.NewGuid();
            var destinationPath = Path.Combine(_importDirectory, $"{pageId:N}{extension.ToLowerInvariant()}");
            File.Copy(sourceImagePath, destinationPath, overwrite: false);

            var dimensions = await ReadImageDimensionsAsync(destinationPath);
            var fileInfo = new FileInfo(destinationPath);

            CurrentPage = new NotebookPage
            {
                Id = pageId,
                Title = Path.GetFileNameWithoutExtension(sourceImagePath),
                CreatedAt = importedAt,
                UpdatedAt = importedAt,
                ImportedAt = importedAt,
                SourceImagePath = destinationPath,
                OriginalFileName = Path.GetFileName(sourceImagePath),
                SourceImageBytes = fileInfo.Length,
                ImagePixelWidth = dimensions.Width,
                ImagePixelHeight = dimensions.Height
            };

            RefreshPageImage();
            SyncEditableFieldsFromCurrentPage();
            StatusMessage = "Notebook page imported. Save it to keep it in the encrypted local index.";
            NotifyPageStateChanged();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Unable to import notebook page: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task SaveAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            if (!HasImportedImage)
            {
                StatusMessage = "Import a notebook page before saving.";
                OnPropertyChanged(nameof(StatusMessage));
                return;
            }

            await _storageService.SavePageAsync(CurrentPage);
            StatusMessage = "Saved notebook page metadata to the encrypted local index.";
            NotifyPageStateChanged();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Unable to save page: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<NotebookPage?> LoadLatestAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var page = await _storageService.LoadLatestPageAsync();
            if (page is null)
            {
                StatusMessage = "No saved page was found.";
                OnPropertyChanged(nameof(StatusMessage));
                return null;
            }

            CurrentPage = page;
            RefreshPageImage();
            SyncEditableFieldsFromCurrentPage();
            StatusMessage = "Loaded the latest notebook page from the encrypted local index.";
            NotifyPageStateChanged();
            OnPropertyChanged(nameof(CurrentPage));
            return page;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Unable to load page: {ex.Message}";
            return null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<NotebookPage?> LoadPageAsync(Guid pageId, CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var page = await _storageService.LoadPageAsync(pageId, cancellationToken);
            if (page is null)
            {
                StatusMessage = "The selected page could not be found.";
                OnPropertyChanged(nameof(StatusMessage));
                return null;
            }

            CurrentPage = page;
            RefreshPageImage();
            SyncEditableFieldsFromCurrentPage();
            StatusMessage = "Loaded notebook page from the encrypted local index.";
            NotifyPageStateChanged();
            OnPropertyChanged(nameof(CurrentPage));
            return page;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Unable to load page: {ex.Message}";
            return null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void ResetPage()
    {
        CurrentPage = new NotebookPage
        {
            Title = "Untitled notebook page"
        };
        PageImage = null;
        SyncEditableFieldsFromCurrentPage();
        StatusMessage = "Import a photo or scan from your physical notebook.";
        NotifyPageStateChanged();
        OnPropertyChanged(nameof(CurrentPage));
    }

    public async Task RotatePageAsync(double degrees, CancellationToken cancellationToken = default)
    {
        if (!HasImportedImage)
        {
            StatusMessage = "Import a notebook page before rotating it.";
            OnPropertyChanged(nameof(StatusMessage));
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            CurrentPage.RotationDegrees = NormalizeRotation(CurrentPage.RotationDegrees + degrees);
            CurrentPage.UpdatedAt = DateTimeOffset.UtcNow;

            await _storageService.SavePageAsync(CurrentPage, cancellationToken);
            StatusMessage = $"Rotated page to {PageImageRotationDegrees:0} degrees.";
            NotifyPageStateChanged();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Unable to save page rotation: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task TranscribeAsync(CancellationToken cancellationToken = default)
    {
        if (!HasImportedImage)
        {
            StatusMessage = "Import a notebook page before transcription.";
            OnPropertyChanged(nameof(StatusMessage));
            return;
        }

        if (IsImportedImageMissing)
        {
            StatusMessage = "The imported image file is missing. Reimport this notebook page before transcription.";
            OnPropertyChanged(nameof(StatusMessage));
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            StatusMessage = "Preparing page image for transcription.";
            OnPropertyChanged(nameof(StatusMessage));

            var preprocessingResult = await _preprocessingService.PrepareAsync(CurrentPage, cancellationToken);
            ApplyPreprocessingResult(preprocessingResult);

            var result = await _transcriptionProvider.TranscribeAsync(CurrentPage, preprocessingResult, cancellationToken);
            CurrentPage.RawTranscriptionText = result.RawText;
            if (string.IsNullOrWhiteSpace(CurrentPage.CorrectedTranscriptionText)
                && string.IsNullOrWhiteSpace(CorrectedTranscriptionDraft))
            {
                CurrentPage.CorrectedTranscriptionText = result.RawText;
            }

            CurrentPage.UpdatedAt = DateTimeOffset.UtcNow;
            await _storageService.SavePageAsync(CurrentPage, cancellationToken);
            SyncEditableFieldsFromCurrentPage();
            StatusMessage = $"Generated raw transcription with {result.ProviderName}.";
            NotifyPageStateChanged();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Unable to transcribe page: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task SaveTextEditsAsync(CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            if (!HasImportedImage)
            {
                StatusMessage = "Import a notebook page before saving text edits.";
                OnPropertyChanged(nameof(StatusMessage));
                return;
            }

            CurrentPage.Title = string.IsNullOrWhiteSpace(EditablePageTitle)
                ? "Untitled notebook page"
                : EditablePageTitle.Trim();
            CurrentPage.CorrectedTranscriptionText = string.IsNullOrWhiteSpace(CorrectedTranscriptionDraft)
                ? null
                : CorrectedTranscriptionDraft;
            CurrentPage.UpdatedAt = DateTimeOffset.UtcNow;

            await _storageService.SavePageAsync(CurrentPage, cancellationToken);
            SyncEditableFieldsFromCurrentPage();
            StatusMessage = "Saved page title and corrected text.";
            NotifyPageStateChanged();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Unable to save corrected text: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task ClearRawTranscriptionAsync(CancellationToken cancellationToken = default)
    {
        if (!HasImportedImage)
        {
            StatusMessage = "Import a notebook page before clearing transcription text.";
            OnPropertyChanged(nameof(StatusMessage));
            return;
        }

        CurrentPage.RawTranscriptionText = null;
        CurrentPage.UpdatedAt = DateTimeOffset.UtcNow;
        await _storageService.SavePageAsync(CurrentPage, cancellationToken);
        StatusMessage = "Cleared raw transcription.";
        NotifyPageStateChanged();
    }

    public async Task ClearCorrectedTranscriptionAsync(CancellationToken cancellationToken = default)
    {
        if (!HasImportedImage)
        {
            StatusMessage = "Import a notebook page before clearing corrected text.";
            OnPropertyChanged(nameof(StatusMessage));
            return;
        }

        CurrentPage.CorrectedTranscriptionText = null;
        CorrectedTranscriptionDraft = string.Empty;
        CurrentPage.UpdatedAt = DateTimeOffset.UtcNow;
        await _storageService.SavePageAsync(CurrentPage, cancellationToken);
        StatusMessage = "Cleared corrected text.";
        NotifyPageStateChanged();
    }

    public string? GetCorrectedTextForCopy()
    {
        var text = string.IsNullOrWhiteSpace(CorrectedTranscriptionDraft)
            ? CurrentPage.CorrectedTranscriptionText ?? CurrentPage.TranscriptionText
            : CorrectedTranscriptionDraft;

        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    public void MarkCorrectedTextCopied()
    {
        StatusMessage = "Copied corrected text to clipboard.";
        OnPropertyChanged(nameof(StatusMessage));
    }

    public void MarkCorrectedTextCopyUnavailable()
    {
        StatusMessage = "There is no corrected text to copy.";
        OnPropertyChanged(nameof(StatusMessage));
    }

    private void NotifyPageStateChanged()
    {
        OnPropertyChanged(nameof(StatusMessage));
        OnPropertyChanged(nameof(PageImage));
        OnPropertyChanged(nameof(HasImportedImage));
        OnPropertyChanged(nameof(HasReadablePageImage));
        OnPropertyChanged(nameof(IsImportedImageMissing));
        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(SourceFileLabel));
        OnPropertyChanged(nameof(ImageDetails));
        OnPropertyChanged(nameof(ImageSizeLabel));
        OnPropertyChanged(nameof(PageStatusLabel));
        OnPropertyChanged(nameof(PageImageRotationDegrees));
        OnPropertyChanged(nameof(RotationLabel));
        OnPropertyChanged(nameof(TranscriptionText));
        OnPropertyChanged(nameof(ImportedDateLabel));
        OnPropertyChanged(nameof(UpdatedDateLabel));
        OnPropertyChanged(nameof(TranscriptionStatus));
        OnPropertyChanged(nameof(HasUnsavedTextEdits));
        OnPropertyChanged(nameof(TextEditStateLabel));
        OnPropertyChanged(nameof(SaveButtonLabel));
        OnPropertyChanged(nameof(RawTranscriptionText));
        OnPropertyChanged(nameof(CorrectedTranscriptionText));
        OnPropertyChanged(nameof(EditablePageTitle));
        OnPropertyChanged(nameof(CorrectedTranscriptionDraft));
    }

    private void NotifyTextEditStateChanged()
    {
        OnPropertyChanged(nameof(HasUnsavedTextEdits));
        OnPropertyChanged(nameof(TextEditStateLabel));
        OnPropertyChanged(nameof(SaveButtonLabel));
        OnPropertyChanged(nameof(PageStatusLabel));
    }

    private void ApplyPreprocessingResult(PageImagePreprocessingResult result)
    {
        CurrentPage.PreparedImagePath = result.PreparedImagePath;
        CurrentPage.PreparedAt = result.PreparedAt;
        CurrentPage.PreprocessingSummary = result.Summary;
        CurrentPage.RotationDegrees = result.RotationDegrees;
        CurrentPage.CropX = result.CropX;
        CurrentPage.CropY = result.CropY;
        CurrentPage.CropWidth = result.CropWidth;
        CurrentPage.CropHeight = result.CropHeight;
        CurrentPage.DeskewApplied = result.DeskewApplied;
    }

    private void SyncEditableFieldsFromCurrentPage()
    {
        EditablePageTitle = CurrentPage.Title;
        CorrectedTranscriptionDraft = CurrentPage.CorrectedTranscriptionText
            ?? CurrentPage.TranscriptionText
            ?? string.Empty;
    }

    private void RefreshPageImage()
    {
        PageImage = !string.IsNullOrWhiteSpace(CurrentPage.SourceImagePath) && File.Exists(CurrentPage.SourceImagePath)
            ? new BitmapImage(new Uri(CurrentPage.SourceImagePath))
            : null;
    }

    private string GetImageDetails()
    {
        if (!HasImportedImage)
        {
            return "Import a JPG, PNG, BMP, GIF, or TIFF image from a photographed or scanned notebook page.";
        }

        if (IsImportedImageMissing)
        {
            return $"Missing imported image: {CurrentPage.SourceImagePath}";
        }

        var dimensions = CurrentPage.ImagePixelWidth is not null && CurrentPage.ImagePixelHeight is not null
            ? $"{CurrentPage.ImagePixelWidth} x {CurrentPage.ImagePixelHeight}px"
            : "dimensions unknown";

        var size = CurrentPage.SourceImageBytes is not null
            ? $"{CurrentPage.SourceImageBytes.Value / 1024.0:F1} KB"
            : "size unknown";

        var imported = CurrentPage.ImportedAt?.ToLocalTime().ToString("g") ?? "unknown import time";
        return $"{dimensions} | {size} | imported {imported}";
    }

    private string GetImageSizeLabel()
    {
        if (!HasImportedImage)
        {
            return "No image selected";
        }

        if (IsImportedImageMissing)
        {
            return "Imported image missing";
        }

        var dimensions = CurrentPage.ImagePixelWidth is not null && CurrentPage.ImagePixelHeight is not null
            ? $"{CurrentPage.ImagePixelWidth} x {CurrentPage.ImagePixelHeight}px"
            : "dimensions unknown";

        var size = CurrentPage.SourceImageBytes is not null
            ? $"{CurrentPage.SourceImageBytes.Value / 1024.0:F1} KB"
            : "size unknown";

        return $"{dimensions} | {size}";
    }

    private static double NormalizeRotation(double degrees)
    {
        var normalized = degrees % 360;
        return normalized < 0 ? normalized + 360 : normalized;
    }

    private static string GetNormalizedTitle(string? title) => string.IsNullOrWhiteSpace(title)
        ? "Untitled notebook page"
        : title.Trim();

    private static string GetNormalizedCorrectedText(string? text) => string.IsNullOrWhiteSpace(text)
        ? string.Empty
        : text;

    private static async Task<(int Width, int Height)> ReadImageDimensionsAsync(string imagePath)
    {
        var file = await StorageFile.GetFileFromPathAsync(imagePath);
        await using var stream = await file.OpenStreamForReadAsync();
        var randomAccessStream = stream.AsRandomAccessStream();
        var decoder = await BitmapDecoder.CreateAsync(randomAccessStream);
        return ((int)decoder.PixelWidth, (int)decoder.PixelHeight);
    }
}
