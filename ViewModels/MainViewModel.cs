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
        var pages = await _storageService.LoadPagesAsync(cancellationToken);

        ImportedPages.Clear();
        foreach (var page in pages.Where(page => !string.IsNullOrWhiteSpace(page.SourceImagePath)))
        {
            ImportedPages.Add(new ImportedPageListItemViewModel(page));
        }

        SelectedPage = ImportedPages.FirstOrDefault(page => page.Id == selectedPageId)
            ?? ImportedPages.FirstOrDefault();
        OnPropertyChanged(nameof(HasImportedPages));
    }
}
