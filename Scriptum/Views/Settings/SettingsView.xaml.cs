using Microsoft.UI.Xaml.Controls;
using Scriptum.ViewModels;

namespace Scriptum.Views.Settings;

/// <summary>
/// Settings page for configuring application preferences.
/// </summary>
public sealed partial class SettingsView : UserControl
{
    public SettingsViewModel ViewModel { get; }

    public SettingsView()
    {
        ViewModel = new SettingsViewModel();
        this.InitializeComponent();
    }
}
