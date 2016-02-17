using System;
using Windows.UI.Xaml;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Microsoft.Band;
using Microsoft.Band.Sensors;
using Microsoft.Band.Personalization;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Niuware.MSBandViewer.DataModel;
using System.Collections.Generic;
using Windows.Storage;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Niuware.MSBandViewer.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LandingView : Page
    {
        IBandInfo[] pairedBands;
        IBandClient bandClient;
        bool msBandUserConsent;

        DispatcherTimer timer;

        List<LineGraph> accelerometerLineGraph;
        List<LineGraph> gyroscopeLineGraph;

        List<SensorValue> gyroscopeXValues;

        public LandingView()
        {
            this.InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += Timer_Tick;

            heartRateStoryboard.Begin();
            heartRateStoryboard.Pause();

            accelerometerLineGraph = new List<LineGraph>()
            {
                new LineGraph(ref accelerometerGraphCanvas, new SolidColorBrush(Windows.UI.Colors.White), 10.0),
                new LineGraph(ref accelerometerGraphCanvas, (SolidColorBrush)Resources["SystemControlHighlightAltAccentBrush"], 10.0, -10.0),
                new LineGraph(ref accelerometerGraphCanvas, new SolidColorBrush(Windows.UI.Colors.Gray), 10.0, 10.0)
            };

            gyroscopeLineGraph = new List<LineGraph>()
            {
                new LineGraph(ref gyroscopeGraphCanvas, new SolidColorBrush(Windows.UI.Colors.White), 10.0),
                new LineGraph(ref gyroscopeGraphCanvas, (SolidColorBrush)Resources["SystemControlHighlightAltAccentBrush"], 10.0, -10.0),
                new LineGraph(ref gyroscopeGraphCanvas, new SolidColorBrush(Windows.UI.Colors.Gray), 10.0, 10.0)
            };

            gyroscopeXValues = new List<SensorValue>();
        }

        private async void Timer_Tick(object sender, object e)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { msBandClockTextBlock.Text = DateTime.Now.ToString("h:mm"); });
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { msBandDayStringTextBlock.Text = DateTime.Now.ToString("ddd"); });
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { msBandDayNumberTextBlock.Text = DateTime.Now.ToString("dd"); });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AppShell.Current.RemoteCheckTogglePaneButton();
        }

        private void StartBandClock()
        {
            double clockOffset = 0;

            if (DateTime.Now.ToString("%h").Length > 1)
            {
                clockOffset = 16;
            }

            msBandDayStringTextBlock.Margin = new Thickness(msBandDayStringTextBlock.Margin.Left + clockOffset, 100, 0, 0);
            msBandDayNumberTextBlock.Margin = new Thickness(msBandDayNumberTextBlock.Margin.Left + clockOffset, 112, 0, 0);

            timer.Start();
        }

        private async void PrepareDashboard()
        {
            SetSyncMessage("Preparing the dashboard...");

            // Unsuscribe all sensors for possible previous unterminated connections
            UnsuscribeAllSensors();

            // Get band image and clock
            StartBandClock();

            string[] msBandName = pairedBands[0].Name.Split(':');

            msBandNameTextBlock.Text = msBandName[0].Remove(msBandName[0].LastIndexOf(' '));

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

            // Get Me Tile image
            BandImage bandImage = await bandClient.PersonalizationManager.GetMeTileImageAsync();

            WriteableBitmap bandImageBitmap = bandImage.ToWriteableBitmap();

            var bitmp = new BitmapImage();

            msBandMeTileImage.Source = bandImageBitmap;

            await bandClient.NotificationManager.VibrateAsync(Microsoft.Band.Notifications.VibrationType.NotificationOneTone);

            syncGrid.Visibility = Visibility.Collapsed;
            commandBar.IsEnabled = true;
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
                msBandUserConsent = true;
            }
            else
            {
                msBandUserConsent = await bandClient.SensorManager.HeartRate.RequestUserConsentAsync();
            }

            if (!msBandUserConsent)
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

        private async void RRInterval_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandRRIntervalReading> e)
        {
            IBandRRIntervalReading rrIntervalRead = e.SensorReading;

            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                heartRateRRTextblock.Text = rrIntervalRead.Interval.ToString();
            });
        }

        private async void HeartRate_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandHeartRateReading> e)
        {
            IBandHeartRateReading heartRateRead = e.SensorReading;

            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => 
            {
                heartRateBpmTextBlock.Text = heartRateRead.HeartRate.ToString();
                heartRateStatusTextBlock.Text = heartRateRead.Quality.ToString();

                if (heartRateRead.Quality == HeartRateQuality.Locked)
                {
                    heartRateStoryboard.Resume();
                }
                else
                {
                    heartRateStoryboard.Pause();
                }
            });

            //heartRateStoryboard.SpeedRatio+= heartRateStoryboard.SpeedRatio*.1;
        }

        private async void Gsr_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandGsrReading> e)
        {
            IBandGsrReading gsrRead = e.SensorReading;

            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                gsrTextBlock.Text = String.Format("{0,-10:0.##}", ((double)gsrRead.Resistance / 1000.0));
            });
        }

        private async void SkinTemperature_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandSkinTemperatureReading> e)
        {
            IBandSkinTemperatureReading skinTempRead = e.SensorReading;

            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                skinTemperatureTextBlock.Text = String.Format("{0:0.#}", skinTempRead.Temperature);
            });
        }

        private async void Accelerometer_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandAccelerometerReading> e)
        {
            IBandAccelerometerReading accelerometerRead = e.SensorReading;

            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                accelerometerValueX.Text = String.Format("X : {0:0.####}", accelerometerRead.AccelerationX);
                accelerometerValueY.Text = String.Format("Y : {0:0.####}", accelerometerRead.AccelerationY);
                accelerometerValueZ.Text = String.Format("Z : {0:0.####}", accelerometerRead.AccelerationZ);

                accelerometerLineGraph[0].UpdateGraph(accelerometerRead.AccelerationX);
                accelerometerLineGraph[1].UpdateGraph(accelerometerRead.AccelerationY);
                accelerometerLineGraph[2].UpdateGraph(accelerometerRead.AccelerationZ);
            });
        }

        private async void Gyroscope_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandGyroscopeReading> e)
        {
            IBandGyroscopeReading gyroscropeRead = e.SensorReading;

            gyroscopeXValues.Add(new SensorValue { Timestamp = DateTime.Now, Value = gyroscropeRead.AngularVelocityX });

            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                gyroscopeValueX.Text = String.Format("X : {0:0.####}", gyroscropeRead.AngularVelocityX);
                gyroscopeValueY.Text = String.Format("Y : {0:0.####}", gyroscropeRead.AngularVelocityY);
                gyroscopeValueZ.Text = String.Format("Z : {0:0.####}", gyroscropeRead.AngularVelocityZ);

                gyroscopeLineGraph[0].UpdateGraph(gyroscropeRead.AngularVelocityX);
                gyroscopeLineGraph[1].UpdateGraph(gyroscropeRead.AngularVelocityY);
                gyroscopeLineGraph[2].UpdateGraph(gyroscropeRead.AngularVelocityZ);
            });
        }

        private async void SyncBand()
        {
            MessageDialog msgDlg = new MessageDialog("");
            syncGrid.Visibility =
            syncStackPanel.Visibility = Visibility.Visible;
            commandBar.IsEnabled = false;

            SetSyncMessage("Connecting to your band...");

            try
            {
                // Get the list of Microsoft Bands paired to the computer.
                pairedBands = await BandClientManager.Instance.GetBandsAsync();

                if (pairedBands.Length < 1)
                {
                    msgDlg.Content = "You need to pair a Microsoft Band. Go to settings to pair it now.";
                    msgDlg.Commands.Add(new UICommand("Go to settings", new UICommandInvokedHandler(this.CommandInvokedHandler), 1));
                }
                else
                {
                    bandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[0]);
                }
            }
            catch (BandAccessDeniedException)
            {
                msgDlg.Content = "Make sure your Microsoft Band (" + pairedBands[0].Name + ") has permission to synchorize to this computer and try again.";
                msgDlg.Commands.Add(new UICommand("Try Again", new UICommandInvokedHandler(this.CommandInvokedHandler), 0));
                msgDlg.Commands.Add(new UICommand("Close", new UICommandInvokedHandler(this.CommandInvokedHandler), -1));
            }
            catch (BandIOException)
            {
                msgDlg.Content = "Failed to connect to your Microsoft Band (" + pairedBands[0].Name + ").";
                msgDlg.Commands.Add(new UICommand("Try Again", new UICommandInvokedHandler(this.CommandInvokedHandler), 0));
                msgDlg.Commands.Add(new UICommand("Close", new UICommandInvokedHandler(this.CommandInvokedHandler), -1));
            }
            catch (Exception ex)
            {
                msgDlg.Content = ex.ToString();
            }
            finally
            {
                if (msgDlg.Content != "")
                {
                    syncStackPanel.Visibility = Visibility.Collapsed;

                    await msgDlg.ShowAsync();
                }
                else
                {
                    PrepareDashboard();
                }
            }
        }

        private void CommandInvokedHandler(IUICommand command)
        {
            switch ((int)command.Id)
            {
                case 0:
                    SyncBand();
                    break;
                case 1:
                    // TODO: Go to settings
                    break;
                default: break;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SyncBand();
        }

        private void syncBandButton_Click(object sender, RoutedEventArgs e)
        {
            UnsuscribeAllSensors();

            SyncBand();
        }

        public void UnsuscribeAllSensors()
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
            }
        }

        private async void saveSessionButton_Click(object sender, RoutedEventArgs e)
        {
            syncGrid.Visibility = Visibility.Visible;
            SetSyncMessage("Saving session data...");

            UnsuscribeAllSensors();

            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

            try
            {
                StorageFile sampleFile = await storageFolder.CreateFileAsync("gyrox.csv", CreationCollisionOption.GenerateUniqueName);

                foreach (SensorValue sv in gyroscopeXValues)
                {
                    await FileIO.AppendTextAsync(sampleFile, sv.ToString() + "\n");
                }

                SetSyncMessage("Session data succesfully saved.", false);
            }
            catch(Exception ex)
            {
                SetSyncMessage("Unable to save the session data. " + ex.Message, false);
            }
        }

        private async void SetSyncMessage(string message, bool progress = true)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => 
            {
                syncProgressRing.IsActive = progress;
                syncTextBlock.Text = message;
            });
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            accelerometerLineGraph[0].SizeChanged();
            accelerometerLineGraph[1].SizeChanged();
            accelerometerLineGraph[2].SizeChanged();

            gyroscopeLineGraph[0].SizeChanged();
            gyroscopeLineGraph[1].SizeChanged();
            gyroscopeLineGraph[2].SizeChanged();
        }
    }
}
