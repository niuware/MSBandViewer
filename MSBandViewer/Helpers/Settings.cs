using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Niuware.MSBandViewer.DataModels;

namespace Niuware.MSBandViewer.Helpers
{
    public class Settings
    {
        SettingData data;
        public SettingData Data { get { return data; } set { data = value; } }

        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        //public ApplicationDataContainer LocalData { get { return localSettings; } set { localSettings = value; } }

        public Settings()
        {
            LoadSettings();
        }

        public void UpdateValue(string key, object value)
        {
            localSettings.Values[key] = value;
        }

        private void LoadSettings()
        {
            data = new SettingData();

            CreateSettingsIfNotExist();

            data.pairedIndex = (int)localSettings.Values["MSBandViewer-pairedIndex"];
            data.sessionTrackInterval = (double)localSettings.Values["MSBandViewer-sessionTrackInterval"];
            data.fileSeparator = localSettings.Values["MSBandViewer-fileSeparator"].ToString();
            data.sessionDataPath = localSettings.Values["MSBandViewer-sessionDataPath"].ToString();

            VerifySettings();
        }

        private void CreateSettingsIfNotExist()
        {
            try
            {
                int obj = (int)localSettings.Values["MSBandViewer-pairedIndex"];
            }
            catch
            {
                // If settings don't exist yet.
                SetDefaultSettings();
            }
        }

        private void SetDefaultSettings()
        {
            localSettings.Values["MSBandViewer-pairedIndex"] = 1;
            localSettings.Values["MSBandViewer-sessionTrackInterval"] = 500.0;
            localSettings.Values["MSBandViewer-fileSeparator"] = "Comma";
            localSettings.Values["MSBandViewer-sessionDataPath"] = ApplicationData.Current.LocalFolder.Name;
        }

        private void VerifySettings()
        {
            if (data.pairedIndex == 0)
            {
                data.pairedIndex = 1;
                localSettings.Values["MSBandViewer-pairedIndex"] = 1;
            }

            if (data.sessionTrackInterval == 0.0)
            {
                data.sessionTrackInterval = 500.0;
                localSettings.Values["MSBandViewer-sessionTrackInterval"] = 500.0;
            }

            if (data.fileSeparator == "")
            {
                data.fileSeparator = "Comma";
                localSettings.Values["MSBandViewer-fileSeparator"] = "Comma";
            }

            if (data.sessionDataPath == "")
            {
                data.sessionDataPath = ApplicationData.Current.LocalFolder.Name;
                localSettings.Values["MSBandViewer-sessionDataPath"] = ApplicationData.Current.LocalFolder.Name;
            }
        }
    }
}
