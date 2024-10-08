﻿using System.Text;
using System.IO;
using System.Net.Http;
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
using System.IO.Compression;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Security.Cryptography.Xml;
using System.IO.Packaging;
using System.Security.Policy;
using System;
using System.Net.NetworkInformation;

namespace Syncraft_Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        public string repoUrl = "https://github.com/SYNCRAFT-GITHUB/CuraFiles/releases/latest/download/files.zip";

        static bool InternetConnection()
        {
            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send("www.github.com");
                return (reply.Status == IPStatus.Success);
            }
            catch (PingException)
            {
                return false;
            }
        }

        private void ClearDirectoryContents(string directory)
        {
            string[] files = Directory.GetFiles(directory, $"*syncraft*", SearchOption.AllDirectories);

            foreach (string filePath in files)
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting file {filePath}: {ex.Message}");
                    return;
                }
            }
            MessageBox.Show("Syncraft files removed from Cura.", "File Removal");
        }

        static async Task DownloadFileAsync(string url, string destinationPath)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    byte[] fileData = await client.GetByteArrayAsync(url);
                    await File.WriteAllBytesAsync(destinationPath, fileData);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error downloading file: {ex.Message}");
                }
            }
        }

        async private Task UnzipFileAsync(string zipFilePath, string extractionPath)
        {
            try
            {
                if (!Directory.Exists(extractionPath))
                {
                    Directory.CreateDirectory(extractionPath);
                }

                ZipFile.ExtractToDirectory(zipFilePath, extractionPath, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error");
            }
        }

        private bool IsCuraRunning()
        {
            Process[] processes = Process.GetProcessesByName("UltiMaker-Cura");

            return processes.Length > 0;
        }

        private string ExtractionPath(string version)
        {
            return $@"C:\Program Files\Ultimaker Cura {version}\share\cura\resources\";
        }

        private string DestinationPath(string version)
        {
            return $@"C:\Program Files\Ultimaker Cura {version}\share\cura\resources\files.zip";
        }

        private bool CuraInstalled(string version)
        {
            return Directory.Exists($@"C:\Program Files\Ultimaker Cura {version}\share\cura");
        }

        private void OpenCuraDownloadUrl(string v)
        {
            if (!InternetConnection())
            {
                MessageBox.Show($"Unable to connect.", "Warning");
                return;
            }
            string url = $"https://github.com/Ultimaker/Cura/releases/download/{v}/UltiMaker-Cura-{v}-win64-X64.msi";
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        private void OpenFileExplorer(string version)
        {
            string path = $@"C:\Program Files\Ultimaker Cura {version}\share\cura\resources";
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }

        async private void PerformInstall(string v)
        {
            if (!CuraInstalled(version: v))
            {
                MessageBox.Show($"This version ({v}) is not installed.", "Warning");
                return;
            }

            if (!InternetConnection())
            {
                MessageBox.Show($"Unable to download the files, check your Internet connection.", "Warning");
                return;
            }

            if (IsCuraRunning())
            {
                MessageBox.Show("Close Cura to install.", "Warning");
                return;
            }

            await DownloadFileAsync(repoUrl, DestinationPath(version: v));
            await UnzipFileAsync(DestinationPath(version: v), ExtractionPath(version: v));

            MessageBox.Show("Installation complete.", "Status");
        }

        private void PerformUninstall(string v)
        {
            if (!CuraInstalled(version: v))
            {
                MessageBox.Show($"This version ({v}) is not installed.", "Warning");
                return;
            }
            if (IsCuraRunning())
            {
                MessageBox.Show("Close Cura to uninstall Syncraft files.", "Warning");
                return;
            }
            ClearDirectoryContents(directory: ExtractionPath(version: v));
        }

        private void PerformOpenFolder(string v)
        {
            if (!CuraInstalled(version: v))
            {
                MessageBox.Show($"This version ({v}) is not installed.", "Warning");
                return;
            }
            OpenFileExplorer(version: v);
        }

        private void InstallVersion581_Click(object sender, RoutedEventArgs e)
        {
            PerformInstall(v: "5.8.1");
        }

        private void RemoveAll581_Click(object sender, RoutedEventArgs e)
        {
            PerformUninstall(v: "5.8.1");
        }

        private void DownloadCura581_Click(object sender, RoutedEventArgs e)
        {
            OpenCuraDownloadUrl(v: "5.8.1");
        }

        private void OpenFolder581_Click(object sender, RoutedEventArgs e)
        {
            PerformOpenFolder(v: "5.8.1");
        }

    }
}