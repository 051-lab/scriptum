using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
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
        NotebookPageSurface.NewPageRequested += NotebookPageSurface_NewPageRequested;
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

    private async void NotebookPageSurface_NewPageRequested(object? sender, EventArgs e)
    {
        await NewPageAsync();
    }

    private async void DeleteSelectedPage_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await DeleteSelectedPageWithConfirmationAsync();
    }

    private async void NewPageKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        await NewPageAsync();
    }

    private async void ImportPageKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        await NotebookPageSurface.ImportPageAsync();
        Bindings.Update();
    }

    private async void SavePageKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        await NotebookPageSurface.SavePageAsync();
        Bindings.Update();
    }

    private async void LoadLatestKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        await NotebookPageSurface.LoadLatestPageAsync();
        Bindings.Update();
    }

    private async void TranscribePageKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        await NotebookPageSurface.TranscribePageAsync();
        Bindings.Update();
    }

    private async void DeleteSelectedPageKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (IsTextInputFocused())
        {
            return;
        }

        args.Handled = true;
        await DeleteSelectedPageWithConfirmationAsync();
    }

    private async Task DeleteSelectedPageWithConfirmationAsync()
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

    private async Task NewPageAsync()
    {
        await ViewModel.NewPageAsync();
        NotebookPageSurface.RefreshView();
        Bindings.Update();
    }

    private static bool IsTextInputFocused()
    {
        var focusedElement = FocusManager.GetFocusedElement();
        return focusedElement is TextBox or PasswordBox or RichEditBox;
    }
}
