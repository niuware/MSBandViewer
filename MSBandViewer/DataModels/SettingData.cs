namespace Niuware.MSBandViewer.DataModels
{
    /// <summary>
    /// App data settings class
    /// </summary>
    public class SettingData
    {
        // The band Id we wish to connect to
        public int pairedIndex;

        // The time we want to wait for tracking the sensors' values during a session
        public double sessionTrackInterval;

        // The separator for the exported file
        public string fileSeparator;

        // The path for saving the exported file
        public string sessionDataPath;

        // If the path is an 'Access required' path, then save the token for accessing in future app sessionss
        public string sessionDataPathToken;
    }
}