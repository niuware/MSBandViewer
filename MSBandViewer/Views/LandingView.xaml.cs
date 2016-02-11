using System;
using Windows.UI.Xaml;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Microsoft.Band;
using Microsoft.Band.Sensors;
using Microsoft.Band.Personalization;
using Windows.UI.Xaml.Media.Imaging;

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
        bool bandUserConsent;

        DispatcherTimer timer;

        public LandingView()
        {
            this.InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += Timer_Tick;
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

        private async void PrepareDashboard()
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { syncTextBlock.Text = "Preparing the dashboard..."; });

            // Update band image clock
            timer.Start();

            string [] msBandName = pairedBands[0].Name.Split(':');

            msBandNameTextBlock.Text = msBandName[0].Remove(msBandName[0].LastIndexOf(' '));

            // Get Me Tile image
            BandImage bandImage = await bandClient.PersonalizationManager.GetMeTileImageAsync();

            WriteableBitmap bandImageBitmap = bandImage.ToWriteableBitmap();

            var bitmp = new BitmapImage();

            msBandMeTileImage.Source = bandImageBitmap;

            await bandClient.NotificationManager.VibrateAsync(Microsoft.Band.Notifications.VibrationType.NotificationOneTone);

            syncGrid.Visibility = Visibility.Collapsed;
            dashboardGrid.Visibility = Visibility.Visible;
        }

        private async void SyncBand()
        {
            MessageDialog msgDlg = new MessageDialog("");
            syncGrid.Visibility = Visibility.Visible;

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
                    syncGrid.Visibility = Visibility.Collapsed;

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

            heartRateStoryboard.Begin();
        }

        private void syncBandButton_Click(object sender, RoutedEventArgs e)
        {
            heartRateStoryboard.SpeedRatio+= heartRateStoryboard.SpeedRatio*.1;
        }
    }
}
