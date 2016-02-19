using System;
using System.Threading.Tasks;
using Microsoft.Band;
using Microsoft.Band.Sensors;
using Microsoft.Band.Personalization;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;

namespace Niuware.MSBandViewer.Sensor
{
    public enum BandSyncStatus
    {
        NO_PAIR_BAND_FOUND,
        NO_SYNC_PERMISSION,
        BAND_IO_EXCEPTION,
        SYNC_ERROR,
        SYNCED,
        UNKNOWN
    }

    class Band
    {
        IBandInfo[] pairedBands;
        IBandClient bandClient;

        BandSyncStatus status;
        public BandSyncStatus Status { get { return status; } }
        public HeartRateQuality HeartRateLocked { get; private set; }
        public WriteableBitmap BandScreenImage { get; private set; }
        public string BandName { get; private set; }
        public SensorData Data { get { return data; } }

        bool bandUserConsent;
        int pairedBandIndex;

        SensorData data;

        public Band()
        {
            data = new SensorData();
            status = BandSyncStatus.UNKNOWN;
        }

        public async Task SyncBand(int bandIndex = 0)
        {
            pairedBandIndex = bandIndex;

            try
            {
                // Get the list of Microsoft Bands paired to the computer.
                pairedBands = await BandClientManager.Instance.GetBandsAsync();

                if (pairedBands.Length < 1)
                {
                    status = BandSyncStatus.NO_PAIR_BAND_FOUND;
                }
                else
                {
                    bandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[pairedBandIndex]);

                    status = BandSyncStatus.SYNCED;
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

        public async Task SuscribeSensors()
        {
            // Contact sensor
            SuscribeContactSensor();

            //Heart rate sensor
            SuscribeHeartRateSensor();

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

            // Get band info
            SuscribeBandInfo();

            await bandClient.NotificationManager.VibrateAsync(Microsoft.Band.Notifications.VibrationType.NotificationOneTone);
        }

        public void UnsuscribeSensors(bool unsuscribeContact = false)
        {
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
            }
        }

        private async void SuscribeBandInfo()
        {
            string[] bandName = pairedBands[pairedBandIndex].Name.Split(':');

            BandName = bandName[0].Remove(bandName[0].LastIndexOf(' '));

            // Get Me Tile image
            BandImage bandImage = await bandClient.PersonalizationManager.GetMeTileImageAsync();

            BandScreenImage = bandImage.ToWriteableBitmap();
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

        private async void SuscribeHeartRateSensor()
        {
            if (bandClient.SensorManager.HeartRate.GetCurrentUserConsent() == UserConsent.Granted)
            {
                bandUserConsent = true;
            }
            else
            {
                bandUserConsent = await bandClient.SensorManager.HeartRate.RequestUserConsentAsync();
            }

            if (!bandUserConsent)
            {
                MessageDialog msgDlg = new MessageDialog("Access to the Heart Rate Sensor was denied.");
                await msgDlg.ShowAsync();

                return;
            }

            // Subscribe to HeartRate sensor
            bandClient.SensorManager.HeartRate.ReadingChanged += HeartRate_ReadingChanged;

            await bandClient.SensorManager.HeartRate.StartReadingsAsync();
        }

        private async void SuscribeRRInterval()
        {
            bandClient.SensorManager.RRInterval.ReadingChanged += RRInterval_ReadingChanged;
            await bandClient.SensorManager.RRInterval.StartReadingsAsync();
        }

        private void Contact_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandContactReading> e)
        {
            IBandContactReading contactRead = e.SensorReading;

            data.contact = Convert.ToBoolean(contactRead.State);
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

            data.accelerometer = new DataModel.VectorData3D<double>()
            {
                X = accelerometerRead.AccelerationX,
                Y = accelerometerRead.AccelerationY,
                Z = accelerometerRead.AccelerationZ
            };
        }

        private void Gyroscope_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandGyroscopeReading> e)
        {
            IBandGyroscopeReading gyroscropeRead = e.SensorReading;

            data.gyroscopeAngVel = new DataModel.VectorData3D<double>()
            {
                X = gyroscropeRead.AngularVelocityX,
                Y = gyroscropeRead.AngularVelocityY,
                Z = gyroscropeRead.AngularVelocityZ
            };
        }
    }
}
