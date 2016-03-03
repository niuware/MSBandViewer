using System;
using System.ComponentModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Niuware.MSBandViewer.DataModels;
using Niuware.MSBandViewer.Helpers;

namespace Niuware.MSBandViewer.Views
{
    /// <summary>
    /// Page for adjusting the app settings
    /// </summary>
    public sealed partial class SettingsPage : Page, INotifyPropertyChanged
    {
        Settings settings;
        public SettingData Settings
        {
            get { return settings.Data; }
            set
            {
                settings.Data = value;
                NotifyPropertyChanged("Settings");
            }
        }

        public SettingsPage()
        {
            this.InitializeComponent();

            settings = new Settings();

            // We add the value changed event after loading the previously saved values, if not this event will be triggered when loading the xaml
            // and the value will always be the one set as the slider minimum property
            slider.ValueChanged += slider_ValueChanged;
        }

        #region Page Control Events

        /// <summary>
        /// Open Windows BT settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void pairNowButton_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:bluetooth"));
        }

        /// <summary>
        /// Set the path to save the session files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void changeSessionDataPathButton_Click(object sender, RoutedEventArgs e)
        {
            // If the app is snapped the FolderPicker won't work
            bool unsnapped = ((Windows.UI.ViewManagement.ApplicationView.Value != Windows.UI.ViewManagement.ApplicationViewState.Snapped) || 
                Windows.UI.ViewManagement.ApplicationView.TryUnsnap());

            if (unsnapped)
            {
                FolderPicker fp = new FolderPicker();
                fp.FileTypeFilter.Add("*");

                StorageFolder sf = await fp.PickSingleFolderAsync();

                if (sf != null)
                {
                    // Save accessToken for the selected folder
                    string pickedFolderToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(sf);

                    settings.UpdateValue("MSBandViewer-sessionDataPathToken", pickedFolderToken);

                    settings.Data.sessionDataPath = sessionDataPathTextBlock.Text = sf.Path;
                    settings.UpdateValue("MSBandViewer-sessionDataPath", settings.Data.sessionDataPath);

                    ToolTipService.SetToolTip(sessionDataPathTextBlock, settings.Data.sessionDataPath);
                }
            }
        }

        /// <summary>
        /// Reset the default path (App Local State Storage Folder)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void resetSessionDataPathButton_Click(object sender, RoutedEventArgs e)
        {
            settings.UpdateValue("MSBandViewer-sessionDataPathToken", "");
            settings.Data.sessionDataPathToken = "";

            settings.Data.sessionDataPath = sessionDataPathTextBlock.Text = ApplicationData.Current.LocalFolder.Path;
            settings.UpdateValue("MSBandViewer-sessionDataPath", settings.Data.sessionDataPath);

            ToolTipService.SetToolTip(sessionDataPathTextBlock, settings.Data.sessionDataPath);
        }

        private void separatorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            settings.UpdateValue("MSBandViewer-fileSeparator", ((ComboBox)sender).SelectedValue.ToString());
        }

        private void pairedIndexComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            settings.UpdateValue("MSBandViewer-pairedIndex", (int)((ComboBox)sender).SelectedValue);
        }

        private void slider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            settings.UpdateValue("MSBandViewer-sessionTrackInterval", ((Slider)sender).Value);
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
