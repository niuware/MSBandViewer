using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Band;
using Microsoft.Band.Sensors;
using Microsoft.Band.Personalization;
using Niuware.MSBandViewer.DataModels;

namespace Niuware.MSBandViewer.MSBand
{
    /// <summary>
    /// Available status for the band
    /// </summary>
    public enum BandSyncStatus
    {
        NO_PAIR_BAND_FOUND,
        NO_SYNC_PERMISSION,
        BAND_IO_EXCEPTION,
        SYNC_ERROR,
        SYNCED,
        SYNCED_SUSCRIBING,
        SYNCED_LIMITED_ACCESS,
        SYNCED_TERMINATED,
        UNKNOWN
    }

    /// <summary>
    /// Class to sync, suscribe and read data from the Microsoft Band sensors
    /// </summary>
    class Band
    {
        // Microsoft Band interfaces
        IBandInfo[] pairedBands;
        IBandClient bandClient;

        // Status of the band
        BandSyncStatus status;
        public BandSyncStatus Status { get { return status; } }
        public HeartRateQuality HeartRateLocked { get; private set; }

        // Info of the band
        public BandImage BandBackgroundImage { get; private set; }
        public string BandName { get; private set; }

        // MSBand data
        public SensorData Data { get { return data; } }
        SensorData data;

        // Session data dictionary
        Dictionary<DateTime, SensorData> sessionData;
        public Dictionary<DateTime, SensorData> SessionData { get { return sessionData; } }

        // Session track interval timer
        Timer sessionTrackIntervalTimer;
        public int SessionTrackInterval{ get; set; }

        bool sessionInProgress;
        public bool IsSessionInProgress { get { return sessionInProgress; } }

        // User's band information
        bool sensorRRUserConsent, sensorHRUserConsent;
        public bool SensorRRUserConsent { get { return sensorRRUserConsent; } }
        public bool SensorHRUserConsent {  get { return sensorHRUserConsent; } }

        bool isWorn = true;
        public bool IsWorn { get { return isWorn; } }

        // Current preselected band
        int pairedBandIndex;

        public Band()
        {
            data = new SensorData();
            status = BandSyncStatus.UNKNOWN;
            SessionTrackInterval = 1000; // in milliseconds

            sessionData = new Dictionary<DateTime, SensorData>();

            sessionTrackIntervalTimer = new Timer(sessionTrackIntervalTimer_Callback, null, Timeout.Infinite, SessionTrackInterval);
        }

        /// <summary>
        /// If we had a previous sync, just reset the status and recycle the same object
        /// </summary>
        public void Reconnect()
        {
            if (status == BandSyncStatus.SYNCED_TERMINATED)
            {
                status = BandSyncStatus.SYNCED_SUSCRIBING;
            }
        }

        /// <summary>
        /// Sync to a band
        /// </summary>
        /// <param name="bandIndex">The band to sync to. '0' is the first paired band found on the system</param>
        /// <returns></returns>
        public async Task SyncBand(int bandIndex = 1)
        {
            pairedBandIndex = (bandIndex <= 0) ? 0 : bandIndex - 1;

            try
            {
                // Get the list of all Microsoft Bands paired to the device
                pairedBands = await BandClientManager.Instance.GetBandsAsync();

                if (pairedBands.Length < 1)
                {
                    status = BandSyncStatus.NO_PAIR_BAND_FOUND;
                }
                else
                {
                    pairedBandIndex = (pairedBandIndex >= pairedBands.Length) ? 0 : pairedBandIndex;

                    // Get the band's name with the bluetooth address removed
                    string[] bandName = pairedBands[pairedBandIndex].Name.Split(':');

                    BandName = bandName[0].Remove(bandName[0].LastIndexOf(' '));

                    // Connect to the band
                    bandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[pairedBandIndex]);

                    status = BandSyncStatus.SYNCED_SUSCRIBING;
                }
            }
            catch (BandAccessDeniedException)
            {
                status = BandSyncStatus.NO_SYNC_PERMISSION;

                throw;
            }
            catch (BandIOException)
            {
                status = BandSyncStatus.BAND_IO_EXCEPTION;

                throw;
            }
            catch (Exception)
            {
                status = BandSyncStatus.SYNC_ERROR;

                throw;
            }
        }

        #region Save session

