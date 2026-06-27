using Microsoft.UI.Xaml;
using Windows.Storage;

namespace Scriptum.Helpers;

/// <summary>
/// Helper class for managing application settings.
/// </summary>
public static class AppSettingsHelper
{
    private const string QwenApiKeyKey = "QwenApiKey";
    private const string ThemeKey = "AppTheme";

    /// <summary>
    /// Gets or sets the Qwen-VL API key.
    /// </summary>
    public static string? QwenApiKey
    {
        get => GetSetting(QwenApiKeyKey, string.Empty);
        set => SaveSetting(QwenApiKeyKey, value);
    }

    /// <summary>
    /// Gets or sets the application theme (Light/Dark/System).
    /// </summary>
    public static string AppTheme
    {
        get => GetSetting(ThemeKey, "System");
        set => SaveSetting(ThemeKey, value);
    }

    /// <summary>
    /// Saves a setting to local storage.
    /// </summary>
    public static void SaveSetting(string key, string? value)
    {
        var localSettings = ApplicationData.Current.LocalSettings;
        localSettings.Values[key] = value ?? string.Empty;
    }

    /// <summary>
    /// Gets a setting from local storage.
    /// </summary>
    public static string GetSetting(string key, string defaultValue = "")
    {
        var localSettings = ApplicationData.Current.LocalSettings;
        if (localSettings.Values.TryGetValue(key, out var value))
        {
            return value?.ToString() ?? defaultValue;
        }
        return defaultValue;
    }

    /// <summary>
    /// Checks if the Qwen API key is configured.
    /// </summary>
    public static bool IsQwenApiConfigured()
    {
        return !string.IsNullOrWhiteSpace(QwenApiKey);
    }
}
