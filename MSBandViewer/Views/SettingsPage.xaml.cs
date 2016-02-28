using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Niuware.MSBandViewer.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        enum FileSeparator
        {
            COMMA,
            TAB,
            SEMICOLON,
            SPACE
        }

        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private void MenuPaneButton_Click(object sender, RoutedEventArgs e)
        {
            AppShell.Current.RemoteCheckTogglePaneButton();
        }

        private async void pairNowButton_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:bluetooth"));
        }
    }
}
