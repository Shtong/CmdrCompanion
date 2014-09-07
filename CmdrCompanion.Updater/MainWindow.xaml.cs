using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace CmdrCompanion.Updater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string BASE_URL = "http://cmdr-companion.shtong.mm.st/";

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

                string[] args = Environment.GetCommandLineArgs();

                if (args.Length < 2)
                {
                    info.Text = "Communication error";
                    Trace.WriteLine("The arguments list is empty !");
                    return;
                }

                Uri baseUri = new Uri(BASE_URL);
                WebClient client = new WebClient();
                string tempFolder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                string archivePath = Path.Combine(tempFolder, "update.zip");
                string softPath = args[1];

                if (!File.Exists(softPath))
                {
                    info.Text = "Could not find the software :(";
                    Trace.WriteLine("Could not find folder " + softPath);
                    return;
                }

                try
                {
                    // Download the version number
                    string sVersion = await client.DownloadStringTaskAsync(new Uri(baseUri, "version.txt"));
                    Version version = null;
                    if (!Version.TryParse(sVersion, out version))
                    {
                        info.Text = "Invalid remote version :(";
                        Trace.WriteLine("Downloaded version code: " + sVersion);
                        return;
                    }

                    // Download the archive
                    await client.DownloadFileTaskAsync(new Uri(baseUri, version.ToString() + ".zip"), archivePath);

                    // Wait for the application to close
                    info.Text = "Waiting for the application to close...";
                    await Task.Run(() =>
                    {
                        EventWaitHandle handle = null;
                        if (EventWaitHandle.TryOpenExisting("CmdrCompanion#running", out handle))
                            handle.WaitOne();
                    });

                    info.Text = "Installing...";
                    // Remove the previous version
                    File.Delete(softPath);

                    // Extract the new version
                    ZipArchive archive = ZipFile.Open(archivePath, ZipArchiveMode.Read);
                    archive.Entries[0].ExtractToFile(softPath);

                    // And launch the newly installed version !
                    Process.Start(softPath);

                    // Aaand it's done
                    Application.Current.Shutdown();
                }
                catch (WebException ex)
                {
                    info.Text = "Could not download the update :(";
                    Trace.WriteLine("Download error: " + ex.Message);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured ! " + ex.ToString());
            }
        }


    }
}
