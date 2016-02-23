using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Microsoft.Band;
using Microsoft.Band.Sensors;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Niuware.MSBandViewer.DataModels;
using Niuware.MSBandViewer.Sensor;
using System.Collections.Generic;
using Windows.Storage;

namespace Niuware.MSBandViewer.Views
{
    public sealed partial class LandingView : Page, INotifyPropertyChanged
    {
        int maxHeartBpm;
        public int MaxHeartBpm
        {
            get { return maxHeartBpm; } set { maxHeartBpm = value; NotifyPropertyChanged("MaxHeartBpm"); }
        }

        int minHeartBpm;
        public int MinHeartBpm
        {
            get { return minHeartBpm; }
            set { minHeartBpm = value; NotifyPropertyChanged("MinHeartBpm"); }
        }

        DispatcherTimer timer, sensorTimer;

        //List<LineGraph> accelerometerLineGraph;
        //List<LineGraph> gyroscopeLineGraph;

        Band band;

        SensorData bandData;
        public SensorData BandData {
            get { return band.Data; }
            set {
                bandData = value;
                NotifyPropertyChanged("BandData");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public LandingView()
        {
            this.InitializeComponent();

            MinHeartBpm = 250;

            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += Timer_Tick;
            timer.Start();

            sensorTimer = new DispatcherTimer();
            sensorTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            sensorTimer.Tick += SensorTimer_Tick;

            heartRateStoryboard.Begin();
            heartRateStoryboard.Pause();

            //accelerometerLineGraph = new List<LineGraph>()
            //{
            //    new LineGraph(ref accelerometerGraphCanvas, new SolidColorBrush(Windows.UI.Colors.White), 0.0, 10.0, 2.5),
            //    new LineGraph(ref accelerometerGraphCanvas, (SolidColorBrush)Resources["SystemControlHighlightAccentBrush"], -10.0, 10.0, 2.5),
            //    new LineGraph(ref accelerometerGraphCanvas, new SolidColorBrush(Windows.UI.Colors.Gray), 10.0, 10.0, 2.5)
            //};

            //gyroscopeLineGraph = new List<LineGraph>()
            //{
            //    new LineGraph(ref gyroscopeGraphCanvas, new SolidColorBrush(Windows.UI.Colors.White), 0.0, 10.0, 0.15),
            //    new LineGraph(ref gyroscopeGraphCanvas, (SolidColorBrush)Resources["SystemControlHighlightAccentBrush"], -10.0, 10.0, 0.15),
            //    new LineGraph(ref gyroscopeGraphCanvas, new SolidColorBrush(Windows.UI.Colors.Gray), 10.0, 10.0, 0.15)
            //};

            band = new Band();

            accelerometerLineGraphCanvas.Label = "Accelerometer";
            accelerometerLineGraphCanvas.AddLineGraph(new SolidColorBrush(Windows.UI.Colors.White), 0.0, 10.0, 2.5, "X:");
            accelerometerLineGraphCanvas.AddLineGraph((SolidColorBrush)Resources["SystemControlHighlightAccentBrush"], -10.0, 10.0, 2.5, "Y:");
            accelerometerLineGraphCanvas.AddLineGraph(new SolidColorBrush(Windows.UI.Colors.Gray), 10.0, 10.0, 2.5, "Z:");

            gyroscopeLineGraphCanvas.Label = "Gyroscope (angular vel.)";
            gyroscopeLineGraphCanvas.AddLineGraph(new SolidColorBrush(Windows.UI.Colors.White), 0.0, 10.0, 0.15, "X:");
            gyroscopeLineGraphCanvas.AddLineGraph((SolidColorBrush)Resources["SystemControlHighlightAccentBrush"], -10.0, 10.0, 0.15, "Y:");
            gyroscopeLineGraphCanvas.AddLineGraph(new SolidColorBrush(Windows.UI.Colors.Gray), 10.0, 10.0, 0.15, "Z:");
        }

        private async void SensorTimer_Tick(object sender, object e)
        {
            if (band.Status == BandSyncStatus.SYNCED_TERMINATED)
            {
                return;
            }

            BandData = band.Data;

            if (band.HeartRateLocked == HeartRateQuality.Locked)
            {
                MaxHeartBpm = (BandData.heartRate > MaxHeartBpm) ? BandData.heartRate : MaxHeartBpm;
                MinHeartBpm = (MinHeartBpm < BandData.heartRate) ? MinHeartBpm : BandData.heartRate;
            }

            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                heartRateStatusTextBlock.Text = band.HeartRateLocked.ToString();

                if (band.HeartRateLocked == HeartRateQuality.Locked)
                {
                    heartRateStoryboard.Resume();
                    heartRatePath.Fill = new SolidColorBrush(Windows.UI.Colors.White);

                    // Update heart beat animation according to the real heart rate
                    heartRateStoryboard.SpeedRatio = Math.Round((band.Data.heartRate / 65.0), 2);
                }
                else
                {
                    heartRatePath.Fill = new SolidColorBrush(Windows.UI.Colors.Transparent);
                    heartRateStoryboard.Pause();
                }

                //accelerometerLineGraph[0].UpdateGraph(band.Data.accelerometer.X);
                //accelerometerLineGraph[1].UpdateGraph(band.Data.accelerometer.Y);
                //accelerometerLineGraph[2].UpdateGraph(band.Data.accelerometer.Z);

                //gyroscopeLineGraph[0].UpdateGraph(band.Data.gyroscopeAngVel.X);
                //gyroscopeLineGraph[1].UpdateGraph(band.Data.gyroscopeAngVel.Y);
                //gyroscopeLineGraph[2].UpdateGraph(band.Data.gyroscopeAngVel.Z);

                accelerometerLineGraphCanvas.UpdateValues(new double[]
                {
                    BandData.accelerometer.X,
                    BandData.accelerometer.Y,
                    BandData.accelerometer.Z,
                });

                gyroscopeLineGraphCanvas.UpdateValues(new double[] 
                {
                    BandData.gyroscopeAngVel.X,
                    BandData.gyroscopeAngVel.Y,
                    BandData.gyroscopeAngVel.Z,
                });

                if (band.IsWorn)
                {
                    syncGrid.Visibility = Visibility.Collapsed;
                }
                else
                {
                    gyroscopeLineGraphCanvas.Reset();
                    accelerometerLineGraphCanvas.Reset();
                    //gyroscopeGraphCanvas.Children.Clear();
                    //accelerometerGraphCanvas.Children.Clear();

                    SetSyncMessage("Waiting for band user...");
                }
            });
        }

