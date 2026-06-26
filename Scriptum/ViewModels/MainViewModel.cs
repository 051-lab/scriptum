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
    private string _subtitle = "Premium Local-First Digital Notebook";

    public MainViewModel()
    {
        // Initialize services and load initial state here
    }
}
