using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Scriptum.Helpers;

namespace Scriptum.ViewModels;

/// <summary>
/// ViewModel for the settings page.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private string _qwenApiKey = string.Empty;

    [ObservableProperty]
    private string _selectedTheme = "System";

    [ObservableProperty]
    private bool _isApiConfigured;

    public SettingsViewModel()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        QwenApiKey = AppSettingsHelper.QwenApiKey ?? string.Empty;
        SelectedTheme = AppSettingsHelper.AppTheme;
        IsApiConfigured = AppSettingsHelper.IsQwenApiConfigured();
    }

    [RelayCommand]
    private void SaveSettings()
    {
        AppSettingsHelper.QwenApiKey = QwenApiKey;
        AppSettingsHelper.AppTheme = SelectedTheme;
        IsApiConfigured = AppSettingsHelper.IsQwenApiConfigured();
    }

    [RelayCommand]
    private void ClearApiKey()
    {
        QwenApiKey = string.Empty;
        SaveSettingsCommand.Execute(null);
    }
}