        /// <summary>
        /// Save the band sensors' data each 'SessionTrackInterval' (value in milliseconds).
        /// </summary>
        /// <param name="state"></param>
        private void sessionTrackIntervalTimer_Callback(object state)
        {
            DateTime currentTime =
                new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            if (!sessionData.ContainsKey(currentTime))
            {
                data.contact = isWorn;
                sessionData.Add(currentTime, data.Copy());
            }
        }

        /// <summary>
        /// Start or resume tracking a new session
        /// </summary>
        public void StartSession()
        {
            sessionInProgress = true;

            sessionTrackIntervalTimer.Change(0, SessionTrackInterval);
        }

        /// <summary>
        /// End the current tracked session
        /// </summary>
        /// <param name="clear">Reset the saved session data?</param>
        public void EndSession(bool clear = false)
        {
            sessionInProgress = false;

            sessionTrackIntervalTimer.Change(Timeout.Infinite, SessionTrackInterval);

            if (clear)
            {
                ClearSession();
            }
        }

        /// <summary>
        /// Clear the current saved session
        /// </summary>
        public void ClearSession()
        {
            sessionData.Clear();
        }

        #endregion

        #region Sensor suscribing

        /// <summary>
        /// Suscribe all sensors asynchronously
        /// </summary>
        /// <returns></returns>
        public async Task SuscribeSensors()
        {
            // Get band info
            SuscribeBandInfo();

            // Contact sensor
            SuscribeContactSensor();

            //Heart rate sensor
            await SuscribeHeartRateSensor();

            // RR Interval
            SuscribeRRInterval();

            // GSR
            SuscribeGSRSensor();

            // Skin temperature
            SuscribeSkinTemperatureSensor();

            // Accelerometer
            SuscribeAccelerometerSensor();

            // Gyroscope
            SuscribeGyroscopeSensor();

            // Give the user a feedback when the suscribing process has finished
            await bandClient.NotificationManager.VibrateAsync(Microsoft.Band.Notifications.VibrationType.NotificationOneTone);

            status = BandSyncStatus.SYNCED;
        }

        /// <summary>
        /// Unsuscribe all sensors and remove their events
        /// </summary>
        /// <param name="unsuscribeContact">If there is no need to know if the user is wearing the band anymore, then unscribe contact sensor too</param>
        /// <param name="updateStatus">Update the band sync status</param>
        /// <param name="dispose">Dispose the bandclient object</param>
        public void UnsuscribeSensors(bool unsuscribeContact = false, bool updateStatus = false, bool dispose = false)
        {
            if (updateStatus)
            {
                status = BandSyncStatus.SYNCED_TERMINATED;
            }

            if (bandClient != null)
            {
                bandClient.SensorManager.HeartRate.ReadingChanged -= HeartRate_ReadingChanged;
                bandClient.SensorManager.HeartRate.StopReadingsAsync();

                bandClient.SensorManager.RRInterval.ReadingChanged -= RRInterval_ReadingChanged;
                bandClient.SensorManager.RRInterval.StopReadingsAsync();

                bandClient.SensorManager.SkinTemperature.ReadingChanged -= SkinTemperature_ReadingChanged;
                bandClient.SensorManager.SkinTemperature.StopReadingsAsync();

                bandClient.SensorManager.Gsr.ReadingChanged -= Gsr_ReadingChanged;
                bandClient.SensorManager.Gsr.StopReadingsAsync();

                bandClient.SensorManager.Accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
                bandClient.SensorManager.Accelerometer.StopReadingsAsync();

                bandClient.SensorManager.Gyroscope.ReadingChanged -= Gyroscope_ReadingChanged;
                bandClient.SensorManager.Gyroscope.StopReadingsAsync();

                if (unsuscribeContact)
                {
                    bandClient.SensorManager.Contact.ReadingChanged -= Contact_ReadingChanged;
                    bandClient.SensorManager.Contact.StopReadingsAsync();
                }

                // Necessary to dispose explicitly in case we want to reload the dashboard along an app session
                if (dispose)
                {
                    bandClient.Dispose();
                }
            }
        }

        private async void SuscribeBandInfo()
        {
            // Get Me Tile image
            BandBackgroundImage = await bandClient.PersonalizationManager.GetMeTileImageAsync();
        }

        private async void SuscribeContactSensor()
        {
            bandClient.SensorManager.Contact.ReadingChanged += Contact_ReadingChanged;
            await bandClient.SensorManager.Contact.StartReadingsAsync();
        }

