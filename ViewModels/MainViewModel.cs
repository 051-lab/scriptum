using CommunityToolkit.Mvvm.ComponentModel;

namespace Scriptum.ViewModels;

/// <summary>
/// Main ViewModel for the application shell.
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _applicationTitle = "Scriptum";

    [ObservableProperty]
    private string _subtitle = "Capture handwritten notebook pages and prepare them for transcription";

    public MainViewModel()
    {
        // Initialize services and load initial state here
    }
}
