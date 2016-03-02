using Microsoft.Band;
using Microsoft.Band.Sensors;
using Niuware.MSBandViewer.Helpers;
using Niuware.MSBandViewer.MSBand;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Niuware.MSBandViewer.Views
{
    public sealed partial class DashboardPage : Page, INotifyPropertyChanged
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

        DispatcherTimer clockTimer, sensorTimer;

        Band band;

        SensorData bandData;
        public SensorData BandData {
            get { return band.Data; }
            set {
                bandData = value;
                NotifyPropertyChanged("BandData");
            }
        }

        Settings settings;

        public DashboardPage()
        {
            this.InitializeComponent();

            settings = new Settings();

            MinHeartBpm = 250;

            clockTimer = new DispatcherTimer();
            clockTimer.Interval = new TimeSpan(0, 0, 1);
            clockTimer.Tick += ClockTimer_Tick;
            clockTimer.Start();

            sensorTimer = new DispatcherTimer();
            sensorTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            sensorTimer.Tick += SensorTimer_Tick;

            heartRateStoryboard.Begin();
            //heartRateStoryboard.Pause();

            band = new Band()
            {
                SessionTrackInterval = (int)settings.Data.sessionTrackInterval
            };

            accelerometerLineGraphCanvas.Label = "Accelerometer";
            accelerometerLineGraphCanvas.AddLineGraph(0.0, "X", new SolidColorBrush(Windows.UI.Colors.White));
            accelerometerLineGraphCanvas.AddLineGraph(-10.0, "Y", (SolidColorBrush)Resources["SystemControlHighlightAccentBrush"]);
            accelerometerLineGraphCanvas.AddLineGraph(10.0, "Z", new SolidColorBrush(Windows.UI.Colors.Gray));

            gyroscopeLineGraphCanvas.Label = "Gyroscope (angular vel.)";
            gyroscopeLineGraphCanvas.AddLineGraph(0.0, "X", new SolidColorBrush(Windows.UI.Colors.White));
            gyroscopeLineGraphCanvas.AddLineGraph(-10.0, "Y", (SolidColorBrush)Resources["SystemControlHighlightAccentBrush"]);
            gyroscopeLineGraphCanvas.AddLineGraph(10.0, "Z", new SolidColorBrush(Windows.UI.Colors.Gray));
        }

        private void UpdateUI()
        {
            switch (band.Status)
            {
                case BandSyncStatus.SYNCED:
                case BandSyncStatus.SYNCED_LIMITED_ACCESS:

                    syncBandButton.Icon = new SymbolIcon(Symbol.DisableUpdates);
                    syncBandButton.Label = "unsync band";

                    syncGrid.Visibility = Visibility.Collapsed;
                    startOrStopSessionButtton.IsEnabled = true;

                    commandBar.IsEnabled = true;
                    commandBar.IsOpen = true;
                    break;
                case BandSyncStatus.SYNCED_TERMINATED:
                    
                    syncBandButton.Icon = new SymbolIcon(Symbol.Sync);
                    syncBandButton.Label = "sync band";
                    startOrStopSessionButtton.IsEnabled = false;
                    break;
                default:
                    commandBar.IsEnabled = false;
                    break;
            }
        }

        /// <summary>
        /// "Drawing loop" for all the controls, graphics, etc.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SensorTimer_Tick(object sender, object e)
        {
            if (band.Status == BandSyncStatus.SYNCED_TERMINATED || band.Status == BandSyncStatus.SYNCED_SUSCRIBING)
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
                    heartRatePath.Fill = new SolidColorBrush(Windows.UI.Colors.White);

                    // Update heart beat animation according to the real heart rate
                    heartRateStoryboard.SpeedRatio = Math.Round((band.Data.heartRate / 65.0), 2);
                }
                else
                {
                    heartRatePath.Fill = new SolidColorBrush(Windows.UI.Colors.Transparent);
                    heartRateStoryboard.SpeedRatio = 1.0;
                }

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
                    startOrStopSessionButtton.IsEnabled = true;
                }
                else
                {
                    gyroscopeLineGraphCanvas.Reset();
                    accelerometerLineGraphCanvas.Reset();

                    startOrStopSessionButtton.IsEnabled = false;

                    SetSyncMessage("Waiting for band user...");
                }
            });
        }

        private async void ClockTimer_Tick(object sender, object e)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { msBandClockTextBlock.Text = DateTime.Now.ToString("h:mm"); });
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { msBandDayStringTextBlock.Text = DateTime.Now.ToString("ddd"); });
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { msBandDayNumberTextBlock.Text = DateTime.Now.ToString("dd"); });
        }

        private async Task StartBandClock()
        {
            double clockOffset = 0;

            // Fix the left margin, if the hour requires to display 2 digits
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

            // Unsuscribe all sensors of current sessions
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

            UpdateUI();
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
                await band.SyncBand(settings.Data.pairedIndex);
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
                    if (band.Status == BandSyncStatus.SYNCED_SUSCRIBING)
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

        public void FinalizeAllTasks(bool dispose = false)
        {
            sensorTimer.Stop();
            clockTimer.Stop();
            band.UnsuscribeSensors(true, true, dispose);
        }

        public bool IsUnloadActive()
        {
            if (band.Status == BandSyncStatus.UNKNOWN || band.Status == BandSyncStatus.SYNCED_SUSCRIBING)
            {
                return false;
            }

            return true;
        }

        #region Page Commands

        private void CommandInvokedHandler(IUICommand command)
        {
            switch ((int)command.Id)
            {
                case 0:
                    SyncBand();
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

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            FinalizeAllTasks(true);
        }

        #endregion

        #region Page Control Events

        private void syncBandButton_Click(object sender, RoutedEventArgs e)
        {
            if (band.Status == BandSyncStatus.SYNCED || band.Status == BandSyncStatus.SYNCED_LIMITED_ACCESS)
            {
                band.UnsuscribeSensors(true, true);

                UpdateUI();

                SetSyncMessage("Your band has been unsynced.", false);
            }
            else 
            {
                band.Reconnect();

                StartDashboard();
            }
        }

        private async void startOrStopSessionButtton_Click(object sender, RoutedEventArgs e)
        {
            if (!band.IsSessionInProgress)
            {
                band.StartSession();

                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    startOrStopSessionButtton.Icon = new SymbolIcon(Symbol.Pause);
                    startOrStopSessionButtton.Label = "pause session";
                    msBandRecordTextBlock.Visibility = Visibility.Visible;
                    msBandRecordTextBlockStoryboard.Begin();

                    saveSessionButton.IsEnabled = false;
                });
            }
            else
            {
                band.EndSession();

                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    startOrStopSessionButtton.Icon = new SymbolIcon(Symbol.Play);
                    startOrStopSessionButtton.Label = "resume session";
                    msBandRecordTextBlock.Visibility = Visibility.Collapsed;
                    msBandRecordTextBlockStoryboard.Stop();

                    saveSessionButton.IsEnabled = true;
                });
            }
        }

        private async void saveSessionButton_Click(object sender, RoutedEventArgs e)
        {
            if (band.SessionData.Count == 0)
            {
                saveSessionButton.IsEnabled = false;
                return;
            }

            SetSyncMessage("Saving session data...");

            band.EndSession();

            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                startOrStopSessionButtton.IsEnabled = false;
                startOrStopSessionButtton.Icon = new SymbolIcon(Symbol.Play);
                startOrStopSessionButtton.Label = "start session";
                msBandRecordTextBlock.Visibility = Visibility.Collapsed;
                msBandRecordTextBlockStoryboard.Stop();
            });
            

            band.UnsuscribeSensors(true, true);

            StorageFolder storageFolder;

            if (settings.Data.sessionDataPathToken != "")
            {
                storageFolder =
                    await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFolderAsync(settings.Data.sessionDataPathToken);
            }
            else
            {
                storageFolder = await StorageFolder.GetFolderFromPathAsync(settings.Data.sessionDataPath);
            }

            try
            {
                StorageFile sessionFile = 
                    await storageFolder.CreateFileAsync("msbv-session-data" + DateTime.Now.ToString("ddMMyy-HHmm") + 
                                                            ".csv", CreationCollisionOption.GenerateUniqueName);

                string sp = settings.GetStringFileSeparator();

                // Headers for the file
                await FileIO.AppendTextAsync(sessionFile, 
                    "TIMESTAMP" + sp + " HR" + sp + " RR" + sp + " GSR" + sp + " TEMP" + sp + " ACCEL X" + sp +
                    " ACCEL Y" + sp + " ACCEL Z" + sp + " GYRO X" + sp + " GYRO Y" + sp + " GYRO Z" + sp + " CONTACT" + "\n");

                foreach (KeyValuePair<DateTime, SensorData> kvp in band.SessionData)
                {
                    await FileIO.AppendTextAsync(sessionFile, kvp.Key.ToString("HH:mm:ss") + sp + kvp.Value.Output(sp) + "\n");
                }

                SetSyncMessage("Session data saved succesfully.", false);
            }
            catch(Exception ex)
            {
                SetSyncMessage("Unable to save the session data. " + ex.Message, false);
            }

            band.ClearSession();

            UpdateUI();

            saveSessionButton.IsEnabled = false;
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
