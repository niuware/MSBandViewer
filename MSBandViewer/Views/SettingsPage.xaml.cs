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
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page, INotifyPropertyChanged
    {
        //ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

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

            //LoadSettings();

            // We add the value changed event after loading the previously saved values, if not this event will be triggered when loading the xaml
            // and the value will always be the one set as the slider minimum property
            slider.ValueChanged += slider_ValueChanged;
        }

        //private void CreateSettingsIfNotExist()
        //{
        //    try
        //    {
        //        int obj = (int)localSettings.Values["MSBandViewer-pairedIndex"];
        //    }
        //    catch
        //    {
        //        // Settings don't exist yet.
        //        SetDefaultSettings();
        //    }
        //}

        //private void SetDefaultSettings()
        //{
        //    localSettings.Values["MSBandViewer-pairedIndex"] = 1;
        //    localSettings.Values["MSBandViewer-sessionTrackInterval"] = 500.0;
        //    localSettings.Values["MSBandViewer-fileSeparator"] = "Comma";
        //    localSettings.Values["MSBandViewer-sessionDataPath"] = ApplicationData.Current.LocalFolder.Name;
        //}

        //private void VerifySettings()
        //{
        //    if (settings.pairedIndex == 0)
        //    {
        //        settings.pairedIndex = 1;
        //        localSettings.Values["MSBandViewer-pairedIndex"] = 1;
        //    }

        //    if (settings.sessionTrackInterval == 0.0)
        //    {
        //        settings.sessionTrackInterval = 500.0;
        //        localSettings.Values["MSBandViewer-sessionTrackInterval"] = 500.0;
        //    }

        //    if (settings.fileSeparator == "")
        //    {
        //        settings.fileSeparator = "Comma";
        //        localSettings.Values["MSBandViewer-fileSeparator"] = "Comma";
        //    }

        //    if (settings.sessionDataPath == "")
        //    {
        //        settings.sessionDataPath = ApplicationData.Current.LocalFolder.Name;
        //        localSettings.Values["MSBandViewer-sessionDataPath"] = ApplicationData.Current.LocalFolder.Name;
        //    }
        //}

        //private void LoadSettings()
        //{
        //    settings = new SettingData();

        //    CreateSettingsIfNotExist();

        //    settings.pairedIndex = (int)localSettings.Values["MSBandViewer-pairedIndex"];
        //    settings.sessionTrackInterval = (double)localSettings.Values["MSBandViewer-sessionTrackInterval"];
        //    settings.fileSeparator = localSettings.Values["MSBandViewer-fileSeparator"].ToString();
        //    settings.sessionDataPath = localSettings.Values["MSBandViewer-sessionDataPath"].ToString();

        //    VerifySettings();
        //}

        private void MenuPaneButton_Click(object sender, RoutedEventArgs e)
        {
            AppShell.Current.RemoteCheckTogglePaneButton();
        }

        private async void pairNowButton_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:bluetooth"));
        }

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

        private async void changeSessionDataPathButton_Click(object sender, RoutedEventArgs e)
        {
            bool unsnapped = ((Windows.UI.ViewManagement.ApplicationView.Value != Windows.UI.ViewManagement.ApplicationViewState.Snapped) || 
                Windows.UI.ViewManagement.ApplicationView.TryUnsnap());

            if (unsnapped)
            {
                FolderPicker fp = new FolderPicker();
                fp.FileTypeFilter.Add("*");

                StorageFolder sf = await fp.PickSingleFolderAsync();

                if (sf != null)
                {
                    settings.Data.sessionDataPath = sessionDataPathTextBlock.Text = sf.Path;
                    //localSettings.Values["MSBandViewer-sessionDataPath"] = settings.Data.sessionDataPath;
                    settings.UpdateValue("MSBandViewer-sessionDataPath", settings.Data.sessionDataPath);
                }
            }
        }

        private void separatorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //localSettings.Values["MSBandViewer-fileSeparator"] = ((ComboBox)sender).SelectedValue.ToString();
            settings.UpdateValue("MSBandViewer-fileSeparator", ((ComboBox)sender).SelectedValue.ToString());
        }

        private void pairedIndexComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //localSettings.Values["MSBandViewer-pairedIndex"] = (int)((ComboBox)sender).SelectedValue;
            settings.UpdateValue("MSBandViewer-pairedIndex", (int)((ComboBox)sender).SelectedValue);
        }

        private void slider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            //localSettings.Values["MSBandViewer-sessionTrackInterval"] = ((Slider)sender).Value;
            settings.UpdateValue("MSBandViewer-sessionTrackInterval", ((Slider)sender).Value);
        }
    }
}
