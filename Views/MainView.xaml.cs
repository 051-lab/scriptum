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
    private ImportedPageListItemViewModel? _lastConfirmedSelectedPage;

    public MainView()
    {
        InitializeComponent();
        DataContext = ViewModel;
        NotebookPageSurface.SetViewModel(ViewModel.NotebookPage);
        NotebookPageSurface.PageLibraryChanged += NotebookPageSurface_PageLibraryChanged;
        NotebookPageSurface.NewPageRequested += NotebookPageSurface_NewPageRequested;
        NotebookPageSurface.ImportPageRequested += NotebookPageSurface_ImportPageRequested;
        NotebookPageSurface.LoadLatestPageRequested += NotebookPageSurface_LoadLatestPageRequested;
    }

    private async void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        await ViewModel.InitializeAsync();
        _lastConfirmedSelectedPage = ViewModel.SelectedPage;
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
            var requestedPage = ViewModel.SelectedPage;
            if (requestedPage?.Id == _lastConfirmedSelectedPage?.Id)
            {
                return;
            }

            if (!await ConfirmDiscardUnsavedTextEditsAsync())
            {
                ViewModel.SelectedPage = _lastConfirmedSelectedPage;
                Bindings.Update();
                return;
            }

            await ViewModel.SelectPageAsync(requestedPage);
            _lastConfirmedSelectedPage = ViewModel.SelectedPage;
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
        _lastConfirmedSelectedPage = ViewModel.SelectedPage;
        NotebookPageSurface.RefreshView();
        Bindings.Update();
    }

    private async void NotebookPageSurface_NewPageRequested(object? sender, EventArgs e)
    {
        await NewPageAsync();
    }

    private async void NotebookPageSurface_ImportPageRequested(object? sender, EventArgs e)
    {
        await ImportPageAsync();
    }

    private async void NotebookPageSurface_LoadLatestPageRequested(object? sender, EventArgs e)
    {
        await LoadLatestPageAsync();
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
        await ImportPageAsync();
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
        await LoadLatestPageAsync();
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

        if (!await ConfirmDiscardUnsavedTextEditsAsync())
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
        _lastConfirmedSelectedPage = ViewModel.SelectedPage;
        NotebookPageSurface.RefreshView();
        Bindings.Update();
    }

    private async Task NewPageAsync()
    {
        if (!await ConfirmDiscardUnsavedTextEditsAsync())
        {
            return;
        }

        await ViewModel.NewPageAsync();
        _lastConfirmedSelectedPage = null;
        NotebookPageSurface.RefreshView();
        Bindings.Update();
    }

    private async Task ImportPageAsync()
    {
        if (!await ConfirmDiscardUnsavedTextEditsAsync())
        {
            return;
        }

        await NotebookPageSurface.ImportPageAsync();
        _lastConfirmedSelectedPage = ViewModel.SelectedPage;
        NotebookPageSurface.RefreshView();
        Bindings.Update();
    }

    private async Task LoadLatestPageAsync()
    {
        if (!await ConfirmDiscardUnsavedTextEditsAsync())
        {
            return;
        }

        await NotebookPageSurface.LoadLatestPageAsync();
        _lastConfirmedSelectedPage = ViewModel.SelectedPage;
        NotebookPageSurface.RefreshView();
        Bindings.Update();
    }

    private async Task<bool> ConfirmDiscardUnsavedTextEditsAsync()
    {
        if (!ViewModel.NotebookPage.HasUnsavedTextEdits)
        {
            return true;
        }

        var dialog = new ContentDialog
        {
            Title = "Discard unsaved edits?",
            Content = "The current page has unsaved title or corrected-text edits. Save them before switching pages, or discard them to continue.",
            PrimaryButtonText = "Discard",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot
        };

        return await dialog.ShowAsync() == ContentDialogResult.Primary;
    }

    private static bool IsTextInputFocused()
    {
        var focusedElement = FocusManager.GetFocusedElement();
        return focusedElement is TextBox or PasswordBox or RichEditBox;
    }
}
