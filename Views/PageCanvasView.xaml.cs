using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Scriptum.ViewModels;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Scriptum.Views;

/// <summary>
/// Import-first surface for physical notebook pages.
/// </summary>
public sealed partial class PageCanvasView : UserControl
{
    public PageCanvasViewModel ViewModel { get; } = new();

    public PageCanvasView()
    {
        InitializeComponent();
        DataContext = ViewModel;
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
        UpdateEmptyState();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.SaveAsync();
        UpdateEmptyState();
    }

    private async void LoadLatestButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadLatestAsync();
        UpdateEmptyState();
    }

    private void PrepareTranscriptionButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.PrepareTranscription();
        UpdateEmptyState();
    }

    private void UpdateEmptyState()
    {
        EmptyState.Visibility = ViewModel.HasImportedImage ? Visibility.Collapsed : Visibility.Visible;
    }
}