        private async void Timer_Tick(object sender, object e)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { msBandClockTextBlock.Text = DateTime.Now.ToString("h:mm"); });
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { msBandDayStringTextBlock.Text = DateTime.Now.ToString("ddd"); });
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { msBandDayNumberTextBlock.Text = DateTime.Now.ToString("dd"); });
        }

        private async Task StartBandClock()
        {
            double clockOffset = 0;

            if (DateTime.Now.ToString("%h").Length > 1)
            {
                clockOffset = 16;
            }

            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                msBandDayStringTextBlock.Margin = new Thickness(msBandDayStringTextBlock.Margin.Left + clockOffset, 100, 0, 0);
                msBandDayNumberTextBlock.Margin = new Thickness(msBandDayNumberTextBlock.Margin.Left + clockOffset, 112, 0, 0);
            });
        }

        private async void StartDashboard()
        {
            SetSyncMessage("Preparing the dashboard...");

            band.UnsuscribeSensors();
            await band.SuscribeSensors();

            if (band.Status == BandSyncStatus.SYNCED_LIMITED_ACCESS)
            {
                MessageDialog msgDlg = new MessageDialog("The access to the following sensors was denied:\n");

                if (!band.SensorHRUserConsent)
                {
                    msgDlg.Content += "Heart Rate sensor\n";
                }
                if (!band.SensorRRUserConsent)
                {
                    msgDlg.Content += "RR Interval sensor";
                }

                await msgDlg.ShowAsync();
            }

            UpdateBandBackgroundImage();

            // Start band UI clock
            await StartBandClock();

            sensorTimer.Start();

            syncGrid.Visibility = Visibility.Collapsed;
            commandBar.IsEnabled = true;
        }

        private void UpdateBandBackgroundImage()
        {
            try
            {
                var bitmp = new BitmapImage();

                msBandMeTileImage.Source = band.BandBackgroundImage.ToWriteableBitmap();
            }
            catch { }
        }

        private async void SyncBand()
        {
            MessageDialog msgDlg = new MessageDialog("");
            syncStackPanel.Visibility = Visibility.Visible;
            commandBar.IsEnabled = false;

            SetSyncMessage("Connecting to your band...");

            try
            {
                await band.SyncBand();
            }
            catch (BandAccessDeniedException)
            {
                msgDlg.Content = "Make sure your Microsoft Band (" + band.BandName + ") has permission to synchorize to this computer and try again.";
                msgDlg.Commands.Add(new UICommand("Try Again", new UICommandInvokedHandler(this.CommandInvokedHandler), 0));
                msgDlg.Commands.Add(new UICommand("Close", new UICommandInvokedHandler(this.CommandInvokedHandler), -1));
            }
            catch (BandIOException)
            {
                msgDlg.Content = "Failed to connect to your Microsoft Band (" + band.BandName + ").";
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
                    if (band.Status == BandSyncStatus.SYNCED)
                    {
                        StartDashboard();
                    }
                }
            }
        }

        private async void SetSyncMessage(string message, bool progress = true)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                syncProgressRing.IsActive = progress;
                syncTextBlock.Text = message;
                syncGrid.Visibility = Visibility.Visible;
            });
        }

        public void FinalizeAllTasks()
        {
            sensorTimer.Stop();
            timer.Stop();
            band.UnsuscribeSensors(true, true);
        }

        #region Page Commands

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

        #endregion

        #region Page Events

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SyncBand();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //accelerometerLineGraph[0].SizeChanged();
            //accelerometerLineGraph[1].SizeChanged();
            //accelerometerLineGraph[2].SizeChanged();

            //gyroscopeLineGraph[0].SizeChanged();
            //gyroscopeLineGraph[1].SizeChanged();
            //gyroscopeLineGraph[2].SizeChanged();
        }

        #endregion

        #region Page Button Events

        private void syncBandButton_Click(object sender, RoutedEventArgs e)
        {
            band.UnsuscribeSensors(true, true);

            //SyncBand();
        }

        private async void startOrStopSessionButtton_Click(object sender, RoutedEventArgs e)
        {
            if (!band.IsSessionInProgress)
            {
                band.StartSession();

                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    startOrStopSessionButtton.Icon = new SymbolIcon(Symbol.Stop);
                    startOrStopSessionButtton.Label = "stop session";
                    msBandRecordTextBlock.Visibility = Visibility.Visible;
                    msBandRecordTextBlockStoryboard.Begin();
                });
            }
            else
            {
                band.EndSession();

                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    startOrStopSessionButtton.Icon = new SymbolIcon(Symbol.Play);
                    startOrStopSessionButtton.Label = "start session";
                    msBandRecordTextBlock.Visibility = Visibility.Collapsed;
                    msBandRecordTextBlockStoryboard.Stop();
                });
            }
        }

        private async void saveSessionButton_Click(object sender, RoutedEventArgs e)
        {
            SetSyncMessage("Saving session data...");

            band.UnsuscribeSensors(true, true);

            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

            try
            {
                StorageFile sessionFile = 
                    await storageFolder.CreateFileAsync("msbv-session-data" + DateTime.Now.ToString("ddMMyy-HHmm") + 
                                                            ".csv", CreationCollisionOption.GenerateUniqueName);

                // Headers for the file
                await FileIO.AppendTextAsync(sessionFile, 
                    "TIMESTAMP, HR, RR, GSR, TEMP, ACCEL X, ACCEL Y, ACCEL Z, GYRO X, GYRO Y, GYRO Z, CONTACT" + "\n");

                foreach (KeyValuePair<DateTime, SensorData> kvp in band.SessionData)
                {
                    await FileIO.AppendTextAsync(sessionFile, kvp.Key.ToString("HH:mm:ss") + "," + kvp.Value.ToString() + "\n");
                }

                SetSyncMessage("Session data succesfully saved.", false);
            }
            catch(Exception ex)
            {
                SetSyncMessage("Unable to save the session data. " + ex.Message, false);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AppShell.Current.RemoteCheckTogglePaneButton();
        }

        #endregion
    }
}
