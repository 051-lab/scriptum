using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Scriptum.ViewModels;

/// <summary>
/// Base ViewModel class providing common MVVM functionality via CommunityToolkit.Mvvm.
/// All ViewModels in Scriptum inherit from this class.
/// </summary>
public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    partial void OnErrorMessageChanged(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    protected virtual void ClearError()
    {
        ErrorMessage = null;
    }
}