        private async void SuscribeGyroscopeSensor()
        {
            bandClient.SensorManager.Gyroscope.ReadingChanged += Gyroscope_ReadingChanged; ;
            await bandClient.SensorManager.Gyroscope.StartReadingsAsync();
        }

        private async void SuscribeAccelerometerSensor()
        {
            bandClient.SensorManager.Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
            await bandClient.SensorManager.Accelerometer.StartReadingsAsync();
        }

        private async void SuscribeSkinTemperatureSensor()
        {
            bandClient.SensorManager.SkinTemperature.ReadingChanged += SkinTemperature_ReadingChanged; ;
            await bandClient.SensorManager.SkinTemperature.StartReadingsAsync();
        }

        private async void SuscribeGSRSensor()
        {
            bandClient.SensorManager.Gsr.ReadingChanged += Gsr_ReadingChanged;
            await bandClient.SensorManager.Gsr.StartReadingsAsync();
        }

        private async Task SuscribeHeartRateSensor()
        {
            // We need the user consent for using this sensor
            if (bandClient.SensorManager.HeartRate.GetCurrentUserConsent() == UserConsent.Granted)
            {
                sensorHRUserConsent = true;
            }
            else
            {
                sensorHRUserConsent = await bandClient.SensorManager.HeartRate.RequestUserConsentAsync();
            }

            if (!sensorHRUserConsent)
            {
                status = BandSyncStatus.SYNCED_LIMITED_ACCESS;

                return;
            }

            // Subscribe to HeartRate sensor
            bandClient.SensorManager.HeartRate.ReadingChanged += HeartRate_ReadingChanged;
            await bandClient.SensorManager.HeartRate.StartReadingsAsync();
        }

        private async void SuscribeRRInterval()
        {
            // We need the user consent for this sensor. Actually we need the Heart Rate sensor consent only
            if (bandClient.SensorManager.RRInterval.GetCurrentUserConsent() == UserConsent.Granted)
            {
                sensorRRUserConsent = true;
            }
            else
            {
                sensorRRUserConsent = await bandClient.SensorManager.RRInterval.RequestUserConsentAsync();
            }

            if (!sensorRRUserConsent)
            {
                status = BandSyncStatus.SYNCED_LIMITED_ACCESS;

                return;
            }

            bandClient.SensorManager.RRInterval.ReadingChanged += RRInterval_ReadingChanged;
            await bandClient.SensorManager.RRInterval.StartReadingsAsync();
        }

        #endregion

        #region Sensor events

        private void Contact_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandContactReading> e)
        {
            IBandContactReading contactRead = e.SensorReading;

            isWorn = Convert.ToBoolean(contactRead.State);
        }

        private void RRInterval_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandRRIntervalReading> e)
        {
            IBandRRIntervalReading rrIntervalRead = e.SensorReading;

            data.rrInterval = rrIntervalRead.Interval;
        }

        private void HeartRate_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandHeartRateReading> e)
        {
            IBandHeartRateReading heartRateRead = e.SensorReading;

            data.heartRate = heartRateRead.HeartRate;
            HeartRateLocked = heartRateRead.Quality;
        }

        private void Gsr_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandGsrReading> e)
        {
            IBandGsrReading gsrRead = e.SensorReading;

            data.gsr = gsrRead.Resistance;
        }

        private void SkinTemperature_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandSkinTemperatureReading> e)
        {
            IBandSkinTemperatureReading skinTempRead = e.SensorReading;

            data.temperature = skinTempRead.Temperature;
        }

        private void Accelerometer_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandAccelerometerReading> e)
        {
            IBandAccelerometerReading accelerometerRead = e.SensorReading;

            data.accelerometer = new VectorData3D<double>()
            {
                X = accelerometerRead.AccelerationX,
                Y = accelerometerRead.AccelerationY,
                Z = accelerometerRead.AccelerationZ
            };
        }

        private void Gyroscope_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandGyroscopeReading> e)
        {
            IBandGyroscopeReading gyroscropeRead = e.SensorReading;

            data.gyroscopeAngVel = new VectorData3D<double>()
            {
                X = gyroscropeRead.AngularVelocityX,
                Y = gyroscropeRead.AngularVelocityY,
                Z = gyroscropeRead.AngularVelocityZ
            };
        }

        #endregion
    }
}
