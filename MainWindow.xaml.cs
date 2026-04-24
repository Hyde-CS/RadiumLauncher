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
            this.Loaded += (s, e) => CheckPath();
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

        private void LaunchGame(string batName)
        {
            string folder = Properties.Settings.Default.GamePath;
            string batPath = Path.Combine(folder, batName);

            if (File.Exists(batPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = batPath,
                    WorkingDirectory = folder,
                    UseShellExecute = true
                });
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

        private void Minimize_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void Close_Click(object sender, RoutedEventArgs e) => this.Close();

        private void ChangeDir_Click(object sender, RoutedEventArgs e) => PromptForFolder();
    }
}