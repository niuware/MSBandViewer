using Windows.Storage;
using Niuware.MSBandViewer.DataModels;

namespace Niuware.MSBandViewer.Helpers
{
    /// <summary>
    /// Class for loading and saving the app settings
    /// </summary>
    public class Settings
    {
        SettingData data;
        public SettingData Data { get { return data; } set { data = value; } }

        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public Settings()
        {
            LoadSettings();
        }

        /// <summary>
        /// Adds or modifies a setting value
        /// </summary>
        /// <param name="key">Setting key name</param>
        /// <param name="value">Setting value</param>
        public void UpdateValue(string key, object value)
        {
            localSettings.Values[key] = value;
        }

        /// <summary>
        /// Gets the column separator character
        /// </summary>
        /// <returns>String separator</returns>
        public string GetStringFileSeparator()
        {
            switch (data.fileSeparator)
            {
                case "Semicolon":
                    return ";";
                case "Colon":
                    return ":";
                case "Tab":
                    return "\t";
                case "Space":
                    return " ";
                case "Comma":
                default:
                    return ",";
            }
        }

        /// <summary>
        /// Load settings data
        /// </summary>
        private void LoadSettings()
        {
            data = new SettingData();

            CreateSettingsIfNotExist();

            data.pairedIndex = (int)localSettings.Values["MSBandViewer-pairedIndex"];
            data.sessionTrackInterval = (double)localSettings.Values["MSBandViewer-sessionTrackInterval"];
            data.fileSeparator = localSettings.Values["MSBandViewer-fileSeparator"].ToString();
            data.sessionDataPath = localSettings.Values["MSBandViewer-sessionDataPath"].ToString();
            data.sessionDataPathToken = localSettings.Values["MSBandViewer-sessionDataPathToken"].ToString();

            VerifySettings();
        }

        /// <summary>
        /// If it's the first time to load the app, add the default setting values
        /// </summary>
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

        /// <summary>
        /// Sets default values on the settings file
        /// </summary>
        private void SetDefaultSettings()
        {
            localSettings.Values["MSBandViewer-pairedIndex"] = 1;
            localSettings.Values["MSBandViewer-sessionTrackInterval"] = 500.0;
            localSettings.Values["MSBandViewer-fileSeparator"] = "Comma";
            localSettings.Values["MSBandViewer-sessionDataPath"] = ApplicationData.Current.LocalFolder.Path;
            localSettings.Values["MSBandViewer-sessionDataPathToken"] = "";
        }

        /// <summary>
        /// Verify individually that all loaded settings have the correct format if not,
        /// set and save each default value
        /// </summary>
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
                localSettings.Values["MSBandViewer-sessionDataPath"] = ApplicationData.Current.LocalFolder.Path;
            }
        }
    }
}
