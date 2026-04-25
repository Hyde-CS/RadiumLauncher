using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace RadiumLauncher
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeWebView();
            this.Loaded += (s, e) => CheckPath();
        }

        private async void InitializeWebView()
        {
            try
            {
                await webView.EnsureCoreWebView2Async(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("WebView2 failed to load: " + ex.Message);
            }
        }

        private void CheckPath()
        {
            string path = Properties.Settings.Default.GamePath;
            if (string.IsNullOrEmpty(path) || !File.Exists(Path.Combine(path, "Radium.exe")))
            {
                PromptForFolder();
            }
        }

        private void PromptForFolder()
        {
            MessageBox.Show("Please select your Radium folder (containing Radium.exe)");
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Select Folder"
            };

            if (dialog.ShowDialog() == true)
            {
                string folderPath = Path.GetDirectoryName(dialog.FileName);
                if (File.Exists(Path.Combine(folderPath, "Radium.exe")))
                {
                    Properties.Settings.Default.GamePath = folderPath;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    MessageBox.Show("Invalid folder. Radium.exe not found.");
                    PromptForFolder();
                }
            }
        }

        private async void LaunchGame(string batName)
        {
            string folder = Properties.Settings.Default.GamePath;
            string batPath = Path.Combine(folder, batName);

            if (File.Exists(batPath))
            {
                try
                {
                    Process gameProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = batPath,
                        WorkingDirectory = folder,
                        UseShellExecute = true
                    });

                    if (gameProcess != null)
                    {
                        DateTime startTime = DateTime.Now;

                        this.Hide();

                        await gameProcess.WaitForExitAsync();

                        TimeSpan sessionTime = DateTime.Now - startTime;

                        Properties.Settings.Default.TotalPlayTime += sessionTime.TotalMinutes;
                        Properties.Settings.Default.Save();

                        this.Show();
                        UpdatePlayTimeDisplay();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error launching game: {ex.Message}");
                    this.Show();
                }
            }
            else
            {
                MessageBox.Show($"Could not find {batName} in the game folder.");
            }
        }

        private void LaunchScreen_Click(object sender, RoutedEventArgs e) => LaunchGame("Radium_ScreenMode.bat");

        private void LaunchVR_Click(object sender, RoutedEventArgs e) => LaunchGame("Radium_VR.bat");

        private void KillProcess_Click(object sender, RoutedEventArgs e)
        {
            foreach (var process in Process.GetProcessesByName("Radium"))
            {
                process.Kill();
            }
        }

        private void Discord_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://discord.gg/radium-rr") { UseShellExecute = true });
        }

        private void ViewLog_Click(object sender, RoutedEventArgs e)
        {
            string path = Properties.Settings.Default.GamePath;
            string batPath = System.IO.Path.Combine(path, "RecRoomLog.bat");

            if (System.IO.File.Exists(batPath))
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = batPath,
                    WorkingDirectory = path,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            else
            {
                MessageBox.Show("Could not find RecRoomLog.bat in the game folder.");
            }
        }

        private void UpdatePlayTimeDisplay()
        {
            double totalMinutes = Properties.Settings.Default.TotalPlayTime;
            int hours = (int)(totalMinutes / 60);
            int mins = (int)(totalMinutes % 60);
            txtPlayTime.Text = $"Played: {hours}h {mins}m";
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void Close_Click(object sender, RoutedEventArgs e) => this.Close();

        private void ChangeDir_Click(object sender, RoutedEventArgs e) => PromptForFolder();
    }
}