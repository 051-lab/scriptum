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
    private readonly ITranscriptionProvider _transcriptionProvider;
    private readonly string _importDirectory;
    private string _editablePageTitle = "Untitled notebook page";
    private string _correctedTranscriptionDraft = string.Empty;

    public NotebookPageViewModel()
        : this(new SqlitePageStorageService(), new MockTranscriptionProvider())
    {
    }

    public NotebookPageViewModel(IPageStorageService storageService, ITranscriptionProvider? transcriptionProvider = null)
    {
        _storageService = storageService;
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
        }
    }

    public string SourceFileLabel => CurrentPage.OriginalFileName ?? "No page imported";

    public string ImageDetails => GetImageDetails();

    public string ImageSizeLabel => GetImageSizeLabel();

    public string PageStatusLabel => HasImportedImage ? "Preserved original" : "Ready to import";

    public string TranscriptionText => string.IsNullOrWhiteSpace(CurrentPage.CorrectedTranscriptionText ?? CurrentPage.TranscriptionText)
        ? "No transcription yet."
        : CurrentPage.CorrectedTranscriptionText ?? CurrentPage.TranscriptionText ?? string.Empty;

    public string ImportedDateLabel => CurrentPage.ImportedAt?.ToLocalTime().ToString("f") ?? "Not imported yet";

    public string TranscriptionStatus => string.IsNullOrWhiteSpace(CurrentPage.CorrectedTranscriptionText ?? CurrentPage.RawTranscriptionText ?? CurrentPage.TranscriptionText)
        ? "Waiting for transcription"
        : "Draft ready";

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
            OnPropertyChanged(nameof(StatusMessage));
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

    public async Task TranscribeAsync(CancellationToken cancellationToken = default)
    {
        if (!HasImportedImage)
        {
            StatusMessage = "Import a notebook page before transcription.";
            OnPropertyChanged(nameof(StatusMessage));
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var result = await _transcriptionProvider.TranscribeAsync(CurrentPage, cancellationToken);
            CurrentPage.RawTranscriptionText = result.RawText;
            if (string.IsNullOrWhiteSpace(CurrentPage.CorrectedTranscriptionText)
                && string.IsNullOrWhiteSpace(CorrectedTranscriptionDraft))
            {
                CurrentPage.CorrectedTranscriptionText = result.RawText;
            }

            CurrentPage.UpdatedAt = DateTimeOffset.UtcNow;
            await _storageService.SavePageAsync(CurrentPage, cancellationToken);
            SyncEditableFieldsFromCurrentPage();
            StatusMessage = $"Generated mock raw transcription with {result.ProviderName}.";
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

    private void NotifyPageStateChanged()
    {
        OnPropertyChanged(nameof(StatusMessage));
        OnPropertyChanged(nameof(PageImage));
        OnPropertyChanged(nameof(HasImportedImage));
        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(SourceFileLabel));
        OnPropertyChanged(nameof(ImageDetails));
        OnPropertyChanged(nameof(ImageSizeLabel));
        OnPropertyChanged(nameof(PageStatusLabel));
        OnPropertyChanged(nameof(TranscriptionText));
        OnPropertyChanged(nameof(ImportedDateLabel));
        OnPropertyChanged(nameof(TranscriptionStatus));
        OnPropertyChanged(nameof(RawTranscriptionText));
        OnPropertyChanged(nameof(CorrectedTranscriptionText));
        OnPropertyChanged(nameof(EditablePageTitle));
        OnPropertyChanged(nameof(CorrectedTranscriptionDraft));
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

        var dimensions = CurrentPage.ImagePixelWidth is not null && CurrentPage.ImagePixelHeight is not null
            ? $"{CurrentPage.ImagePixelWidth} x {CurrentPage.ImagePixelHeight}px"
            : "dimensions unknown";

        var size = CurrentPage.SourceImageBytes is not null
            ? $"{CurrentPage.SourceImageBytes.Value / 1024.0:F1} KB"
            : "size unknown";

        return $"{dimensions} | {size}";
    }

    private static async Task<(int Width, int Height)> ReadImageDimensionsAsync(string imagePath)
    {
        var file = await StorageFile.GetFileFromPathAsync(imagePath);
        await using var stream = await file.OpenStreamForReadAsync();
        var randomAccessStream = stream.AsRandomAccessStream();
        var decoder = await BitmapDecoder.CreateAsync(randomAccessStream);
        return ((int)decoder.PixelWidth, (int)decoder.PixelHeight);
    }
}
