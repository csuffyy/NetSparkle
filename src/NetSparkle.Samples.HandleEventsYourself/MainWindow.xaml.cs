﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;

namespace NetSparkle.Samples.HandleEventsYourself
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Sparkle _sparkle;
        private UpdateInfo _updateInfo;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree("Software\\Microsoft\\NetSparkle.TestAppNetCoreWPF");
            }
            catch { }

            // get sparkle ready
            DownloadUpdateButton.IsEnabled = false;

            _sparkle = new Sparkle("https://netsparkleupdater.github.io/NetSparkle/files/sample-app/appcast.xml")
            {
                UIFactory = null,
            };
            // TLS 1.2 required by GitHub (https://developer.github.com/changes/2018-02-01-weak-crypto-removal-notice/)
            _sparkle.SecurityProtocolType = System.Net.SecurityProtocolType.Tls12;
        }

        private async void CheckUpdates_Click(object sender, RoutedEventArgs e)
        {
            _updateInfo = await _sparkle.CheckForUpdatesAtUserRequest();
            // use _sparkle.CheckForUpdatesQuietly() if you don't want the user to know you are checking for updates!
            // if you use CheckForUpdatesAtUserRequest() and are using a UI, then handling things yourself is rather silly
            // as it will show a UI for things
            if (_updateInfo != null)
            {
                switch (_updateInfo.Status)
                {
                    case Enums.UpdateStatus.UpdateAvailable:
                        UpdateInfo.Content = "There's an update available!";
                        DownloadUpdateButton.IsEnabled = true;
                        break;
                    case Enums.UpdateStatus.UpdateNotAvailable:
                        UpdateInfo.Content = "There's no update available :(";
                        DownloadUpdateButton.IsEnabled = false;
                        break;
                    case Enums.UpdateStatus.UserSkipped:
                        UpdateInfo.Content = "The user skipped this update!";
                        DownloadUpdateButton.IsEnabled = false;
                        break;
                    case Enums.UpdateStatus.CouldNotDetermine:
                        UpdateInfo.Content = "We couldn't tell if there was an update...";
                        DownloadUpdateButton.IsEnabled = false;
                        break;
                }
            }
        }

        private async void DownloadUpdate_Click(object sender, RoutedEventArgs e)
        {
            // this is async so that it can grab the download file name from the server
            _sparkle.FinishedDownloading -= _sparkle_FinishedDownloading;
            _sparkle.FinishedDownloading += _sparkle_FinishedDownloading;
            await _sparkle.StartDownloadingUpdate(_updateInfo.Updates.First());
            // ok, the file is downloading now
        }

        private void _sparkle_FinishedDownloading(string path)
        {
            MessageBox.Show("Done downloading!");
        }
    }
}