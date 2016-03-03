using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Niuware.MSBandViewer.DataModels;

namespace Niuware.MSBandViewer.Helpers
{
    /// <summary>
    /// Exports a file with the session data
    /// </summary>
    class SessionExport
    {
        Settings settings;
        StorageFile sessionFile;
        public Dictionary<DateTime, SensorData> Data { private get; set; }

        string sp;  // Separator for each column

        public SessionExport()
        {
            settings = new Settings();
            sp = settings.GetStringFileSeparator();
        }

        /// <summary>
        /// Exports the session data file
        /// </summary>
        /// <returns>True if success</returns>
        public async Task<bool> ExportFile()
        {
            if (await CreateFile())
            {
                await FileIO.AppendLinesAsync(sessionFile, GetAllHeaders());

                foreach (KeyValuePair<DateTime, SensorData> kvp in Data)
                {
                    await FileIO.AppendTextAsync(sessionFile, kvp.Key.ToString("HH:mm:ss") + sp + kvp.Value.Output(sp) + "\n");
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Creates the StorageFile to export
        /// </summary>
        /// <returns>True if success</returns>
        private async Task<bool> CreateFile()
        {
            // Load the export path
            StorageFolder storageFolder;

            if (settings.Data.sessionDataPathToken != "")
            {
                storageFolder =
                    await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFolderAsync(
                        settings.Data.sessionDataPathToken);
            }
            else
            {
                storageFolder = await StorageFolder.GetFolderFromPathAsync(settings.Data.sessionDataPath);
            }

            try
            {
                sessionFile =
                    await storageFolder.CreateFileAsync("msbv-session-data" + DateTime.Now.ToString("ddMMyy-HHmm") +
                                                            ".csv", CreationCollisionOption.GenerateUniqueName);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns all headers for the session data file
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GetAllHeaders()
        {
            string[] lines = {
                GetDurationHeader(),
                GetDataHeaders()
            };

            return lines;
        }

        /// <summary>
        /// Duration of the exported session
        /// </summary>
        /// <returns>String with the duration in sec/min of the session</returns>
        private string GetDurationHeader()
        {
            // Session duration
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, Data.Count * (int)settings.Data.sessionTrackInterval).Duration();

            string durationStr = (duration.TotalSeconds > 60.0) ? duration.TotalMinutes.ToString() + "min" : duration.TotalSeconds.ToString() + "s";

            return "TOTAL" + sp + "DURATION" + sp + durationStr;
        }

        /// <summary>
        /// Headers for the data 
        /// </summary>
        /// <returns>Separated string with the header names</returns>
        private string GetDataHeaders()
        {
            string header = "timestamp" + sp;

            header += Helper.GetFieldsAsHeaders(typeof(SensorData), sp);

            return header.Substring(0, header.LastIndexOf(sp));
        }
    }
}
