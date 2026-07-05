using CommunityToolkit.Mvvm.ComponentModel;
using Scriptum.Models;
using Scriptum.Services;
using System.Collections.ObjectModel;

namespace Scriptum.ViewModels;

/// <summary>
/// Main ViewModel for the application shell.
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly IPageStorageService _storageService;
    private readonly string _importDirectory;

    [ObservableProperty]
    private string _applicationTitle = "Scriptum";

    [ObservableProperty]
    private string _subtitle = "Capture handwritten notebook pages and prepare them for transcription";

    [ObservableProperty]
    private ImportedPageListItemViewModel? _selectedPage;

    public MainViewModel()
        : this(new SqlitePageStorageService())
    {
    }

    public MainViewModel(IPageStorageService storageService)
    {
        _storageService = storageService;
        _importDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Scriptum",
            "ImportedPages");
        NotebookPage = new NotebookPageViewModel(_storageService);
    }

    public NotebookPageViewModel NotebookPage { get; }

    public ObservableCollection<ImportedPageListItemViewModel> ImportedPages { get; } = new();

    public bool HasImportedPages => ImportedPages.Count > 0;

    public async Task InitializeAsync()
    {
        await RefreshImportedPagesAsync();
        if (SelectedPage is not null)
        {
            await SelectPageAsync(SelectedPage);
        }
    }

    public async Task ImportPageAsync(string sourceImagePath, CancellationToken cancellationToken = default)
    {
        await NotebookPage.ImportImageAsync(sourceImagePath, cancellationToken);
        if (!NotebookPage.HasImportedImage)
        {
            return;
        }

        await NotebookPage.SaveAsync();
        await RefreshImportedPagesAsync(cancellationToken);
        SelectedPage = ImportedPages.FirstOrDefault(page => page.Id == NotebookPage.CurrentPage.Id);
    }

    public async Task SaveCurrentPageAsync(CancellationToken cancellationToken = default)
    {
        await NotebookPage.SaveAsync();
        await RefreshImportedPagesAsync(cancellationToken);
        SelectedPage = ImportedPages.FirstOrDefault(page => page.Id == NotebookPage.CurrentPage.Id);
    }

    public async Task LoadLatestPageAsync(CancellationToken cancellationToken = default)
    {
        var page = await NotebookPage.LoadLatestAsync();
        if (page is not null)
        {
            await RefreshImportedPagesAsync(cancellationToken);
            SelectedPage = ImportedPages.FirstOrDefault(item => item.Id == page.Id);
        }
    }

    public async Task SelectPageAsync(ImportedPageListItemViewModel? page, CancellationToken cancellationToken = default)
    {
        if (page is null)
        {
            return;
        }

        var loadedPage = await NotebookPage.LoadPageAsync(page.Id, cancellationToken);
        if (loadedPage is not null)
        {
            SelectedPage = ImportedPages.FirstOrDefault(item => item.Id == loadedPage.Id) ?? page;
        }
    }

    public async Task RefreshImportedPagesAsync(CancellationToken cancellationToken = default)
    {
        var selectedPageId = SelectedPage?.Id ?? NotebookPage.CurrentPage.Id;
        await RefreshImportedPagesAsync(selectedPageId, cancellationToken);
    }

    public async Task RefreshCurrentPageListItemAsync(CancellationToken cancellationToken = default)
    {
        if (!NotebookPage.HasImportedImage)
        {
            await RefreshImportedPagesAsync(cancellationToken);
            return;
        }

        var page = await _storageService.LoadPageAsync(NotebookPage.CurrentPage.Id, cancellationToken);
        if (page is null)
        {
            await RefreshImportedPagesAsync(cancellationToken);
            return;
        }

        var existing = ImportedPages.FirstOrDefault(item => item.Id == page.Id);
        if (existing is null)
        {
            await RefreshImportedPagesAsync(page.Id, cancellationToken);
            return;
        }

        existing.UpdateFrom(page);
        SelectedPage = existing;
    }

    private async Task RefreshImportedPagesAsync(Guid? selectedPageId, CancellationToken cancellationToken = default)
    {
        var pages = (await _storageService.LoadPagesAsync(cancellationToken))
            .Where(page => !string.IsNullOrWhiteSpace(page.SourceImagePath))
            .ToList();
        var pageIds = pages.Select(page => page.Id).ToHashSet();

        for (var index = ImportedPages.Count - 1; index >= 0; index--)
        {
            if (!pageIds.Contains(ImportedPages[index].Id))
            {
                ImportedPages.RemoveAt(index);
            }
        }

        for (var index = 0; index < pages.Count; index++)
        {
            var page = pages[index];
            var existing = ImportedPages.FirstOrDefault(item => item.Id == page.Id);
            if (existing is null)
            {
                ImportedPages.Insert(index, new ImportedPageListItemViewModel(page));
            }
            else
            {
                existing.UpdateFrom(page);
                var currentIndex = ImportedPages.IndexOf(existing);
                if (currentIndex != index)
                {
                    ImportedPages.Move(currentIndex, index);
                }
            }
        }

        SelectedPage = ImportedPages.FirstOrDefault(page => page.Id == selectedPageId)
            ?? ImportedPages.FirstOrDefault();
        OnPropertyChanged(nameof(HasImportedPages));
    }

    public async Task<bool> DeleteSelectedPageAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedPage is null)
        {
            return false;
        }

        var pageId = SelectedPage.Id;
        var page = await _storageService.LoadPageAsync(pageId, cancellationToken);
        await _storageService.DeletePageAsync(pageId, cancellationToken);
        TryDeleteImportedImage(page?.SourceImagePath);

        SelectedPage = null;
        await RefreshImportedPagesAsync(cancellationToken);

        if (SelectedPage is not null)
        {
            await SelectPageAsync(SelectedPage, cancellationToken);
        }
        else
        {
            NotebookPage.ResetPage();
        }

        return true;
    }

    private void TryDeleteImportedImage(string? sourceImagePath)
    {
        if (string.IsNullOrWhiteSpace(sourceImagePath) || !File.Exists(sourceImagePath))
        {
            return;
        }

        try
        {
            var importRoot = Path.GetFullPath(_importDirectory);
            var imagePath = Path.GetFullPath(sourceImagePath);
            if (imagePath.StartsWith(importRoot, StringComparison.OrdinalIgnoreCase))
            {
                File.Delete(imagePath);
            }
        }
        catch
        {
            // The database record is the source of truth; image cleanup can be retried later.
        }
    }
}
