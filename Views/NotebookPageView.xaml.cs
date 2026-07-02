using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Scriptum.ViewModels;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Scriptum.Views;

/// <summary>
/// Import-first surface for physical notebook pages.
/// </summary>
public sealed partial class NotebookPageView : UserControl
{
    public event EventHandler? PageLibraryChanged;

    public NotebookPageViewModel ViewModel { get; private set; } = new();

    public NotebookPageView()
    {
        InitializeComponent();
        DataContext = ViewModel;
        UpdateEmptyState();
    }

    public void SetViewModel(NotebookPageViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = ViewModel;
        Bindings.Update();
        UpdateEmptyState();
    }

    public void RefreshView()
    {
        Bindings.Update();
        UpdateEmptyState();
    }

    private async void ImportImageButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.PicturesLibrary
        };

        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".bmp");
        picker.FileTypeFilter.Add(".gif");
        picker.FileTypeFilter.Add(".tif");
        picker.FileTypeFilter.Add(".tiff");

        if (MainWindow.Active is not null)
        {
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(MainWindow.Active));
        }

        var file = await picker.PickSingleFileAsync();
        if (file is null)
        {
            return;
        }

        await ViewModel.ImportImageAsync(file.Path);
        if (ViewModel.HasImportedImage)
        {
            await ViewModel.SaveAsync();
            PageLibraryChanged?.Invoke(this, EventArgs.Empty);
        }

        UpdateEmptyState();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.SaveTextEditsAsync();
        PageLibraryChanged?.Invoke(this, EventArgs.Empty);
        Bindings.Update();
        UpdateEmptyState();
    }

    private async void SaveCorrectionButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.SaveTextEditsAsync();
        PageLibraryChanged?.Invoke(this, EventArgs.Empty);
        Bindings.Update();
        UpdateEmptyState();
    }

    private async void LoadLatestButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadLatestAsync();
        PageLibraryChanged?.Invoke(this, EventArgs.Empty);
        UpdateEmptyState();
    }

    private async void PrepareTranscriptionButton_Click(object sender, RoutedEventArgs e)
    {
        await RunTranscriptionAsync();
    }

    private async void TranscribeButton_Click(object sender, RoutedEventArgs e)
    {
        await RunTranscriptionAsync();
    }

    private async Task RunTranscriptionAsync()
    {
        await ViewModel.TranscribeAsync();
        PageLibraryChanged?.Invoke(this, EventArgs.Empty);
        Bindings.Update();
        UpdateEmptyState();
    }

    private void UpdateEmptyState()
    {
        EmptyState.Visibility = ViewModel.HasImportedImage ? Visibility.Collapsed : Visibility.Visible;
    }
}
