using Microsoft.UI.Xaml.Controls;
using Scriptum.ViewModels;

namespace Scriptum.Views;

/// <summary>
/// Code-behind for the main application view.
/// </summary>
public sealed partial class MainView : Page
{
    public MainViewModel ViewModel { get; } = new();
    private bool _initialized;
    private bool _selectingPage;

    public MainView()
    {
        InitializeComponent();
        DataContext = ViewModel;
        NotebookPageSurface.SetViewModel(ViewModel.NotebookPage);
        NotebookPageSurface.PageLibraryChanged += NotebookPageSurface_PageLibraryChanged;
    }

    private async void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        await ViewModel.InitializeAsync();
        NotebookPageSurface.RefreshView();
        Bindings.Update();
    }

    private async void ImportedPages_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_selectingPage)
        {
            return;
        }

        _selectingPage = true;
        try
        {
            await ViewModel.SelectPageAsync(ViewModel.SelectedPage);
            NotebookPageSurface.RefreshView();
            Bindings.Update();
        }
        finally
        {
            _selectingPage = false;
        }
    }

    private async void NotebookPageSurface_PageLibraryChanged(object? sender, EventArgs e)
    {
        await ViewModel.RefreshCurrentPageListItemAsync();
        NotebookPageSurface.RefreshView();
        Bindings.Update();
    }

    private async void DeleteSelectedPage_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (ViewModel.SelectedPage is null)
        {
            return;
        }

        var dialog = new ContentDialog
        {
            Title = "Delete imported page?",
            Content = "This removes the selected page from the local archive and deletes its copied image from Scriptum storage.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            return;
        }

        await ViewModel.DeleteSelectedPageAsync();
        NotebookPageSurface.RefreshView();
        Bindings.Update();
    }
}
