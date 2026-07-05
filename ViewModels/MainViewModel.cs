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
    private readonly List<ImportedPageListItemViewModel> _allImportedPages = new();

    [ObservableProperty]
    private string _applicationTitle = "Scriptum";

    [ObservableProperty]
    private string _subtitle = "Capture handwritten notebook pages and prepare them for transcription";

    [ObservableProperty]
    private ImportedPageListItemViewModel? _selectedPage;

    private string _pageSearchText = string.Empty;

    public MainViewModel()
        : this(new SqlitePageStorageService())
    {
    }

    public MainViewModel(IPageStorageService storageService)
    {
        _storageService = storageService;
        DefaultNotebook = Notebook.CreateDefault();
        _importDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Scriptum",
            "ImportedPages");
        NotebookPage = new NotebookPageViewModel(_storageService, defaultNotebook: DefaultNotebook);
        Notebooks.Add(new NotebookListItemViewModel(DefaultNotebook));
    }

    public Notebook DefaultNotebook { get; }

    public NotebookPageViewModel NotebookPage { get; }

    public ObservableCollection<ImportedPageListItemViewModel> ImportedPages { get; } = new();

    public ObservableCollection<NotebookListItemViewModel> Notebooks { get; } = new();

    public bool HasImportedPages => ImportedPages.Count > 0;

    public string PageSearchText
    {
        get => _pageSearchText;
        set
        {
            if (_pageSearchText == value)
            {
                return;
            }

            _pageSearchText = value;
            OnPropertyChanged();
            ApplyPageSearchFilter();
        }
    }

    public bool HasPageSearchFilter => !string.IsNullOrWhiteSpace(PageSearchText);

    public string PageSearchResultLabel => HasPageSearchFilter
        ? $"{ImportedPages.Count} of {_allImportedPages.Count} pages"
        : $"{ImportedPages.Count} pages";

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

    public Task NewPageAsync()
    {
        SelectedPage = null;
        NotebookPage.ResetPage();
        return Task.CompletedTask;
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
            existing = _allImportedPages.FirstOrDefault(item => item.Id == page.Id);
            if (existing is null)
            {
                await RefreshImportedPagesAsync(page.Id, cancellationToken);
                return;
            }
        }

        existing.UpdateFrom(page);
        ApplyPageSearchFilter(page.Id);
        if (ImportedPages.Contains(existing))
        {
            SelectedPage = existing;
            return;
        }

        SelectedPage = null;
    }

    private async Task RefreshImportedPagesAsync(Guid? selectedPageId, CancellationToken cancellationToken = default)
    {
        var pages = (await _storageService.LoadPagesAsync(cancellationToken))
            .Where(page => !string.IsNullOrWhiteSpace(page.SourceImagePath))
            .ToList();
        foreach (var page in pages)
        {
            EnsurePageNotebook(page);
        }

        _allImportedPages.Clear();

        foreach (var page in pages)
        {
            _allImportedPages.Add(new ImportedPageListItemViewModel(page));
        }

        ApplyPageSearchFilter(selectedPageId);
        SelectedPage = ImportedPages.FirstOrDefault(page => page.Id == selectedPageId)
            ?? ImportedPages.FirstOrDefault();
        RefreshNotebookListItems(pages);
    }

    private void ApplyPageSearchFilter(Guid? preferredSelectedPageId = null)
    {
        var filteredPages = string.IsNullOrWhiteSpace(PageSearchText)
            ? _allImportedPages
            : _allImportedPages
                .Where(page => page.SearchText.Contains(PageSearchText.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();

        ImportedPages.Clear();
        foreach (var page in filteredPages)
        {
            ImportedPages.Add(page);
        }

        if (SelectedPage is not null && ImportedPages.All(page => page.Id != SelectedPage.Id))
        {
            SelectedPage = preferredSelectedPageId is not null
                ? ImportedPages.FirstOrDefault(page => page.Id == preferredSelectedPageId)
                : null;
        }

        OnPropertyChanged(nameof(HasImportedPages));
        OnPropertyChanged(nameof(HasPageSearchFilter));
        OnPropertyChanged(nameof(PageSearchResultLabel));
    }

    private void EnsurePageNotebook(NotebookPage page)
    {
        if (page.NotebookId is null || page.NotebookId == Guid.Empty)
        {
            page.NotebookId = DefaultNotebook.Id;
        }

        if (string.IsNullOrWhiteSpace(page.NotebookTitle))
        {
            page.NotebookTitle = DefaultNotebook.Title;
        }
    }

    private void RefreshNotebookListItems(IReadOnlyCollection<NotebookPage> pages)
    {
        var defaultNotebookItem = Notebooks.FirstOrDefault(item => item.Id == DefaultNotebook.Id);
        if (defaultNotebookItem is null)
        {
            Notebooks.Insert(0, new NotebookListItemViewModel(DefaultNotebook));
            defaultNotebookItem = Notebooks[0];
        }

        var defaultPageCount = pages.Count(page => page.NotebookId == DefaultNotebook.Id);
        defaultNotebookItem.UpdateFrom(DefaultNotebook, defaultPageCount);
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
        TryDeletePreparedImage(page?.PreparedImagePath);

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

    private void TryDeletePreparedImage(string? preparedImagePath)
    {
        if (string.IsNullOrWhiteSpace(preparedImagePath) || !File.Exists(preparedImagePath))
        {
            return;
        }

        try
        {
            var preparedRoot = Path.GetFullPath(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Scriptum",
                "PreparedPages"));
            var imagePath = Path.GetFullPath(preparedImagePath);
            if (imagePath.StartsWith(preparedRoot, StringComparison.OrdinalIgnoreCase))
            {
                File.Delete(imagePath);
            }
        }
        catch
        {
            // Prepared images can be regenerated from the imported source image.
        }
    }
}
