using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Scriptum.Models;
using Scriptum.Services;

namespace Scriptum.ViewModels;

/// <summary>
/// Main ViewModel for the application shell.
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly IDataService? _dataService;

    [ObservableProperty]
    private string _applicationTitle = "Scriptum";

    [ObservableProperty]
    private string _subtitle = "Premium Local-First Digital Notebook";

    [ObservableProperty]
    private ObservableCollection<Notebook> _notebooks = new();

    [ObservableProperty]
    private Notebook? _selectedNotebook;

    [ObservableProperty]
    private bool _isLoading;

    public MainViewModel() : this(null)
    {
    }

    public MainViewModel(IDataService? dataService)
    {
        _dataService = dataService;
    }

    [RelayCommand]
    private async Task LoadNotebooksAsync()
    {
        if (_dataService == null) return;

        IsLoading = true;
        try
        {
            await _dataService.InitializeAsync();
            var notebooks = await _dataService.GetNotebooksAsync();
            Notebooks.Clear();
            foreach (var notebook in notebooks)
            {
                Notebooks.Add(notebook);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CreateNotebookAsync(string name)
    {
        if (_dataService == null || string.IsNullOrWhiteSpace(name)) return;

        var notebook = await _dataService.CreateNotebookAsync(name);
        Notebooks.Add(notebook);
        SelectedNotebook = notebook;
    }
}
